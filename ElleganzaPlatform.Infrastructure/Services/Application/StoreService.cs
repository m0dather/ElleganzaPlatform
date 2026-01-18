using ElleganzaPlatform.Application.Common;
using ElleganzaPlatform.Application.Services;
using ElleganzaPlatform.Application.ViewModels.Store;
using ElleganzaPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ElleganzaPlatform.Infrastructure.Services.Application;

public class StoreService : IStoreService
{
    private readonly ApplicationDbContext _context;
    private readonly IStoreContextService _storeContext;

    public StoreService(
        ApplicationDbContext context,
        IStoreContextService storeContext)
    {
        _context = context;
        _storeContext = storeContext;
    }

    public async Task<HomeViewModel> GetHomePageDataAsync()
    {
        var storeId = await _storeContext.GetCurrentStoreIdAsync();

        // Get featured products (limit to 8)
        var featuredProducts = await _context.Products
            .Include(p => p.Vendor)
            .Where(p => p.Status == Domain.Enums.ProductStatus.Active)
            .OrderByDescending(p => p.CreatedAt)
            .Take(8)
            .Select(p => new ProductCardViewModel
            {
                Id = p.Id,
                Name = p.Name,
                MainImage = p.MainImage,
                Price = p.Price,
                CompareAtPrice = p.CompareAtPrice,
                VendorName = p.Vendor.Name,
                IsInStock = p.StockQuantity > 0
            })
            .ToListAsync();

        // Get vendors as brands (limit to 10)
        var brands = await _context.Vendors
            .Where(v => v.IsActive)
            .Select(v => new BrandViewModel
            {
                Id = v.Id,
                Name = v.Name,
                Logo = v.Logo,
                ProductCount = v.Products.Count(p => p.Status == Domain.Enums.ProductStatus.Active)
            })
            .Take(10)
            .ToListAsync();

        return new HomeViewModel
        {
            FeaturedProducts = featuredProducts,
            Brands = brands,
            Categories = new List<CategoryViewModel>() // Placeholder for future category implementation
        };
    }

    public async Task<ShopViewModel> GetShopPageDataAsync(int page = 1, int pageSize = 12)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 12;

        var query = _context.Products
            .Include(p => p.Vendor)
            .Where(p => p.Status == Domain.Enums.ProductStatus.Active);

        var totalProducts = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductCardViewModel
            {
                Id = p.Id,
                Name = p.Name,
                MainImage = p.MainImage,
                Price = p.Price,
                CompareAtPrice = p.CompareAtPrice,
                VendorName = p.Vendor.Name,
                IsInStock = p.StockQuantity > 0
            })
            .ToListAsync();

        return new ShopViewModel
        {
            Products = products,
            CurrentPage = page,
            TotalPages = totalPages,
            TotalProducts = totalProducts,
            PageSize = pageSize
        };
    }

    public async Task<ProductDetailsViewModel?> GetProductDetailsAsync(int productId)
    {
        var product = await _context.Products
            .Include(p => p.Vendor)
            .Where(p => p.Id == productId && p.Status == Domain.Enums.ProductStatus.Active)
            .Select(p => new ProductDetailsViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Sku = p.Sku,
                Price = p.Price,
                CompareAtPrice = p.CompareAtPrice,
                StockQuantity = p.StockQuantity,
                MainImage = p.MainImage,
                Vendor = new VendorInfoViewModel
                {
                    Id = p.Vendor.Id,
                    Name = p.Vendor.Name,
                    Logo = p.Vendor.Logo
                }
            })
            .FirstOrDefaultAsync();

        return product;
    }
}
