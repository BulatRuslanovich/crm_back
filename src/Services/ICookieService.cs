namespace CrmBack.Services;

public interface ICookieService
{
    public void SetAccessTokenCookie(string token, DateTime expiresAt);
    public void SetRefreshTokenCookie(string token, DateTime expiresAt);
    public string? GetAccessTokenFromCookie();
    public string? GetRefreshTokenFromCookie();
    public void ClearAuthCookies();
}
