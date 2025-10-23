namespace CrmBack.Core.Utils.Middleware;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseHealthCheck(this IApplicationBuilder builder) =>
        builder.UseMiddleware<HealthCheckMiddleware>();
}
