namespace CrmBack.Services;

public interface IJwtService {
    public string GenerateAccessToken(int userId, string login, List<string> roles);
    public string GenerateRefreshToken();
    public bool ValidateToken(string token);
    public int? GetUserIdFromToken(string token);
    public List<string>? GetRolesFromToken(string token);
}