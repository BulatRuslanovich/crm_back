using CrmBack.Core.Models.Entities;
using CrmBack.Data;
using Microsoft.EntityFrameworkCore;

namespace CrmBack.DAO.Impl;

public class RefreshTokenDAO(AppDBContext context) : IRefreshTokenDAO
{
    public async Task<RefreshTokenEntity?> GetUserToken(int userId, CancellationToken ct = default)
    {
        return await context.RefreshTokens
            .Where(rt => rt.UsrId == userId && !rt.IsDeleted && rt.ExpiresAt > DateTime.UtcNow)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<RefreshTokenEntity> CreateAsync(int userId, string tokenHash, DateTime expiresAt, CancellationToken ct = default)
    {
        await context.RefreshTokens
            .Where(rt => rt.UsrId == userId && !rt.IsDeleted)
            .ExecuteUpdateAsync(setters => setters.SetProperty(rt => rt.IsDeleted, true), ct);

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
        int affectedRows = await context.RefreshTokens
            .Where(rt => rt.UsrId == userId && !rt.IsDeleted)
            .ExecuteUpdateAsync(setters => setters.SetProperty(rt => rt.IsDeleted, true), ct);

        return affectedRows > 0;
    }

}
