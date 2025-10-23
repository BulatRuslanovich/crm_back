using CrmBack.Core.Models.Entities;
using CrmBack.Data;
using Microsoft.EntityFrameworkCore;

namespace CrmBack.DAO.Impl;

public class RefreshTokenDAO(AppDBContext context) : IRefreshTokenDAO
{
    public async Task<RefreshTokenEntity?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default)
    {
        return await context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow, ct);
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

    public async Task<bool> RevokeExpiredTokensAsync(CancellationToken ct = default)
    {
        var expiredTokens = await context.RefreshTokens
            .Where(rt => rt.ExpiresAt <= DateTime.UtcNow && !rt.IsRevoked)
            .ToListAsync(ct);

        foreach (var token in expiredTokens)
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

    public async Task<RefreshTokenEntity?> GetUserTokenByHashAsync(int userId, string tokenHash, CancellationToken ct = default)
    {
        return await context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.UsrId == userId && rt.TokenHash == tokenHash && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow, ct);
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
