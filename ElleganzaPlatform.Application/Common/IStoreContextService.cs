namespace ElleganzaPlatform.Application.Common;

/// <summary>
/// Service for resolving the current store context per request
/// </summary>
public interface IStoreContextService
{
    /// <summary>
    /// Gets the current store ID based on request context (domain, path, or default store)
    /// </summary>
    Task<int?> GetCurrentStoreIdAsync();

    /// <summary>
    /// Gets the current store code
    /// </summary>
    Task<string?> GetCurrentStoreCodeAsync();

    /// <summary>
    /// Checks if a store code exists
    /// </summary>
    Task<bool> StoreExistsAsync(string storeCode);
}
