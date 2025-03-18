namespace BancaWeb.Models.ViewModels
{
    public class TransactionViewModel
    {
        public int Id { get; set; }
        public int CreditCardId { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
    }

    public enum TransactionType
    {
        Purchase = 0,
        Payment = 1
    }
}