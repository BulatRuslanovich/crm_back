namespace CrmBack.Data.Repositories;

using CrmBack.Core.Models.Entities;
using CrmBack.Core.Repositories;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

public class UserRepository(IDbConnection dbConnection) : BaseRepository<UserEntity>(dbConnection), IUserRepository
{
    private const string UserColumns = "usr_id, first_name, middle_name, last_name, login, password_hash, is_deleted";

    private const string SelectByIdSql = $@"
        SELECT  {UserColumns}
        FROM usr
        WHERE usr_id = @id AND NOT is_deleted
        LIMIT 1";

    private const string SelectByLoginSql = $@"
        SELECT  {UserColumns}
        FROM usr
        WHERE login = @id AND NOT is_deleted
        LIMIT 1";

    public Task<UserEntity?> GetByIdAsync(int id, CancellationToken ct = default) =>
        QuerySingleAsync(SelectByIdSql, id, ct);

    public Task<UserEntity?> GetByLoginAsync(string login, CancellationToken ct = default) =>
        QuerySingleAsync(SelectByLoginSql, login, ct);

    public Task<IEnumerable<UserEntity>> GetAllAsync(bool isDeleted, int page, int pageSize, CancellationToken ct = default)
    {
        var where = isDeleted ? "" : "WHERE NOT is_deleted";
        var sql = $@"SELECT {UserColumns}
                    FROM usr
                    {where}
                    LIMIT @pageSize OFFSET @offset";

        return QueryAsync(sql, new { pageSize, offset = (page - 1) * pageSize }, ct);
    }

    public Task<int> CreateAsync(UserEntity user, CancellationToken ct = default)
    {
        const string sql = @"
            INSERT INTO usr (first_name, middle_name, last_name, login, password_hash)
            VALUES (@first_name, @middle_name, @last_name, @login, @password_hash)
            RETURNING usr_id";

        return ExecuteScalarAsync(sql, user, ct);
    }

    public async Task<bool> UpdateAsync(UserEntity user, CancellationToken ct = default)
    {
        var existing = await GetByIdAsync(user.usr_id, ct);

        if (existing == null) return false;

        var updated = new UserEntity(
            usr_id: existing.usr_id,
            first_name: user.first_name ?? existing.first_name,
            last_name: user.last_name ?? existing.last_name,
            middle_name: user.middle_name ?? existing.middle_name,
            login: string.IsNullOrEmpty(user.login) ? existing.login : user.login,
            password_hash: user.password_hash ?? existing.password_hash
        );

        const string sql = @"
                UPDATE usr
                SET first_name = @first_name, middle_name = @middle_name,
                    last_name = @last_name, login = @login, password_hash = @password_hash
                WHERE usr_id = @usr_id";

        return await ExecuteAsync(sql, updated, ct);
    }

    public Task<bool> HardDeleteAsync(int id, CancellationToken ct = default) =>
        ExecuteAsync("DELETE FROM usr WHERE usr_id = @id", new { id }, ct);

    public Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default) =>
        ExecuteAsync("UPDATE usr SET is_deleted = true WHERE usr_id = @id", new { id }, ct);
}