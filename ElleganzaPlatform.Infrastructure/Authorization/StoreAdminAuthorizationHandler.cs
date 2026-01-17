using ElleganzaPlatform.Application.Common;
using Microsoft.AspNetCore.Authorization;

namespace ElleganzaPlatform.Infrastructure.Authorization;

/// <summary>
/// Authorization requirement for StoreAdmin role.
/// Can optionally specify a required store ID for additional validation.
/// </summary>
public class StoreAdminRequirement : IAuthorizationRequirement
{
    public int? RequiredStoreId { get; set; }
}

/// <summary>
/// Handles authorization for StoreAdmin policy.
/// Validates that the user is authenticated and has the StoreAdmin role.
/// SuperAdmin users bypass this check (they have access to all stores).
/// </summary>
public class StoreAdminAuthorizationHandler : AuthorizationHandler<StoreAdminRequirement>
{
    private readonly ICurrentUserService _currentUserService;

    public StoreAdminAuthorizationHandler(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        StoreAdminRequirement requirement)
    {
        // Check if user is authenticated
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            return Task.CompletedTask;
        }

        // SuperAdmin bypass - they can access all stores
        if (_currentUserService.IsSuperAdmin)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Check if user has StoreAdmin role
        if (_currentUserService.IsStoreAdmin)
        {
            // Verify user has a StoreId claim
            if (_currentUserService.StoreId.HasValue)
            {
                // If a specific store is required, validate it matches the user's store
                if (requirement.RequiredStoreId == null || 
                    requirement.RequiredStoreId == _currentUserService.StoreId.Value)
                {
                    context.Succeed(requirement);
                }
            }
        }

        return Task.CompletedTask;
    }
}
