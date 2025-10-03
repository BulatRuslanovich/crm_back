namespace CrmBack.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

/// <summary>
/// Base controller providing common functionality for all API controllers.
/// Includes caching, validation, and logging helpers.
/// </summary>
public abstract class BaseApiController(IMemoryCache cache, ILogger logger) : ControllerBase
{

    /// <summary>
    /// Validates that an ID is positive.
    /// Logs a warning if validation fails.
    /// </summary>
    /// <param name="id">ID to validate</param>
    /// <param name="entityName">Name of entity for logging (optional)</param>
    /// <returns>True if ID is valid, false otherwise</returns>
    protected bool ValidateId(int id, string? entityName = null)
    {
        if (id > 0) return true;
        logger.LogWarning("Invalid {Entity} ID {Id}", entityName ?? "entity", id);
        return false;
    }

    /// <summary>
    /// Retrieves data from cache or fetches and caches it if not found.
    /// Returns NotFound if data doesn't exist.
    /// </summary>
    /// <typeparam name="T">Type of data to retrieve</typeparam>
    /// <param name="cacheKey">Unique cache key</param>
    /// <param name="fetchData">Function to fetch data if not in cache</param>
    /// <param name="expiration">Cache expiration time</param>
    /// <returns>ActionResult with data or NotFound</returns>
    protected async Task<ActionResult<T>> GetOrSetCache<T>(
        string cacheKey,
        Func<Task<T?>> fetchData,
        TimeSpan expiration) where T : class
    {
        if (cache.TryGetValue(cacheKey, out T? cached))
        {
            logger.LogDebug("Cache hit: {Key}", cacheKey);
            return Ok(cached);
        }

        var data = await fetchData();
        if (data == null)
        {
            logger.LogWarning("Data not found for key: {Key}", cacheKey);
            return NotFound();
        }

        cache.Set(cacheKey, data, expiration);
        logger.LogDebug("Cached: {Key}", cacheKey);
        return Ok(data);
    }

    /// <summary>
    /// Invalidates cache entries for a specific entity ID and the "all" cache.
    /// </summary>
    /// <param name="id">Entity ID to invalidate</param>
    /// <param name="entityPrefix">Cache key prefix (e.g., "user_", "org_")</param>
    /// <param name="allCacheKey">Key for the "all entities" cache</param>
    protected void InvalidateCache(int id, string entityPrefix, string allCacheKey)
    {
        cache.Remove($"{entityPrefix}{id}");
        cache.Remove(allCacheKey);
    }
}
