using ElleganzaPlatform.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElleganzaPlatform.Areas.Admin.Store.Controllers;

[Area("Admin")]
[Route("admin/invoices")]
[Authorize(Policy = AuthorizationPolicies.RequireStoreAdmin)]
/// <summary>
/// Invoice management controller for admin interface
/// Uses Admin theme (Metronic)
/// </summary>
public class InvoicesController : Controller
{
    private readonly ILogger<InvoicesController> _logger;

    public InvoicesController(ILogger<InvoicesController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Invoice list view
    /// </summary>
    [HttpGet("")]
    public IActionResult Index()
    {
        return View();
    }
}
