using AutoMapper;
using BancaMinimalAPI.Common.Mappings;
using BancaMinimalAPI.Common;
using BancaMinimalAPI.Data;
using BancaMinimalAPI.Features.CreditCards.DTOs;
using BancaMinimalAPI.Features.Transactions.DTOs;
using BancaMinimalAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Data.SqlClient;
using BancaMinimalAPI.Services;
using FluentValidation;
using BancaMinimalAPI.Features.Transactions.Validators;
using BancaMinimalAPI.Middleware;
using BancaMinimalAPI.Common.Exceptions;
using AspNetCoreRateLimit;
using Swashbuckle.AspNetCore.Filters;
using BancaMinimalAPI.Features.Configuration.DTOs;
using BancaMinimalAPI.Features.Configuration.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddScoped<PdfGeneratorService>();
builder.Services.AddValidatorsFromAssemblyContaining<CreatePaymentDTOValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ConfigurationValidator>();
//defino el context para la BD
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Banca API",
        Version = "v1",
        Description = "API para gestión de tarjetas de crédito y transacciones",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Diego Iraheta",
            Email = "diego.iraheta77@gmail.com"
        }
    });
    
    // Add Swagger examples configuration
    options.ExampleFilters();
});
//lo agrego como ejemplo..
builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

// Add Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddInMemoryRateLimiting();

// Add after existing service configurations
builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

var app = builder.Build();

// middleware de excepciones 
app.UseGlobalExceptionHandler();

// Add before other middleware
app.UseIpRateLimiting();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Organizar endpoints por grupos
var creditCardsGroup = app.MapGroup("/api/creditcards")
    .WithTags("Credit Cards")
    .WithOpenApi();

var transactionsGroup = app.MapGroup("/api/transactions")
    .WithTags("Transactions")
    .WithOpenApi();

var configurationGroup = app.MapGroup("/api/configuration")
    .WithTags("Configuration")
    .WithOpenApi();

// Credit Cards endpoints
creditCardsGroup.MapGet("/{id}", async (int id, AppDbContext db, IMapper mapper) =>
{
    var statement = await db.GetCreditCardStatementAsync(id);
    if (statement == null) return Results.NotFound();
    return Results.Ok(statement);
})
.WithName("GetCreditCard")
.WithOpenApi(operation => {
    operation.Summary = "Obtiene el estado de una tarjeta de crédito";
    operation.Description = "Retorna información básica de la tarjeta y su estado actual";
    operation.Parameters[0].Description = "ID de la tarjeta de crédito";
    return operation;
});

creditCardsGroup.MapGet("/{id}/transactions", async (int id, AppDbContext db, IMapper mapper) =>
{
    var transactions = await db.Transactions
        .Where(t => t.CreditCardId == id)
        .OrderByDescending(t => t.Date)
        .ToListAsync();
    
    return Results.Ok(mapper.Map<List<TransactionDTO>>(transactions));
})
.WithName("GetTransactions");

creditCardsGroup.MapGet("/{id}/transactions/current-month", async (int id, AppDbContext db, IMapper mapper) =>
{
    var summary = await db.GetCurrentMonthTransactionsAsync(id);
    return Results.Ok(summary);
})
.WithName("GetCurrentMonthTransactions");

creditCardsGroup.MapGet("/{id}/statement/pdf", async (int id, AppDbContext db, PdfGeneratorService pdfService) =>
{
    var statement = await db.GetCreditCardStatementAsync(id);
    if (statement == null) return Results.NotFound();

    var pdfBytes = pdfService.GenerateStatementPdf(statement);
    return Results.File(
        pdfBytes,
        "application/pdf",
        $"estado-cuenta-{id}-{DateTime.Now:yyyyMMdd}.pdf"
    );
})
.WithName("ExportStatementPdf");

creditCardsGroup.MapGet("/{id}/full-statement", async (int id, AppDbContext db) =>
{
    try
    {
        var statement = await db.GetFullCreditCardStatementAsync(id);
        if (statement == null)
            throw new BusinessException("Tarjeta de crédito no encontrada", 404);
            
        return Results.Ok(statement);
    }
    catch (Exception ex)
    {
        throw new BusinessException($"Error al obtener el estado de cuenta: {ex.Message}");
    }
})
.WithName("GetFullCreditCardStatement")
.WithOpenApi(operation => {
    operation.Summary = "Obtiene el estado de cuenta completo";
    operation.Description = "Incluye saldo actual, intereses, pagos mínimos y transacciones del mes";
    operation.Parameters[0].Description = "ID de la tarjeta de crédito";
    return operation;
})
.Produces<CreditCardStatementDTO>(200)
.Produces(404);

