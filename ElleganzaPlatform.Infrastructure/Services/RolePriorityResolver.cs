using ElleganzaPlatform.Application.Common;
using ElleganzaPlatform.Domain.Enums;
using ElleganzaPlatform.Infrastructure.Authorization;

namespace ElleganzaPlatform.Infrastructure.Services;

/// <summary>
/// Implementation of role priority resolution for determining primary user role.
/// Priority order: SuperAdmin > StoreAdmin > Vendor > Customer
/// </summary>
public class RolePriorityResolver : IRolePriorityResolver
{
    /// <summary>
    /// Resolves the primary role from a collection of role names based on priority.
    /// If user has multiple roles, the highest priority role is selected.
    /// </summary>
    /// <param name="roles">Collection of role names (e.g., from UserManager.GetRolesAsync)</param>
    /// <returns>PrimaryRole enum representing the highest priority role, or None if no roles</returns>
    public PrimaryRole ResolvePrimaryRole(IEnumerable<string> roles)
    {
        if (roles == null || !roles.Any())
        {
            return PrimaryRole.None;
        }

        // Check roles in priority order (highest to lowest)
        // Priority: SuperAdmin > StoreAdmin > Vendor > Customer

        if (roles.Contains(Roles.SuperAdmin, StringComparer.OrdinalIgnoreCase))
        {
            return PrimaryRole.SuperAdmin;
        }

        if (roles.Contains(Roles.StoreAdmin, StringComparer.OrdinalIgnoreCase))
        {
            return PrimaryRole.StoreAdmin;
        }

        if (roles.Contains(Roles.Vendor, StringComparer.OrdinalIgnoreCase))
        {
            return PrimaryRole.Vendor;
        }

        if (roles.Contains(Roles.Customer, StringComparer.OrdinalIgnoreCase))
        {
            return PrimaryRole.Customer;
        }

        // If user has roles but none match our system roles
        return PrimaryRole.None;
    }

    /// <summary>
    /// Maps a PrimaryRole to its corresponding dashboard route.
    /// Routes are centralized in DashboardRoutes constants.
    /// </summary>
    /// <param name="primaryRole">The primary role to map</param>
    /// <returns>The dashboard URL string</returns>
    public string GetDashboardRouteForRole(PrimaryRole primaryRole)
    {
        return primaryRole switch
        {
            PrimaryRole.SuperAdmin => DashboardRoutes.SuperAdmin,
            PrimaryRole.StoreAdmin => DashboardRoutes.StoreAdmin,
            PrimaryRole.Vendor => DashboardRoutes.Vendor,
            PrimaryRole.Customer => DashboardRoutes.Customer,
            PrimaryRole.None => DashboardRoutes.Default,
            _ => DashboardRoutes.Default
        };
    }
}
