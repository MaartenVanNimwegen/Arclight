using FluentValidation;

namespace Arclight.Api.Filters;

public class ValidationFilter<T>(IValidator<T> validator) : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var argument = context.Arguments.FirstOrDefault(x => x is T) as T;

        if (argument is null)
        {
            return Results.BadRequest("Invalid request body.");
        }

        var validationResult = await validator.ValidateAsync(argument, context.HttpContext.RequestAborted);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).Distinct().ToArray()
                );

            return Results.ValidationProblem(errors);
        }

        return await next(context);
    }
}
