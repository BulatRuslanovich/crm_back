using Microsoft.AspNetCore.Http;

namespace CrmBack.Services.Impl;

public class CookieService(IHttpContextAccessor httpContextAccessor) : ICookieService
{
    private const string AccessTokenCookieName = "access_token";
    private const string RefreshTokenCookieName = "refresh_token";
    private readonly bool _isSecure = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production";
    private const bool IsHttpOnly = true;
    private const SameSiteMode SameSite = SameSiteMode.Strict;

    public void SetAccessTokenCookie(string token, DateTime expiresAt)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null) return;

        var cookieOptions = new CookieOptions
        {
            HttpOnly = IsHttpOnly,
            Secure = _isSecure,
            SameSite = SameSite,
            Expires = expiresAt,
            Path = "/"
        };

        httpContext.Response.Cookies.Append(AccessTokenCookieName, token, cookieOptions);
    }

    public void SetRefreshTokenCookie(string token, DateTime expiresAt)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null) return;

        var cookieOptions = new CookieOptions
        {
            HttpOnly = IsHttpOnly,
            Secure = _isSecure,
            SameSite = SameSite,
            Expires = expiresAt,
            Path = "/"
        };

        httpContext.Response.Cookies.Append(RefreshTokenCookieName, token, cookieOptions);
    }

    public string? GetAccessTokenFromCookie()
    {
        var httpContext = httpContextAccessor.HttpContext;
        return httpContext?.Request.Cookies[AccessTokenCookieName];
    }

    public string? GetRefreshTokenFromCookie()
    {
        var httpContext = httpContextAccessor.HttpContext;
        return httpContext?.Request.Cookies[RefreshTokenCookieName];
    }

    public void ClearAuthCookies()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null) return;

        var cookieOptions = new CookieOptions
        {
            HttpOnly = IsHttpOnly,
            Secure = _isSecure,
            SameSite = SameSite,
            Expires = DateTime.UtcNow.AddDays(-1),
            Path = "/"
        };

        httpContext.Response.Cookies.Append(AccessTokenCookieName, "", cookieOptions);
        httpContext.Response.Cookies.Append(RefreshTokenCookieName, "", cookieOptions);
    }
}
