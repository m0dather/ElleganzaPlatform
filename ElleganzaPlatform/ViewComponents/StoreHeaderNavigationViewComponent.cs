using ElleganzaPlatform.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace ElleganzaPlatform.ViewComponents;

/// <summary>
/// ViewComponent for Store/Ecomus header navigation
/// Displays role-based account/dashboard links for authenticated users
/// </summary>
public class StoreHeaderNavigationViewComponent : ViewComponent
{
    private readonly ICurrentUserService _currentUserService;

    public StoreHeaderNavigationViewComponent(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public IViewComponentResult Invoke()
    {
        var model = new StoreHeaderNavigationModel
        {
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false
        };

        if (model.IsAuthenticated)
        {
            // Determine dashboard link based on role (highest priority role wins)
            if (_currentUserService.IsSuperAdmin)
            {
                model.DashboardText = "Super Admin Dashboard";
                model.DashboardUrl = "/super-admin";
                model.DashboardIcon = "bi-shield-check";
            }
            else if (_currentUserService.IsStoreAdmin)
            {
                model.DashboardText = "Admin Dashboard";
                model.DashboardUrl = "/admin";
                model.DashboardIcon = "bi-speedometer2";
            }
            else if (_currentUserService.IsVendorAdmin)
            {
                model.DashboardText = "Vendor Dashboard";
                model.DashboardUrl = "/vendor";
                model.DashboardIcon = "bi-shop";
            }
            else if (_currentUserService.IsCustomer)
            {
                model.DashboardText = "My Account";
                model.DashboardUrl = "/account";
                model.DashboardIcon = "bi-person-circle";
            }
        }

        return View(model);
    }
}

/// <summary>
/// Model for store header navigation
/// </summary>
public class StoreHeaderNavigationModel
{
    public bool IsAuthenticated { get; set; }
    public string? DashboardText { get; set; }
    public string? DashboardUrl { get; set; }
    public string? DashboardIcon { get; set; }
}
