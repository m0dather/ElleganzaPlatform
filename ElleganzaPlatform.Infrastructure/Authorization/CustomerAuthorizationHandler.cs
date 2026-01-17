using Microsoft.AspNetCore.Authorization;

namespace ElleganzaPlatform.Infrastructure.Authorization;

public class CustomerRequirement : IAuthorizationRequirement
{
}

public class CustomerAuthorizationHandler : AuthorizationHandler<CustomerRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CustomerRequirement requirement)
    {
        if (context.User.IsInRole(Roles.Customer) || 
            context.User.Identity?.IsAuthenticated == true)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
