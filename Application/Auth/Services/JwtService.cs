using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace CrmBack.Application.Auth.Services;

/// <summary>
/// JWT token service implementation
/// Generates and validates JWT tokens using HMAC-SHA256 signing
/// Security: Uses symmetric key encryption for token signing
/// </summary>
public class JwtService(IConfiguration conf) : IJwtService
{
	private readonly string _key = conf["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured");
	private readonly string _issuer = conf["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured");
	private readonly string _audience = conf["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience is not configured");

	/// <summary>
	/// Generate access token for authenticated user
	/// Contains: User ID, Login, Roles, JTI (unique token ID), IAT (issued at)
	/// Expiration: 1 hour
	/// </summary>
	public string GenerateAccessTkn(int userId, string login, List<string> roles)
	{
		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, userId.ToString()),  // User ID claim
            new(ClaimTypes.Name, login),                         // Login claim
            new(JwtRegisteredClaimNames.Sub, userId.ToString()), // Subject (standard JWT claim)
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique token ID
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64) // Issued at timestamp
        };

		// Add role claims for authorization
		claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

		return Generate(claims, TimeSpan.FromHours(1));
	}

	/// <summary>
	/// Generate refresh token for user
	/// Contains: User ID, JTI (unique token ID), IAT (issued at)
	/// Expiration: 7 days
	/// </summary>
	public string GenerateRefreshTkn(int userId)
	{
		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, userId.ToString()),  // User ID claim
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique token ID
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64) // Issued at timestamp
        };

		return Generate(claims, TimeSpan.FromDays(7));
	}

	/// <summary>
	/// Internal method to generate JWT token
	/// Uses HMAC-SHA256 algorithm for signing
	/// </summary>
	private string Generate(List<Claim> claims, TimeSpan expiration)
	{
		// Create symmetric security key from configuration
		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
		// Use HMAC-SHA256 for signing (fast and secure)
		var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var token = new JwtSecurityToken(
			issuer: _issuer,           // Token issuer
			audience: _audience,       // Token audience
			claims: claims,            // Token claims (user info, roles)
			expires: DateTime.UtcNow.Add(expiration), // Token expiration
			signingCredentials: credentials // Signing credentials
		);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}

	/// <summary>
	/// Extract user ID from refresh token
	/// Reads token without validation (for token extraction only)
	/// Returns null if token is invalid or user ID cannot be extracted
	/// </summary>
	public int? GetUsrId(string refTkn)
	{
		try
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			// Read token without validation (we only need to extract user ID)
			var userIdClaim = tokenHandler.ReadJwtToken(refTkn).Claims
				.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
			return userIdClaim is not null ? int.Parse(userIdClaim.Value) : null;
		}
		catch
		{
			// Return null if token is malformed or invalid
			return null;
		}
	}
}
