using CrmBack.Domain.Auth;

namespace CrmBack.Infrastructure.Persistence.Auth;

/// <summary>
/// Refresh token Data Access Object interface
/// Handles refresh token persistence operations
/// </summary>
public interface IRefreshTokenDAO
{
    /// <summary>Get active refresh token for user (not expired, not deleted)</summary>
    public Task<RefreshTokenEntity?> GetUserToken(int userId, CancellationToken ct = default);
    
    /// <summary>Create new refresh token and invalidate old ones</summary>
    public Task<RefreshTokenEntity> CreateAsync(int userId, string tokenHash, DateTime expiresAt, CancellationToken ct = default);
    
    /// <summary>Soft delete all refresh tokens for user (logout)</summary>
    public Task<bool> DeleteAll(int userId, CancellationToken ct = default);
}
