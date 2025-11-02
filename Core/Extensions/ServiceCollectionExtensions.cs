using CrmBack.DAO;
using CrmBack.DAO.Impl;
using CrmBack.Services;
using CrmBack.Services.Impl;

namespace CrmBack.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Cookie Service
        services.AddScoped<ICookieService, CookieService>();

        // DAO Layer
        services.AddScoped<IUserDAO, UserDAO>()
                 .AddScoped<IActivDAO, ActivDAO>()
                 .AddScoped<IOrgDAO, OrgDAO>()
                 .AddScoped<IRefreshTokenDAO, RefreshTokenDAO>();

        // Service Layer
        services.AddScoped<IUserService, UserService>()
                 .AddScoped<IActivService, ActivService>()
                 .AddScoped<IOrgService, OrgService>()
                 .AddScoped<IJwtService, JwtService>();

        return services;
    }
}

