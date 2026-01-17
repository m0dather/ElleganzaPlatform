using ElleganzaPlatform.Application.Common;
using ElleganzaPlatform.Domain.Interfaces;
using ElleganzaPlatform.Infrastructure.Authorization;
using ElleganzaPlatform.Infrastructure.Data;
using ElleganzaPlatform.Infrastructure.Repositories;
using ElleganzaPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ElleganzaPlatform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IStoreContextService, StoreContextService>();
        services.AddScoped<IPostLoginRedirectService, PostLoginRedirectService>();

        // Authorization Handlers
        services.AddScoped<IAuthorizationHandler, SuperAdminAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, StoreAdminAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, VendorAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, CustomerAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, SameStoreAuthorizationHandler>();

        return services;
    }
}
