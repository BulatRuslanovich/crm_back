using Microsoft.AspNetCore.Http;

namespace CrmBack.Services.Impl;

public class CookieService(IHttpContextAccessor httpContextAccessor) : ICookieService
{
    private const string AccessTokenCookieName = "access_token";
    private const string RefreshTokenCookieName = "refresh_token";
    private readonly bool _isSecure = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production";
    private const bool IsHttpOnly = true;
    private const SameSiteMode SameSite = SameSiteMode.Strict;

    private void SetTokenCookie(string cookieName, string token, DateTime expiresAt)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null) return;

        httpContext.Response.Cookies.Append(cookieName, token, new CookieOptions
        {
            HttpOnly = IsHttpOnly,
            Secure = _isSecure,
            SameSite = SameSite,
            Expires = expiresAt,
            Path = "/"
        });
    }

    public void SetAccessTokenCookie(string token, DateTime expiresAt) => SetTokenCookie(AccessTokenCookieName, token, expiresAt);

    public void SetRefreshTokenCookie(string token, DateTime expiresAt) => SetTokenCookie(RefreshTokenCookieName, token, expiresAt);

    public string? GetAccessTokenFromCookie() => httpContextAccessor.HttpContext?.Request.Cookies[AccessTokenCookieName];

    public string? GetRefreshTokenFromCookie() => httpContextAccessor.HttpContext?.Request.Cookies[RefreshTokenCookieName];

    public void ClearAuthCookies()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null) return;

        var expiredOptions = new CookieOptions
        {
            HttpOnly = IsHttpOnly,
            Secure = _isSecure,
            SameSite = SameSite,
            Expires = DateTime.UtcNow.AddDays(-1),
            Path = "/"
        };

        httpContext.Response.Cookies.Append(AccessTokenCookieName, "", expiredOptions);
        httpContext.Response.Cookies.Append(RefreshTokenCookieName, "", expiredOptions);
    }
}
