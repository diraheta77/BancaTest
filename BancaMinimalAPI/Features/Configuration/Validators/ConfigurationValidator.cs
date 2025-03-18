using BancaMinimalAPI.Features.Configuration.DTOs;
using FluentValidation;

namespace BancaMinimalAPI.Features.Configuration.Validators
{
    public class ConfigurationValidator : AbstractValidator<ConfigurationDTO>
    {
        public ConfigurationValidator()
        {
            RuleFor(x => x.InterestRate)
                .GreaterThanOrEqualTo(0)
                .LessThanOrEqualTo(100)
                .WithMessage("La tasa de interés debe estar entre 0 y 100");

            RuleFor(x => x.MinimumPaymentRate)
                .GreaterThanOrEqualTo(0)
                .LessThanOrEqualTo(100)
                .WithMessage("La tasa de pago mínimo debe estar entre 0 y 100");
        }
    }
}