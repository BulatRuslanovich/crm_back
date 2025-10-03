namespace CrmBack.Data.Repositories;

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CrmBack.Core.Config;
using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Base repository providing common database operations for all entities.
/// Implements logging and transaction management using Dapper.
/// </summary>
/// <typeparam name="TEntity">The entity type this repository manages</typeparam>
public abstract class BaseRepository<TEntity>(
    IDbConnection dbConnection,
    ILogger logger,
    IOptions<DatabaseLoggingOptions> loggingOptions) where TEntity : class
{
    private readonly bool _enableDbLog = loggingOptions.Value.EnableDatabaseLogging;

    /// <summary>
    /// Logs SQL query and parameters when database logging is enabled.
    /// </summary>
    /// <param name="sql">SQL query to log</param>
    /// <param name="parameters">Query parameters to log (optional)</param>
    protected void LogSql(string sql, object? parameters = null)
    {
        if (_enableDbLog)
        {
            logger.LogDebug("[SQL] {Sql} | Params: {@Parameters}", sql, parameters);
        }
    }

    /// <summary>
    /// Executes a query and returns a single entity or null if not found.
    /// Automatically wraps the ID parameter in an anonymous object.
    /// </summary>
    /// <typeparam name="TId">Type of the ID parameter (int, string, Guid, etc.)</typeparam>
    /// <param name="sql">SQL query with @id parameter placeholder</param>
    /// <param name="id">ID value to search for</param>
    /// <param name="transaction">Optional database transaction</param>
    /// <returns>Entity if found, null otherwise</returns>
    protected async Task<TEntity?> QuerySingleAsync<TId>(
        string sql,
        TId id,
        IDbTransaction? transaction = null)
    {
        var parameters = new { id };
        LogSql(sql, parameters);

        var result = await dbConnection.QuerySingleOrDefaultAsync<TEntity>(
            sql, parameters, transaction).ConfigureAwait(false);

        if (_enableDbLog && result == null)
        {
            logger.LogDebug("{Entity} not found: {Id}", typeof(TEntity).Name, id);
        }

        return result;
    }

    /// <summary>
    /// Executes a query and returns a collection of entities.
    /// Returns empty collection if no results found.
    /// </summary>
    /// <param name="sql">SQL query to execute</param>
    /// <param name="parameters">Query parameters (optional)</param>
    /// <returns>Collection of entities matching the query</returns>
    protected async Task<IEnumerable<TEntity>> QueryAsync(string sql, object? parameters = null)
    {
        LogSql(sql, parameters);
        var result = await dbConnection.QueryAsync<TEntity>(sql, parameters).ConfigureAwait(false);

        if (_enableDbLog && !result.Any())
        {
            logger.LogDebug("No {Entity} found", typeof(TEntity).Name);
        }

        return result;
    }

    /// <summary>
    /// Executes an INSERT query and returns the generated ID.
    /// Typically used with RETURNING clause in PostgreSQL or similar mechanisms.
    /// </summary>
    /// <param name="sql">INSERT SQL query with RETURNING id clause</param>
    /// <param name="entity">Entity to insert</param>
    /// <returns>Generated ID of the created entity</returns>
    protected async Task<int> ExecuteScalarAsync(string sql, object entity)
    {
        LogSql(sql, entity);
        var id = await dbConnection.ExecuteScalarAsync<int>(sql, entity).ConfigureAwait(false);

        if (_enableDbLog)
        {
            logger.LogDebug("Created {Entity}: {Id}", typeof(TEntity).Name, id);
        }

        return id;
    }

    /// <summary>
    /// Executes a non-query SQL command (UPDATE, DELETE, etc.).
    /// Returns true if at least one row was affected.
    /// </summary>
    /// <param name="sql">SQL command to execute</param>
    /// <param name="parameters">Command parameters</param>
    /// <param name="transaction">Optional database transaction</param>
    /// <returns>True if operation affected any rows, false otherwise</returns>
    protected async Task<bool> ExecuteAsync(
        string sql,
        object parameters,
        IDbTransaction? transaction = null)
    {
        LogSql(sql, parameters);
        var rows = await dbConnection.ExecuteAsync(sql, parameters, transaction).ConfigureAwait(false);

        if (_enableDbLog)
        {
            logger.LogDebug("{Entity} affected rows: {Rows}", typeof(TEntity).Name, rows);
        }

        return rows > 0;
    }

    /// <summary>
    /// Executes a function within a database transaction.
    /// Automatically commits on success or rolls back on exception.
    /// </summary>
    /// <typeparam name="TResult">Return type of the operation</typeparam>
    /// <param name="action">Function to execute within transaction</param>
    /// <returns>Result of the operation</returns>
    /// <exception cref="Exception">Rethrows any exception after rollback</exception>
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
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
