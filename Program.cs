using CrmBack.Core.Extensions;
using CrmBack.Core.Utils.Middleware;
using CrmBack.Core.Validators;
using CrmBack.Data;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Host.UseSerilog((context, config) =>
{
    config
        .WriteTo.Console(
            theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code,
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {LogType:l} {Message:lj}{NewLine}{Exception}")
        .WriteTo.Debug()
        .MinimumLevel.Information()
        .MinimumLevel.Override("CrmBack.Data", Serilog.Events.LogEventLevel.Information)
        .MinimumLevel.Override("CrmBack.Api.Middleware", Serilog.Events.LogEventLevel.Information)
        .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Information);
});

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

// Response Compression для уменьшения размера ответов
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
});
builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});
builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

// HTTP Response Caching для кэширования ответов на уровне HTTP
builder.Services.AddResponseCaching(options =>
{
    options.MaximumBodySize = 64 * 1024 * 1024; // 64 MB
    options.UseCaseSensitivePaths = false; // Регистр пути не важен
    options.SizeLimit = 100 * 1024 * 1024; // 100 MB общий размер кэша
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSwagger", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// JWT Authentication
string jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured");
string jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured");
string jwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience is not configured");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (string.IsNullOrEmpty(context.Token))
                {
                    context.Token = context.Request.Cookies["access_token"];
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Representative", policy => policy.RequireRole("Representative"))
    .AddPolicy("Manager", policy => policy.RequireRole("Manager"))
    .AddPolicy("Director", policy => policy.RequireRole("Director"))
    .AddPolicy("Admin", policy => policy.RequireRole("Admin"))
    .AddPolicy("ManagerOrAbove", policy => policy.RequireRole("Manager", "Director", "Admin"))
    .AddPolicy("DirectorOrAbove", policy => policy.RequireRole("Director", "Admin"));

// Swagger
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "CRM API",
        Version = "v1",
        Description = "CRM API"
    });

    option.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Database
builder.Services.AddDbContext<AppDBContext>(op =>
{
    var connectionString = builder.Configuration.GetConnectionString("DbConnectionString");
    op.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        npgsqlOptions.MaxBatchSize(100);
    });
    
    op.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    op.EnableThreadSafetyChecks(false);
    
    if (builder.Environment.IsDevelopment())
    {
        op.EnableSensitiveDataLogging();
        op.EnableDetailedErrors();
        op.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
    }
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "CrmBack";
});

builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    return ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!);
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DbConnectionString")!)
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!);

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = System.Threading.RateLimiting.PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        string ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        string userAgent = context.Request.Headers.UserAgent.ToString();
        string deviceKey = $"{ipAddress}:{userAgent.GetHashCode()}";

        return System.Threading.RateLimiting.RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: deviceKey,
            factory: _ => new System.Threading.RateLimiting.SlidingWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
                QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst,
                QueueLimit = 50
            });
    });
});

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<LoginUserDtoValidator>()
                 .AddValidatorsFromAssemblyContaining<CreateUserDtoValidator>()
                 .AddValidatorsFromAssemblyContaining<UpdateUserDtoValidator>();

builder.Services.AddFluentValidationAutoValidation(config =>
{
    config.DisableDataAnnotationsValidation = true;
});
builder.Services.AddFluentValidationClientsideAdapters()
                 .AddApplicationServices();

// Build and configure middleware
var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var errorFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (errorFeature != null)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { error = "Ops, error ;)" });
            Log.Error(errorFeature.Error, "Unhandled exception in {Path}", context.Request.Path);
        }
    });
});

app.UseResponseCompression();
app.UseSerilogRequestLogging();
app.UseCors("AllowSwagger");
app.UseRateLimiter();
app.UseMiddleware<TokenRefreshMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CRM API V1");
        options.RoutePrefix = "swagger";
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseResponseCaching(); // HTTP Response Caching (после Authentication, но ответы кэшируются с учетом авторизации)
app.MapHealthChecks("/health");
app.MapControllers();

app.Run("http://localhost:5555");

public partial class Program { }
