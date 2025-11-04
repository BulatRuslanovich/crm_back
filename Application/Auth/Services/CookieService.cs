namespace CrmBack.Application.Auth.Services;

/// <summary>
/// Cookie service implementation
/// Manages HTTP-only cookies for JWT tokens
/// Security: HTTP-only cookies prevent JavaScript access (XSS protection)
/// SameSite=Strict prevents CSRF attacks
/// </summary>
public class CookieService(IHttpContextAccessor accessor) : ICookieService
{
    private const string AccessTokenCookieName = "access_token";
    private const string RefreshTokenCookieName = "refresh_token";
    
    /// <summary>
    /// Secure flag: true in production (requires HTTPS)
    /// Prevents cookies from being sent over unencrypted connections
    /// </summary>
    private readonly bool _isSecure = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production";
    
    /// <summary>
    /// HTTP-only flag: prevents JavaScript access to cookies
    /// Security: Mitigates XSS attacks
    /// </summary>
    private const bool IsHttpOnly = true;
    
    /// <summary>
    /// SameSite mode: Strict prevents cross-site cookie sending
    /// Security: Mitigates CSRF attacks
    /// </summary>
    private const SameSiteMode SameSite = SameSiteMode.Strict;

    /// <summary>
    /// Internal method to set token cookie with security options
    /// </summary>
    private void SetToken(string name, string token, DateTime expiresAt)
    {
        var httpContext = accessor.HttpContext;
        if (httpContext is null) return;

        httpContext.Response.Cookies.Append(name, token, new CookieOptions
        {
            HttpOnly = IsHttpOnly,      // Prevent JavaScript access
            Secure = _isSecure,         // HTTPS only in production
            SameSite = SameSite,        // Prevent cross-site requests
            Expires = expiresAt,        // Cookie expiration time
            Path = "/"                  // Cookie available for all paths
        });
    }

    /// <summary>Set access token cookie</summary>
    public void SetAccessTkn(string token, DateTime expiresAt) => SetToken(AccessTokenCookieName, token, expiresAt);

    /// <summary>Set refresh token cookie</summary>
    public void SetRefreshTkn(string token, DateTime expiresAt) => SetToken(RefreshTokenCookieName, token, expiresAt);

    /// <summary>Get access token from request cookie</summary>
    public string? GetAccessTkn() => accessor.HttpContext?.Request.Cookies[AccessTokenCookieName];

    /// <summary>Get refresh token from request cookie</summary>
    public string? GetRefreshTkn() => accessor.HttpContext?.Request.Cookies[RefreshTokenCookieName];

    /// <summary>
    /// Clear all authentication cookies
    /// Sets cookies with expired date to force browser deletion
    /// </summary>
    public void Clear()
    {
        var httpContext = accessor.HttpContext;
        if (httpContext is null) return;

        // Create expired cookie options to force browser deletion
        var expiredOptions = new CookieOptions
        {
            HttpOnly = IsHttpOnly,
            Secure = _isSecure,
            SameSite = SameSite,
            Expires = DateTime.UtcNow.AddDays(-1), // Expired date forces deletion
            Path = "/"
        };

        // Set empty cookies with expired date
        httpContext.Response.Cookies.Append(AccessTokenCookieName, "", expiredOptions);
        httpContext.Response.Cookies.Append(RefreshTokenCookieName, "", expiredOptions);
    }
}
