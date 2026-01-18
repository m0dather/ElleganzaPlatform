using ElleganzaPlatform.Application.ViewModels.Admin;

namespace ElleganzaPlatform.Application.Services;

public interface IVendorOrderService
{
    Task<OrderListViewModel> GetVendorOrdersAsync(int page = 1, int pageSize = 20);
    Task<AdminOrderDetailsViewModel?> GetVendorOrderDetailsAsync(int orderId);
}
