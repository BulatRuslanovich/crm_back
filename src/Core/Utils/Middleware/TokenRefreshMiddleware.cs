using System.IdentityModel.Tokens.Jwt;
using CrmBack.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace CrmBack.Core.Utils.Middleware;

public class TokenRefreshMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ICookieService cookieService, IUserService userService)
    {
        var accessToken = cookieService.GetAccessTokenFromCookie();

        if (!string.IsNullOrEmpty(accessToken))
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.ReadJwtToken(accessToken);

                if (token.ValidTo <= DateTime.UtcNow.AddMinutes(5))
                {
                    // Пытаемся обновить токен
                    var refreshToken = cookieService.GetRefreshTokenFromCookie();
                    if (!string.IsNullOrEmpty(refreshToken))
                    {
                        try
                        {
                            await userService.RefreshToken(refreshToken, context.RequestAborted);
                        }
                        catch
                        {
                            cookieService.ClearAuthCookies();
                        }
                    }
                }
            }
            catch
            {
                cookieService.ClearAuthCookies();
            }
        }

        await next(context);
    }
}
