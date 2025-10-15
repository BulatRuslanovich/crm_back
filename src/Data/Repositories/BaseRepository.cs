namespace CrmBack.Data.Repositories;

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;


public abstract class BaseRepository<TEntity>(
    IDbConnection dbConnection,
    ILogger logger) where TEntity : class
{

    protected void LogSql(string sql, object? parameters = null) =>
        logger.LogDebug("[SQL] {Sql} | Params: {@Parameters}", sql, parameters);


    protected async Task<TEntity?> QuerySingleAsync<TId>(
        string sql,
        TId id,
        IDbTransaction? transaction = null)
    {
        var parameters = new { id };
        LogSql(sql, parameters);

        var result = await dbConnection.QuerySingleOrDefaultAsync<TEntity>(
            sql, parameters, transaction).ConfigureAwait(false);

        if (result == null)
            logger.LogDebug("{Entity} not found: {Id}", typeof(TEntity).Name, id);


        return result;
    }

    protected async Task<IEnumerable<TEntity>> QueryAsync(string sql, object? parameters = null)
    {
        LogSql(sql, parameters);
        var result = await dbConnection.QueryAsync<TEntity>(sql, parameters).ConfigureAwait(false);

        if (!result.Any())
            logger.LogDebug("No {Entity} found", typeof(TEntity).Name);


        return result;
    }

    protected async Task<int> ExecuteScalarAsync(string sql, object entity)
    {
        LogSql(sql, entity);

        var id = await dbConnection.ExecuteScalarAsync<int>(sql, entity);

        logger.LogDebug("Created {Entity}: {Id}", typeof(TEntity).Name, id);

        return id;
    }

    protected async Task<bool> ExecuteAsync(
        string sql,
        object parameters,
        IDbTransaction? transaction = null)
    {
        LogSql(sql, parameters);
        var rows = await dbConnection.ExecuteAsync(sql, parameters, transaction).ConfigureAwait(false);

        logger.LogDebug("{Entity} affected rows: {Rows}", typeof(TEntity).Name, rows);

        return rows > 0;
    }


    protected async Task<TResult> WithTransactionAsync<TResult>(
        Func<IDbTransaction, Task<TResult>> action)
    {
        using var transaction = dbConnection.BeginTransaction();

        try
        {
            var result = await action(transaction);
            transaction.Commit();
            return result;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            logger.LogError(ex, "Transaction failed and rolled back for {Entity}", typeof(TEntity).Name);
            throw;
        }
    }
}
