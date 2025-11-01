using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace CrmBack.Services.Impl;

public class JwtService(IConfiguration conf) : IJwtService
{
    private readonly string _key = conf["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured");
    private readonly string _issuer = conf["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured");
    private readonly string _audience = conf["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience is not configured");

    // Генерация access-токена с claims (ID пользователя, логин, роли) сроком действия 1 час
    public string GenerateAccessTkn(int userId, string login, List<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, login),
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        return Generate(claims, TimeSpan.FromHours(1));
    }

    // Генерация refresh-токена с минимальным набором claims, срок действия 7 дней
    public string GenerateRefreshTkn(int userId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        return Generate(claims, TimeSpan.FromDays(7));
    }

    // Создание JWT-токена с указанными claims, подписью HmacSha256 и сроком действия
    private string Generate(List<Claim> claims, TimeSpan expiration)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.Add(expiration),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Извлечение ID пользователя из refresh-токена (без проверки подписи, только чтение claims)
    public int? GetUsrId(string refTkn)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var userIdClaim = tokenHandler.ReadJwtToken(refTkn).Claims
                .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            return userIdClaim is not null ? int.Parse(userIdClaim.Value) : null;
        }
        catch
        {
            return null;
        }
    }
}
