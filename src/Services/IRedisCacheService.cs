namespace CrmBack.Services;

public interface IRedisCacheService
{
    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default);
    public Task RemoveAsync(string key, CancellationToken ct = default);
}
