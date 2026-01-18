using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ElleganzaPlatform.Infrastructure.Authorization;

/// <summary>
/// Helper service for menu authorization in Razor views.
/// Provides clean boolean properties that check authorization policies
/// without exposing role logic directly to views.
/// 
/// Note: This helper uses synchronous authorization checks suitable for Razor views.
/// For async operations, use IAuthorizationService directly in controllers.
/// </summary>
public class MenuAuthorizationHelper
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Lazy<bool> _isAuthenticated;
    private readonly Lazy<bool> _canShowCustomerMenu;
    private readonly Lazy<bool> _canShowVendorDashboard;
    private readonly Lazy<bool> _canShowAdminDashboard;

    public MenuAuthorizationHelper(
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;

        // Lazy initialization to compute values only once when accessed
        _isAuthenticated = new Lazy<bool>(CheckIsAuthenticated);
        _canShowCustomerMenu = new Lazy<bool>(CheckCanShowCustomerMenu);
        _canShowVendorDashboard = new Lazy<bool>(CheckCanShowVendorDashboard);
        _canShowAdminDashboard = new Lazy<bool>(CheckCanShowAdminDashboard);
    }

    /// <summary>
    /// Checks if the current user is authenticated.
    /// </summary>
    public bool IsAuthenticated => _isAuthenticated.Value;

    /// <summary>
    /// Checks if Login/Register links should be shown.
    /// Returns true if user is NOT authenticated (guest user).
    /// </summary>
    public bool CanShowLogin => !IsAuthenticated;

    /// <summary>
    /// Checks if Customer menu items (Account, Orders) should be shown.
    /// Returns true if user is authenticated and authorized with RequireCustomer policy.
    /// </summary>
    public bool CanShowCustomerMenu => _canShowCustomerMenu.Value;

    /// <summary>
    /// Checks if Vendor Dashboard link should be shown.
    /// Returns true if user is authenticated and authorized with RequireVendor policy.
    /// </summary>
    public bool CanShowVendorDashboard => _canShowVendorDashboard.Value;

    /// <summary>
    /// Checks if Admin Dashboard link should be shown.
    /// Returns true if user is authenticated and authorized with RequireStoreAdmin or RequireSuperAdmin policy.
    /// </summary>
    public bool CanShowAdminDashboard => _canShowAdminDashboard.Value;

    private bool CheckIsAuthenticated()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }

    private bool CheckCanShowCustomerMenu()
    {
        if (!IsAuthenticated) return false;

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return false;

        return CheckPolicySync(httpContext.User, AuthorizationPolicies.RequireCustomer);
    }

    private bool CheckCanShowVendorDashboard()
    {
        if (!IsAuthenticated) return false;

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return false;

        return CheckPolicySync(httpContext.User, AuthorizationPolicies.RequireVendor);
    }

    private bool CheckCanShowAdminDashboard()
    {
        if (!IsAuthenticated) return false;

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return false;

        // Check if user is StoreAdmin or SuperAdmin
        return CheckPolicySync(httpContext.User, AuthorizationPolicies.RequireStoreAdmin) ||
               CheckPolicySync(httpContext.User, AuthorizationPolicies.RequireSuperAdmin);
    }

    /// <summary>
    /// Performs synchronous policy authorization check.
    /// This is safe for Razor views as the authorization handlers are designed to be synchronous.
    /// </summary>
    private bool CheckPolicySync(ClaimsPrincipal user, string policyName)
    {
        try
        {
            // Use Task.Run to avoid potential deadlocks in synchronous contexts
            var result = Task.Run(async () =>
                await _authorizationService.AuthorizeAsync(user, policyName)
            ).GetAwaiter().GetResult();

            return result.Succeeded;
        }
        catch
        {
            // If authorization check fails, deny access
            return false;
        }
    }
}
