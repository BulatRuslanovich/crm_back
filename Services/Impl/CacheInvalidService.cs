using System.Collections.Concurrent;

namespace CrmBack.Services.Impl;

public class CacheInvalidService(ITagCacheService cache) : ICacheInvalidService
{
    private readonly ConcurrentQueue<InvalidTask> queue = new();
    private readonly SemaphoreSlim semaphore = new(1, 1);
    private readonly CancellationTokenSource ctSource = new();

    public void Enqueue(string tag, CancellationToken ct = default)
    {
        queue.Enqueue(new InvalidTask(tag, ct));
        _ = Task.Run(ProcessQueue, ctSource.Token);
    }

    public void Enqueue(IEnumerable<string> tags, CancellationToken ct = default)
    {
        foreach (var tag in tags)
            queue.Enqueue(new InvalidTask(tag, ct));

        _ = Task.Run(ProcessQueue, ctSource.Token);
    }

    private async Task ProcessQueue()
    {
        await semaphore.WaitAsync();

        try
        {
            while (queue.TryDequeue(out var task))
                await cache.RemoveByTagAsync(task.Tag, task.CancellationToken);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private record InvalidTask(
        string Tag,
        CancellationToken CancellationToken
    );
}

