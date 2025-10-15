namespace CrmBack.Data.Repositories;

using Dapper;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;


public abstract class BaseRepository<TEntity>(IDbConnection dbConnection) where TEntity : class
{
    protected async Task<TEntity?> QuerySingleAsync<TId>(string sql, TId id) =>
        await dbConnection.QuerySingleOrDefaultAsync<TEntity>(sql, id).ConfigureAwait(false);

    protected async Task<IEnumerable<TEntity>> QueryAsync(string sql, object? parameters = null) =>
        await dbConnection.QueryAsync<TEntity>(sql, parameters).ConfigureAwait(false);

    protected async Task<int> ExecuteScalarAsync(string sql, object entity) =>
        await dbConnection.ExecuteScalarAsync<int>(sql, entity);

    protected async Task<bool> ExecuteAsync(string sql, object parameters) =>
        await dbConnection.ExecuteAsync(sql, parameters).ConfigureAwait(false) > 0;
}