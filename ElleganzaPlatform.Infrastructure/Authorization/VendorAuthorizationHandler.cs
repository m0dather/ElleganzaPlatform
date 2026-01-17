using Microsoft.AspNetCore.Authorization;

namespace ElleganzaPlatform.Infrastructure.Authorization;

public class VendorRequirement : IAuthorizationRequirement
{
    public int? RequiredVendorId { get; set; }
}

public class VendorAuthorizationHandler : AuthorizationHandler<VendorRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        VendorRequirement requirement)
    {
        // SuperAdmin can access all vendors
        if (context.User.IsInRole(Roles.SuperAdmin))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // VendorAdmin must have VendorId claim
        if (context.User.IsInRole(Roles.VendorAdmin))
        {
            var vendorIdClaim = context.User.FindFirst(Authorization.ClaimTypes.VendorId);
            if (vendorIdClaim != null && int.TryParse(vendorIdClaim.Value, out var userVendorId))
            {
                // If a specific vendor is required, check it matches
                if (requirement.RequiredVendorId == null || requirement.RequiredVendorId == userVendorId)
                {
                    context.Succeed(requirement);
                }
            }
        }

        return Task.CompletedTask;
    }
}
