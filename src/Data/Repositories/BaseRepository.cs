namespace CrmBack.Data.Repositories;

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CrmBack.Core.Config;
using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public abstract class BaseRepository<TEntity> where TEntity : class
{
    protected readonly IDbConnection DbConnection;
    protected readonly ILogger Logger;
    private readonly bool _enableDbLog;

    protected BaseRepository(
        IDbConnection dbConnection,
        ILogger logger,
        IOptions<DatabaseLoggingOptions> loggingOptions)
    {
        DbConnection = dbConnection;
        Logger = logger;
        _enableDbLog = loggingOptions.Value.EnableDatabaseLogging;
    }

    protected void LogSql(string sql, object? parameters = null)
    {
        if (_enableDbLog)
        {
            Logger.LogDebug("[SQL] {Sql} | Params: {@Parameters}", sql, parameters);
        }
    }

    protected async Task<TEntity?> QuerySingleAsync<TId>(
        string sql,
        TId id,
        IDbTransaction? transaction = null)
    {
        var parameters = new { id };
        LogSql(sql, parameters);

        var result = await DbConnection.QuerySingleOrDefaultAsync<TEntity>(
            sql, parameters, transaction).ConfigureAwait(false);

        if (_enableDbLog && result == null)
        {
            Logger.LogDebug("{Entity} not found: {Id}", typeof(TEntity).Name, id);
        }

        return result;
    }

    protected async Task<IEnumerable<TEntity>> QueryAsync(string sql, object? parameters = null)
    {
        LogSql(sql, parameters);
        var result = await DbConnection.QueryAsync<TEntity>(sql, parameters).ConfigureAwait(false);

        if (_enableDbLog && !result.Any())
        {
            Logger.LogDebug("No {Entity} found", typeof(TEntity).Name);
        }

        return result;
    }

    protected async Task<int> ExecuteScalarAsync(string sql, object entity)
    {
        LogSql(sql, entity);
        var id = await DbConnection.ExecuteScalarAsync<int>(sql, entity).ConfigureAwait(false);

        if (_enableDbLog)
        {
            Logger.LogDebug("Created {Entity}: {Id}", typeof(TEntity).Name, id);
        }

        return id;
    }

    protected async Task<bool> ExecuteAsync(
        string sql,
        object parameters,
        IDbTransaction? transaction = null)
    {
        LogSql(sql, parameters);
        var rows = await DbConnection.ExecuteAsync(sql, parameters, transaction).ConfigureAwait(false);

        if (_enableDbLog)
        {
            Logger.LogDebug("{Entity} affected rows: {Rows}", typeof(TEntity).Name, rows);
        }

        return rows > 0;
    }

    protected async Task<TResult> WithTransactionAsync<TResult>(
        Func<IDbTransaction, Task<TResult>> action)
    {
        using var transaction = DbConnection.BeginTransaction();
        try
        {
            var result = await action(transaction);
            transaction.Commit();
            return result;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
