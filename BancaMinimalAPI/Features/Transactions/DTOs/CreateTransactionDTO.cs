namespace BancaMinimalAPI.Features.Transactions.DTOs
{
    public class CreateTransactionDTO
    {
        public int CreditCardId { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}