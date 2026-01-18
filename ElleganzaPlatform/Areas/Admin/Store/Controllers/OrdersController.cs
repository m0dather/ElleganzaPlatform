using ElleganzaPlatform.Application.Services;
using ElleganzaPlatform.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElleganzaPlatform.Areas.Admin.Store.Controllers;

[Area("Admin")]
[Route("admin/orders")]
[Authorize(Policy = AuthorizationPolicies.RequireStoreAdmin)]
/// <summary>
/// Order management controller for admin interface
/// Uses Admin theme (Metronic)
/// </summary>
public class OrdersController : Controller
{
    private readonly ILogger<OrdersController> _logger;
    private readonly IAdminOrderService _orderService;

    public OrdersController(
        ILogger<OrdersController> logger,
        IAdminOrderService orderService)
    {
        _logger = logger;
        _orderService = orderService;
    }

    /// <summary>
    /// Order list view
    /// </summary>
    [HttpGet("")]
    public async Task<IActionResult> Index(int page = 1)
    {
        var model = await _orderService.GetOrdersAsync(page);
        return View(model);
    }

    /// <summary>
    /// Order details view
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> Details(int id)
    {
        var model = await _orderService.GetOrderDetailsAsync(id);
        if (model == null)
            return NotFound();

        return View(model);
    }
}
