using ElleganzaPlatform.Application.ViewModels.Store;

namespace ElleganzaPlatform.Application.Services;

public interface IOrderService
{
    Task<CustomerOrderListViewModel> GetCustomerOrdersAsync(int page = 1, int pageSize = 10);
    Task<CustomerOrderDetailsViewModel?> GetOrderDetailsAsync(int orderId);
}
