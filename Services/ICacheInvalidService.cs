namespace CrmBack.Services;

public interface ICacheInvalidService
{
    public void Enqueue(string tag, CancellationToken ct = default);
    public void Enqueue(IEnumerable<string> tags, CancellationToken ct = default);
}
