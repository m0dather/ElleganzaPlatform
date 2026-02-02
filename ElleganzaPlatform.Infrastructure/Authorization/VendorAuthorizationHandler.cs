using ElleganzaPlatform.Application.Common;
using ElleganzaPlatform.Domain.Enums;
using ElleganzaPlatform.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

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
/// Stage 4.2: Added vendor status validation - only Approved vendors can access dashboard.
/// SuperAdmin users bypass this check (they have access to all vendor resources).
/// </summary>
public class VendorAuthorizationHandler : AuthorizationHandler<VendorRequirement>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ApplicationDbContext _context;

    public VendorAuthorizationHandler(
        ICurrentUserService currentUserService,
        ApplicationDbContext context)
    {
        _currentUserService = currentUserService;
        _context = context;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        VendorRequirement requirement)
    {
        // Check if user is authenticated
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        // SuperAdmin bypass - they can access all vendors
        if (_currentUserService.IsSuperAdmin)
        {
            context.Succeed(requirement);
            return;
        }

        // Check if user has Vendor role (using IsVendorAdmin property which checks for Vendor role)
        if (_currentUserService.IsVendorAdmin)
        {
            // Verify user has a VendorId claim
            if (_currentUserService.VendorId.HasValue)
            {
                var vendorId = _currentUserService.VendorId.Value;
                
                // Stage 4.2: Check vendor status - only approved vendors can access
                var vendor = await _context.Vendors
                    .AsNoTracking()
                    .FirstOrDefaultAsync(v => v.Id == vendorId);

                if (vendor != null && vendor.Status == VendorStatus.Approved)
                {
                    // If a specific vendor is required, validate it matches the user's vendor
                    if (requirement.RequiredVendorId == null || 
                        requirement.RequiredVendorId == vendorId)
                    {
                        context.Succeed(requirement);
                    }
                }
                // If vendor is not approved (Pending, Rejected, Suspended), deny access
            }
        }
    }
}
