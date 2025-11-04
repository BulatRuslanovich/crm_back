namespace CrmBack.Application.Auth.Services;

/// <summary>
/// JWT token service interface
/// Handles generation and validation of JWT tokens
/// </summary>
public interface IJwtService
{
    /// <summary>Generate access token with user ID, login, and roles</summary>
    public string GenerateAccessTkn(int userId, string login, List<string> roles);
    
    /// <summary>Generate refresh token with user ID</summary>
    public string GenerateRefreshTkn(int userId);
    
    /// <summary>Extract user ID from refresh token</summary>
    public int? GetUsrId(string refreshToken);
}
