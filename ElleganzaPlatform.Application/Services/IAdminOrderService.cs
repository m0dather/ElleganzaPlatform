using ElleganzaPlatform.Application.ViewModels.Admin;

namespace ElleganzaPlatform.Application.Services;

public interface IAdminOrderService
{
    Task<OrderListViewModel> GetOrdersAsync(int page = 1, int pageSize = 20);
    Task<AdminOrderDetailsViewModel?> GetOrderDetailsAsync(int orderId);
}
