using System.IO.Compression;
using System.Text.Json.Serialization;
using CrmBack.Api.Filters;
using CrmBack.Api.Middleware;
using CrmBack.Application.Activities.Services;
using CrmBack.Application.Auth.Services;
using CrmBack.Application.Auth.Validators;
using CrmBack.Application.Common.Validators;
using CrmBack.Application.Organizations.Services;
using CrmBack.Application.Users.Services;
using CrmBack.Application.Users.Validators;
using CrmBack.Infrastructure.Data;
using CrmBack.Infrastructure.Persistence.Activities;
using CrmBack.Infrastructure.Persistence.Auth;
using CrmBack.Infrastructure.Persistence.Organizations;
using CrmBack.Infrastructure.Persistence.Users;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Logging Configuration
// Using Serilog for structured logging with environment-specific log levels
// Performance: Reduced log verbosity in production to minimize I/O overhead
builder.Host.UseSerilog((context, config) =>
{
    config
        .WriteTo.Console(
            theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code,
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {LogType:l} {Message:lj}{NewLine}{Exception}")
        .MinimumLevel.Information()
        // Reduce EF Core logging in production to avoid performance impact
        .MinimumLevel.Override("CrmBack.Data", context.HostingEnvironment.IsProduction() ? Serilog.Events.LogEventLevel.Warning : Serilog.Events.LogEventLevel.Information)
        // Suppress middleware logs unless warnings/errors
        .MinimumLevel.Override("CrmBack.Api.Middleware", Serilog.Events.LogEventLevel.Warning)
        // Reduce ASP.NET Core framework logs
        .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning);

    // Additional debug output in development environment
    if (!context.HostingEnvironment.IsProduction())
        config.WriteTo.Debug();
});

// Controllers Configuration
// Performance optimizations for JSON serialization:
// - IgnoreCycles: Prevents stack overflow on circular references (performance critical)
// - WhenWritingNull: Reduces payload size by omitting null values
// - WriteIndented: false in production for smaller response size
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Prevents serialization errors on circular object references
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        // Reduces JSON payload size by excluding null properties
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        // Compact JSON format for production (no indentation)
        options.JsonSerializerOptions.WriteIndented = false;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // Disable automatic model state validation filter to use custom ValidationFilter
        // This allows us to return errors in unified ApiResponse format
        options.SuppressModelStateInvalidFilter = true;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

// Validation Filter - converts validation errors to unified ApiResponse format
builder.Services.AddScoped<ValidationFilter>();

// Cookie Service - handles HTTP-only cookie operations for JWT tokens
builder.Services.AddScoped<ICookieService, CookieService>();

// Data Access Object (DAO) Layer - database persistence
// Scoped lifetime ensures one instance per HTTP request (thread-safe)
builder.Services.AddScoped<IUserDAO, UserDAO>()
            .AddScoped<IActivDAO, ActivDAO>()
            .AddScoped<IOrgDAO, OrgDAO>()
            .AddScoped<IRefreshTokenDAO, RefreshTokenDAO>();

// Service Layer - business logic abstraction
// Scoped lifetime ensures consistent state within request scope
builder.Services.AddScoped<IUserService, UserService>()
            .AddScoped<IActivService, ActivService>()
            .AddScoped<IOrgService, OrgService>()
            .AddScoped<IJwtService, JwtService>();

// Routing Configuration
// Lowercase URLs improve compatibility with various clients and HTTP proxies
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
});

// API Versioning Configuration
// Allows multiple API versions to coexist and evolve independently
builder.Services.AddApiVersioning(options =>
{
    // If version not specified, use default (v1.0)
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    // Include API version info in response headers
    options.ReportApiVersions = true;
});

// API Explorer for Versioning Support
// Configures Swagger to properly document versioned APIs
builder.Services.AddVersionedApiExplorer(options =>
{
    // Format: "v1", "v2", etc.
    options.GroupNameFormat = "'v'VVV";
    // Replace {version:apiVersion} placeholder in route templates
    options.SubstituteApiVersionInUrl = true;
});

// Response Compression Configuration
// Performance optimization: Reduces network bandwidth by compressing responses
// Brotli provides better compression ratios than Gzip, but Gzip has wider browser support
builder.Services.AddResponseCompression(options =>
{
    // Enable compression for HTTPS connections (secure)
    options.EnableForHttps = true;
    // Brotli is preferred (better compression), Gzip is fallback
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
// Fastest compression level balances compression ratio with CPU usage
builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

// Response Caching Configuration
// Performance optimization: Caches HTTP responses to reduce server load
// Cache size limits prevent memory exhaustion under high load
builder.Services.AddResponseCaching(options =>
{
    // Maximum cached response body size: 64MB
    options.MaximumBodySize = 64 * 1024 * 1024;
    // Case-insensitive path matching improves cache hit rate
    options.UseCaseSensitivePaths = false;
    // Total cache size limit: 100MB
    options.SizeLimit = 100 * 1024 * 1024;
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

// JWT Authentication Configuration
// Security: Validates JWT tokens for API authentication
// Token can be provided via Authorization header or HTTP-only cookie
string jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured");
string jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured");
string jwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience is not configured");

builder.Services
    .AddAuthentication(options =>
    {
        // Use JWT Bearer authentication scheme as default
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        // Token Validation Parameters
        // Security: Comprehensive validation ensures token integrity and authenticity
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,      // Must match configured issuer
            ValidateAudience = true,     // Must match configured audience
            ValidateLifetime = true,     // Check token expiration
            ValidateIssuerSigningKey = true, // Verify signature
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(jwtKey)),
            // ClockSkew: Allow 30s tolerance for clock differences between servers
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        // Custom token extraction: Support tokens from HTTP-only cookies
        // This allows stateless authentication without exposing tokens to JavaScript (XSS protection)
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // If token not found in Authorization header, check cookie
                if (string.IsNullOrEmpty(context.Token))
                {
                    context.Token = context.Request.Cookies["access_token"];
                }
                return Task.CompletedTask;
            }
        };
    });

// Authorization Policies
// Role-based access control (RBAC) with hierarchical permissions
// Policies can be applied to controllers/actions using [Authorize(Policy = "PolicyName")]
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Representative", policy => policy.RequireRole("Representative"))
    .AddPolicy("Manager", policy => policy.RequireRole("Manager"))
    .AddPolicy("Director", policy => policy.RequireRole("Director"))
    .AddPolicy("Admin", policy => policy.RequireRole("Admin"))
    // Composite policies for hierarchical access (e.g., Manager can access Manager+ endpoints)
    .AddPolicy("ManagerOrAbove", policy => policy.RequireRole("Manager", "Director", "Admin"))
    .AddPolicy("DirectorOrAbove", policy => policy.RequireRole("Director", "Admin"));

// Swagger
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CRM API",
        Version = "v1",
        Description = "CRM API"
    });

    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Entity Framework Core Database Configuration
