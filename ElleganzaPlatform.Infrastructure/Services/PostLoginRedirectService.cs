using ElleganzaPlatform.Application.Common;
using ElleganzaPlatform.Domain.Entities;
using ElleganzaPlatform.Infrastructure.Authorization;
using Microsoft.AspNetCore.Identity;

namespace ElleganzaPlatform.Infrastructure.Services;

/// <summary>
/// Centralized service for determining post-login redirect URLs based on user roles
/// Implements role priority: SuperAdmin > StoreAdmin > VendorAdmin > Customer
/// </summary>
public class PostLoginRedirectService : IPostLoginRedirectService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public PostLoginRedirectService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<string> GetRedirectUrlAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return "/";

        var roles = await _userManager.GetRolesAsync(user);

        // Role priority: SuperAdmin > StoreAdmin > VendorAdmin > Customer
        // SuperAdmin → /super-admin
        if (roles.Contains(Roles.SuperAdmin))
            return "/super-admin";

        // StoreAdmin → /admin
        if (roles.Contains(Roles.StoreAdmin))
            return "/admin";

        // VendorAdmin → /vendor
        if (roles.Contains(Roles.VendorAdmin))
            return "/vendor";

        // Customer → /account
        if (roles.Contains(Roles.Customer))
            return "/account";

        // Default fallback to storefront
        return "/";
    }
}
