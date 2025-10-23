using CrmBack.Core.Utils.Health;
using CrmBack.Core.Utils.Middleware;
using CrmBack.DAO;
using CrmBack.DAO.Impl;
using CrmBack.Data;
using CrmBack.Services;
using CrmBack.Services.Impl;
using Microsoft.EntityFrameworkCore;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

ConfigureLogging(builder);

builder.Services.AddMemoryCache();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

ConfigureCors(builder.Services);
ConfigureAuthentication(builder);
ConfigureSwagger(builder.Services);
ConfigureDatabase(builder.Services, builder.Configuration);
ConfigureApplicationServices(builder.Services);

var app = builder.Build();


ConfigureMiddleware(app);

app.Run("http://localhost:5555");

static void ConfigureLogging(WebApplicationBuilder builder)
{
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
}

static void ConfigureCors(IServiceCollection services)
{
    services.AddCors(options =>
    {
        options.AddPolicy("AllowSwagger", policy =>
        {
            policy.WithOrigins("http://localhost:3000")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    });
}

static void ConfigureAuthentication(WebApplicationBuilder builder)
{
    var jwtKey = builder.Configuration["Jwt:Key"]
        ?? throw new InvalidOperationException("JWT Key is not configured");
    var jwtIssuer = builder.Configuration["Jwt:Issuer"]
        ?? throw new InvalidOperationException("JWT Issuer is not configured");
    var jwtAudience = builder.Configuration["Jwt:Audience"]
        ?? throw new InvalidOperationException("JWT Audience is not configured");

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

     builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("Representative", policy => policy.RequireRole("Representative"));
        options.AddPolicy("Manager", policy => policy.RequireRole("Manager"));
        options.AddPolicy("Director", policy => policy.RequireRole("Director"));
        options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));

        options.AddPolicy("ManagerOrAbove", policy =>
            policy.RequireRole("Manager", "Director", "Admin"));
        options.AddPolicy("DirectorOrAbove", policy =>
            policy.RequireRole("Director", "Admin"));
    });
}

static void ConfigureSwagger(IServiceCollection services)
{
    services.AddSwaggerGen(option =>
    {
        option.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "CRM API",
            Version = "v1",
            Description = "CRM API for managing users, organizations, and activities"
        });

        option.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Enter JWT token in format: Bearer {token}",
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
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
}

static void ConfigureDatabase(IServiceCollection services, IConfiguration configuration)
{
    services.AddDbContext<AppDBContext>(op =>
    {
        op.UseNpgsql(configuration.GetConnectionString("DbConnectionString"));

        op.EnableSensitiveDataLogging();
        op.EnableDetailedErrors();
    });

    services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = configuration.GetConnectionString("Redis");
        options.InstanceName = "CrmBack";
    });

    services.AddSingleton<IConnectionMultiplexer>(provider =>
    {
        var configuration = provider.GetRequiredService<IConfiguration>();
        return ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!);
    });
}

static void ConfigureApplicationServices(IServiceCollection services)
{
    services.AddHttpContextAccessor();

    services.AddScoped<IRedisCacheService, RedisCacheService>();
    services.AddScoped<ITaggedCacheService, TaggedCacheService>();
    services.AddScoped<IAsyncCacheInvalidationService, AsyncCacheInvalidationService>();

    services.AddScoped<ICookieService, CookieService>();

    services.AddScoped<IUserDAO, CachedUserDAO>();
    services.AddScoped<IActivDAO, ActivDAO>();
    services.AddScoped<IOrgDAO, OrgDAO>();
    services.AddScoped<IPlanDAO, PlanDAO>();
    services.AddScoped<IRefreshTokenDAO, RefreshTokenDAO>();

    services.AddScoped<IUserService, UserService>();
    services.AddScoped<IActivService, ActivService>();
    services.AddScoped<IOrgService, OrgService>();
    services.AddScoped<IPlanService, PlanService>();

    services.AddScoped<IJwtService, JwtService>();
}

static void ConfigureMiddleware(WebApplication app)
{
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

    app.UseSerilogRequestLogging();
    app.UseCors("AllowSwagger");

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
    app.UseHealthCheck();
    app.MapControllers();
}
