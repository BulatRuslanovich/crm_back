using System.Security.Claims;

namespace CrmBack.Core.Utils;

public static class JwtHelper
{
    // Извлечение ID пользователя из claims аутентифицированного пользователя в HttpContext
    public static int? GetUserIdFromContext(HttpContext context) =>
        context.User.FindFirst(ClaimTypes.NameIdentifier) is { Value: var value }
            && int.TryParse(value, out int userId)
            ? userId
            : null;
}
