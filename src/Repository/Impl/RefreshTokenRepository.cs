namespace CrmBack.Repository.Impl;

using System.Data;
using CrmBack.Core.Models.Entities;
using Microsoft.Extensions.Caching.Distributed;
using Dapper;

public class RefreshTokenRepository(IDbConnection dbConnection, IDistributedCache cache) : BaseRepository<RefreshTokenEntity, int>(dbConnection, cache), IRefreshTokenRepository
{

    public async Task<RefreshTokenEntity?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default)
    {
        const string sql = @"
            SELECT token_id, usr_id, token_hash, expires_at, created_at, is_revoked
            FROM refresh_tokens
            WHERE token_hash = @tokenHash AND is_revoked = false AND expires_at > NOW()";

        var token = await dbConnection.QueryFirstOrDefaultAsync<RefreshTokenEntity>(sql, new { tokenHash });
        return token;
    }

    public async Task<RefreshTokenEntity?> FindValidTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        const string sql = @"
            SELECT token_id, usr_id, token_hash, expires_at, created_at, is_revoked
            FROM refresh_tokens
            WHERE is_revoked = false AND expires_at > NOW()";

        var tokens = await dbConnection.QueryAsync<RefreshTokenEntity>(sql);
        
        foreach (var token in tokens)
        {
            if (BCrypt.Net.BCrypt.Verify(refreshToken, token.token_hash))
            {
                return token;
            }
        }
        
        return null;
    }

    public async Task<bool> RevokeTokenAsync(string tokenHash, CancellationToken ct = default)
    {
        const string sql = @"
            UPDATE refresh_tokens 
            SET is_revoked = true 
            WHERE token_hash = @tokenHash";

        var rowsAffected = await dbConnection.ExecuteAsync(sql, new { tokenHash });
        return rowsAffected > 0;
    }

    public async Task<bool> RevokeAllUserTokensAsync(int userId, CancellationToken ct = default)
    {
        const string sql = @"
            UPDATE refresh_tokens 
            SET is_revoked = true 
            WHERE usr_id = @userId";

        var rowsAffected = await dbConnection.ExecuteAsync(sql, new { userId });
        return rowsAffected > 0;
    }

    public async Task CleanupExpiredTokensAsync(CancellationToken ct = default)
    {
        const string sql = @"
            DELETE FROM refresh_tokens 
            WHERE expires_at < NOW() OR is_revoked = true";

        await dbConnection.ExecuteAsync(sql);
    }
}
