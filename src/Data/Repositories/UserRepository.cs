namespace CrmBack.Data.Repositories;

using CrmBack.Core.Models.Entities;
using CrmBack.Core.Repositories;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

public class UserRepository(
    IDbConnection dbConnection,
    ILogger<UserRepository> logger) : BaseRepository<UserEntity>(dbConnection, logger), IUserRepository
{
    private const string SelectQuery = @"
        SELECT usr_id, first_name, middle_name, last_name, login, password_hash,
               created_at, updated_at, created_by, updated_by, is_deleted
        FROM usr
        WHERE {0} AND NOT is_deleted
        LIMIT 1";

    public Task<UserEntity?> GetByIdAsync(int id) =>
        QuerySingleAsync(string.Format(SelectQuery, "usr_id = @id"), id);

    public Task<UserEntity?> GetByLoginAsync(string login) =>
        QuerySingleAsync(string.Format(SelectQuery, "login = @id"), login);

    public Task<IEnumerable<UserEntity>> GetAllAsync(bool includeDeleted = false, int page = 1, int pageSize = 10)
    {
        var sql = $@"
            SELECT usr_id, first_name, middle_name, last_name, login, password_hash,
                   created_at, updated_at, created_by, updated_by, is_deleted
            FROM usr
            {(includeDeleted ? "" : "WHERE NOT is_deleted")}
            LIMIT @PageSize OFFSET @Offset";

        return QueryAsync(sql, new { PageSize = pageSize, Offset = (page - 1) * pageSize });
    }

    public Task<int> CreateAsync(UserEntity user)
    {
        const string sql = @"
            INSERT INTO usr (first_name, middle_name, last_name, login, password_hash, created_by, updated_by)
            VALUES (@first_name, @middle_name, @last_name, @login, @password_hash, 'system', 'system')
            RETURNING usr_id";

        return ExecuteScalarAsync(sql, user);
    }

    public Task<bool> UpdateAsync(UserEntity user) =>
        WithTransactionAsync(async transaction =>
        {
            var existing = await QuerySingleAsync(
                string.Format(SelectQuery, "usr_id = @id"), user.usr_id, transaction);

            if (existing == null) return false;

            var updated = new UserEntity(
                usr_id: existing.usr_id,
                first_name: user.first_name ?? existing.first_name,
                last_name: user.last_name ?? existing.last_name,
                middle_name: user.middle_name ?? existing.middle_name,
                login: user.login ?? existing.login,
                password_hash: user.password_hash ?? existing.password_hash
            );

            const string sql = @"
                UPDATE usr
                SET first_name = @first_name, middle_name = @middle_name,
                    last_name = @last_name, login = @login, password_hash = @password_hash
                WHERE usr_id = @usr_id";

            return await ExecuteAsync(sql, updated, transaction);
        });

    public Task<bool> HardDeleteAsync(int id) =>
        ExecuteAsync("DELETE FROM usr WHERE usr_id = @Id", new { Id = id });

    public Task<bool> SoftDeleteAsync(int id) =>
        ExecuteAsync("UPDATE usr SET is_deleted = true WHERE usr_id = @Id", new { Id = id });
}