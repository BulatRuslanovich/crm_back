using System.Security.Claims;

namespace CrmBack.Core.Utils;

public static class JwtHelper
{
    public static int? GetUserIdFromContext(HttpContext context) =>
        context.User.FindFirst(ClaimTypes.NameIdentifier) is { Value: var value }
            && int.TryParse(value, out var userId)
            ? userId
            : null;
}
