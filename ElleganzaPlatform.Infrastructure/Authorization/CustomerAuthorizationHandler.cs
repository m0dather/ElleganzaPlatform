using ElleganzaPlatform.Application.Common;
using Microsoft.AspNetCore.Authorization;

namespace ElleganzaPlatform.Infrastructure.Authorization;

/// <summary>
/// Authorization requirement for Customer role.
/// </summary>
public class CustomerRequirement : IAuthorizationRequirement
{
}

/// <summary>
/// Handles authorization for Customer policy.
/// Validates that the user is authenticated and has the Customer role.
/// </summary>
public class CustomerAuthorizationHandler : AuthorizationHandler<CustomerRequirement>
{
    private readonly ICurrentUserService _currentUserService;

    public CustomerAuthorizationHandler(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CustomerRequirement requirement)
    {
        // Check if user is authenticated
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return Task.CompletedTask;
        }

        // Check if user has Customer role
        if (_currentUserService.IsCustomer)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
