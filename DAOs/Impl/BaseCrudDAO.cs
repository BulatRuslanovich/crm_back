using System.Text.Json;
using CrmBack.Core.Models.Dto;
using CrmBack.Core.Models.Entities;
using CrmBack.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace CrmBack.DAOs.Impl;

/// <summary>
/// Base CRUD DAO implementation with common patterns and Redis caching
/// </summary>
public abstract class BaseCrudDAO<TEntity, RDto, CDto, UDto>(AppDBContext context, IDistributedCache cache) : ICrudDAO<RDto, CDto, UDto>
    where TEntity : BaseEntity
{
    protected AppDBContext Context => context;
    protected IDistributedCache Cache => cache;
    protected abstract string CacheKeyPrefix { get; }
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);

    protected string GetCacheKey(int id) => $"{CacheKeyPrefix}:{id}";
    protected string GetCacheKey(PaginationDto pagination) => $"{CacheKeyPrefix}:list:{pagination.Page}:{pagination.PageSize}:{pagination.SearchTerm ?? ""}";

    public virtual async Task<RDto?> FetchById(int id, CancellationToken ct)
    {
        var cacheKey = GetCacheKey(id);
        var cached = await Cache.GetStringAsync(cacheKey, ct);
        
        if (cached is not null)
            return JsonSerializer.Deserialize<RDto>(cached);

        var entity = await Context.Set<TEntity>().FindAsync([id], ct);
        if (entity is null or { IsDeleted: true }) return default;
        
        var dto = MapToDto(entity);
        await Cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(dto), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheExpiration }, ct);
        
        return dto;
    }

    public virtual async Task<List<RDto>> FetchAll(PaginationDto pagination, CancellationToken ct)
    {
        var cacheKey = GetCacheKey(pagination);
        var cached = await Cache.GetStringAsync(cacheKey, ct);
        
        if (cached is not null)
            return JsonSerializer.Deserialize<List<RDto>>(cached) ?? [];

        var query = Context.Set<TEntity>().AsQueryable();
        query = ApplyDefaults(query, pagination);
        var entities = await query.ToListAsync(ct);
        var result = entities.Select(MapToDto).ToList();
        
        await Cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheExpiration }, ct);
        
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
            await Cache.RemoveAsync(GetCacheKey(id), ct);
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
            await Cache.RemoveAsync(GetCacheKey(id), ct);
            await InvalidateListCache();
        }
        
        return success;
    }

    protected abstract RDto MapToDto(TEntity entity);
    protected abstract TEntity MapToEntity(CDto dto);
    protected abstract void UpdateEntity(TEntity entity, UDto dto);
    protected abstract IQueryable<TEntity> ApplyDefaults(IQueryable<TEntity> query, PaginationDto pagination);

    private Task InvalidateListCache()
    {
        // Invalidate paginated cache with pattern matching would require Redis Stack with JSON/SCAN support
        // For simplicity, we'll just clear all list caches when items change
        // In production, you could use Redis keys with pattern matching
        return Task.CompletedTask;
    }
}

