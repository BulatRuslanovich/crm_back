using CrmBack.Core.Models.Entities;

namespace CrmBack.DAO;

public interface IRefreshTokenDAO
{
    public Task<RefreshTokenEntity?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default);
    public Task<List<RefreshTokenEntity>> GetAllTokensAsync(CancellationToken ct = default);
    public Task<RefreshTokenEntity> CreateAsync(int userId, string tokenHash, DateTime expiresAt, string? deviceInfo = null, string? ipAddress = null, CancellationToken ct = default);
    public Task<bool> RevokeTokenAsync(string tokenHash, CancellationToken ct = default);
    public Task<bool> RevokeAllUserTokensAsync(int userId, CancellationToken ct = default);
    public Task<bool> RevokeExpiredTokensAsync(CancellationToken ct = default);
    public Task<List<RefreshTokenEntity>> GetUserTokensAsync(int userId, CancellationToken ct = default);
    public Task<RefreshTokenEntity?> GetUserTokenByHashAsync(int userId, string tokenHash, CancellationToken ct = default);
    public Task<bool> RevokeTokenByIdAsync(int tokenId, int userId, CancellationToken ct = default);
}
