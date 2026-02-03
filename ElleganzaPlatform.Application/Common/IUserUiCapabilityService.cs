namespace ElleganzaPlatform.Application.Common;

/// <summary>
/// Service for determining UI capability visibility based on user type.
/// This service provides capability-based checks instead of role-based checks,
/// allowing for more flexible and maintainable UI logic.
/// 
/// Key principle: Capabilities are additive, not exclusive.
/// A Vendor can shop like a Customer, but also access Vendor features.
/// </summary>
public interface IUserUiCapabilityService
{
    /// <summary>
    /// Determines if the current user can browse and shop in the store.
    /// Returns true for: Guest, Customer, Vendor
    /// Returns false for: Admin, SuperAdmin
    /// </summary>
    bool CanAccessStore { get; }

    /// <summary>
    /// Determines if the current user can use the shopping cart.
    /// Returns true for: Guest, Customer, Vendor
    /// Returns false for: Admin, SuperAdmin
    /// </summary>
    bool CanUseCart { get; }

    /// <summary>
    /// Determines if the current user can use the wishlist.
    /// Returns true for: Customer, Vendor
    /// Returns false for: Guest, Admin, SuperAdmin
    /// </summary>
    bool CanUseWishlist { get; }

    /// <summary>
    /// Determines if the current user should see the Login link.
    /// Returns true only for: Guest (unauthenticated users)
    /// Returns false for: Any authenticated user
    /// </summary>
    bool CanShowLogin { get; }

    /// <summary>
    /// Determines if the current user should see the Logout link.
    /// Returns true for: Any authenticated user
    /// Returns false for: Guest
    /// </summary>
    bool CanShowLogout { get; }
}
