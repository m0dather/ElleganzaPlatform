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
    /// <summary>
    /// Customer account dashboard/overview
    /// </summary>
    [HttpGet("")]
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Customer order history
    /// </summary>
    [HttpGet("orders")]
    public IActionResult Orders()
    {
        return View();
    }

    /// <summary>
    /// Customer order details
    /// </summary>
    [HttpGet("orders/{id}")]
    public IActionResult OrderDetails(int id)
    {
        return View();
    }

    /// <summary>
    /// Customer address management
    /// </summary>
    [HttpGet("addresses")]
    public IActionResult Addresses()
    {
        return View();
    }

    /// <summary>
    /// Customer profile edit
    /// </summary>
    [HttpGet("edit-profile")]
    public IActionResult EditProfile()
    {
        return View();
    }

    /// <summary>
    /// Customer wishlist
    /// </summary>
    [HttpGet("wishlist")]
    public IActionResult Wishlist()
    {
        return View();
    }
}
