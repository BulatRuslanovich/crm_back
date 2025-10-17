
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace CrmBack.Api.Controllers;

public abstract class BaseApiController(IDistributedCache cache) : ControllerBase
{
    protected bool ValidateId(int id) => id > 0;

    private static readonly JsonSerializerOptions CacheJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    protected async Task<ActionResult<T>> GetDataFromCache<T>(
        string key,
        Func<Task<T?>> fetchData,
        TimeSpan expiration) where T : class
    {
        var cacheData = await cache.GetStringAsync(key, HttpContext.RequestAborted);

        if (!string.IsNullOrEmpty(cacheData))
        {
            var value = JsonSerializer.Deserialize<T>(cacheData, CacheJsonOptions);
            return Ok(value);
        }

        var data = await fetchData();

        if (data == null)
        {
            return NotFound();
        }

        var cacheOpt = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        };

        var serData = JsonSerializer.Serialize(data, CacheJsonOptions);
        await cache.SetStringAsync(key, serData, cacheOpt, HttpContext.RequestAborted);
        return Ok(data);
    }

    protected async Task CleanCache(string key) =>
        await cache.RemoveAsync(key, HttpContext.RequestAborted);
}