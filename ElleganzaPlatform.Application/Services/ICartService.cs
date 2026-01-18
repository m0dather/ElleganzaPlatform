using ElleganzaPlatform.Application.ViewModels.Store;

namespace ElleganzaPlatform.Application.Services;

public interface ICartService
{
    Task<CartViewModel> GetCartAsync();
    Task<bool> AddToCartAsync(int productId, int quantity = 1);
    Task<bool> UpdateCartItemAsync(int productId, int quantity);
    Task<bool> RemoveFromCartAsync(int productId);
    Task ClearCartAsync();
    Task MergeGuestCartAsync();
    Task<int> GetCartItemCountAsync();
}
