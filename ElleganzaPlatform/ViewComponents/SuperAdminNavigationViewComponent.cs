using ElleganzaPlatform.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace ElleganzaPlatform.ViewComponents;

/// <summary>
/// ViewComponent for Super Admin navigation menu
/// Displays navigation items only accessible to SuperAdmin role
/// </summary>
public class SuperAdminNavigationViewComponent : ViewComponent
{
    private readonly ICurrentUserService _currentUserService;

    public SuperAdminNavigationViewComponent(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public IViewComponentResult Invoke()
    {
        // Only show if user is SuperAdmin
        if (!_currentUserService.IsSuperAdmin)
        {
            return Content(string.Empty);
        }

        var menuItems = new List<NavigationItem>
        {
            new NavigationItem { Text = "Dashboard", Icon = "bi-speedometer2", Url = "/super-admin", IsActive = IsActive("/super-admin") },
            new NavigationItem { Text = "Stores", Icon = "bi-shop", Url = "/super-admin/Stores", IsActive = IsActive("/super-admin/stores") },
            new NavigationItem { Text = "Vendors", Icon = "bi-people", Url = "/super-admin/Vendors", IsActive = IsActive("/super-admin/vendors") },
            new NavigationItem { Text = "Users", Icon = "bi-person-badge", Url = "/super-admin/Users", IsActive = IsActive("/super-admin/users") },
            new NavigationItem { Text = "Reports", Icon = "bi-graph-up", Url = "/super-admin/Reports", IsActive = IsActive("/super-admin/reports") }
        };

        return View(menuItems);
    }

    private bool IsActive(string path)
    {
        return HttpContext.Request.Path.StartsWithSegments(path, StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Navigation item model
/// </summary>
public class NavigationItem
{
    public string Text { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
