using CrmBack.Core.Models.Entities;

namespace CrmBack.DAO;

public interface IRefreshTokenDAO
{
    public Task<List<RefreshTokenEntity>> GetUserTokensForValidationAsync(int userId, CancellationToken ct = default);
    public Task<RefreshTokenEntity> CreateAsync(int userId, string tokenHash, DateTime expiresAt, CancellationToken ct = default);
    public Task<bool> RevokeAllUserTokensAsync(int userId, CancellationToken ct = default);
    public Task<bool> RevokeTokenByIdAsync(int tokenId, int userId, CancellationToken ct = default);
}
