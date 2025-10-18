namespace CrmBack.Core.Utils.Health;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseHealthCheck(this IApplicationBuilder builder) =>
        builder.UseMiddleware<HealthCheckMiddleware>();
}
