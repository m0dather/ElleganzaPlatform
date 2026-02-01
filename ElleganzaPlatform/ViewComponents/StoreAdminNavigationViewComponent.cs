using ElleganzaPlatform.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace ElleganzaPlatform.ViewComponents;

/// <summary>
/// ViewComponent for Store Admin navigation menu
/// Displays navigation items only accessible to StoreAdmin role
/// </summary>
public class StoreAdminNavigationViewComponent : ViewComponent
{
    private readonly ICurrentUserService _currentUserService;

    public StoreAdminNavigationViewComponent(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public IViewComponentResult Invoke()
    {
        // Show to both StoreAdmin and SuperAdmin (SuperAdmin has access to all store admin features)
        if (!_currentUserService.IsStoreAdmin && !_currentUserService.IsSuperAdmin)
        {
            return Content(string.Empty);
        }

        var menuItems = new List<NavigationItem>
        {
            new NavigationItem { Text = "Dashboard", Icon = "bi-speedometer2", Url = "/admin/dashboard", IsActive = IsActive("/admin/dashboard") },
            new NavigationItem { Text = "Orders", Icon = "bi-cart-check", Url = "/admin/Orders", IsActive = IsActive("/admin/orders") },
            new NavigationItem { Text = "Products", Icon = "bi-box-seam", Url = "/admin/Products", IsActive = IsActive("/admin/products") },
            new NavigationItem { Text = "Vendors", Icon = "bi-shop", Url = "/admin/Vendors", IsActive = IsActive("/admin/vendors") },
            new NavigationItem { Text = "Customers", Icon = "bi-people", Url = "/admin/Customers", IsActive = IsActive("/admin/customers") },
            new NavigationItem { Text = "Reports", Icon = "bi-graph-up", Url = "/admin/Reports", IsActive = IsActive("/admin/reports") },
            new NavigationItem { Text = "Settings", Icon = "bi-gear", Url = "/admin/Settings", IsActive = IsActive("/admin/settings") },
            new NavigationItem { Text = "Logout", Icon = "bi-box-arrow-right", Url = "/logout", IsActive = false, IsPostAction = true }
        };

        return View(menuItems);
    }

    private bool IsActive(string path)
    {
        return HttpContext.Request.Path.StartsWithSegments(path, StringComparison.OrdinalIgnoreCase);
    }
}
