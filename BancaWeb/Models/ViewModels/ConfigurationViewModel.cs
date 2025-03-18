namespace BancaWeb.Models.ViewModels
{
    public class ConfigurationViewModel
    {
        public int Id { get; set; }
        public decimal InterestRate { get; set; }
        public decimal MinimumPaymentRate { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}