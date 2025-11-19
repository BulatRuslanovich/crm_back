using System;
using System.Threading;
using System.Threading.Tasks;
using CrmBack.Domain.Auth;

namespace CrmBack.Infrastructure.Persistence.Auth;


public interface IRefreshTokenDAO
{
	public Task<RefreshTokenEntity?> GetUserToken(int userId, CancellationToken ct = default);

	public Task<RefreshTokenEntity> CreateAsync(int userId, string tokenHash, DateTime expiresAt, CancellationToken ct = default);

	public Task<bool> DeleteAll(int userId, CancellationToken ct = default);
}
