using CrmBack.Core.Models.Entities;
using CrmBack.Data;
using Microsoft.EntityFrameworkCore;

namespace CrmBack.DAO.Impl;

public class RefreshTokenDAO(AppDBContext context) : IRefreshTokenDAO
{
    // Получение всех валидных (не удаленных и не истекших) refresh-токенов пользователя
    public async Task<List<RefreshTokenEntity>> GetUserTokens(int userId, CancellationToken ct = default)
    {
        return await context.RefreshTokens
            .Include(rt => rt.User)
            .Where(rt => rt.UsrId == userId && !rt.IsDeleted && rt.ExpiresAt > DateTime.UtcNow)
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


    public async Task<bool> DeleteAll(int userId, CancellationToken ct = default)
    {
        var tokens = await context.RefreshTokens
            .Where(rt => rt.UsrId == userId && !rt.IsDeleted)
            .ToListAsync(ct);

        foreach (var token in tokens) token.IsDeleted = true;

        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteById(int tokenId, int userId, CancellationToken ct = default)
    {
        var token = await context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.RefreshTokenId == tokenId && rt.UsrId == userId && !rt.IsDeleted, ct);

        if (token is null) return false;

        token.IsDeleted = true;
        await context.SaveChangesAsync(ct);
        return true;
    }
}
