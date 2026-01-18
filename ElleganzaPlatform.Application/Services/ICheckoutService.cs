using ElleganzaPlatform.Application.ViewModels.Store;

namespace ElleganzaPlatform.Application.Services;

public interface ICheckoutService
{
    Task<CheckoutViewModel?> GetCheckoutDataAsync();
    Task<OrderConfirmationViewModel?> PlaceOrderAsync(PlaceOrderRequest request);
}
