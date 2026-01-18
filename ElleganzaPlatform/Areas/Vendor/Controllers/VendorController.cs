using ElleganzaPlatform.Application.Services;
using ElleganzaPlatform.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElleganzaPlatform.Areas.Vendor.Controllers;

[Area("Vendor")]
[Route("vendor")]
[Authorize(Policy = AuthorizationPolicies.RequireVendor)]
/// <summary>
/// Vendor dashboard controller.
/// Enforces RequireVendor policy - accessible to Vendor (own vendor) and SuperAdmin (all vendors).
/// </summary>
public class VendorController : Controller
{
    private readonly IVendorOrderService _orderService;

    public VendorController(IVendorOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet("")]
    [HttpGet("Dashboard")]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("Products")]
    public IActionResult Products()
    {
        return View();
    }

    [HttpGet("Orders")]
    public async Task<IActionResult> Orders(int page = 1)
    {
        var model = await _orderService.GetVendorOrdersAsync(page);
        return View(model);
    }

    [HttpGet("Orders/{id}")]
    public async Task<IActionResult> OrderDetails(int id)
    {
        var model = await _orderService.GetVendorOrderDetailsAsync(id);
        if (model == null)
            return NotFound();

        return View(model);
    }

    [HttpGet("Reports")]
    public IActionResult Reports()
    {
        return View();
    }
}
