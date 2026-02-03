using ElleganzaPlatform.Application.Common;
using Microsoft.AspNetCore.Authorization;

namespace ElleganzaPlatform.Infrastructure.Authorization;

/// <summary>
/// Authorization requirement for shopping access (Customer or Vendor).
/// This allows both Customers and Vendors to access shopping features.
/// </summary>
public class ShopperAccessRequirement : IAuthorizationRequirement
{
}

/// <summary>
/// Handles authorization for ShopperAccess policy.
/// Validates that the user is authenticated and has either Customer or Vendor role.
/// Admins are explicitly excluded as they don't have shopping privileges.
/// </summary>
public class ShopperAccessAuthorizationHandler : AuthorizationHandler<ShopperAccessRequirement>
{
    private readonly ICurrentUserService _currentUserService;

    public ShopperAccessAuthorizationHandler(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ShopperAccessRequirement requirement)
    {
        // Check if user is authenticated
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return Task.CompletedTask;
        }

        // Allow if user is Customer or Vendor
        // Admins are explicitly excluded - they don't shop
        if (_currentUserService.IsCustomer || _currentUserService.IsVendorAdmin)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
