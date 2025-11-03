namespace CrmBack.Application.Auth.Services;

public interface ICookieService
{
    public void SetAccessTkn(string token, DateTime expiresAt);
    public void SetRefreshTkn(string token, DateTime expiresAt);
    public string? GetAccessTkn();
    public string? GetRefreshTkn();
    public void Clear();
}
