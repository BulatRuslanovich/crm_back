namespace CrmBack.Services;

public interface ITagCacheService
{
    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    public Task SetAsync<T>(string key, T value, IEnumerable<string> tags, TimeSpan? expiration = null, CancellationToken ct = default);
    public Task RemoveByTagAsync(string tag, CancellationToken ct = default);
}
