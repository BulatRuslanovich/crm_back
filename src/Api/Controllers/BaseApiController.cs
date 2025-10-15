namespace CrmBack.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

public abstract class BaseApiController(IMemoryCache cache, ILogger logger) : ControllerBase
{
    protected bool ValidateId(int id, string? entityName = null)
    {
        if (id > 0) return true;
        logger.LogWarning("Invalid {Entity} ID {Id}", entityName ?? "entity", id);
        return false;
    }

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

    protected void InvalidateCache(int id, string entityPrefix, string allCacheKey)
    {
        cache.Remove($"{entityPrefix}{id}");
        cache.Remove(allCacheKey);
    }
}
