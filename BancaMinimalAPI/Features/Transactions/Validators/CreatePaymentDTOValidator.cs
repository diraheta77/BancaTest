using FluentValidation;
using BancaMinimalAPI.Features.Transactions.DTOs;
using BancaMinimalAPI.Data;

namespace BancaMinimalAPI.Features.Transactions.Validators
{
    public class CreatePaymentDTOValidator : AbstractValidator<CreatePaymentDTO>
    {
        private readonly AppDbContext _context;

        public CreatePaymentDTOValidator(AppDbContext context)
        {
            _context = context;

            RuleFor(x => x.CreditCardId)
                .NotEmpty()
                .WithMessage("El ID de la tarjeta es requerido")
                .MustAsync(async (id, cancellation) =>
                {
                    var card = await _context.CreditCards.FindAsync(id);
                    return card != null;
                })
                .WithMessage("La tarjeta de crédito no existe");

            RuleFor(x => x.Amount)
                .NotEmpty()
                .WithMessage("El monto es requerido")
                .GreaterThan(0)
                .WithMessage("El monto debe ser mayor a cero")
                .MustAsync(async (payment, amount, cancellation) =>
                {
                    var card = await _context.CreditCards.FindAsync(payment.CreditCardId);
                    return card != null && amount <= card.CurrentBalance;
                })
                .WithMessage("El pago no puede ser mayor al saldo actual");

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