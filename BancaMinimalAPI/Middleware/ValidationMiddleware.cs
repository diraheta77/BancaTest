using FluentValidation;

namespace BancaMinimalAPI.Middleware
{
    public static class ValidationMiddleware
    {
        public static async Task<IResult> ValidateAsync<T>(T model, IValidator<T> validator)
        {
            var validationResult = await validator.ValidateAsync(model);
            
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => x.ErrorMessage).ToArray()
                    );
                
                return Results.BadRequest(new { errors });
            }
            
            return null;
        }
    }
}