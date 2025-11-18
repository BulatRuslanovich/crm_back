namespace CrmBack.Application.Auth.Services;


public class CookieService(IHttpContextAccessor accessor) : ICookieService
{
	private const string AccessTokenCookieName = "access_token";
	private const string RefreshTokenCookieName = "refresh_token";
	private readonly bool _isSecure = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production";
	private const bool IsHttpOnly = true;
	private const SameSiteMode SameSite = SameSiteMode.Strict;

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

	public void SetAccessTkn(string token, DateTime expiresAt) => SetToken(AccessTokenCookieName, token, expiresAt);
	public void SetRefreshTkn(string token, DateTime expiresAt) => SetToken(RefreshTokenCookieName, token, expiresAt);
	public string? GetAccessTkn() => accessor.HttpContext?.Request.Cookies[AccessTokenCookieName];
	public string? GetRefreshTkn() => accessor.HttpContext?.Request.Cookies[RefreshTokenCookieName];

	public void Clear()
	{
		var httpContext = accessor.HttpContext;
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
