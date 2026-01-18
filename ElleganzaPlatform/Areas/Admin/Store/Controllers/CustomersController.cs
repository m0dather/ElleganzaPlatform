using ElleganzaPlatform.Application.Services;
using ElleganzaPlatform.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElleganzaPlatform.Areas.Admin.Store.Controllers;

[Area("Admin")]
[Route("admin/customers")]
[Authorize(Policy = AuthorizationPolicies.RequireStoreAdmin)]
/// <summary>
/// Customer management controller for admin interface
/// Uses Admin theme (Metronic)
/// </summary>
public class CustomersController : Controller
{
    private readonly ILogger<CustomersController> _logger;
    private readonly IAdminCustomerService _customerService;

    public CustomersController(
        ILogger<CustomersController> logger,
        IAdminCustomerService customerService)
    {
        _logger = logger;
        _customerService = customerService;
    }

    /// <summary>
    /// Customer list view
    /// </summary>
    [HttpGet("")]
    public async Task<IActionResult> Index(int page = 1)
    {
        var model = await _customerService.GetCustomersAsync(page);
        return View(model);
    }

    /// <summary>
    /// Customer details view
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> Details(string id)
    {
        var model = await _customerService.GetCustomerDetailsAsync(id);
        if (model == null)
            return NotFound();

        return View(model);
    }
}
