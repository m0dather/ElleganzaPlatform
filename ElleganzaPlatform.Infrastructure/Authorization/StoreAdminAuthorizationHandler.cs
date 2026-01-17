using Microsoft.AspNetCore.Authorization;

namespace ElleganzaPlatform.Infrastructure.Authorization;

public class StoreAdminRequirement : IAuthorizationRequirement
{
    public int? RequiredStoreId { get; set; }
}

public class StoreAdminAuthorizationHandler : AuthorizationHandler<StoreAdminRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        StoreAdminRequirement requirement)
    {
        // SuperAdmin can access all stores
        if (context.User.IsInRole(Roles.SuperAdmin))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // StoreAdmin must have StoreId claim
        if (context.User.IsInRole(Roles.StoreAdmin))
        {
            var storeIdClaim = context.User.FindFirst(Authorization.ClaimTypes.StoreId);
            if (storeIdClaim != null && int.TryParse(storeIdClaim.Value, out var userStoreId))
            {
                // If a specific store is required, check it matches
                if (requirement.RequiredStoreId == null || requirement.RequiredStoreId == userStoreId)
                {
                    context.Succeed(requirement);
                }
            }
        }

        return Task.CompletedTask;
    }
}
