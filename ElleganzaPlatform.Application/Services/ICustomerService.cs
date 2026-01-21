using ElleganzaPlatform.Application.ViewModels.Store;

namespace ElleganzaPlatform.Application.Services;

public interface ICustomerService
{
    Task<CustomerAccountViewModel?> GetCustomerAccountAsync(string userId);
    Task<CustomerOrdersViewModel> GetCustomerOrdersAsync(string userId, int page = 1, int pageSize = 10);
    Task<OrderDetailsViewModel?> GetOrderDetailsAsync(int orderId, string userId);
    
    // Address management
    Task<AddressListViewModel> GetCustomerAddressesAsync(string userId);
    Task<CustomerAddressViewModel?> GetCustomerAddressAsync(int addressId, string userId);
    Task<int> CreateCustomerAddressAsync(CustomerAddressViewModel model, string userId);
    Task<bool> UpdateCustomerAddressAsync(CustomerAddressViewModel model, string userId);
    Task<bool> DeleteCustomerAddressAsync(int addressId, string userId);
}
