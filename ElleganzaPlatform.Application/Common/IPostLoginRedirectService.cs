using ElleganzaPlatform.Domain.Entities;
using ElleganzaPlatform.Domain.Enums;

namespace ElleganzaPlatform.Application.Common;

/// <summary>
/// Centralized service for determining post-login redirect URLs based on user roles and store context.
/// Implements role priority resolution: SuperAdmin > StoreAdmin > Vendor > Customer
/// </summary>
public interface IPostLoginRedirectService
{
    /// <summary>
    /// Determines the appropriate redirect URL after successful login based on user's primary role.
    /// Priority: SuperAdmin > StoreAdmin > Vendor > Customer
    /// </summary>
    /// <param name="userId">The authenticated user ID</param>
    /// <returns>The URL to redirect to</returns>
    Task<string> GetRedirectUrlAsync(string userId);

    /// <summary>
    /// Determines the appropriate redirect URL based on ApplicationUser and their roles.
    /// This overload provides more context for advanced redirect logic.
    /// </summary>
    /// <param name="user">The authenticated ApplicationUser</param>
    /// <param name="roles">The user's roles (from UserManager.GetRolesAsync)</param>
    /// <returns>The URL to redirect to</returns>
    Task<string> GetRedirectUrlAsync(ApplicationUser user, IEnumerable<string> roles);

    /// <summary>
    /// Gets the redirect URL for a specific primary role.
    /// Useful for testing or when role is already known.
    /// </summary>
    /// <param name="primaryRole">The primary role</param>
    /// <returns>The dashboard URL for the role</returns>
    string GetRedirectUrlForRole(PrimaryRole primaryRole);
}
