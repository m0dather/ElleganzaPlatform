using ElleganzaPlatform.Application.ViewModels.Store;

namespace ElleganzaPlatform.Application.Services;

public interface IStoreService
{
    Task<HomeViewModel> GetHomePageDataAsync();
    Task<ShopViewModel> GetShopPageDataAsync(int page = 1, int pageSize = 12);
    Task<ProductDetailsViewModel?> GetProductDetailsAsync(int productId);
}
