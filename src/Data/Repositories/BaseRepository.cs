namespace CrmBack.Data.Repositories;

using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;


public abstract class BaseRepository<TEntity>(IDbConnection dbConnection) where TEntity : class
{
    protected async Task<TEntity?> QuerySingleAsync<TId>(string sql, TId id, CancellationToken ct = default)
    {
        var command = new CommandDefinition(sql, new { id }, cancellationToken: ct);
        return await dbConnection.QuerySingleOrDefaultAsync<TEntity>(command).ConfigureAwait(false);
    }

    protected async Task<IEnumerable<TEntity>> QueryAsync(string sql, object? parameters = null, CancellationToken ct = default)
    {
        var command = new CommandDefinition(sql, parameters, cancellationToken: ct);
        return await dbConnection.QueryAsync<TEntity>(command).ConfigureAwait(false);
    }

    protected async Task<int> ExecuteScalarAsync(string sql, object entity, CancellationToken ct = default)
    {
        var command = new CommandDefinition(sql, entity, cancellationToken: ct);
        return await dbConnection.ExecuteScalarAsync<int>(command).ConfigureAwait(false);
    }

    protected async Task<bool> ExecuteAsync(string sql, object parameters, CancellationToken ct = default)
    {
        var command = new CommandDefinition(sql, parameters, cancellationToken: ct);
        return await dbConnection.ExecuteAsync(command).ConfigureAwait(false) > 0;
    }
}