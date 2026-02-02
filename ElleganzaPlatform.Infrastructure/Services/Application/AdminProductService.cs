using ElleganzaPlatform.Application.Common;
using ElleganzaPlatform.Application.Services;
using ElleganzaPlatform.Application.ViewModels.Admin;
using ElleganzaPlatform.Domain.Entities;
using ElleganzaPlatform.Domain.Enums;
using ElleganzaPlatform.Domain.Interfaces;
using ElleganzaPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ElleganzaPlatform.Infrastructure.Services.Application;

public class AdminProductService : IAdminProductService
{
    private readonly ApplicationDbContext _context;
    private readonly IRepository<Product> _productRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public AdminProductService(
        ApplicationDbContext context,
        IRepository<Product> productRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _productRepository = productRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductListViewModel> GetProductsAsync(int page = 1, int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var query = _context.Products
            .Include(p => p.Vendor);

        var totalProducts = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductListItemViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Sku = p.Sku,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                Status = p.Status,
                VendorName = p.Vendor.Name,
                MainImage = p.MainImage,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        return new ProductListViewModel
        {
            Products = products,
            CurrentPage = page,
            TotalPages = totalPages,
            TotalProducts = totalProducts
        };
    }

    public async Task<ProductFormViewModel> GetProductFormAsync(int? productId = null)
    {
        var vendors = await _context.Vendors
            .Where(v => v.IsActive)
            .Select(v => new VendorSelectViewModel
            {
                Id = v.Id,
                Name = v.Name
            })
            .ToListAsync();

        if (productId.HasValue)
        {
            var product = await _context.Products
                .Where(p => p.Id == productId.Value)
                .Select(p => new ProductFormViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    NameAr = p.NameAr,
                    Description = p.Description,
                    DescriptionAr = p.DescriptionAr,
                    Sku = p.Sku,
                    Price = p.Price,
                    CompareAtPrice = p.CompareAtPrice,
                    StockQuantity = p.StockQuantity,
                    Status = p.Status,
                    VendorId = p.VendorId,
                    MainImage = p.MainImage,
                    AvailableVendors = vendors
                })
                .FirstOrDefaultAsync();

            return product ?? new ProductFormViewModel { AvailableVendors = vendors };
        }

        return new ProductFormViewModel { AvailableVendors = vendors };
    }

    public async Task<bool> CreateProductAsync(ProductFormViewModel model)
    {
        var storeId = _currentUserService.StoreId;
        if (!storeId.HasValue)
            return false;

        var product = new Product
        {
            StoreId = storeId.Value,
            VendorId = model.VendorId,
            Name = model.Name,
            NameAr = model.NameAr,
            Description = model.Description,
            DescriptionAr = model.DescriptionAr,
            Sku = model.Sku,
            Price = model.Price,
            CompareAtPrice = model.CompareAtPrice,
            StockQuantity = model.StockQuantity,
            Status = model.Status,
            MainImage = model.MainImage
        };

        await _productRepository.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> UpdateProductAsync(ProductFormViewModel model)
    {
        var product = await _productRepository.GetByIdAsync(model.Id);
        if (product == null)
            return false;

        product.VendorId = model.VendorId;
        product.Name = model.Name;
        product.NameAr = model.NameAr;
        product.Description = model.Description;
        product.DescriptionAr = model.DescriptionAr;
        product.Sku = model.Sku;
        product.Price = model.Price;
        product.CompareAtPrice = model.CompareAtPrice;
        product.StockQuantity = model.StockQuantity;
        product.Status = model.Status;
        product.MainImage = model.MainImage;

        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync();
        
        return true;
    }

    // Stage 4.2: Product approval workflow methods
    public async Task<ProductListViewModel> GetProductsByStatusAsync(ProductStatus status, int page = 1, int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var query = _context.Products
            .Include(p => p.Vendor)
            .Where(p => p.Status == status);

        var totalProducts = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductListItemViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Sku = p.Sku,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                Status = p.Status,
                VendorName = p.Vendor.Name,
                MainImage = p.MainImage,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        return new ProductListViewModel
        {
            Products = products,
            CurrentPage = page,
            TotalPages = totalPages,
            TotalProducts = totalProducts
        };
    }

    public async Task<bool> ApproveProductAsync(int productId, string approvedBy)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
            return false;

        product.Status = ProductStatus.Active;
        product.ApprovedAt = DateTime.UtcNow;
        product.ApprovedBy = approvedBy;
        product.RejectionReason = null;
        product.RejectedAt = null;
        product.RejectedBy = null;

        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> RejectProductAsync(int productId, string rejectedBy, string reason)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
            return false;

        product.Status = ProductStatus.Rejected;
        product.RejectionReason = reason;
        product.RejectedAt = DateTime.UtcNow;
        product.RejectedBy = rejectedBy;

        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> DisableProductAsync(int productId, string disabledBy)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
            return false;

        product.Status = ProductStatus.Disabled;

        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> EnableProductAsync(int productId, string enabledBy)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
            return false;

        product.Status = ProductStatus.Active;
        product.ApprovedAt = DateTime.UtcNow;
        product.ApprovedBy = enabledBy;

        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync();
        
        return true;
    }
}