// Performance optimizations for production environment
builder.Services.AddDbContext<AppDBContext>(op =>
{
    op.UseNpgsql(builder.Configuration.GetConnectionString("DbConnectionString"));

    // Production optimizations: Reduce overhead and improve performance
    if (builder.Environment.IsProduction())
    {
        // Disable sensitive data logging (security + performance)
        // Prevents logging of SQL parameters containing sensitive data
        op.EnableSensitiveDataLogging(false);
        // Disable detailed error messages (security + performance)
        // Prevents exposing internal implementation details
        op.EnableDetailedErrors(false);
        // Cache service provider (performance: reduces DI overhead)
        op.EnableServiceProviderCaching();
        // Disable thread safety checks (performance: removes synchronization overhead)
        // Safe when DbContext is scoped per request (not shared across threads)
        op.EnableThreadSafetyChecks(false);
    }
    else
    {
        // Development: Enable detailed logging for debugging
        op.EnableSensitiveDataLogging(true);
        op.EnableDetailedErrors(true);
    }
});

// Redis Distributed Cache Configuration
// Used for response caching and session storage across multiple server instances
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    // Instance name prefix for key namespacing (prevents collisions in shared Redis)
    options.InstanceName = "CrmBack:";
});

// StackExchange.Redis ConnectionMultiplexer
// Singleton pattern: Single connection pool shared across all requests (performance)
// More efficient than creating new connections per request
builder.Services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp =>
{
    var config = builder.Configuration.GetConnectionString("Redis");
    var redisConfig = StackExchange.Redis.ConfigurationOptions.Parse(config!);

    // Connection Pool Performance Optimizations
    // Timeout values balance between responsiveness and resilience
    redisConfig.SyncTimeout = 5000;           // 5s timeout for synchronous operations
    redisConfig.AsyncTimeout = 5000;          // 5s timeout for async operations
    redisConfig.ConnectTimeout = 5000;        // 5s timeout for initial connection
    // Don't abort on connect failure: allows retry logic to handle temporary issues
    redisConfig.AbortOnConnectFail = false;
    redisConfig.ConnectRetry = 3;            // Retry connection 3 times before failing
    redisConfig.DefaultDatabase = 0;          // Use Redis database 0

    return StackExchange.Redis.ConnectionMultiplexer.Connect(redisConfig);
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DbConnectionString")!)
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!);

// FluentValidation Configuration
// Automatic validation using FluentValidation validators
// Validators are discovered from assemblies containing marker types
builder.Services.AddValidatorsFromAssemblyContaining<LoginUserDtoValidator>()
                 .AddValidatorsFromAssemblyContaining<CreateUserDtoValidator>()
                 .AddValidatorsFromAssemblyContaining<UpdateUserDtoValidator>()
                 .AddValidatorsFromAssemblyContaining<PaginationDtoValidator>();

// Auto-validation: Automatically validates models before action execution
// Disables DataAnnotations validation to avoid duplicate validation
builder.Services.AddFluentValidationAutoValidation(config =>
{
    config.DisableDataAnnotationsValidation = true;
});

// Client-side adapters: Generate JavaScript validation rules for frontend
builder.Services.AddFluentValidationClientsideAdapters();

var app = builder.Build();

// Middleware Pipeline Configuration
// Order matters: Middleware executes in the order they are registered

// Exception handling must be first to catch all exceptions in the pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Response compression: Compress responses before sending (reduces bandwidth)
app.UseResponseCompression();
// Response caching: Cache responses based on headers (reduces server load)
app.UseResponseCaching();
// Request logging: Log all HTTP requests (development only for performance)
if (app.Environment.IsDevelopment())
{
    app.UseSerilogRequestLogging();
}
// CORS: Allow cross-origin requests from configured origins
app.UseCors("AllowSwagger");

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
app.MapHealthChecks("/health");
app.MapControllers();

// Server Binding Configuration
// Production: Bind to 0.0.0.0 (all interfaces) for nginx reverse proxy
// Development: Bind to localhost for local development access
var listenAddress = builder.Environment.IsProduction()
    ? "http://0.0.0.0:5555"  // Accessible from nginx container/network
    : "http://localhost:5555"; // Local development only

app.Run(listenAddress);

