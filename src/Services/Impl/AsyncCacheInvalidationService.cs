using System.Collections.Concurrent;

namespace CrmBack.Services.Impl;

public class AsyncCacheInvalidationService(ITaggedCacheService taggedCache) : IAsyncCacheInvalidationService
{
    private readonly ConcurrentQueue<CacheInvalidationTask> _invalidationQueue = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public void EnqueueInvalidation(string tag, CancellationToken ct = default)
    {
        _invalidationQueue.Enqueue(new CacheInvalidationTask { Tag = tag, CancellationToken = ct });
        _ = Task.Run(ProcessInvalidationQueue, _cancellationTokenSource.Token);
    }

    public void EnqueueInvalidations(IEnumerable<string> tags, CancellationToken ct = default)
    {
        foreach (var tag in tags)
        {
            _invalidationQueue.Enqueue(new CacheInvalidationTask { Tag = tag, CancellationToken = ct });
        }
        _ = Task.Run(ProcessInvalidationQueue, _cancellationTokenSource.Token);
    }

    private async Task ProcessInvalidationQueue()
    {
        await _semaphore.WaitAsync();
        try
        {
            while (_invalidationQueue.TryDequeue(out var task))
            {
                try
                {
                    await taggedCache.RemoveByTagAsync(task.Tag, task.CancellationToken);
                }
                catch (Exception ex)
                {
                    // Логируем ошибку, но не прерываем обработку
                    Console.WriteLine($"Error invalidating cache for tag {task.Tag}: {ex.Message}");
                }
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _semaphore.Dispose();
    }

    private record CacheInvalidationTask
    {
        public string Tag { get; init; } = string.Empty;
        public CancellationToken CancellationToken { get; init; }
    }
}

public interface IAsyncCacheInvalidationService
{
    public void EnqueueInvalidation(string tag, CancellationToken ct = default);
    public void EnqueueInvalidations(IEnumerable<string> tags, CancellationToken ct = default);
}
