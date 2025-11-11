using CrmBack.Application.Common.Dto;
using CrmBack.Domain.Common;
using CrmBack.Infrastructure.Data;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace CrmBack.Infrastructure.Persistence.Common;

/// <summary>
/// Base class for CRUD Data Access Objects
/// Provides common CRUD operations with Redis caching layer
/// Performance optimizations:
/// - Redis caching with MessagePack serialization (faster than JSON)
/// - LZ4 compression for cache data (reduces memory usage)
/// - Cache invalidation on write operations
/// </summary>
/// <typeparam name="TEntity">Entity type (must inherit from BaseEntity)</typeparam>
/// <typeparam name="RDto">Read DTO type</typeparam>
/// <typeparam name="CDto">Create DTO type</typeparam>
/// <typeparam name="UDto">Update DTO type</typeparam>
public abstract class BaseCrudDAO<TEntity, RDto, CDto, UDto>(AppDBContext context, IConnectionMultiplexer redis) : ICrudDAO<RDto, CDto, UDto>
	where TEntity : BaseEntity
{
	protected AppDBContext Context => context;
	protected IConnectionMultiplexer Redis => redis;

	/// <summary>
	/// Cache key prefix for Redis keys (must be implemented by derived classes)
	/// Example: "User", "Organization", "Activity"
	/// </summary>
	protected abstract string CacheKeyPrefix { get; }

	/// <summary>
	/// Cache expiration time: 5 minutes
	/// Balance between cache hit rate and data freshness
	/// </summary>
	private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);

	/// <summary>
	/// MessagePack serialization options with optimizations:
	/// - ContractlessStandardResolver: No need for [MessagePackObject] attributes
	/// - LZ4Block compression: Fast compression algorithm, reduces cache size
	/// Performance: MessagePack is ~3x faster than JSON for serialization
	/// </summary>
	private static readonly MessagePackSerializerOptions MessagePackOptions = MessagePackSerializerOptions.Standard
		.WithResolver(ContractlessStandardResolver.Instance)
		.WithCompression(MessagePackCompression.Lz4Block);

	/// <summary>Generate cache key for single entity by ID</summary>
	protected string GetCacheKey(int id) => $"CrmBack:{CacheKeyPrefix}:{id}";

	/// <summary>Generate cache key for paginated list query</summary>
	protected string GetCacheKey(PaginationDto pagination) => $"CrmBack:{CacheKeyPrefix}:list:{pagination.Page}:{pagination.PageSize}:{pagination.SearchTerm ?? ""}";

	private IDatabase GetDatabase() => Redis.GetDatabase();

	/// <summary>
	/// Fetch entity by ID with caching
	/// Performance: Checks Redis cache first, falls back to database if cache miss
	/// </summary>
	public virtual async Task<RDto?> FetchById(int id, CancellationToken ct)
	{
		string cacheKey = GetCacheKey(id);
		var db = GetDatabase();
		var cached = await db.StringGetAsync(cacheKey);

		// Cache hit: Deserialize from MessagePack and return
		if (cached.HasValue)
			return MessagePackSerializer.Deserialize<RDto>(cached!, MessagePackOptions, ct);

		// Cache miss: Query database
		var entity = await Context.Set<TEntity>().FindAsync([id], ct);
		// Soft delete check: Don't return deleted entities
		if (entity is null or { IsDeleted: true }) return default;

		var dto = MapToDto(entity);
		// Cache the result for future requests
		var serialized = MessagePackSerializer.Serialize(dto, MessagePackOptions, ct);
		await db.StringSetAsync(cacheKey, serialized, CacheExpiration);

		return dto;
	}

	/// <summary>
	/// Fetch all entities with pagination and caching
	/// Performance: Caches paginated results based on page, pageSize, and searchTerm
	/// </summary>
	public virtual async Task<List<RDto>> FetchAll(PaginationDto pagination, CancellationToken ct)
	{
		string cacheKey = GetCacheKey(pagination);
		var db = GetDatabase();
		var cached = await db.StringGetAsync(cacheKey);

		// Cache hit: Return cached results
		if (cached.HasValue)
			return MessagePackSerializer.Deserialize<List<RDto>>(cached!, MessagePackOptions, ct) ?? [];

		// Cache miss: Query database with pagination
		var query = Context.Set<TEntity>().AsQueryable();
		query = ApplyDefaults(query, pagination);
		var entities = await query.ToListAsync(ct);
		var result = entities.Select(MapToDto).ToList();

		// Cache the paginated results
		var serialized = MessagePackSerializer.Serialize(result, MessagePackOptions, ct);
		await db.StringSetAsync(cacheKey, serialized, CacheExpiration);

		return result;
	}

	/// <summary>
	/// Create new entity
	/// Invalidates list cache after creation (list may have changed)
	/// </summary>
	public virtual async Task<RDto?> Create(CDto dto, CancellationToken ct)
	{
		var entity = MapToEntity(dto);
		Context.Set<TEntity>().Add(entity);
		await Context.SaveChangesAsync(ct);

		var result = MapToDto(entity);
		// Invalidate list cache (new entity added to list)
		await InvalidateListCache();

		return result;
	}

	/// <summary>
	/// Update existing entity
	/// Invalidates both entity cache and list cache after update
	/// </summary>
	public virtual async Task<bool> Update(int id, UDto dto, CancellationToken ct)
	{
		var existing = await Context.Set<TEntity>().FindAsync([id], ct);
		if (existing is null or { IsDeleted: true }) return false;

		UpdateEntity(existing, dto);
		var success = await Context.SaveChangesAsync(ct) > 0;

		if (success)
		{
			var db = GetDatabase();
			// Invalidate entity cache (entity data changed)
			await db.KeyDeleteAsync(GetCacheKey(id));
			// Invalidate list cache (entity in list may have changed)
			await InvalidateListCache();
		}

		return success;
	}

	/// <summary>
	/// Soft delete entity (sets IsDeleted flag)
	/// Invalidates both entity cache and list cache after deletion
	/// </summary>
	public virtual async Task<bool> Delete(int id, CancellationToken ct)
	{
		var entity = await Context.Set<TEntity>().FindAsync([id], ct);
		if (entity is null or { IsDeleted: true }) return false;

		// Soft delete: Set flag instead of physical deletion
		entity.IsDeleted = true;
		var success = await Context.SaveChangesAsync(ct) > 0;

		if (success)
		{
			var db = GetDatabase();
			// Invalidate entity cache (entity is now deleted)
			await db.KeyDeleteAsync(GetCacheKey(id));
			// Invalidate list cache (entity removed from list)
			await InvalidateListCache();
		}

		return success;
	}

	// Abstract methods that must be implemented by derived classes
	protected abstract RDto MapToDto(TEntity entity);
	protected abstract TEntity MapToEntity(CDto dto);
	protected abstract void UpdateEntity(TEntity entity, UDto dto);
	protected abstract IQueryable<TEntity> ApplyDefaults(IQueryable<TEntity> query, PaginationDto pagination);

	/// <summary>
	/// Invalidate all list cache entries for this entity type
	/// Performance: Uses Redis SCAN to find all matching keys (pattern: "list:*")
	/// This is necessary because list cache keys vary by pagination parameters
	/// </summary>
	private async Task InvalidateListCache()
	{
		var db = Redis.GetDatabase();
		string pattern = $"CrmBack:{CacheKeyPrefix}:list:*";

		// Use SCAN to find all keys matching the pattern
		// SCAN is preferred over KEYS for production (non-blocking, cursor-based)
		var server = Redis.GetServer(Redis.GetEndPoints()[0]);
		var keys = server.KeysAsync(pattern: pattern);

		// Delete all matching cache keys
		await foreach (var key in keys)
		{
			await db.KeyDeleteAsync(key);
		}
	}
}

