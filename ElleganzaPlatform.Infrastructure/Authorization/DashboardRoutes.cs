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
    /// Customer account route: /account
    /// </summary>
    public const string Customer = "/account";

    /// <summary>
    /// Default fallback route for unauthenticated or no-role users
    /// </summary>
    public const string Default = "/";

    /// <summary>
    /// Login page route
    /// </summary>
    public const string Login = "/Identity/Account/Login";

    /// <summary>
    /// Access denied route
    /// </summary>
    public const string AccessDenied = "/Identity/Account/AccessDenied";
}
