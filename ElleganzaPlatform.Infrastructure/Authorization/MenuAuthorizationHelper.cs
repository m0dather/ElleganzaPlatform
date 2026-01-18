using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace ElleganzaPlatform.Infrastructure.Authorization;

/// <summary>
/// Helper service for menu authorization in Razor views.
/// Provides clean boolean properties that check authorization policies
/// without exposing role logic directly to views.
/// </summary>
public class MenuAuthorizationHelper
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MenuAuthorizationHelper(
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Checks if the current user is authenticated.
    /// </summary>
    public bool IsAuthenticated => 
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    /// <summary>
    /// Checks if Login/Register links should be shown.
    /// Returns true if user is NOT authenticated (guest user).
    /// </summary>
    public bool CanShowLogin => !IsAuthenticated;

    /// <summary>
    /// Checks if Customer menu items (Account, Orders) should be shown.
    /// Returns true if user is authenticated and authorized with RequireCustomer policy.
    /// </summary>
    public bool CanShowCustomerMenu
    {
        get
        {
            if (!IsAuthenticated) return false;

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return false;

            var result = _authorizationService
                .AuthorizeAsync(httpContext.User, AuthorizationPolicies.RequireCustomer)
                .GetAwaiter()
                .GetResult();

            return result.Succeeded;
        }
    }

    /// <summary>
    /// Checks if Vendor Dashboard link should be shown.
    /// Returns true if user is authenticated and authorized with RequireVendor policy.
    /// </summary>
    public bool CanShowVendorDashboard
    {
        get
        {
            if (!IsAuthenticated) return false;

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return false;

            var result = _authorizationService
                .AuthorizeAsync(httpContext.User, AuthorizationPolicies.RequireVendor)
                .GetAwaiter()
                .GetResult();

            return result.Succeeded;
        }
    }

    /// <summary>
    /// Checks if Admin Dashboard link should be shown.
    /// Returns true if user is authenticated and authorized with RequireStoreAdmin or RequireSuperAdmin policy.
    /// </summary>
    public bool CanShowAdminDashboard
    {
        get
        {
            if (!IsAuthenticated) return false;

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return false;

            // Check if user is StoreAdmin
            var storeAdminResult = _authorizationService
                .AuthorizeAsync(httpContext.User, AuthorizationPolicies.RequireStoreAdmin)
                .GetAwaiter()
                .GetResult();

            if (storeAdminResult.Succeeded) return true;

            // Check if user is SuperAdmin
            var superAdminResult = _authorizationService
                .AuthorizeAsync(httpContext.User, AuthorizationPolicies.RequireSuperAdmin)
                .GetAwaiter()
                .GetResult();

            return superAdminResult.Succeeded;
        }
    }
}
