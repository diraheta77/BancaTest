using FluentValidation;
using BancaMinimalAPI.Features.Transactions.DTOs;

namespace BancaMinimalAPI.Features.Transactions.Validators
{
    public class CreateTransactionDTOValidator : AbstractValidator<CreateTransactionDTO>
    {
        public CreateTransactionDTOValidator()
        {
            RuleFor(x => x.CreditCardId)
                .GreaterThan(0)
                .WithMessage("El ID de la tarjeta de crédito es requerido");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("El monto debe ser mayor a cero");

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage("La descripción es requerida")
                .MaximumLength(200)
                .WithMessage("La descripción no puede exceder 200 caracteres");

            RuleFor(x => x.Date)
                .NotEmpty()
                .WithMessage("La fecha es requerida")
                .LessThanOrEqualTo(DateTime.Now)
                .WithMessage("La fecha no puede ser futura");
        }
    }
}