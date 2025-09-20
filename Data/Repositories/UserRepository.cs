namespace CrmBack.Data.Repositories;

using System.Data;
using Dapper;
using CrmBack.Core.Repositories;
using System.Threading.Tasks;
using CrmBack.Core.Models.Payload;
using CrmBack.Core.Models.Entities;
using System.Collections.Generic;

public class UserRepository(IDbConnection dbConnection) : IUserRepository
{
    public async Task<int> CreateAsync(UserEntity user)
    {
        const string sql = @"
            INSERT INTO usr (name, login) 
            VALUES (@Name, @Login) 
            RETURNING usr_id";

        return await dbConnection.ExecuteScalarAsync<int>(sql, user).ConfigureAwait(false);
    }

    public async Task<IEnumerable<UserEntity>> GetAllAsync(bool includeDeleted = false)
    {
        var sql = @"SELECT usr_id, name, login, created_at, updated_at, is_deleted
                    FROM usr";

        if (!includeDeleted)
        {
            sql += " WHERE NOT is_deleted";
        }

        return await dbConnection.QueryAsync<UserEntity>(sql).ConfigureAwait(false);
    }

    public async Task<UserEntity?> GetByIdAsync(int id)
    {

        var sql = @"SELECT usr_id, name, login, created_at, updated_at, is_deleted
                    FROM usr
                    WHERE usr_id = @usrId AND NOT is_deleted
                    LIMIT 1";
        return await dbConnection.QuerySingleOrDefaultAsync<UserEntity>(sql, new { usrId = id }).ConfigureAwait(false);
    }

    public async Task<UserEntity?> GetByLoginAsync(string login)
    {
        var sql = @"SELECT usr_id, name, login, created_at, updated_at, is_deleted
                    FROM usr
                    WHERE login = @Login AND NOT is_deleted
                    LIMIT 1";
        return await dbConnection.QuerySingleOrDefaultAsync<UserEntity>(sql, new { Login = login }).ConfigureAwait(false);
    }

    public async Task<bool> HardDeleteAsync(int id)
    {
         const string sql = "DELETE FROM usr WHERE usr_id = @Id";

        var affectedRows = await dbConnection.ExecuteAsync(sql, new { Id = id }).ConfigureAwait(false);
        return affectedRows > 0;
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var sql = @"
            UPDATE usr 
            SET is_deleted = true
            WHERE usr_id = @Id";

        var affectedRows = await dbConnection.ExecuteAsync(sql, new { Id = id }).ConfigureAwait(false);
        return affectedRows > 0;
    }

    public async Task<bool> UpdateAsync(UserEntity user)
    {

        var sql = @"
            UPDATE usr 
            SET name = @name, login = @login
            WHERE usr_id = @usr_id";

        var affectedRows = await dbConnection.ExecuteAsync(sql, user).ConfigureAwait(false);
        return affectedRows > 0;
    }
}