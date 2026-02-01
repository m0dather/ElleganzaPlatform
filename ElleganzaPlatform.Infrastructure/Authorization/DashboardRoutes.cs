namespace ElleganzaPlatform.Infrastructure.Authorization;

/// <summary>
/// Centralized dashboard route definitions for role-based redirects.
/// These routes MUST match the actual controller routes in the application.
/// </summary>
public static class DashboardRoutes
{
    /// <summary>
    /// SuperAdmin dashboard route: /super-admin
    /// </summary>
    public const string SuperAdmin = "/super-admin";

    /// <summary>
    /// StoreAdmin dashboard route: /admin
    /// </summary>
    public const string StoreAdmin = "/admin";

    /// <summary>
    /// Vendor dashboard route: /vendor
    /// </summary>
    public const string Vendor = "/vendor";

    /// <summary>
    /// Vendor pending approval route: /vendor/pending
    /// Stage 4.1: Added for vendors awaiting admin approval
    /// </summary>
    public const string VendorPending = "/vendor/pending";

    /// <summary>
    /// Customer post-login redirect: / (storefront home)
    /// Note: This is the landing page after login, NOT the account area route.
    /// Customers are shoppers, not admin users - they live in the storefront.
    /// Account pages are accessed via /account/* routes but login redirects here.
    /// </summary>
    public const string Customer = "/";

    /// <summary>
    /// Default fallback route for unauthenticated or no-role users
    /// </summary>
    public const string Default = "/";

    /// <summary>
    /// Login page route
    /// </summary>
    public const string Login = "/login";

    /// <summary>
    /// Access denied route
    /// </summary>
    public const string AccessDenied = "/access-denied";
}
