using ElleganzaPlatform.Application.Common;
using ElleganzaPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ElleganzaPlatform.Infrastructure.Services;

/// <summary>
/// Resolves the current store context based on domain, path, or default store
/// For multi-store architecture support
/// </summary>
public class StoreContextService : IStoreContextService
{
    private readonly ApplicationDbContext _context;
    private int? _cachedStoreId;
    private string? _cachedStoreCode;

    public StoreContextService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int?> GetCurrentStoreIdAsync()
    {
        if (_cachedStoreId.HasValue)
            return _cachedStoreId;

        // For now, always return the default store (demo)
        // In future, this can be extended to check domain/subdomain
        var defaultStore = await _context.Stores
            .Where(s => s.IsDefault && s.IsActive && !s.IsDeleted)
            .FirstOrDefaultAsync();

        _cachedStoreId = defaultStore?.Id;
        _cachedStoreCode = defaultStore?.Code;

        return _cachedStoreId;
    }

    public async Task<string?> GetCurrentStoreCodeAsync()
    {
        if (_cachedStoreCode != null)
            return _cachedStoreCode;

        await GetCurrentStoreIdAsync();
        return _cachedStoreCode;
    }

    public async Task<bool> StoreExistsAsync(string storeCode)
    {
        return await _context.Stores
            .AnyAsync(s => s.Code == storeCode && s.IsActive && !s.IsDeleted);
    }
}
