namespace BancaMinimalAPI.Features.CreditCards.DTOs
{
    public class CreditCardDTO
    {
        public int Id { get; set; }
        public string CardNumber { get; set; } = string.Empty;
        public string HolderName { get; set; } = string.Empty;
        public decimal CreditLimit { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal InterestRate { get; set; }
        public decimal MinimumPaymentRate { get; set; }
        
        // para calculos
        public decimal AvailableBalance { get; set; }
        public decimal BonusInterest { get; set; }
        public decimal MinimumPayment { get; set; }
        public decimal CashPaymentWithInterest { get; set; }
        
        // montos mensuales
        public decimal CurrentMonthPurchases { get; set; }
        public decimal PreviousMonthPurchases { get; set; }
    }
}

