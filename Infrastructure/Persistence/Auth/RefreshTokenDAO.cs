using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmBack.Domain.Auth;
using CrmBack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CrmBack.Infrastructure.Persistence.Auth;


public class RefreshTokenDAO(AppDBContext context) : IRefreshTokenDAO
{
	public async Task<RefreshTokenEntity?> GetUserToken(int userId, CancellationToken ct = default)
	{
		return await context.RefreshTokens
			.Include(rt => rt.User)  // Eager load user for token refresh operations
			.Where(rt => rt.UsrId == userId && !rt.IsDeleted && rt.ExpiresAt > DateTime.UtcNow)
			.FirstOrDefaultAsync(ct);
	}

	public async Task<RefreshTokenEntity> CreateAsync(int userId, string tokenHash, DateTime expiresAt, CancellationToken ct = default)
	{
		var oldTokens = await context.RefreshTokens
			.Where(rt => rt.UsrId == userId && !rt.IsDeleted)
			.ToListAsync(ct);

		foreach (var token in oldTokens) token.IsDeleted = true;

		var refreshToken = new RefreshTokenEntity
		{
			UsrId = userId,
			TokenHash = tokenHash,
			ExpiresAt = expiresAt,
		};

		context.RefreshTokens.Add(refreshToken);
		await context.SaveChangesAsync(ct);
		return refreshToken;
	}

	public async Task<bool> DeleteAll(int userId, CancellationToken ct = default)
	{
		var tokens = await context.RefreshTokens
			.Where(rt => rt.UsrId == userId && !rt.IsDeleted)
			.ToListAsync(ct);

		foreach (var token in tokens) token.IsDeleted = true;

		await context.SaveChangesAsync(ct);
		return true;
	}
}
