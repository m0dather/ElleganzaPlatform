using ElleganzaPlatform.Application.ViewModels.Admin;
using ElleganzaPlatform.Domain.Enums;

namespace ElleganzaPlatform.Application.Services;

public interface IAdminProductService
{
    Task<ProductListViewModel> GetProductsAsync(int page = 1, int pageSize = 20);
    Task<ProductFormViewModel> GetProductFormAsync(int? productId = null);
    Task<bool> CreateProductAsync(ProductFormViewModel model);
    Task<bool> UpdateProductAsync(ProductFormViewModel model);
    
    // Stage 4.2: Product approval workflow
    Task<ProductListViewModel> GetProductsByStatusAsync(ProductStatus status, int page = 1, int pageSize = 20);
    Task<bool> ApproveProductAsync(int productId, string approvedBy);
    Task<bool> RejectProductAsync(int productId, string rejectedBy, string reason);
    Task<bool> DisableProductAsync(int productId, string disabledBy);
    Task<bool> EnableProductAsync(int productId, string enabledBy);
}
