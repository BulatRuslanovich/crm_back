namespace CrmBack.Application.Auth.Services;

/// <summary>
/// Cookie service interface
/// Handles HTTP-only cookie operations for JWT tokens
/// Provides secure cookie management for authentication
/// </summary>
public interface ICookieService
{
	/// <summary>Set access token in HTTP-only cookie</summary>
	public void SetAccessTkn(string token, DateTime expiresAt);

	/// <summary>Set refresh token in HTTP-only cookie</summary>
	public void SetRefreshTkn(string token, DateTime expiresAt);

	/// <summary>Get access token from cookie</summary>
	public string? GetAccessTkn();

	/// <summary>Get refresh token from cookie</summary>
	public string? GetRefreshTkn();

	/// <summary>Clear all authentication cookies (logout)</summary>
	public void Clear();
}
