using Microsoft.EntityFrameworkCore;
using BancaMinimalAPI.Models;
using Microsoft.Data.SqlClient;
using BancaMinimalAPI.Features.CreditCards.DTOs;
using BancaMinimalAPI.Features.Transactions.DTOs;


namespace BancaMinimalAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<CreditCard> CreditCards { get; set; } = null!;
        public DbSet<Transaction> Transactions { get; set; } = null!;

        public DbSet<Configuration> Configurations { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CreditCard>()
                .Property(cc => cc.CurrentBalance)
                .HasPrecision(18, 2);

            modelBuilder.Entity<CreditCard>()
                .Property(cc => cc.CreditLimit)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasPrecision(18, 2);

                modelBuilder.Entity<Configuration>().HasData(
            new Configuration
            {
                Id = 1,
                InterestRate = 25.0m,
                MinimumPaymentRate = 5.0m,
                LastUpdated = DateTime.UtcNow
            }
        );

        }

        // public async Task<CreditCard> GetCreditCardStatementAsync(int creditCardId)
        // {
        //     var parameter = new SqlParameter("@CreditCardId", creditCardId);
        //     var result = await CreditCards
        //         .FromSqlRaw("EXEC sp_GetCreditCardStatement @CreditCardId", parameter)
        //         .AsNoTracking()
        //         .ToListAsync();

        //     return result.FirstOrDefault();
        // }

        public async Task<MonthlyTransactionSummaryDTO> GetCurrentMonthTransactionsAsync(int creditCardId)
        {
            var parameter = new SqlParameter("@CreditCardId", creditCardId);

            using var command = Database.GetDbConnection().CreateCommand();
            command.CommandText = "EXEC sp_GetCurrentMonthTransactions @CreditCardId";
            command.Parameters.Add(parameter);

            await Database.OpenConnectionAsync();

            using var reader = await command.ExecuteReaderAsync();

            var summary = new MonthlyTransactionSummaryDTO();

            // Read transactions
            while (await reader.ReadAsync())
            {
                summary.Transactions.Add(new TransactionDTO
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    CreditCardId = reader.GetInt32(reader.GetOrdinal("CreditCardId")),
                    Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                    Description = reader.GetString(reader.GetOrdinal("Description")),
                    Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                    Type = (TransactionType)reader.GetInt32(reader.GetOrdinal("Type"))
                });
            }

            // Read summary
            if (await reader.NextResultAsync() && await reader.ReadAsync())
            {
                summary.TotalPurchases = reader.GetDecimal(reader.GetOrdinal("TotalPurchases"));
                summary.TotalPayments = reader.GetDecimal(reader.GetOrdinal("TotalPayments"));
                summary.PurchaseCount = reader.GetInt32(reader.GetOrdinal("PurchaseCount"));
                summary.PaymentCount = reader.GetInt32(reader.GetOrdinal("PaymentCount"));
            }

            return summary;
        }


        public async Task<int> CreateTransactionAsync(Transaction transaction)
        {
            var parameters = new[]
            {
        new SqlParameter("@CreditCardId", transaction.CreditCardId),
        new SqlParameter("@Date", transaction.Date),
        new SqlParameter("@Description", transaction.Description),
        new SqlParameter("@Amount", transaction.Amount),
        new SqlParameter("@Type", (int)transaction.Type)
    };

            var sql = @"
        EXEC sp_CreateTransaction @CreditCardId, @Date, @Description, @Amount, @Type; 
        SELECT CAST(SCOPE_IDENTITY() AS INT) AS Id;";

            var result = await Database
                .SqlQueryRaw<int>(sql, parameters)
                .ToListAsync();

            return result.FirstOrDefault();
        }


        public async Task<CreditCardStatementDTO?> GetCreditCardStatementAsync(int creditCardId)
        {
            var parameter = new SqlParameter("@CreditCardId", creditCardId);

            using var command = Database.GetDbConnection().CreateCommand();
            command.CommandText = "EXEC sp_GetCreditCardStatement @CreditCardId";
            command.Parameters.Add(parameter);

            await Database.OpenConnectionAsync();

            using var reader = await command.ExecuteReaderAsync();

            CreditCardStatementDTO? statement = null;

            if (await reader.ReadAsync())
            {
                statement = new CreditCardStatementDTO
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    CardNumber = reader.GetString(reader.GetOrdinal("CardNumber")),
                    HolderName = reader.GetString(reader.GetOrdinal("HolderName")),
                    CreditLimit = reader.GetDecimal(reader.GetOrdinal("CreditLimit")),
                    TotalBalance = reader.GetDecimal(reader.GetOrdinal("TotalBalance")),
                    InterestRate = reader.GetDecimal(reader.GetOrdinal("InterestRate")),
                    MinimumPaymentRate = reader.GetDecimal(reader.GetOrdinal("MinimumPaymentRate")),
                    AvailableBalance = reader.GetDecimal(reader.GetOrdinal("AvailableBalance")),
                    BonusInterest = reader.GetDecimal(reader.GetOrdinal("BonusInterest")),
                    MinimumPayment = reader.GetDecimal(reader.GetOrdinal("MinimumPayment")),
                    TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                    TotalAmountWithInterest = reader.GetDecimal(reader.GetOrdinal("TotalAmountWithInterest")),
                    CurrentMonthPurchases = reader.GetDecimal(reader.GetOrdinal("CurrentMonthPurchases")),
                    PreviousMonthPurchases = reader.GetDecimal(reader.GetOrdinal("PreviousMonthPurchases"))
                };
            }

            if (statement != null && await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    statement.Transactions.Add(new TransactionDTO
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                        Description = reader.GetString(reader.GetOrdinal("Description")),
                        Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                        Type = (TransactionType)reader.GetInt32(reader.GetOrdinal("Type"))
                    });
                }
            }

            return statement;
        }


        public async Task<CreditCardStatementDTO?> GetFullCreditCardStatementAsync(int creditCardId)
        {
            var parameter = new SqlParameter("@CreditCardId", creditCardId);
            
            using var command = Database.GetDbConnection().CreateCommand();
            command.CommandText = "EXEC sp_GetFullCreditCardStatement @CreditCardId";
            command.Parameters.Add(parameter);
            
            await Database.OpenConnectionAsync();
            
            using var reader = await command.ExecuteReaderAsync();
            
            if (!await reader.ReadAsync())
                return null;

            var statement = new CreditCardStatementDTO
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                CardNumber = reader.GetString(reader.GetOrdinal("CardNumber")),
                HolderName = reader.GetString(reader.GetOrdinal("HolderName")),
                CreditLimit = reader.GetDecimal(reader.GetOrdinal("CreditLimit")),
                TotalBalance = reader.GetDecimal(reader.GetOrdinal("TotalBalance")),
                InterestRate = reader.GetDecimal(reader.GetOrdinal("InterestRate")),
                MinimumPaymentRate = reader.GetDecimal(reader.GetOrdinal("MinimumPaymentRate")),
                AvailableBalance = reader.GetDecimal(reader.GetOrdinal("AvailableBalance")),
                BonusInterest = reader.GetDecimal(reader.GetOrdinal("BonusInterest")),
                MinimumPayment = reader.GetDecimal(reader.GetOrdinal("MinimumPayment")),
                TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                TotalAmountWithInterest = reader.GetDecimal(reader.GetOrdinal("TotalAmountWithInterest")),
                CurrentMonthPurchases = reader.GetDecimal(reader.GetOrdinal("CurrentMonthPurchases")),
                PreviousMonthPurchases = reader.GetDecimal(reader.GetOrdinal("PreviousMonthPurchases")),
                Transactions = new List<TransactionDTO>()
            };

            // Leer transacciones si hay un segundo resultset
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    statement.Transactions.Add(new TransactionDTO
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                        Description = reader.GetString(reader.GetOrdinal("Description")),
                        Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                        Type = (TransactionType)reader.GetInt32(reader.GetOrdinal("Type"))
                    });
                }
            }

            return statement;
        }


        public async Task<PaymentAmountsDTO?> CalculatePaymentAmountsAsync(int creditCardId)
{
    var parameter = new SqlParameter("@CreditCardId", creditCardId);
    
    using var command = Database.GetDbConnection().CreateCommand();
    command.CommandText = "EXEC sp_CalculatePaymentAmounts @CreditCardId";
    command.Parameters.Add(parameter);
    
    await Database.OpenConnectionAsync();
    
    using var reader = await command.ExecuteReaderAsync();
    
    if (!await reader.ReadAsync())
        return null;

    return new PaymentAmountsDTO
    {
        TotalBalance = reader.GetDecimal(reader.GetOrdinal("TotalBalance")),
        BonusInterest = reader.GetDecimal(reader.GetOrdinal("BonusInterest")),
        MinimumPayment = reader.GetDecimal(reader.GetOrdinal("MinimumPayment")),
        TotalAmountWithInterest = reader.GetDecimal(reader.GetOrdinal("TotalAmountWithInterest"))
    };
}



