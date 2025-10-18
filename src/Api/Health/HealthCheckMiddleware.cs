using Npgsql;
using StackExchange.Redis;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CrmBack.Api.Health;

public class HealthCheckMiddleware(RequestDelegate next, IConfiguration configuration)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/health"))
        {
            var healthStatus = await CheckServicesHealth();

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = healthStatus.IsHealthy ? 200 : 503;

            await context.Response.WriteAsync(JsonSerializer.Serialize(healthStatus));
            return;
        }

        await next(context);
    }

    private async Task<HealthStatus> CheckServicesHealth()
    {
        Dictionary<string, ServiceHealth> services = [];

        var postgresHealth = await CheckPostgreSQL(services);
        var redisHealth = await CheckRedis(services);

        bool isHealthy = services.Values.All(s => s.IsHealthy);

        return new HealthStatus(DateTime.UtcNow, isHealthy, services);
    }

    private async Task<bool> CheckPostgreSQL(Dictionary<string, ServiceHealth> services)
    {
        try
        {
            var connectionString = configuration.GetConnectionString("DbConnectionString");
            if (string.IsNullOrEmpty(connectionString))
            {
                var error = "Connection string not configured";

                services.Add("postgresql", new ServiceHealth(
                    IsHealthy: false,
                    ResponseTime: 0,
                    Error: error,
                    Details: null));
                return false;
            }

            var stopwatch = Stopwatch.StartNew();

            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand("SELECT 1", connection);
            await command.ExecuteScalarAsync();

            stopwatch.Stop();

            services.Add("postgresql", new ServiceHealth(
                IsHealthy: true,
                ResponseTime: stopwatch.ElapsedMilliseconds,
                Error: null,
                Details: $"Connected to database"
            ));
            return true;
        }
        catch (Exception ex)
        {
            services.Add("postgresql", new ServiceHealth(
                IsHealthy: false,
                ResponseTime: 0,
                Error: ex.Message,
                Details: null
            ));
            return false;
        }
    }

    private async Task<bool> CheckRedis(Dictionary<string, ServiceHealth> services)
    {
        try
        {
            var redisConnectionString = configuration.GetConnectionString("Redis");
            if (string.IsNullOrEmpty(redisConnectionString))
            {
                var error = "Redis connection string not configured";
                services.Add("redis", new ServiceHealth(
                    IsHealthy: false,
                    ResponseTime: 0,
                    Error: error,
                    Details: null
                ));
                return false;
            }

            var stopwatch = Stopwatch.StartNew();

            using var connection = await ConnectionMultiplexer.ConnectAsync(redisConnectionString);
            var database = connection.GetDatabase();

            await database.PingAsync();

            stopwatch.Stop();
            services.Add("redis", new ServiceHealth(
                IsHealthy: true,
                ResponseTime: stopwatch.ElapsedMilliseconds,
                Error: null,
                Details: $"Connected to Redis"
            ));
            return true;
        }
        catch (Exception ex)
        {
            services.Add("redis", new ServiceHealth(
                IsHealthy: false,
                ResponseTime: 0,
                Error: ex.Message,
                Details: null
            ));
            return false;
        }

    }
}


public record HealthStatus(
    [property: JsonPropertyName("timestamp")]
    DateTime Timestamp,
    [property: JsonPropertyName("is_healthy")]
    bool IsHealthy,
    [property: JsonPropertyName("services")]
    Dictionary<string, ServiceHealth> Services
);


public record ServiceHealth(
    [property: JsonPropertyName("is_healthy")]
    bool IsHealthy,
    [property: JsonPropertyName("response_time")]
    long ResponseTime,
    [property: JsonPropertyName("error")]
    string? Error,
    [property: JsonPropertyName("details")]
    string? Details
);