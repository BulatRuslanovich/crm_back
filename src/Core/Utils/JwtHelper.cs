using System.Security.Claims;

namespace CrmBack.Core.Utils;

public static class JwtHelper
{
    public static int? GetUserIdFromContext(HttpContext context)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        return null;
    }

    public static string? GetUserLoginFromContext(HttpContext context)
    {
        return context.User.FindFirst(ClaimTypes.Name)?.Value;
    }

    public static List<string> GetUserRolesFromContext(HttpContext context)
    {
        return context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
    }

    public static bool IsAuthenticated(HttpContext context)
    {
        return context.User.Identity?.IsAuthenticated == true;
    }
}
