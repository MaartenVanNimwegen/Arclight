using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Arclight.Api.Filters;

public class UserActionLogFilter(ILogger<UserActionLogFilter> logger) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        var user = httpContext.User;

        var userId = user.Identity?.IsAuthenticated == true
            ? user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
            : "Anonymous";

        var path = httpContext.Request.Path;
        var method = SanitizeForLog(httpContext.Request.Method);

        logger.LogInformation("Action started: User {UserId} called {Method} {Path}", userId, method, path);

        var result = await next(context);

        logger.LogInformation("Action completed: User {UserId} finished {Method} {Path}", userId, method, path);

        return result;
    }

    private static string SanitizeForLog(string? value)
    {
        return string.IsNullOrEmpty(value)
            ? string.Empty
            : value.Replace("\r", string.Empty).Replace("\n", string.Empty);
    }
}