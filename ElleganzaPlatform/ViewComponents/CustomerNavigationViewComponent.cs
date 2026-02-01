using ElleganzaPlatform.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace ElleganzaPlatform.ViewComponents;

/// <summary>
/// ViewComponent for Customer account navigation menu
/// Displays navigation items only accessible to Customer role
/// </summary>
public class CustomerNavigationViewComponent : ViewComponent
{
    private readonly ICurrentUserService _currentUserService;

    public CustomerNavigationViewComponent(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public IViewComponentResult Invoke()
    {
        // Only show if user is Customer
        if (!_currentUserService.IsCustomer)
        {
            return Content(string.Empty);
        }

        var menuItems = new List<NavigationItem>
        {
            new NavigationItem { Text = "Dashboard", Icon = "bi-house", Url = "/account", IsActive = IsActive("/account") && HttpContext.Request.Path == "/account" },
            new NavigationItem { Text = "Orders", Icon = "bi-cart-check", Url = "/account/orders", IsActive = IsActive("/account/orders") },
            new NavigationItem { Text = "Addresses", Icon = "bi-geo-alt", Url = "/account/addresses", IsActive = IsActive("/account/addresses") },
            new NavigationItem { Text = "Wishlist", Icon = "bi-heart", Url = "/account/wishlist", IsActive = IsActive("/account/wishlist") },
            new NavigationItem { Text = "Profile", Icon = "bi-person", Url = "/account/edit-profile", IsActive = IsActive("/account/edit-profile") },
            new NavigationItem { Text = "Logout", Icon = "bi-box-arrow-right", Url = "/logout", IsActive = false, IsPostAction = true }
        };

        return View(menuItems);
    }

    private bool IsActive(string path)
    {
        return HttpContext.Request.Path.StartsWithSegments(path, StringComparison.OrdinalIgnoreCase);
    }
}
