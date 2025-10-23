namespace CrmBack.Services;

public interface IAsyncCacheInvalidationService
{
    public void EnqueueInvalidation(string tag, CancellationToken ct = default);
    public void EnqueueInvalidations(IEnumerable<string> tags, CancellationToken ct = default);
}
