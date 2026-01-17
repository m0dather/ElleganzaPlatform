using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElleganzaPlatform.Areas.Customer.Controllers;

[Area("Customer")]
[Route("Account")]
[Authorize(Policy = "CustomerPolicy")]
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
