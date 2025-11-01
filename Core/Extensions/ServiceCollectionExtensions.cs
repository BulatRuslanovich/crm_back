using CrmBack.DAO;
using CrmBack.DAO.Impl;
using CrmBack.Services;
using CrmBack.Services.Impl;

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
        // Регистрируем базовые DAO
        services.AddScoped<UserDAO>()
                 .AddScoped<ActivDAO>()
                 .AddScoped<OrgDAO>()
                 .AddScoped<PlanDAO>();
        
        // Регистрируем кэшированные DAO как декораторы
        services.AddScoped<IUserDAO>(provider =>
        {
            var userDao = provider.GetRequiredService<UserDAO>();
            var cache = provider.GetRequiredService<ITagCacheService>();
            var invalid = provider.GetRequiredService<ICacheInvalidService>();
            return new CachedUserDAO(userDao, cache, invalid);
        });
        
        services.AddScoped<IActivDAO>(provider =>
        {
            var activDao = provider.GetRequiredService<ActivDAO>();
            var cache = provider.GetRequiredService<ITagCacheService>();
            var invalid = provider.GetRequiredService<ICacheInvalidService>();
            return new CachedActivDAO(activDao, cache, invalid);
        });
        
        services.AddScoped<IOrgDAO>(provider =>
        {
            var orgDao = provider.GetRequiredService<OrgDAO>();
            var cache = provider.GetRequiredService<ITagCacheService>();
            var invalid = provider.GetRequiredService<ICacheInvalidService>();
            return new CachedOrgDAO(orgDao, cache, invalid);
        });
        
        services.AddScoped<IPlanDAO>(provider =>
        {
            var planDao = provider.GetRequiredService<PlanDAO>();
            var cache = provider.GetRequiredService<ITagCacheService>();
            var invalid = provider.GetRequiredService<ICacheInvalidService>();
            return new CachedPlanDAO(planDao, cache, invalid);
        });
        
        services.AddScoped<IRefreshTokenDAO, RefreshTokenDAO>();

        // Service Layer
        services.AddScoped<IUserService, UserService>()
                 .AddScoped<IActivService, ActivService>()
                 .AddScoped<IOrgService, OrgService>()
                 .AddScoped<IPlanService, PlanService>()
                 .AddScoped<IJwtService, JwtService>();

        return services;
    }
}

