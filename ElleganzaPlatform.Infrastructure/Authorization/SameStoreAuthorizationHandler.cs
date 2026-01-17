using ElleganzaPlatform.Application.Common;
using Microsoft.AspNetCore.Authorization;

namespace ElleganzaPlatform.Infrastructure.Authorization;

/// <summary>
/// Authorization requirement for same store access.
/// Ensures the user belongs to the same store as the current store context.
/// </summary>
public class SameStoreRequirement : IAuthorizationRequirement
{
}

/// <summary>
/// Handles authorization for RequireSameStore policy.
/// Validates that the user's StoreId matches the current store context.
/// SuperAdmin users bypass this check (they have access to all stores).
/// </summary>
public class SameStoreAuthorizationHandler : AuthorizationHandler<SameStoreRequirement>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IStoreContextService _storeContextService;

    public SameStoreAuthorizationHandler(
        ICurrentUserService currentUserService,
        IStoreContextService storeContextService)
    {
        _currentUserService = currentUserService;
        _storeContextService = storeContextService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SameStoreRequirement requirement)
    {
        // Check if user is authenticated
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        // SuperAdmin bypass - they can access all stores
        if (_currentUserService.IsSuperAdmin)
        {
            context.Succeed(requirement);
            return;
        }

        // Get user's StoreId and current store context StoreId
        var userStoreId = _currentUserService.StoreId;
        var contextStoreId = await _storeContextService.GetCurrentStoreIdAsync();

        // Both must exist and match
        if (userStoreId.HasValue && contextStoreId.HasValue &&
            userStoreId.Value == contextStoreId.Value)
        {
            context.Succeed(requirement);
        }
    }
}
