using Microsoft.AspNetCore.Mvc;

namespace ElleganzaPlatform.ViewComponents;

/// <summary>
/// ViewComponent for Guest (unauthenticated) navigation menu
/// Displays Login and Register links only for guests
/// </summary>
public class GuestNavigationViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        // Only show if user is NOT authenticated
        if (User.Identity?.IsAuthenticated == true)
        {
            return Content(string.Empty);
        }

        var menuItems = new List<NavigationItem>
        {
            new NavigationItem { Text = "Login", Icon = "bi-box-arrow-in-right", Url = "/login", IsActive = false },
            new NavigationItem { Text = "Register", Icon = "bi-person-plus", Url = "/register", IsActive = false }
        };

        return View(menuItems);
    }
}
