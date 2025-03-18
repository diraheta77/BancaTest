namespace BancaMinimalAPI.Features.Configuration.DTOs
{
    public class ConfigurationDTO
    {
        public int Id { get; set; }
        public decimal InterestRate { get; set; }
        public decimal MinimumPaymentRate { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}