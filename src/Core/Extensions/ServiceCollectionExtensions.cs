using CrmBack.Services.Impl;
using CrmBack.Services;
using CrmBack.DAO;
using CrmBack.DAO.Impl;

namespace CrmBack.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Cache Services
        services.AddScoped<ICacheService, CacheService>()
                 .AddScoped<ITagCacheService, TagCacheService>()
                 .AddScoped<ICacheInvalidService, CacheInvalidService>();

        // Cookie Service
        services.AddScoped<ICookieService, CookieService>();

        // DAO Layer
        services.AddScoped<IUserDAO, CachedUserDAO>()
                 .AddScoped<IActivDAO, ActivDAO>()
                 .AddScoped<IOrgDAO, OrgDAO>()
                 .AddScoped<IPlanDAO, PlanDAO>()
                 .AddScoped<IRefreshTokenDAO, RefreshTokenDAO>();

        // Service Layer
        services.AddScoped<IUserService, UserService>()
                 .AddScoped<IActivService, ActivService>()
                 .AddScoped<IOrgService, OrgService>()
                 .AddScoped<IPlanService, PlanService>()
                 .AddScoped<IJwtService, JwtService>();

        return services;
    }
}

