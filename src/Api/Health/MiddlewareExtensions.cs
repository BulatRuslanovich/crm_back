namespace CrmBack.Api.Health;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseHealthCheck(this IApplicationBuilder builder) =>
        builder.UseMiddleware<HealthCheckMiddleware>();
}