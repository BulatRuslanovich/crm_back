using CrmBack.Core.Models.Entities;

namespace CrmBack.DAO;

public interface IRefreshTokenDAO
{
    public Task<RefreshTokenEntity?> GetUserToken(int userId, CancellationToken ct = default);
    public Task<RefreshTokenEntity> CreateAsync(int userId, string tokenHash, DateTime expiresAt, CancellationToken ct = default);
    public Task<bool> DeleteAll(int userId, CancellationToken ct = default);
}
