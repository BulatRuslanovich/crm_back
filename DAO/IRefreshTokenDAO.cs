using CrmBack.Core.Models.Entities;

namespace CrmBack.DAO;

public interface IRefreshTokenDAO
{
    public Task<List<RefreshTokenEntity>> GetUserTokens(int userId, CancellationToken ct = default);
    public Task<RefreshTokenEntity> CreateAsync(int userId, string tokenHash, DateTime expiresAt, CancellationToken ct = default);
    public Task<bool> DeleteAll(int userId, CancellationToken ct = default);
    public Task<bool> DeleteById(int tokenId, int userId, CancellationToken ct = default);
}
