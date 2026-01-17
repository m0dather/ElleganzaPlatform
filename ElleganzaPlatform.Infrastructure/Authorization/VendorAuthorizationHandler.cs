using ElleganzaPlatform.Application.Common;
using Microsoft.AspNetCore.Authorization;

namespace ElleganzaPlatform.Infrastructure.Authorization;

/// <summary>
/// Authorization requirement for Vendor role.
/// Can optionally specify a required vendor ID for additional validation.
/// </summary>
public class VendorRequirement : IAuthorizationRequirement
{
    public int? RequiredVendorId { get; set; }
}

/// <summary>
/// Handles authorization for Vendor policy.
/// Validates that the user is authenticated and has the Vendor role.
/// SuperAdmin users bypass this check (they have access to all vendor resources).
/// </summary>
public class VendorAuthorizationHandler : AuthorizationHandler<VendorRequirement>
{
    private readonly ICurrentUserService _currentUserService;

    public VendorAuthorizationHandler(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        VendorRequirement requirement)
    {
        // Check if user is authenticated
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return Task.CompletedTask;
        }

        // SuperAdmin bypass - they can access all vendors
        if (_currentUserService.IsSuperAdmin)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Check if user has Vendor role (using IsVendorAdmin property which checks for Vendor role)
        if (_currentUserService.IsVendorAdmin)
        {
            // Verify user has a VendorId claim
            if (_currentUserService.VendorId.HasValue)
            {
                // If a specific vendor is required, validate it matches the user's vendor
                if (requirement.RequiredVendorId == null || 
                    requirement.RequiredVendorId == _currentUserService.VendorId.Value)
                {
                    context.Succeed(requirement);
                }
            }
        }

        return Task.CompletedTask;
    }
}
