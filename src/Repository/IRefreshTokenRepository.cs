namespace CrmBack.Repository;

using CrmBack.Core.Models.Entities;

public interface IRefreshTokenRepository
{
    Task<int> CreateAsync(RefreshTokenEntity entity, CancellationToken ct = default);
    Task<RefreshTokenEntity?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default);
    Task<RefreshTokenEntity?> FindValidTokenAsync(string refreshToken, CancellationToken ct = default);
    Task<bool> RevokeTokenAsync(string tokenHash, CancellationToken ct = default);
    Task<bool> RevokeAllUserTokensAsync(int userId, CancellationToken ct = default);
    Task CleanupExpiredTokensAsync(CancellationToken ct = default);
}
