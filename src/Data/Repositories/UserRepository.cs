namespace CrmBack.Data.Repositories;

using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using CrmBack.Core.Config;
using CrmBack.Core.Models.Entities;
using CrmBack.Core.Repositories;
using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class UserRepository(IDbConnection dbConnection, ILogger<UserRepository> logger, IOptions<DatabaseLoggingOptions> loggingOptions) : IUserRepository
{
    private const string BaseSelectQuery = @"SELECT usr_id, first_name, middle_name, last_name, login, password_hash, created_at, updated_at, created_by, updated_by, is_deleted
                                        FROM usr
                                        WHERE {0} AND NOT is_deleted
                                        LIMIT 1";
    private readonly bool enableDbLog = loggingOptions.Value.EnableDatabaseLogging;

    public async Task<UserEntity?> GetByIdAsync(int id)
    {
        return await GetByIdAsync(id, null);
    }

    private async Task<UserEntity?> GetByIdAsync(int id, IDbTransaction? transaction = null)
    {
        var sql = string.Format(BaseSelectQuery, "usr_id = @usr_id");
        var parameters = new { usr_id = id };

        if (enableDbLog)
        {
            logger.LogDebug(new EventId(1, "SQL"), "[SQL] Executing query: {Sql} with parameters: {@Parameters}", sql, parameters);
        }

        var res = await dbConnection.QuerySingleOrDefaultAsync<UserEntity>(sql, parameters, transaction).ConfigureAwait(false);

        if (enableDbLog && res == null)
        {
            logger.LogDebug("No user found with ID: {Id}", id);
        }

        return res;
    }

    public async Task<UserEntity?> GetByLoginAsync(string _login)
    {
        var sql = string.Format(BaseSelectQuery, "login = @login");
        var parameters = new { login = _login };

        LogSql(sql, parameters);

        var res = await dbConnection.QuerySingleOrDefaultAsync<UserEntity>(sql, parameters).ConfigureAwait(false);

        if (enableDbLog && res == null)
        {
            logger.LogDebug("No user found with login: {Login}", _login);
        }

        return res;
    }

    public async Task<IEnumerable<UserEntity>> GetAllAsync(bool includeDeleted = false, int page = 1, int pageSize = 10)
    {
        var sql = @"SELECT usr_id,
                           first_name,
                           middle_name,
                           last_name,
                           login,
                           password_hash,
                           created_at,
                           updated_at,
                           created_by,
                           updated_by,
                           is_deleted
                    FROM usr";

        if (!includeDeleted)
        {
            sql += " WHERE NOT is_deleted";
        }

        var parameters = new { PageSize = pageSize, Offset = (page - 1) * pageSize };

        sql += " LIMIT @PageSize OFFSET @Offset";

        LogSql(sql, parameters);

        var res = await dbConnection.QueryAsync<UserEntity>(sql, parameters).ConfigureAwait(false);

        if (enableDbLog && !res.Any())
        {
            logger.LogDebug("No users found for GetAllAsync with includeDeleted={IncludeDeleted}, page={Page}, pageSize={PageSize}", includeDeleted, page, pageSize);
        }

        return res;
    }

    public async Task<int> CreateAsync(UserEntity user)
    {
        const string sql = @"INSERT INTO usr (first_name, middle_name, last_name, login, password_hash, created_by, updated_by)
                            VALUES (@first_name, @middle_name, @last_name, @login, @password_hash, 'system', 'system')
                            RETURNING usr_id";

        LogSql(sql, user);

        var res = await dbConnection.ExecuteScalarAsync<int>(sql, user).ConfigureAwait(false);

        if (enableDbLog)
        {
            logger.LogDebug("Created user with ID {Id}", res);
        }

        return res;
    }

    public async Task<bool> UpdateAsync(UserEntity user)
    {
        using var tran = dbConnection.BeginTransaction();

        try
        {
            var usrFromDb = await GetByIdAsync(user.usr_id, tran);

            if (usrFromDb == null)
            {
                if (enableDbLog)
                {
                    logger.LogDebug("User with ID {Id} not found for update", user.usr_id);
                }

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

            LogSql(sql, result);

            var affectedRows = await dbConnection.ExecuteAsync(sql, result, tran).ConfigureAwait(false);

            if (enableDbLog)
            {
                logger.LogDebug("Updated user with ID {Id}, affected rows: {AffectedRows}", user.usr_id, affectedRows);
            }

            tran.Commit();
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
        var parameters = new { Id = id };

        LogSql(sql, parameters);

        var affectedRows = await dbConnection.ExecuteAsync(sql, parameters).ConfigureAwait(false);

        if (enableDbLog)
        {
            logger.LogDebug("Hard deleted user with ID {Id}, affected rows: {AffectedRows}", id, affectedRows);
        }

        return affectedRows > 0;
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var sql = @"UPDATE usr 
                    SET is_deleted = true
                    WHERE usr_id = @Id";
        var parameters = new { Id = id };

        LogSql(sql, parameters);

        var affectedRows = await dbConnection.ExecuteAsync(sql, parameters).ConfigureAwait(false);

        if (enableDbLog)
        {
            logger.LogDebug("Soft deleted user with ID {Id}, affected rows: {AffectedRows}", id, affectedRows);
        }

        return affectedRows > 0;
    }

    private void LogSql(string sql, object parameters)
    {
        if (enableDbLog)
        {
            logger.LogDebug(new EventId(99, "SQL"), "[SQL] Executing SQL: {Sql} with parameters: {@Parameters}", sql, parameters);
        }
    }
}