public async Task<List<MonthlyBalanceDTO>> GetMonthlyBalancesAsync(int creditCardId, int monthsBack = 6)
{
    var parameters = new[]
    {
        new SqlParameter("@CreditCardId", creditCardId),
        new SqlParameter("@MonthsBack", monthsBack)
    };
    
    using var command = Database.GetDbConnection().CreateCommand();
    command.CommandText = "EXEC sp_GetMonthlyBalances @CreditCardId, @MonthsBack";
    command.Parameters.AddRange(parameters);
    
    await Database.OpenConnectionAsync();
    
    using var reader = await command.ExecuteReaderAsync();
    
    var balances = new List<MonthlyBalanceDTO>();
    
    while (await reader.ReadAsync())
    {
        balances.Add(new MonthlyBalanceDTO
        {
            MonthYear = reader.GetDateTime(reader.GetOrdinal("MonthYear")),
            TotalPurchases = reader.GetDecimal(reader.GetOrdinal("TotalPurchases")),
            TotalPayments = reader.GetDecimal(reader.GetOrdinal("TotalPayments")),
            PurchaseCount = reader.GetInt32(reader.GetOrdinal("PurchaseCount")),
            PaymentCount = reader.GetInt32(reader.GetOrdinal("PaymentCount")),
            NetBalance = reader.GetDecimal(reader.GetOrdinal("NetBalance"))
        });
    }
    
    return balances;
}


    }
}