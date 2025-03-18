namespace BancaWeb.Models.ViewModels
{
    public class CreditCardViewModel
    {
        public int Id { get; set; }
        public string CardNumber { get; set; } = string.Empty;
        public string HolderName { get; set; } = string.Empty;
        public decimal CreditLimit { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal AvailableBalance { get; set; }
        public decimal InterestRate { get; set; }
        public decimal MinimumPaymentRate { get; set; }
        public decimal CurrentMonthPurchases { get; set; }
        public decimal PreviousMonthPurchases { get; set; }
        public decimal BonusInterest { get; set; }
        public decimal MinimumPayment { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalAmountWithInterest { get; set; }
        public List<TransactionViewModel> Transactions { get; set; } = new();
    }
}