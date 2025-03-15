using Microsoft.EntityFrameworkCore;
using BancaMinimalAPI.Models;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BancaMinimalAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<CreditCard> CreditCards { get; set; } = null!;
        public DbSet<Transaction> Transactions { get; set; } = null!;

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
        }

        public async Task<CreditCard> GetCreditCardStatementAsync(int creditCardId)
        {
            var parameter = new SqlParameter("@CreditCardId", creditCardId);
            var result = await CreditCards
                .FromSqlRaw("EXEC sp_GetCreditCardStatement @CreditCardId", parameter)
                .AsNoTracking()
                .ToListAsync();

            return result.FirstOrDefault();
        }

        public async Task<IEnumerable<Transaction>> GetCurrentMonthTransactionsAsync(int creditCardId)
        {
            var parameter = new SqlParameter("@CreditCardId", creditCardId);
            var result = await Transactions
                .FromSqlRaw("EXEC sp_GetCurrentMonthTransactions @CreditCardId", parameter)
                .AsNoTracking()
                .ToListAsync();

            return result;
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
        SELECT CAST(SCOPE_IDENTITY() AS decimal(18,0)) AS Id;";

            var result = await Database
                .SqlQueryRaw<decimal>(sql, parameters)
                .ToListAsync();

            return result.Any() ? (int)decimal.Round(result.First(), 0) : 0;
        }


    }
}