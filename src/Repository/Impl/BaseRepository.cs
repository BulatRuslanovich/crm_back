
namespace CrmBack.Repository.Impl;

using System.Data;
using System.Text.Json;
using CrmBack.Core.Utils;
using Dapper;
using Microsoft.Extensions.Caching.Distributed;


public class BaseRepository<TEntity, TKey>(IDbConnection dbConnection, IDistributedCache cache) where TEntity : class where TKey : notnull
{
    private readonly string tableName = EntityMetadataExtractor.ExtractMetadata<TEntity>().tableName;
    private readonly string keyColumn = EntityMetadataExtractor.ExtractMetadata<TEntity>().keyColumn;
    private readonly string[] columns = EntityMetadataExtractor.ExtractMetadata<TEntity>().columns;
    private readonly string[] insertColumns = EntityMetadataExtractor.ExtractMetadata<TEntity>().insertColumns;
    private readonly string[] updateColumns = EntityMetadataExtractor.ExtractMetadata<TEntity>().updateColumns;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public virtual async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default)
    {
        var cacheKey = $"{tableName}:{id}";

        var cached = await cache.GetStringAsync(cacheKey, ct);

        if (!string.IsNullOrEmpty(cached)) return JsonSerializer.Deserialize<TEntity>(cached, JsonOptions);


        var sql = $@"
            SELECT {string.Join(", ", columns)}
            FROM {tableName}
            WHERE {keyColumn} = @id AND NOT is_deleted
            LIMIT 1";

        var result = await QuerySingleAsync(sql, new { id }, ct);

        if (result != null)
        {
            var serialized = JsonSerializer.Serialize(result, JsonOptions);
            await cache.SetStringAsync(cacheKey, serialized, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            }, ct);
        }

        return result;
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(
        bool includeDeleted = false,
        int page = 1,
        int pageSize = 10,
        CancellationToken ct = default)
    {
        var whereClause = includeDeleted ? "" : "WHERE NOT is_deleted";
        var sql = $@"
            SELECT {string.Join(", ", columns)}
            FROM {tableName}
            {whereClause}
            ORDER BY {keyColumn} DESC
            LIMIT @pageSize OFFSET @offset";

        var offset = (page - 1) * pageSize;

        return await QueryAsync(sql, new { pageSize, offset }, ct);
    }

    public virtual async Task<TKey?> CreateAsync(TEntity entity, CancellationToken ct = default)
    {
        var columns = string.Join(", ", insertColumns);
        var parameters = string.Join(", ", insertColumns.Select(c => $"@{c}"));

        var sql = $@"
            INSERT INTO {tableName} ({columns})
            VALUES ({parameters})
            RETURNING {keyColumn}";

        var result = await ExecuteScalarAsync<TKey>(sql, entity, ct);

        if (result != null)
        {
            var cacheKey = $"{tableName}:{result}";
            await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(entity, JsonOptions), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            }, ct);
        }

        return result;
    }

    public virtual async Task<bool> UpdateAsync(TEntity entity, CancellationToken ct = default)
    {
        var setClause = string.Join(", ", updateColumns.Select(c => $"{c} = @{c}"));

        var sql = $@"
            UPDATE {tableName}
            SET {setClause}
            WHERE {keyColumn} = @{keyColumn}";

        var result = await ExecuteAsync(sql, entity, ct);

        if (result)
        {
            var cacheKey = $"{tableName}:{entity.GetType().GetProperty(keyColumn)?.GetValue(entity)}";
            await cache.RemoveAsync(cacheKey, ct);
        }

        return result;
    }

    public virtual async Task<bool> SoftDeleteAsync(TKey id, CancellationToken ct = default)
    {
        var sql = $@"
            UPDATE {tableName}
            SET is_deleted = true
            WHERE {keyColumn} = @id";

        var result = await ExecuteAsync(sql, new { id }, ct);

        if (result)
        {
            var cacheKey = $"{tableName}:{id}";
            await cache.RemoveAsync(cacheKey, ct);
        }

        return result;
    }


    public virtual async Task<bool> HardDeleteAsync(TKey id, CancellationToken ct = default)
    {
        var sql = $"DELETE FROM {tableName} WHERE {keyColumn} = @id";

        var result = await ExecuteAsync(sql, new { id }, ct);

        if (result)
        {
            var cacheKey = $"{tableName}:{id}";
            await cache.RemoveAsync(cacheKey, ct);
        }

        return result;
    }


    public virtual async Task<bool> ExistsAsync(TKey id, CancellationToken ct = default)
    {
        var sql = $@"
            SELECT EXISTS(
                    SELECT 1 FROM {tableName} 
                WHERE {keyColumn} = @id AND NOT is_deleted
            )";

        return await ExecuteScalarAsync<bool>(sql, new { id }, ct);
    }


    public virtual async Task<long> CountAsync(bool includeDeleted = false, CancellationToken ct = default)
    {
        var whereClause = includeDeleted ? "" : "WHERE NOT is_deleted";
        var sql = $"SELECT COUNT(*) FROM {tableName} {whereClause}";

        return await ExecuteScalarAsync<long>(sql, new { }, ct);
    }


    protected async Task<TEntity?> QuerySingleAsync(string sql, object parameters, CancellationToken ct = default)
    {
        var command = new CommandDefinition(sql, parameters, cancellationToken: ct);
        return await dbConnection.QuerySingleOrDefaultAsync<TEntity>(command).ConfigureAwait(false);
    }

    protected async Task<IEnumerable<TEntity>> QueryAsync(string sql, object? parameters = null, CancellationToken ct = default)
    {
        var command = new CommandDefinition(sql, parameters, cancellationToken: ct);
        return await dbConnection.QueryAsync<TEntity>(command).ConfigureAwait(false);
    }

    protected async Task<TResult?> ExecuteScalarAsync<TResult>(string sql, object entity, CancellationToken ct = default)
    {
        var command = new CommandDefinition(sql, entity, cancellationToken: ct);
        return await dbConnection.ExecuteScalarAsync<TResult>(command).ConfigureAwait(false);
    }

    protected async Task<bool> ExecuteAsync(string sql, object parameters, CancellationToken ct = default)
    {
        var command = new CommandDefinition(sql, parameters, cancellationToken: ct);
        return await dbConnection.ExecuteAsync(command).ConfigureAwait(false) > 0;
    }
}
