using CrmBack.Core.Models.Entities;
using CrmBack.Data;
using Microsoft.EntityFrameworkCore;

namespace CrmBack.DAO.Impl;

public class RefreshTokenDAO(AppDBContext context) : IRefreshTokenDAO
{
    public async Task<List<RefreshTokenEntity>> GetUserTokensForValidationAsync(int userId, CancellationToken ct = default)
    {
        return await context.RefreshTokens
            .Include(rt => rt.User)
            .Where(rt => rt.UsrId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(ct);
    }

    public async Task<RefreshTokenEntity> CreateAsync(int userId, string tokenHash, DateTime expiresAt, string? deviceInfo = null, string? ipAddress = null, CancellationToken ct = default)
    {
        var refreshToken = new RefreshTokenEntity
        {
            UsrId = userId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            DeviceInfo = deviceInfo,
            IpAddress = ipAddress
        };

        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync(ct);
        return refreshToken;
    }

    public async Task<bool> RevokeTokenAsync(string tokenHash, CancellationToken ct = default)
    {
        var token = await context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, ct);

        if (token == null) return false;

        token.IsRevoked = true;
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> RevokeAllUserTokensAsync(int userId, CancellationToken ct = default)
    {
        var tokens = await context.RefreshTokens
            .Where(rt => rt.UsrId == userId && !rt.IsRevoked)
            .ToListAsync(ct);

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
        }

        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<List<RefreshTokenEntity>> GetUserTokensAsync(int userId, CancellationToken ct = default)
    {
        return await context.RefreshTokens
            .Where(rt => rt.UsrId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<bool> RevokeTokenByIdAsync(int tokenId, int userId, CancellationToken ct = default)
    {
        var token = await context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.RefreshTokenId == tokenId && rt.UsrId == userId && !rt.IsRevoked, ct);

        if (token == null) return false;

        token.IsRevoked = true;
        await context.SaveChangesAsync(ct);
        return true;
    }
}
