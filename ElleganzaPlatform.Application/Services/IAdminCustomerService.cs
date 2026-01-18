using ElleganzaPlatform.Application.ViewModels.Admin;

namespace ElleganzaPlatform.Application.Services;

public interface IAdminCustomerService
{
    Task<CustomerListViewModel> GetCustomersAsync(int page = 1, int pageSize = 20);
    Task<CustomerDetailsViewModel?> GetCustomerDetailsAsync(string customerId);
}
