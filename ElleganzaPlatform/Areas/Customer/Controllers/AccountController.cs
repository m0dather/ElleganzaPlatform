using ElleganzaPlatform.Application.Services;
using ElleganzaPlatform.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SysClaims = System.Security.Claims;

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
    private readonly ICustomerService _customerService;

    public AccountController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    /// <summary>
    /// Customer account dashboard/overview
    /// </summary>
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirst(SysClaims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var model = await _customerService.GetCustomerAccountAsync(userId);
        return View(model);
    }

    /// <summary>
    /// Customer order history
    /// </summary>
    [HttpGet("orders")]
    public async Task<IActionResult> Orders(int page = 1)
    {
        var userId = User.FindFirst(SysClaims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var model = await _customerService.GetCustomerOrdersAsync(userId, page);
        return View(model);
    }

    /// <summary>
    /// Customer order details
    /// </summary>
    [HttpGet("orders/{id}")]
    public async Task<IActionResult> OrderDetails(int id)
    {
        var userId = User.FindFirst(SysClaims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var model = await _customerService.GetOrderDetailsAsync(id, userId);
        if (model == null)
            return NotFound();

        return View(model);
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
