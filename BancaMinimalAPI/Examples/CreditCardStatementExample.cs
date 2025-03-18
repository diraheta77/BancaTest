using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using BancaMinimalAPI.Features.CreditCards.DTOs;
using BancaMinimalAPI.Features.Transactions.DTOs;
using BancaMinimalAPI.Models;

namespace BancaMinimalAPI.Examples
{
    public class CreditCardStatementExample : IExamplesProvider<CreditCardStatementDTO>
    {
        public CreditCardStatementDTO GetExamples()
        {
            return new CreditCardStatementDTO
            {
                Id = 1,
                CardNumber = "4111-1111-1111-1111",
                HolderName = "John Doe",
                CreditLimit = 5000.00m,
                TotalBalance = 1500.00m,
                AvailableBalance = 3500.00m,
                MinimumPayment = 75.00m,
                BonusInterest = 37.50m,
                Transactions = new List<TransactionDTO>
                {
                    new()
                    {
                        Id = 1,
                        Date = DateTime.Now,
                        Description = "Compra Supermercado",
                        Amount = 150.00m,
                        Type = TransactionType.Purchase
                    }
                }
            };
        }
    }
}