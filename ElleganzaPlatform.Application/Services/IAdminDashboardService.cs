using ElleganzaPlatform.Application.ViewModels.Admin;

namespace ElleganzaPlatform.Application.Services;

public interface IAdminDashboardService
{
    Task<DashboardViewModel> GetDashboardDataAsync();
}
