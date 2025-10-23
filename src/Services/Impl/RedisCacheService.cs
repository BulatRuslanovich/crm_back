using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text.Json;

namespace CrmBack.Services.Impl;

public class RedisCacheService(IDistributedCache cache, IConnectionMultiplexer redis) : IRedisCacheService
{
    private readonly IDatabase _database = redis.GetDatabase();

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var cachedData = await cache.GetStringAsync(key, ct);
        return string.IsNullOrEmpty(cachedData) ? default : JsonSerializer.Deserialize<T>(cachedData);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        var options = new DistributedCacheEntryOptions();
        if (expiration.HasValue)
            options.AbsoluteExpirationRelativeToNow = expiration;

        await cache.SetStringAsync(key, JsonSerializer.Serialize(value), options, ct);
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        await cache.RemoveAsync(key, ct);
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken ct = default)
    {
        var keys = await GetKeysByPatternAsync(pattern, ct);
        if (keys.Count > 0)
        {
            await _database.KeyDeleteAsync([.. keys.Select(k => (RedisKey)k)]);
        }
    }

    public async Task<List<string>> GetKeysByPatternAsync(string pattern, CancellationToken ct = default)
    {
        var keys = new List<string>();
        var server = redis.GetServer(redis.GetEndPoints().First());
        
        await foreach (var key in server.KeysAsync(pattern: pattern))
        {
            keys.Add(key.ToString());
        }

        return keys;
    }
}
