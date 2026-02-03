using ElleganzaPlatform.Application.Common;
using Microsoft.AspNetCore.Http;

namespace ElleganzaPlatform.Infrastructure.Services;

/// <summary>
/// Implementation of IUserUiCapabilityService that determines UI visibility based on user capabilities.
/// This service decouples UI visibility from specific roles, allowing for more flexible access control.
/// 
/// Design principle: Shopping capabilities are NOT restricted to Customers only.
/// Vendors can shop like Customers AND manage their vendor operations.
/// Admins focus on administration and do NOT have shopping UI access.
/// </summary>
public class UserUiCapabilityService : IUserUiCapabilityService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserUiCapabilityService(
        ICurrentUserService currentUserService,
        IHttpContextAccessor httpContextAccessor)
    {
        _currentUserService = currentUserService;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Determines if user is authenticated.
    /// </summary>
    private bool IsAuthenticated => 
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    /// <summary>
    /// Determines if user is an Admin (StoreAdmin or SuperAdmin).
    /// Admins do NOT have access to shopping features - they focus on administration.
    /// </summary>
    private bool IsAdmin => 
        _currentUserService.IsSuperAdmin || _currentUserService.IsStoreAdmin;

    /// <inheritdoc/>
    public bool CanAccessStore
    {
        get
        {
            // Admins should NOT access store - they use admin dashboards
            if (IsAdmin) return false;

            // Everyone else (Guest, Customer, Vendor) can access store
            return true;
        }
    }

    /// <inheritdoc/>
    public bool CanUseCart
    {
        get
        {
            // Admins should NOT see cart - they don't shop
            if (IsAdmin) return false;

            // Everyone else (Guest, Customer, Vendor) can use cart
            return true;
        }
    }

    /// <inheritdoc/>
    public bool CanUseWishlist
    {
        get
        {
            // Guest users cannot use wishlist (requires authentication)
            if (!IsAuthenticated) return false;

            // Admins should NOT see wishlist
            if (IsAdmin) return false;

            // Authenticated non-admin users (Customer, Vendor) can use wishlist
            return true;
        }
    }

    /// <inheritdoc/>
    public bool CanShowLogin
    {
        get
        {
            // Only show login for unauthenticated users
            return !IsAuthenticated;
        }
    }

    /// <inheritdoc/>
    public bool CanShowLogout
    {
        get
        {
            // Only show logout for authenticated users
            return IsAuthenticated;
        }
    }
}
