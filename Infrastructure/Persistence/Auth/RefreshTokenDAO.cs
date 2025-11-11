using CrmBack.Domain.Auth;
using CrmBack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CrmBack.Infrastructure.Persistence.Auth;

/// <summary>
/// Refresh token Data Access Object implementation
/// Manages refresh token storage and retrieval
/// Security: Tokens are stored as hashes (BCrypt), not plaintext
/// </summary>
public class RefreshTokenDAO(AppDBContext context) : IRefreshTokenDAO
{

	/// <summary>
	/// Get active refresh token for user
	/// Returns token that is not deleted and not expired
	/// Includes user navigation property for eager loading
	/// </summary>
	public async Task<RefreshTokenEntity?> GetUserToken(int userId, CancellationToken ct = default)
	{
		return await context.RefreshTokens
			.Include(rt => rt.User)  // Eager load user for token refresh operations
			.Where(rt => rt.UsrId == userId && !rt.IsDeleted && rt.ExpiresAt > DateTime.UtcNow)
			.FirstOrDefaultAsync(ct);
	}

	/// <summary>
	/// Create new refresh token and invalidate old ones
	/// Security: Only one active refresh token per user (old tokens are soft-deleted)
	/// This prevents token reuse and improves security
	/// </summary>
	public async Task<RefreshTokenEntity> CreateAsync(int userId, string tokenHash, DateTime expiresAt, CancellationToken ct = default)
	{
		// Soft delete all existing refresh tokens for user (single active token policy)
		var oldTokens = await context.RefreshTokens
			.Where(rt => rt.UsrId == userId && !rt.IsDeleted)
			.ToListAsync(ct);

		foreach (var token in oldTokens) token.IsDeleted = true;

		// Create new refresh token
		var refreshToken = new RefreshTokenEntity
		{
			UsrId = userId,
			TokenHash = tokenHash,  // Stored as BCrypt hash (not plaintext)
			ExpiresAt = expiresAt,
		};

		context.RefreshTokens.Add(refreshToken);
		await context.SaveChangesAsync(ct);
		return refreshToken;
	}


	/// <summary>
	/// Soft delete all refresh tokens for user
	/// Used for logout operation to invalidate all user sessions
	/// </summary>
	public async Task<bool> DeleteAll(int userId, CancellationToken ct = default)
	{
		var tokens = await context.RefreshTokens
			.Where(rt => rt.UsrId == userId && !rt.IsDeleted)
			.ToListAsync(ct);

		// Soft delete all tokens (set IsDeleted flag)
		foreach (var token in tokens) token.IsDeleted = true;

		await context.SaveChangesAsync(ct);
		return true;
	}
}
