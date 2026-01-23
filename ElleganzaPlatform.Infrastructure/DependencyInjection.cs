using ElleganzaPlatform.Application.Common;
using ElleganzaPlatform.Application.Services;
using ElleganzaPlatform.Domain.Interfaces;
using ElleganzaPlatform.Infrastructure.Authorization;
using ElleganzaPlatform.Infrastructure.Data;
using ElleganzaPlatform.Infrastructure.Repositories;
using ElleganzaPlatform.Infrastructure.Services;
using ElleganzaPlatform.Infrastructure.Services.Application;
using ElleganzaPlatform.Infrastructure.Services.Payment;
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
        services.AddScoped<IRolePriorityResolver, RolePriorityResolver>();
        services.AddScoped<IPostLoginRedirectService, PostLoginRedirectService>();
        services.AddScoped<IThemeContext, ThemeContext>();

        // Application Services
        services.AddScoped<IStoreService, StoreService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IAdminDashboardService, AdminDashboardService>();
        services.AddScoped<IAdminCustomerService, AdminCustomerService>();
        services.AddScoped<IAdminOrderService, AdminOrderService>();
        services.AddScoped<IAdminProductService, AdminProductService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<ICheckoutService, CheckoutService>();
        services.AddScoped<ICheckoutSessionService, CheckoutSessionService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IVendorOrderService, VendorOrderService>();
        services.AddScoped<IAuditLogService, AuditLogService>();

        // Phase 4: Payment Services
        services.AddScoped<IPaymentService, StripePaymentService>();

        // Authorization Handlers
        services.AddScoped<IAuthorizationHandler, SuperAdminAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, StoreAdminAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, VendorAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, CustomerAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, SameStoreAuthorizationHandler>();

        // Authorization Helpers (for Razor views)
        services.AddScoped<MenuAuthorizationHelper>();

        return services;
    }
}
