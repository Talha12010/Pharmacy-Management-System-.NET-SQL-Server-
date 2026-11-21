using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PharmacyManagement.API.Middleware;

public sealed class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            var argumentType = argument.GetType();
            var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);
            if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator)
            {
                continue;
            }

            var validationContextType = typeof(ValidationContext<>).MakeGenericType(argumentType);
            var validationContext = (IValidationContext)Activator.CreateInstance(validationContextType, argument)!;
            var result = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);
            if (result.IsValid)
            {
                continue;
            }

            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray(),
                    StringComparer.OrdinalIgnoreCase);

            throw new Domain.Exceptions.ValidationFailedException("One or more validation errors occurred.", errors);
        }

        await next();
    }
}
