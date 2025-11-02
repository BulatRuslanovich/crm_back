using System.IO.Compression;
using System.Text.Json.Serialization;
using CrmBack.Core.Extensions;
using CrmBack.Core.Utils.Middleware;
using CrmBack.Core.Validators;
using CrmBack.Data;
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

// Logging
builder.Host.UseSerilog((context, config) =>
{
    config
        .WriteTo.Console(
            theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code,
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {LogType:l} {Message:lj}{NewLine}{Exception}")
        .MinimumLevel.Information()
        .MinimumLevel.Override("CrmBack.Data", context.HostingEnvironment.IsProduction() ? Serilog.Events.LogEventLevel.Warning : Serilog.Events.LogEventLevel.Information)
        .MinimumLevel.Override("CrmBack.Api.Middleware", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning);

    if (!context.HostingEnvironment.IsProduction())
        config.WriteTo.Debug();
});

// Services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Performance: optimize JSON serialization
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.WriteIndented = false; // Production: no indentation
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

// Routing - make URLs lowercase for better compatibility
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
});

builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
});

// API explorer for versioning support
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Response Compression for reducing response size
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.AddResponseCaching(options =>
{
    options.MaximumBodySize = 64 * 1024 * 1024;
    options.UseCaseSensitivePaths = false;
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

// JWT Authentication
string jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured");
string jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured");
string jwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience is not configured");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        options.Events = new JwtBearerEvents
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

// Database
builder.Services.AddDbContext<AppDBContext>(op =>
{
    op.UseNpgsql(builder.Configuration.GetConnectionString("DbConnectionString"));

    // Performance optimizations
    if (builder.Environment.IsProduction())
    {
        op.EnableSensitiveDataLogging(false);
        op.EnableDetailedErrors(false);
        op.EnableServiceProviderCaching();
        op.EnableThreadSafetyChecks(false);
    }
    else
    {
        op.EnableSensitiveDataLogging(true);
        op.EnableDetailedErrors(true);
    }
});

// Redis Distributed Cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "CrmBack:";
});

// StackExchange.Redis ConnectionMultiplexer for advanced Redis operations
builder.Services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp =>
{
    var config = builder.Configuration.GetConnectionString("Redis");
    var redisConfig = StackExchange.Redis.ConfigurationOptions.Parse(config!);

    // Performance optimizations for Redis connection pool
    redisConfig.SyncTimeout = 5000;
    redisConfig.AsyncTimeout = 5000;
    redisConfig.ConnectTimeout = 5000;
    redisConfig.AbortOnConnectFail = false;
    redisConfig.ConnectRetry = 3;
    redisConfig.DefaultDatabase = 0;

    return StackExchange.Redis.ConnectionMultiplexer.Connect(redisConfig);
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DbConnectionString")!)
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!);

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<LoginUserDtoValidator>()
                 .AddValidatorsFromAssemblyContaining<CreateUserDtoValidator>()
                 .AddValidatorsFromAssemblyContaining<UpdateUserDtoValidator>()
                 .AddValidatorsFromAssemblyContaining<PaginationDtoValidator>();

builder.Services.AddFluentValidationAutoValidation(config =>
{
    config.DisableDataAnnotationsValidation = true;
});

builder.Services.AddFluentValidationClientsideAdapters()
                 .AddApplicationServices();

var app = builder.Build();

app.UseResponseCompression();
app.UseResponseCaching();
if (app.Environment.IsDevelopment())
{
    app.UseSerilogRequestLogging();
}
app.UseCors("AllowSwagger");
app.UseMiddleware<TokenRefreshMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();

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

// In production with nginx, bind to internal network
var listenAddress = builder.Environment.IsProduction()
    ? "http://0.0.0.0:5555"
    : "http://localhost:5555";

app.Run(listenAddress);

