namespace BancaMinimalAPI.Features.Transactions.DTOs
{
    public class MonthlyTransactionSummaryDTO
    {
        public decimal TotalPurchases { get; set; }
        public decimal TotalPayments { get; set; }
        public int PurchaseCount { get; set; }
        public int PaymentCount { get; set; }
        public List<TransactionDTO> Transactions { get; set; } = new();
    }
}