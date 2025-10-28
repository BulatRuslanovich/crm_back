using CrmBack.Core.Models.Entities;
using CrmBack.Data;
using Microsoft.EntityFrameworkCore;

namespace CrmBack.DAO.Impl;

public class RefreshTokenDAO(AppDBContext context) : IRefreshTokenDAO
{
    public async Task<List<RefreshTokenEntity>> GetUserTokens(int userId, CancellationToken ct = default)
    {
        return await context.RefreshTokens
            .Include(rt => rt.User)
            .Where(rt => rt.UsrId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(ct);
    }

    public async Task<RefreshTokenEntity> CreateAsync(int userId, string tokenHash, DateTime expiresAt, CancellationToken ct = default)
    {
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


    public async Task<bool> RevokeAll(int userId, CancellationToken ct = default)
    {
        var tokens = await context.RefreshTokens
            .Where(rt => rt.UsrId == userId && !rt.IsRevoked)
            .ToListAsync(ct);

        foreach (var token in tokens) token.IsRevoked = true;

        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> RevokeById(int tokenId, int userId, CancellationToken ct = default)
    {
        var token = await context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.RefreshTokenId == tokenId && rt.UsrId == userId && !rt.IsRevoked, ct);

        if (token is null) return false;

        token.IsRevoked = true;
        await context.SaveChangesAsync(ct);
        return true;
    }
}
