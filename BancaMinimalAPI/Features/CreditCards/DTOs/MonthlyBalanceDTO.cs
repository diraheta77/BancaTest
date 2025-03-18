namespace BancaMinimalAPI.Features.CreditCards.DTOs
{
    public class MonthlyBalanceDTO
    {
        public DateTime MonthYear { get; set; }
        public decimal TotalPurchases { get; set; }
        public decimal TotalPayments { get; set; }
        public int PurchaseCount { get; set; }
        public int PaymentCount { get; set; }
        public decimal NetBalance { get; set; }
    }
}