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
    public async Task<IActionResult> Addresses()
    {
        var userId = User.FindFirst(SysClaims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var model = await _customerService.GetCustomerAddressesAsync(userId);
        return View(model);
    }

    /// <summary>
    /// Get address details for editing
    /// </summary>
    [HttpGet("addresses/{id}")]
    public async Task<IActionResult> GetAddress(int id)
    {
        var userId = User.FindFirst(SysClaims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var address = await _customerService.GetCustomerAddressAsync(id, userId);
        if (address == null)
            return NotFound();

        return Json(address);
    }

    /// <summary>
    /// Create new address
    /// </summary>
    [HttpPost("addresses")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAddress([FromForm] Application.ViewModels.Store.CustomerAddressViewModel model)
    {
        var userId = User.FindFirst(SysClaims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please correct the errors in the form.";
            return RedirectToAction(nameof(Addresses));
        }

        try
        {
            await _customerService.CreateCustomerAddressAsync(model, userId);
            TempData["Success"] = "Address added successfully.";
        }
        catch (Exception)
        {
            TempData["Error"] = "Failed to add address. Please try again.";
        }

        return RedirectToAction(nameof(Addresses));
    }

    /// <summary>
    /// Update existing address
    /// </summary>
    [HttpPost("addresses/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAddress(int id, [FromForm] Application.ViewModels.Store.CustomerAddressViewModel model)
    {
        var userId = User.FindFirst(SysClaims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please correct the errors in the form.";
            return RedirectToAction(nameof(Addresses));
        }

        model.Id = id;
        
        try
        {
            var success = await _customerService.UpdateCustomerAddressAsync(model, userId);
            if (success)
                TempData["Success"] = "Address updated successfully.";
            else
                TempData["Error"] = "Address not found or you don't have permission to update it.";
        }
        catch (Exception)
        {
            TempData["Error"] = "Failed to update address. Please try again.";
        }

        return RedirectToAction(nameof(Addresses));
    }

    /// <summary>
    /// Delete address
    /// </summary>
    [HttpPost("addresses/{id}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAddress(int id)
    {
        var userId = User.FindFirst(SysClaims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        try
        {
            var success = await _customerService.DeleteCustomerAddressAsync(id, userId);
            if (success)
                TempData["Success"] = "Address deleted successfully.";
            else
                TempData["Error"] = "Cannot delete address. You must have at least one address.";
        }
        catch (Exception)
        {
            TempData["Error"] = "Failed to delete address. Please try again.";
        }

        return RedirectToAction(nameof(Addresses));
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
