namespace CrmBack.Services.Impl;

public class TagCacheService(ICacheService cache) : ITagCacheService
{
    private const string TagPrefix = "tag:";

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        return await cache.GetAsync<T>(key, ct);
    }

    public async Task SetAsync<T>(string key, T value, IEnumerable<string> tags, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        await cache.SetAsync(key, value, expiration, ct);

        foreach (var tag in tags)
        {
            var tagKey = $"{TagPrefix}{tag}";
            var taggedKeys = await cache.GetAsync<HashSet<string>>(tagKey, ct) ?? [];
            taggedKeys.Add(key);
            await cache.SetAsync(tagKey, taggedKeys, expiration, ct);
        }
    }

    public async Task RemoveByTagAsync(string tag, CancellationToken ct = default)
    {
        var tagKey = $"{TagPrefix}{tag}";
        var taggedKeys = await cache.GetAsync<HashSet<string>>(tagKey, ct);

        if (taggedKeys is { Count: > 0 })
        {
            foreach (var key in taggedKeys) await cache.RemoveAsync(key, ct);
            await cache.RemoveAsync(tagKey, ct);
        }
    }

    public async Task RemoveByTagsAsync(IEnumerable<string> tags, CancellationToken ct = default)
    {
        foreach (var tag in tags) await RemoveByTagAsync(tag, ct);
    }
}
