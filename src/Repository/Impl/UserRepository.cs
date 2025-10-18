namespace CrmBack.Repository.Impl;

using System.Data;
using CrmBack.Core.Models.Entities;
using Microsoft.Extensions.Caching.Distributed;

public class UserRepository(IDbConnection dbConnection, IDistributedCache cache) : BaseRepository<UserEntity, int>(dbConnection, cache), IUserRepository
{

    public Task<UserEntity?> GetByLoginAsync(string login, CancellationToken ct = default)
    {
        var sql = @"
            SELECT usr_id, first_name, middle_name, last_name, login, password_hash, is_deleted
            FROM usr
            WHERE login = @login AND NOT is_deleted
            LIMIT 1";

        return QuerySingleAsync(sql, new { login }, ct);
    }
}
