using ElleganzaPlatform.Domain.Enums;

namespace ElleganzaPlatform.Application.Common;

/// <summary>
/// Service responsible for resolving the primary role of a user from multiple roles.
/// Implements priority: SuperAdmin > StoreAdmin > Vendor > Customer
/// </summary>
public interface IRolePriorityResolver
{
    /// <summary>
    /// Determines the primary role from a collection of role names.
    /// Uses role priority: SuperAdmin > StoreAdmin > Vendor > Customer
    /// </summary>
    /// <param name="roles">Collection of role names assigned to the user</param>
    /// <returns>The primary role enum value based on priority</returns>
    PrimaryRole ResolvePrimaryRole(IEnumerable<string> roles);

    /// <summary>
    /// Gets the dashboard route for a given primary role
    /// </summary>
    /// <param name="primaryRole">The primary role</param>
    /// <returns>The dashboard URL for the role</returns>
    string GetDashboardRouteForRole(PrimaryRole primaryRole);
}
