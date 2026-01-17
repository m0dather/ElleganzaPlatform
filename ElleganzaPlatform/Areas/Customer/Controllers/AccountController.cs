using ElleganzaPlatform.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElleganzaPlatform.Areas.Customer.Controllers;

[Area("Customer")]
[Route("account")]
[Authorize(Policy = AuthorizationPolicies.RequireCustomer)]
/// <summary>
/// Customer account dashboard controller.
/// Enforces RequireCustomer policy - only accessible to users with Customer role.
/// </summary>
public class AccountController : Controller
{
    [HttpGet("")]
    [HttpGet("Profile")]
    public IActionResult Profile()
    {
        return View();
    }

    [HttpGet("MyOrders")]
    public IActionResult MyOrders()
    {
        return View();
    }

    [HttpGet("Wishlist")]
    public IActionResult Wishlist()
    {
        return View();
    }
}
