using System.IdentityModel.Tokens.Jwt;
using System.Collections.Concurrent;
using CrmBack.Services;

namespace CrmBack.Core.Utils.Middleware;

public class TokenRefreshMiddleware(RequestDelegate next)
{
    // Кэш для хранения времени последней проверки токена (избегаем проверки на каждом запросе)
    private static readonly ConcurrentDictionary<string, DateTime> TokenCheckCache = new();
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(2); // Кэшируем на 2 минуты

    // Middleware для автоматического обновления access-токена за 5 минут до истечения срока действия
    public async Task InvokeAsync(HttpContext context, ICookieService cookie, IUserService user)
    {
        var accessToken = cookie.GetAccessTkn();

        if (!string.IsNullOrEmpty(accessToken))
        {
            // Генерируем ключ кэша на основе токена (первые 50 символов для уникальности)
            var cacheKey = accessToken.Length > 50 ? accessToken[..50] : accessToken;
            
            // Проверяем, была ли недавняя проверка этого токена
            if (!TokenCheckCache.TryGetValue(cacheKey, out var lastCheck) || 
                DateTime.UtcNow - lastCheck > CacheExpiration)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                
                // Безопасное чтение токена (без валидации подписи - только для проверки времени)
                try
                {
                    var token = tokenHandler.ReadJwtToken(accessToken);

                    if (token.ValidTo <= DateTime.UtcNow.AddMinutes(5))
                    {
                        var refTkn = cookie.GetRefreshTkn();

                        if (!string.IsNullOrEmpty(refTkn))
                        {
                            await user.RefreshToken(refTkn, context.RequestAborted);
                        }
                    }

                    // Обновляем кэш с временем последней проверки
                    TokenCheckCache.AddOrUpdate(cacheKey, DateTime.UtcNow, (key, oldValue) => DateTime.UtcNow);
                }
                catch
                {
                    // Если токен невалидный, просто пропускаем проверку
                }
            }
        }

        // Периодическая очистка устаревших записей из кэша (раз в 100 запросов примерно)
        if (Random.Shared.Next(100) == 0)
        {
            CleanupCache();
        }

        await next(context);
    }

    // Очистка устаревших записей из кэша
    private static void CleanupCache()
    {
        var now = DateTime.UtcNow;
        var keysToRemove = TokenCheckCache
            .Where(kvp => now - kvp.Value > CacheExpiration)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            TokenCheckCache.TryRemove(key, out _);
        }
    }
}
