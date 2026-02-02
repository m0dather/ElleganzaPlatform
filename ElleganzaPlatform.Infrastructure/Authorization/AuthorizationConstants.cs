namespace ElleganzaPlatform.Infrastructure.Authorization;

/// <summary>
/// Centralized policy-based authorization constants.
/// Policies MUST be used instead of role-based [Authorize(Roles = "...")] attributes.
/// </summary>
public static class AuthorizationPolicies
{
    /// <summary>
    /// Requires user to have SuperAdmin role.
    /// SuperAdmin has access to all resources across all stores.
    /// </summary>
    public const string RequireSuperAdmin = "RequireSuperAdmin";

    /// <summary>
    /// Requires user to have StoreAdmin role.
    /// StoreAdmin has access only to their assigned store resources.
    /// SuperAdmin bypass is allowed.
    /// </summary>
    public const string RequireStoreAdmin = "RequireStoreAdmin";

    /// <summary>
    /// Requires user to have Vendor role.
    /// Vendor has access only to their own vendor resources.
    /// SuperAdmin bypass is allowed.
    /// </summary>
    public const string RequireVendor = "RequireVendor";

    /// <summary>
    /// Requires user to have Customer role.
    /// </summary>
    public const string RequireCustomer = "RequireCustomer";

    /// <summary>
    /// Requires user's StoreId to match the current store context.
    /// SuperAdmin bypass is allowed.
    /// </summary>
    public const string RequireSameStore = "RequireSameStore";
}

/// <summary>
/// Role constants for ASP.NET Identity roles.
/// </summary>
public static class Roles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string StoreAdmin = "StoreAdmin";
    public const string Vendor = "Vendor";
    public const string Customer = "Customer";
}

/// <summary>
/// Custom claim type constants.
/// </summary>
public static class ClaimTypes
{
    public const string StoreId = "StoreId";
    public const string VendorId = "VendorId";
}

/// <summary>
/// Permission constants for fine-grained authorization.
/// Stage 4.2: Admin permissions for vendor and product management.
/// </summary>
public static class Permissions
{
    // Vendor Management Permissions
    public const string CanViewVendors = "CanViewVendors";
    public const string CanApproveVendors = "CanApproveVendors";
    public const string CanSuspendVendors = "CanSuspendVendors";
    
    // Product Management Permissions
    public const string CanViewProducts = "CanViewProducts";
    public const string CanApproveProducts = "CanApproveProducts";
    public const string CanDisableProducts = "CanDisableProducts";
}
