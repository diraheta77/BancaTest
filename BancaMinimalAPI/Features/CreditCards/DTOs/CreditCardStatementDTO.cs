using BancaMinimalAPI.Features.Transactions.DTOs;

namespace BancaMinimalAPI.Features.CreditCards.DTOs
{
    public class CreditCardStatementDTO
    {
        public int Id { get; set; }
        public string CardNumber { get; set; } = string.Empty;
        public string HolderName { get; set; } = string.Empty;
        public decimal CreditLimit { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal InterestRate { get; set; }
        public decimal MinimumPaymentRate { get; set; }
        public decimal AvailableBalance { get; set; }
        public decimal BonusInterest { get; set; }
        public decimal MinimumPayment { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalAmountWithInterest { get; set; }
        public decimal CurrentMonthPurchases { get; set; }
        public decimal PreviousMonthPurchases { get; set; }
        public List<TransactionDTO> Transactions { get; set; } = new();
    }
}