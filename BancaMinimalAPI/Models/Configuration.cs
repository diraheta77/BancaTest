namespace BancaMinimalAPI.Models
{
    public class Configuration
    {
        public int Id { get; set; }
        public decimal InterestRate { get; set; }
        public decimal MinimumPaymentRate { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}