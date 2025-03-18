namespace BancaMinimalAPI.Features.CreditCards.DTOs
{
    public class PaymentAmountsDTO
    {
        public decimal TotalBalance { get; set; }
        public decimal BonusInterest { get; set; }
        public decimal MinimumPayment { get; set; }
        public decimal TotalAmountWithInterest { get; set; }
    }
}