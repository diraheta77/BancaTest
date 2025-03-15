namespace BancaMinimalAPI.Models
{
    public class CreditCard
    {
        public int Id { get; set; }
        public string CardNumber { get; set; } = string.Empty;
        public string HolderName { get; set; } = string.Empty;
        public decimal CreditLimit { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal InterestRate { get; set; }
        public decimal MinimumPaymentRate { get; set; }
        
        // Calculated properties
        public decimal AvailableBalance => CreditLimit - CurrentBalance;
        public decimal BonusInterest => CurrentBalance * (InterestRate / 100);
        public decimal MinimumPayment => CurrentBalance * (MinimumPaymentRate / 100);
        public decimal CashPaymentWithInterest => CurrentBalance + BonusInterest;
    }
}