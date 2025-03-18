
using BancaMinimalAPI.Models;

namespace BancaMinimalAPI.Extensions
{
    public static class EnumExtensions
    {
        public static string ToSpanish(this TransactionType type)
        {
            return type switch
            {
                TransactionType.Purchase => "Compra",
                TransactionType.Payment => "Pago",
                _ => type.ToString()
            };
        }
    }
}