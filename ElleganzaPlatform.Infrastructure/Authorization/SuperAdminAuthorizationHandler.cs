using ElleganzaPlatform.Application.Common;
using Microsoft.AspNetCore.Authorization;

namespace ElleganzaPlatform.Infrastructure.Authorization;

/// <summary>
/// Authorization requirement for SuperAdmin role.
/// </summary>
public class SuperAdminRequirement : IAuthorizationRequirement
{
}

/// <summary>
/// Handles authorization for SuperAdmin policy.
/// Validates that the user is authenticated and has the SuperAdmin role.
/// </summary>
public class SuperAdminAuthorizationHandler : AuthorizationHandler<SuperAdminRequirement>
{
    private readonly ICurrentUserService _currentUserService;

    public SuperAdminAuthorizationHandler(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SuperAdminRequirement requirement)
    {
        // Check if user is authenticated
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            return Task.CompletedTask;
        }

        // Check if user has SuperAdmin role using ICurrentUserService
        if (_currentUserService.IsSuperAdmin)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
