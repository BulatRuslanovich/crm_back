using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace CrmBack.Services.Impl;

public class CacheService(IDistributedCache cache) : ICacheService
{
    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var cachedData = await cache.GetStringAsync(key, ct);
        return string.IsNullOrEmpty(cachedData) ? default : JsonSerializer.Deserialize<T>(cachedData);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        var options = new DistributedCacheEntryOptions();
        if (expiration is not null) options.AbsoluteExpirationRelativeToNow = expiration;

        await cache.SetStringAsync(key, JsonSerializer.Serialize(value), options, ct);
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default) =>
        await cache.RemoveAsync(key, ct);
}
