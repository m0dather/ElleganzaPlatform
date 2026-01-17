namespace ElleganzaPlatform.Application.Common;

/// <summary>
/// Service responsible for determining post-login redirect URLs based on user roles and store context
/// </summary>
public interface IPostLoginRedirectService
{
    /// <summary>
    /// Determines the appropriate redirect URL after successful login based on user's primary role
    /// Priority: SuperAdmin > StoreAdmin > VendorAdmin > Customer
    /// </summary>
    /// <param name="userId">The authenticated user ID</param>
    /// <returns>The URL to redirect to</returns>
    Task<string> GetRedirectUrlAsync(string userId);
}
