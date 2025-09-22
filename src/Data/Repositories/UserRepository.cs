namespace CrmBack.Data.Repositories;

using System.Data;
using Dapper;
using CrmBack.Core.Repositories;
using System.Threading.Tasks;
using CrmBack.Core.Models.Entities;
using System.Collections.Generic;
using System.Transactions;

public class UserRepository(IDbConnection dbConnection) : IUserRepository
{
    private const string BaseSelectQuery = @"SELECT usr_id, first_name, middle_name, last_name, login, password_hash, created_at, updated_at, created_by, updated_by, is_deleted
                                        FROM usr
                                        WHERE {0} AND NOT is_deleted
                                        LIMIT 1";

    public async Task<UserEntity?> GetByIdAsync(int id)
    {
        return await GetByIdAsync(id, null);
    }

    private async Task<UserEntity?> GetByIdAsync(int id, IDbTransaction? transaction = null)
    {

        var sql = string.Format(BaseSelectQuery, "usr_id = @usr_id");
        return await dbConnection.QuerySingleOrDefaultAsync<UserEntity>(sql, new { usr_id = id }, transaction).ConfigureAwait(false);
    }

    public async Task<UserEntity?> GetByLoginAsync(string _login)
    {
        var sql = string.Format(BaseSelectQuery, "login = @login");
        return await dbConnection.QuerySingleOrDefaultAsync<UserEntity>(sql, new { login = _login }).ConfigureAwait(false);
    }

    public async Task<IEnumerable<UserEntity>> GetAllAsync(bool includeDeleted = false, int page = 1, int pageSize = 10)
    {
        var sql = @"SELECT usr_id, first_name, middle_name, last_name, login, password_hash, created_at, updated_at, created_by, updated_by, is_deleted
                    FROM usr";

        if (!includeDeleted)
        {
            sql += " WHERE NOT is_deleted";
        }

        sql += " LIMIT @PageSize OFFSET @Offset";

        return await dbConnection.QueryAsync<UserEntity>(sql, new { PageSize = pageSize, Offset = (page - 1) * pageSize }).ConfigureAwait(false);
    }

    public async Task<int> CreateAsync(UserEntity user)
    {
        const string sql = @"INSERT INTO usr (first_name, middle_name, last_name, login, password_hash, created_by, updated_by)
                            VALUES (@first_name,
                            @middle_name,
                            @last_name,
                            @login,
                            @password_hash,
                            'system',
                            'system')
                            RETURNING usr_id";

        return await dbConnection.ExecuteScalarAsync<int>(sql, user).ConfigureAwait(false);
    }

    public async Task<bool> UpdateAsync(UserEntity user)
    {
        using var tran = dbConnection.BeginTransaction();

        try
        {
            var usrFromDb = await GetByIdAsync(user.usr_id, tran);

            if (usrFromDb == null)
            {
                return false;
            }

            var result = new UserEntity(
                usr_id: usrFromDb.usr_id,
                first_name: user.first_name ?? usrFromDb.first_name,
                last_name: user.last_name ?? usrFromDb.last_name,
                middle_name: user.middle_name ?? usrFromDb.middle_name,
                login: user.login ?? usrFromDb.login,
                password_hash: user.password_hash ?? usrFromDb.password_hash
            );

            var sql = @"UPDATE usr
                    SET first_name = @first_name,
                    middle_name = @middle_name,
                    last_name = @last_name,
                    login = @login,
                    password_hash = @password_hash
                    WHERE usr_id = @usr_id";

            var affectedRows = await dbConnection.ExecuteAsync(sql, result, tran).ConfigureAwait(false);
            return affectedRows > 0;
        }
        catch
        {
            tran.Rollback();
            throw;
        }
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