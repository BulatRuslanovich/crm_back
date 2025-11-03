using CrmBack.Application.Common.Dto;
using CrmBack.Domain.Common;
using CrmBack.Infrastructure.Data;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace CrmBack.Infrastructure.Persistence.Common;

public abstract class BaseCrudDAO<TEntity, RDto, CDto, UDto>(AppDBContext context, IConnectionMultiplexer redis) : ICrudDAO<RDto, CDto, UDto>
    where TEntity : BaseEntity
{
    protected AppDBContext Context => context;
    protected IConnectionMultiplexer Redis => redis;
    protected abstract string CacheKeyPrefix { get; }
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);
    private static readonly MessagePackSerializerOptions MessagePackOptions = MessagePackSerializerOptions.Standard
        .WithResolver(ContractlessStandardResolver.Instance)
        .WithCompression(MessagePackCompression.Lz4Block);

    protected string GetCacheKey(int id) => $"CrmBack:{CacheKeyPrefix}:{id}";
    protected string GetCacheKey(PaginationDto pagination) => $"CrmBack:{CacheKeyPrefix}:list:{pagination.Page}:{pagination.PageSize}:{pagination.SearchTerm ?? ""}";

    private IDatabase GetDatabase() => Redis.GetDatabase();

    public virtual async Task<RDto?> FetchById(int id, CancellationToken ct)
    {
        string cacheKey = GetCacheKey(id);
        var db = GetDatabase();
        var cached = await db.StringGetAsync(cacheKey);

        if (cached.HasValue)
            return MessagePackSerializer.Deserialize<RDto>(cached!, MessagePackOptions, ct);

        var entity = await Context.Set<TEntity>().FindAsync([id], ct);
        if (entity is null or { IsDeleted: true }) return default;

        var dto = MapToDto(entity);
        var serialized = MessagePackSerializer.Serialize(dto, MessagePackOptions, ct);
        await db.StringSetAsync(cacheKey, serialized, CacheExpiration);

        return dto;
    }

    public virtual async Task<List<RDto>> FetchAll(PaginationDto pagination, CancellationToken ct)
    {
        string cacheKey = GetCacheKey(pagination);
        var db = GetDatabase();
        var cached = await db.StringGetAsync(cacheKey);

        if (cached.HasValue)
            return MessagePackSerializer.Deserialize<List<RDto>>(cached!, MessagePackOptions, ct) ?? [];

        var query = Context.Set<TEntity>().AsQueryable();
        query = ApplyDefaults(query, pagination);
        var entities = await query.ToListAsync(ct);
        var result = entities.Select(MapToDto).ToList();

        var serialized = MessagePackSerializer.Serialize(result, MessagePackOptions, ct);
        await db.StringSetAsync(cacheKey, serialized, CacheExpiration);

        return result;
    }

    public virtual async Task<RDto?> Create(CDto dto, CancellationToken ct)
    {
        var entity = MapToEntity(dto);
        Context.Set<TEntity>().Add(entity);
        await Context.SaveChangesAsync(ct);

        var result = MapToDto(entity);
        await InvalidateListCache();

        return result;
    }

    public virtual async Task<bool> Update(int id, UDto dto, CancellationToken ct)
    {
        var existing = await Context.Set<TEntity>().FindAsync([id], ct);
        if (existing is null or { IsDeleted: true }) return false;

        UpdateEntity(existing, dto);
        var success = await Context.SaveChangesAsync(ct) > 0;

        if (success)
        {
            var db = GetDatabase();
            await db.KeyDeleteAsync(GetCacheKey(id));
            await InvalidateListCache();
        }

        return success;
    }

    public virtual async Task<bool> Delete(int id, CancellationToken ct)
    {
        var entity = await Context.Set<TEntity>().FindAsync([id], ct);
        if (entity is null or { IsDeleted: true }) return false;

        entity.IsDeleted = true;
        var success = await Context.SaveChangesAsync(ct) > 0;

        if (success)
        {
            var db = GetDatabase();
            await db.KeyDeleteAsync(GetCacheKey(id));
            await InvalidateListCache();
        }

        return success;
    }

    protected abstract RDto MapToDto(TEntity entity);
    protected abstract TEntity MapToEntity(CDto dto);
    protected abstract void UpdateEntity(TEntity entity, UDto dto);
    protected abstract IQueryable<TEntity> ApplyDefaults(IQueryable<TEntity> query, PaginationDto pagination);

    private async Task InvalidateListCache()
    {
        var db = Redis.GetDatabase();
        string pattern = $"CrmBack:{CacheKeyPrefix}:list:*";

        // Use SCAN to find all keys matching the pattern
        var server = Redis.GetServer(Redis.GetEndPoints()[0]);
        var keys = server.KeysAsync(pattern: pattern);

        await foreach (var key in keys)
        {
            await db.KeyDeleteAsync(key);
        }
    }
}

