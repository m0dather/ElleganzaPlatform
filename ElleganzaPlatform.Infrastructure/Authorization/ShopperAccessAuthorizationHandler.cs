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
/// Validates that the user has shopping capabilities (Customer or Vendor role).
/// Delegates capability determination to IUserUiCapabilityService for consistency.
/// </summary>
public class ShopperAccessAuthorizationHandler : AuthorizationHandler<ShopperAccessRequirement>
{
    private readonly IUserUiCapabilityService _uiCapabilityService;

    public ShopperAccessAuthorizationHandler(IUserUiCapabilityService uiCapabilityService)
    {
        _uiCapabilityService = uiCapabilityService;
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

        // Use capability service to determine shopper access
        // This ensures single source of truth for shopping access logic
        if (_uiCapabilityService.CanUseWishlist)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
