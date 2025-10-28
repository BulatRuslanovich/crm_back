using CrmBack.Core.Models.Entities;

namespace CrmBack.DAO;

public interface IRefreshTokenDAO
{
    public Task<List<RefreshTokenEntity>> GetUserTokens(int userId, CancellationToken ct = default);
    public Task<RefreshTokenEntity> CreateAsync(int userId, string tokenHash, DateTime expiresAt, CancellationToken ct = default);
    public Task<bool> RevokeAll(int userId, CancellationToken ct = default);
    public Task<bool> RevokeById(int tokenId, int userId, CancellationToken ct = default);
}
