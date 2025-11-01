using System.IdentityModel.Tokens.Jwt;
using CrmBack.Services;

namespace CrmBack.Core.Utils.Middleware;

public class TokenRefreshMiddleware(RequestDelegate next)
{
    // Middleware для автоматического обновления access-токена за 5 минут до истечения срока действия
    public async Task InvokeAsync(HttpContext context, ICookieService cookie, IUserService user)
    {
        var accessToken = cookie.GetAccessTkn();

        if (!string.IsNullOrEmpty(accessToken))
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(accessToken);

            if (token.ValidTo <= DateTime.UtcNow.AddMinutes(5))
            {
                var refTkn = cookie.GetRefreshTkn();

                if (!string.IsNullOrEmpty(refTkn))
                {
                    await user.RefreshToken(refTkn, context.RequestAborted);
                }
            }
        }

        await next(context);
    }
}
