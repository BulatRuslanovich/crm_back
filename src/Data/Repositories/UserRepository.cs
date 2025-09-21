namespace CrmBack.Data.Repositories;

using System.Data;
using Dapper;
using CrmBack.Core.Repositories;
using System.Threading.Tasks;
using CrmBack.Core.Models.Entities;
using System.Collections.Generic;

public class UserRepository(IDbConnection dbConnection) : IUserRepository
{
    public async Task<UserEntity?> GetByIdAsync(int id)
    {

        var sql = @"SELECT usr_id, first_name, middle_name, last_name, login, password_hash, created_at, updated_at, created_by, updated_by, is_deleted
                    FROM usr
                    WHERE usr_id = @usr_id AND NOT is_deleted
                    LIMIT 1";

        return await dbConnection.QuerySingleOrDefaultAsync<UserEntity>(sql, new { usr_id = id }).ConfigureAwait(false);
    }

    public async Task<IEnumerable<UserEntity>> GetAllAsync(bool includeDeleted = false)
    {
        var sql = @"SELECT usr_id, first_name, middle_name, last_name, login, password_hash, created_at, updated_at, created_by, updated_by, is_deleted
                    FROM usr";

        if (!includeDeleted)
        {
            sql += " WHERE NOT is_deleted";
        }

        return await dbConnection.QueryAsync<UserEntity>(sql).ConfigureAwait(false);
    }

    public async Task<UserEntity?> GetByLoginAsync(string _login)
    {
        var sql = @"SELECT usr_id, first_name, middle_name, last_name, login, password_hash, created_at, updated_at, created_by, updated_by, is_deleted
                    FROM usr
                    WHERE login = @login AND NOT is_deleted
                    LIMIT 1";

        return await dbConnection.QuerySingleOrDefaultAsync<UserEntity>(sql, new { login = _login }).ConfigureAwait(false);
    }

    public async Task<int> CreateAsync(UserEntity user)
    {
        const string sql = @"INSERT INTO usr (first_name, middle_name, last_name, login, password_hash, created_by, updated_by)
                            VALUES (@first_name, @middle_name, @last_name, @login, @password_hash, 'system', 'system')
                            RETURNING usr_id";

        return await dbConnection.ExecuteScalarAsync<int>(sql, user).ConfigureAwait(false);
    }

    public async Task<bool> UpdateAsync(UserEntity user)
    {
        var sql = @"UPDATE usr
                    SET first_name = @first_name, middle_name = @middle_name, last_name = @last_name, login = @login, password_hash = @password_hash
                    WHERE usr_id = @usr_id";

        var affectedRows = await dbConnection.ExecuteAsync(sql, user).ConfigureAwait(false);
        return affectedRows > 0;
    }

    public async Task<bool> HardDeleteAsync(int id)
    {
        const string sql = "DELETE FROM usr WHERE usr_id = @Id";

        var affectedRows = await dbConnection.ExecuteAsync(sql, new { Id = id }).ConfigureAwait(false);
        return affectedRows > 0;
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var sql = @"UPDATE usr 
                    SET is_deleted = true
                    WHERE usr_id = @Id";

        var affectedRows = await dbConnection.ExecuteAsync(sql, new { Id = id }).ConfigureAwait(false);
        return affectedRows > 0;
    }
}