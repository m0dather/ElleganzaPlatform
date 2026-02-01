using ElleganzaPlatform.Application.Common;
using ElleganzaPlatform.Domain.Entities;
using ElleganzaPlatform.Domain.Enums;
using ElleganzaPlatform.Infrastructure.Authorization;
using ElleganzaPlatform.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ElleganzaPlatform.Infrastructure.Services;

/// <summary>
/// Centralized service for determining post-login redirect URLs based on user roles and store context.
/// Implements role priority resolution: SuperAdmin > StoreAdmin > Vendor > Customer
/// </summary>
public class PostLoginRedirectService : IPostLoginRedirectService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRolePriorityResolver _rolePriorityResolver;
    private readonly IStoreContextService _storeContextService;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<PostLoginRedirectService> _logger;

    public PostLoginRedirectService(
        UserManager<ApplicationUser> userManager,
        IRolePriorityResolver rolePriorityResolver,
        IStoreContextService storeContextService,
        ApplicationDbContext dbContext,
        ILogger<PostLoginRedirectService> logger)
    {
        _userManager = userManager;
        _rolePriorityResolver = rolePriorityResolver;
        _storeContextService = storeContextService;
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Determines redirect URL by userId. Loads user and roles, then delegates to overload.
    /// </summary>
    /// <param name="userId">The authenticated user ID</param>
    /// <returns>The redirect URL based on user's primary role</returns>
    public async Task<string> GetRedirectUrlAsync(string userId)
    {
        // Find user
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found during redirect resolution", userId);
            return DashboardRoutes.Default;
        }

        // Get user roles
        var roles = await _userManager.GetRolesAsync(user);

        // Delegate to overload with full context
        return await GetRedirectUrlAsync(user, roles);
    }

    /// <summary>
    /// Determines redirect URL with full user and role context.
    /// Uses RolePriorityResolver to determine primary role, then maps to dashboard route.
    /// Handles edge cases: inactive users, no roles, store context validation.
    /// </summary>
    /// <param name="user">The authenticated ApplicationUser</param>
    /// <param name="roles">The user's roles collection</param>
    /// <returns>The redirect URL based on user's primary role</returns>
    public async Task<string> GetRedirectUrlAsync(ApplicationUser user, IEnumerable<string> roles)
    {
        // Edge Case: User not active
        if (!user.IsActive)
        {
            _logger.LogWarning("Inactive user {UserId} attempted to login", user.Id);
            return DashboardRoutes.Login;
        }

        // Resolve primary role using RolePriorityResolver
        var primaryRole = _rolePriorityResolver.ResolvePrimaryRole(roles);

        // Edge Case: User authenticated but has no recognized roles
        if (primaryRole == PrimaryRole.None)
        {
            _logger.LogWarning("User {UserId} has no recognized roles, denying access", user.Id);
            return DashboardRoutes.AccessDenied;
        }

        // SuperAdmin bypass: No store validation needed
        if (primaryRole == PrimaryRole.SuperAdmin)
        {
            _logger.LogInformation("SuperAdmin {UserId} redirected to {Route}", user.Id, DashboardRoutes.SuperAdmin);
            return DashboardRoutes.SuperAdmin;
        }

        // For StoreAdmin and Vendor: Validate store context
        if (primaryRole == PrimaryRole.StoreAdmin || primaryRole == PrimaryRole.Vendor)
        {
            var currentStoreId = await _storeContextService.GetCurrentStoreIdAsync();
            if (!currentStoreId.HasValue)
            {
                _logger.LogWarning("User {UserId} with role {Role} has no valid store context", user.Id, primaryRole);
                return DashboardRoutes.AccessDenied;
            }
        }

        // For Vendor: Check if vendor is approved (IsActive)
        if (primaryRole == PrimaryRole.Vendor)
        {
            // Get vendor admin association
            var vendorAdmin = await _dbContext.VendorAdmins
                .Include(va => va.Vendor)
                .FirstOrDefaultAsync(va => va.UserId == user.Id && va.IsActive);

            if (vendorAdmin == null)
            {
                _logger.LogWarning("User {UserId} has no active vendor association", user.Id);
                return DashboardRoutes.AccessDenied;
            }

            if (!vendorAdmin.Vendor.IsActive)
            {
                _logger.LogWarning("User {UserId} belongs to inactive vendor {VendorId}, redirecting to pending approval page", 
                    user.Id, vendorAdmin.VendorId);
                // Stage 4.1: Redirect to pending approval page instead of access denied
                return DashboardRoutes.VendorPending;
            }
        }

        // Get dashboard route from resolver
        var redirectUrl = _rolePriorityResolver.GetDashboardRouteForRole(primaryRole);

        _logger.LogInformation("User {UserId} with primary role {PrimaryRole} redirected to {Route}", 
            user.Id, primaryRole, redirectUrl);

        return redirectUrl;
    }

    /// <summary>
    /// Gets the redirect URL for a specific primary role.
    /// Direct delegation to RolePriorityResolver for route mapping.
    /// </summary>
    /// <param name="primaryRole">The primary role</param>
    /// <returns>The dashboard URL for the role</returns>
    public string GetRedirectUrlForRole(PrimaryRole primaryRole)
    {
        return _rolePriorityResolver.GetDashboardRouteForRole(primaryRole);
    }
}

