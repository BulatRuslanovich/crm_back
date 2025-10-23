namespace CrmBack.Services;

public interface IJwtService
{
    public string GenerateAccessToken(int userId, string login, List<string> roles);
    public string GenerateRefreshToken(int userId);
    public int? GetUserIdFromRefreshToken(string refreshToken);
}