creditCardsGroup.MapGet("/{id}/payment-amounts", async (int id, AppDbContext db) =>
{
    try
    {
        var amounts = await db.CalculatePaymentAmountsAsync(id);
        if (amounts == null)
            throw new BusinessException("Tarjeta de crédito no encontrada", 404);
            
        return Results.Ok(amounts);
    }
    catch (Exception ex)
    {
        throw new BusinessException($"Error al calcular los montos: {ex.Message}");
    }
})
.WithName("GetPaymentAmounts");

creditCardsGroup.MapGet("/{id}/monthly-balances", async (int id, int? months, AppDbContext db) =>
{
    try
    {
        var balances = await db.GetMonthlyBalancesAsync(id, months ?? 6);
        if (!balances.Any())
            throw new BusinessException("No se encontraron datos para la tarjeta especificada", 404);
            
        return Results.Ok(balances);
    }
    catch (Exception ex)
    {
        throw new BusinessException($"Error al obtener los saldos mensuales: {ex.Message}");
    }
})
.WithName("GetMonthlyBalances");

// Transactions endpoints
transactionsGroup.MapPost("/purchase", async (CreateTransactionDTO createDto, 
    IValidator<CreateTransactionDTO> validator,
    AppDbContext db) =>
{
    var validationResult = await ValidationMiddleware.ValidateAsync(createDto, validator);
    if (validationResult != null) return validationResult;

    var transaction = new Transaction
    {
        CreditCardId = createDto.CreditCardId,
        Date = createDto.Date,
        Description = createDto.Description,
        Amount = createDto.Amount,
        Type = TransactionType.Purchase
    };

    try
    {
        var transactionId = await db.CreateTransactionAsync(transaction);
        transaction.Id = transactionId;
        return Results.Created($"/api/transactions/{transactionId}", transaction);
    }
    catch (Exception ex)
    {
        throw new BusinessException($"Error al crear la compra: {ex.Message}");
    }
})
.WithName("CreatePurchase")
.WithOpenApi(operation => {
    operation.Summary = "Registra una nueva compra";
    operation.Description = "Crea una nueva transacción de tipo compra y actualiza el saldo de la tarjeta";
    return operation;
})
.ProducesValidationProblem()
.Produces<Transaction>(201)
.Produces(400);

transactionsGroup.MapPost("/payment", async (CreatePaymentDTO createDto, 
    IValidator<CreatePaymentDTO> validator,
    AppDbContext db) =>
{
    var validationResult = await ValidationMiddleware.ValidateAsync(createDto, validator);
    if (validationResult != null) return validationResult;

    var transaction = new Transaction
    {
        CreditCardId = createDto.CreditCardId,
        Date = createDto.Date,
        Description = createDto.Description,
        Amount = createDto.Amount,
        Type = TransactionType.Payment
    };

    try
    {
        var transactionId = await db.CreateTransactionAsync(transaction);
        transaction.Id = transactionId;
        return Results.Created($"/api/transactions/{transactionId}", transaction);
    }
    catch (Exception ex)
    {
        throw new BusinessException($"Error al procesar el pago: {ex.Message}");
    }
})
.WithName("CreatePayment");

// Configuration endpoints
configurationGroup.MapGet("/", async (AppDbContext db, IMapper mapper) =>
{
    var config = await db.Configurations.FirstOrDefaultAsync();
    if (config == null) return Results.NotFound();
    return Results.Ok(mapper.Map<ConfigurationDTO>(config));
})
.WithName("GetConfiguration");


configurationGroup.MapPut("/", async (ConfigurationDTO dto, AppDbContext db, IMapper mapper) =>
{
    var config = await db.Configurations.FirstOrDefaultAsync();
    if (config == null)
    {
        config = mapper.Map<BancaMinimalAPI.Models.Configuration>(dto);
        db.Configurations.Add(config);
    }
    else
    {
        mapper.Map(dto, config);
        config.LastUpdated = DateTime.UtcNow;
    }
    
    await db.SaveChangesAsync();
    return Results.Ok(mapper.Map<ConfigurationDTO>(config));
})
.WithName("UpdateConfiguration");

// Health Check endpoint
app.MapHealthChecks("/health")
    .WithName("HealthCheck")
    .WithOpenApi();

app.Run();
