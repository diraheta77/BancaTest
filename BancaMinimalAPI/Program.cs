using AutoMapper;
using BancaMinimalAPI.Common.Mappings;
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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddScoped<PdfGeneratorService>();
//defino el context para la BD
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateTime.Now.AddDays(index),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

//Agrego los endpoints
app.MapGet("/api/creditcards/{id}", async (int id, AppDbContext db, IMapper mapper) =>
{
    var statement = await db.GetCreditCardStatementAsync(id);
    if (statement == null) return Results.NotFound();
    return Results.Ok(statement);
})
.WithName("GetCreditCard")
.WithOpenApi();

// Endpoint para listar transacciones
app.MapGet("/api/creditcards/{id}/transactions", async (int id, AppDbContext db, IMapper mapper) =>
{
    var transactions = await db.Transactions
        .Where(t => t.CreditCardId == id)
        .OrderByDescending(t => t.Date)
        .ToListAsync();
    
    return Results.Ok(mapper.Map<List<TransactionDTO>>(transactions));
})
.WithName("GetTransactions")
.WithOpenApi();

// Endpoint para crear una compra
app.MapPost("/api/transactions/purchase", async (CreateTransactionDTO createDto, AppDbContext db) =>
{
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
    catch (SqlException ex)
    {
        return Results.BadRequest(ex.Message);
    }
})
.WithName("CreatePurchase")
.WithOpenApi();

// Endpoint para realizar un pago
app.MapPost("/api/transactions/payment", async (CreateTransactionDTO createDto, AppDbContext db) =>
{
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
    catch (SqlException ex)
    {
        return Results.BadRequest(ex.Message);
    }
})
.WithName("CreatePayment")
.WithOpenApi();

// Endpoint para obtener transacciones del mes actual
app.MapGet("/api/creditcards/{id}/transactions/current-month", async (int id, AppDbContext db, IMapper mapper) =>
{
    var summary = await db.GetCurrentMonthTransactionsAsync(id);
    //return Results.Ok(mapper.Map<List<TransactionDTO>>(transactions));
    return Results.Ok(summary);
})
.WithName("GetCurrentMonthTransactions")
.WithOpenApi();


// Endpoint para exportar estado de cuenta a PDF
app.MapGet("/api/creditcards/{id}/statement/pdf", async (int id, AppDbContext db, PdfGeneratorService pdfService) =>
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
.WithName("ExportStatementPdf")
.WithOpenApi();

app.Run();

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}