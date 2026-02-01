using ElleganzaPlatform.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace ElleganzaPlatform.ViewComponents;

/// <summary>
/// ViewComponent for Vendor navigation menu
/// Displays navigation items only accessible to Vendor role
/// </summary>
public class VendorNavigationViewComponent : ViewComponent
{
    private readonly ICurrentUserService _currentUserService;

    public VendorNavigationViewComponent(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public IViewComponentResult Invoke()
    {
        // Show to both Vendor and SuperAdmin (SuperAdmin has access to vendor features)
        if (!_currentUserService.IsVendorAdmin && !_currentUserService.IsSuperAdmin)
        {
            return Content(string.Empty);
        }

        var menuItems = new List<NavigationItem>
        {
            new NavigationItem { Text = "Dashboard", Icon = "bi-speedometer2", Url = "/vendor", IsActive = IsActive("/vendor") },
            new NavigationItem { Text = "Products", Icon = "bi-box-seam", Url = "/vendor/Products", IsActive = IsActive("/vendor/products") },
            new NavigationItem { Text = "Orders", Icon = "bi-cart-check", Url = "/vendor/Orders", IsActive = IsActive("/vendor/orders") },
            new NavigationItem { Text = "Reports", Icon = "bi-graph-up", Url = "/vendor/Reports", IsActive = IsActive("/vendor/reports") },
            new NavigationItem { Text = "Logout", Icon = "bi-box-arrow-right", Url = "/logout", IsActive = false, IsPostAction = true }
        };

        return View(menuItems);
    }

    private bool IsActive(string path)
    {
        return HttpContext.Request.Path.StartsWithSegments(path, StringComparison.OrdinalIgnoreCase);
    }
}
