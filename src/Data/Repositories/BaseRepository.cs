namespace CrmBack.Data.Repositories;

using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using CrmBack.Core.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Dapper;

public abstract class BaseRepository<TEntity>(
    IDbConnection dbConnection,
    ILogger logger,
    IOptions<DatabaseLoggingOptions> loggingOptions) where TEntity : class
{
    private readonly bool enableDbLog = loggingOptions.Value.EnableDatabaseLogging;

    protected void LogSql(string sql, object parameters)
    {
        if (enableDbLog)
        {
            logger.LogDebug("[SQL] Executing SQL: {Sql} with parameters: {@Parameters}", sql, parameters);
        }
    }

    protected async Task<TEntity?> GetByIdAsync<TId>(string sql, TId id, IDbTransaction? transaction = null)
    {
        var parameters = new { id };
        LogSql(sql, parameters);
        var result = await dbConnection.QuerySingleOrDefaultAsync<TEntity>(sql, parameters, transaction).ConfigureAwait(false);

        if (enableDbLog && result == null)
        {
            logger.LogDebug("No {Entity} found with ID: {Id}", typeof(TEntity).Name, id);
        }

        return result;
    }

    protected async Task<IEnumerable<TEntity>> GetAllAsync(string sql, object parameters)
    {
        LogSql(sql, parameters);
        var result = await dbConnection.QueryAsync<TEntity>(sql, parameters).ConfigureAwait(false);

        if (enableDbLog && !result.Any())
        {
            logger.LogDebug("No {Entity} found for GetAllAsync with parameters: {@Parameters}", typeof(TEntity).Name, parameters);
        }

        return result;
    }

    protected async Task<int> CreateAsync(string sql, TEntity entity)
    {
        LogSql(sql, entity);
        var result = await dbConnection.ExecuteScalarAsync<int>(sql, entity).ConfigureAwait(false);

        if (enableDbLog)
        {
            logger.LogDebug("Created {Entity} with ID {Id}", typeof(TEntity).Name, result);
        }

        return result;
    }

    protected async Task<bool> UpdateAsync(string sql, TEntity entity, IDbTransaction? transaction = null)
    {
        LogSql(sql, entity);
        var affectedRows = await dbConnection.ExecuteAsync(sql, entity, transaction).ConfigureAwait(false);

        if (enableDbLog)
        {
            logger.LogDebug("Updated {Entity}, affected rows: {AffectedRows}", typeof(TEntity).Name, affectedRows);
        }

        return affectedRows > 0;
    }

    protected async Task<bool> DeleteAsync(string sql, object parameters, bool isHardDelete)
    {
        LogSql(sql, parameters);
        var affectedRows = await dbConnection.ExecuteAsync(sql, parameters).ConfigureAwait(false);

        if (enableDbLog)
        {
            logger.LogDebug("{DeleteType} deleted {Entity} with parameters: {@Parameters}, affected rows: {AffectedRows}",
                isHardDelete ? "Hard" : "Soft", typeof(TEntity).Name, parameters, affectedRows);
        }

        return affectedRows > 0;
    }
}