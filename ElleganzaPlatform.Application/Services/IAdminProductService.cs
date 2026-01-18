using ElleganzaPlatform.Application.ViewModels.Admin;

namespace ElleganzaPlatform.Application.Services;

public interface IAdminProductService
{
    Task<ProductListViewModel> GetProductsAsync(int page = 1, int pageSize = 20);
    Task<ProductFormViewModel> GetProductFormAsync(int? productId = null);
    Task<bool> CreateProductAsync(ProductFormViewModel model);
    Task<bool> UpdateProductAsync(ProductFormViewModel model);
}
