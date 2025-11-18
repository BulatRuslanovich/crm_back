namespace CrmBack.Application.Auth.Services;

public interface IJwtService
{
	public string GenerateAccessTkn(int userId, string login, List<string> roles);
	public string GenerateRefreshTkn(int userId);

	public int? GetUsrId(string refreshToken);
}
