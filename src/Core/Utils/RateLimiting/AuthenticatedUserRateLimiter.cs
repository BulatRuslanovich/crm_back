using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace CrmBack.Core.Utils.RateLimiting;

public class AuthenticatedUserRateLimiter : IRateLimiterPolicy<string>
{
    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected { get; } = (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        return ValueTask.CompletedTask;
    };

    public RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        // Если пользователь аутентифицирован, используем его ID
        var userId = httpContext.User?.Identity?.Name;
        if (!string.IsNullOrEmpty(userId))
        {
            return RateLimitPartition.GetSlidingWindowLimiter(
                partitionKey: $"user:{userId}",
                factory: _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 200, // 200 запросов в минуту на пользователя
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 6,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 20
                });
        }

        // Если не аутентифицирован, используем IP + User-Agent
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = httpContext.Request.Headers.UserAgent.ToString();
        var deviceKey = $"device:{ipAddress}:{userAgent.GetHashCode()}";
        
        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: deviceKey,
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 100, // 100 запросов в минуту на устройство
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 10
            });
    }
}
