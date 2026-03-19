using System.Security.Claims;

namespace Arclight.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst("sub")?.Value;

        if (Guid.TryParse(userIdClaim, out Guid userId))
        {
            return userId;
        }

        throw new UnauthorizedAccessException("UserId is missing in the security token.");
    }
}