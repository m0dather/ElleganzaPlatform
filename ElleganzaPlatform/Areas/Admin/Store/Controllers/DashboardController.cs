using ElleganzaPlatform.Application.Services;
using ElleganzaPlatform.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElleganzaPlatform.Areas.Admin.Store.Controllers;

[Area("Admin")]
[Route("admin/dashboard")]
[Authorize(Policy = AuthorizationPolicies.RequireStoreAdmin)]
/// <summary>
/// Admin Dashboard controller for the back-office management interface
/// Uses Admin theme (Metronic)
/// </summary>
public class DashboardController : Controller
{
    private readonly ILogger<DashboardController> _logger;
    private readonly IAdminDashboardService _dashboardService;

    public DashboardController(
        ILogger<DashboardController> logger,
        IAdminDashboardService dashboardService)
    {
        _logger = logger;
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Main dashboard view with statistics and charts
    /// </summary>
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var model = await _dashboardService.GetDashboardDataAsync();
        return View(model);
    }
}
