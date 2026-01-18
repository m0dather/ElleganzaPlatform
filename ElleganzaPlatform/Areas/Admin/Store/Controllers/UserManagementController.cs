using ElleganzaPlatform.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElleganzaPlatform.Areas.Admin.Store.Controllers;

[Area("Admin")]
[Route("admin/usermanagement")]
[Authorize(Policy = AuthorizationPolicies.RequireStoreAdmin)]
/// <summary>
/// User management controller for admin interface
/// Uses Admin theme (Metronic)
/// </summary>
public class UserManagementController : Controller
{
    private readonly ILogger<UserManagementController> _logger;

    public UserManagementController(ILogger<UserManagementController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// User management view
    /// </summary>
    [HttpGet("")]
    public IActionResult Index()
    {
        return View();
    }
}
