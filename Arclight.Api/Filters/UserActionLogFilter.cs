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

        var userId = "Anonymous";
        if (user.Identity?.IsAuthenticated == true)
        {
            userId = user.FindFirst("sub")?.Value
                ?? user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                ?? "AuthenticatedUser";
        }
        var sanitizedUserId = SanitizeForLog(userId);

        var path = SanitizeForLog(httpContext.Request.Path.ToString());
        var method = SanitizeForLog(httpContext.Request.Method);

        logger.LogDebug("Action started: User {UserId} called {Method} {Path}", sanitizedUserId, method, path);

        var result = await next(context);

        logger.LogDebug("Action completed: User {UserId} finished {Method} {Path}", sanitizedUserId, method, path);

        return result;
    }

    private static string SanitizeForLog(string? value)
    {
        return string.IsNullOrEmpty(value)
            ? string.Empty
            : value.Replace("\r", string.Empty).Replace("\n", string.Empty);
    }
}
