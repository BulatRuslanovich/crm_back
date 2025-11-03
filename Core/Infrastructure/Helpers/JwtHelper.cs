using System.Security.Claims;

namespace CrmBack.Core.Infrastructure.Helpers;

public static class JwtHelper
{
    public static int? GetUserIdFromContext(HttpContext context) =>
        context.User.FindFirst(ClaimTypes.NameIdentifier) is { Value: var value }
            && int.TryParse(value, out int userId)
            ? userId
            : null;
}
