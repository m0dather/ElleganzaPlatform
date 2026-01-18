using ElleganzaPlatform.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElleganzaPlatform.Areas.Admin.Store.Controllers;

[Area("Admin")]
[Route("admin/subscriptions")]
[Authorize(Policy = AuthorizationPolicies.RequireStoreAdmin)]
/// <summary>
/// Subscription management controller for admin interface
/// Uses Admin theme (Metronic)
/// </summary>
public class SubscriptionsController : Controller
{
    private readonly ILogger<SubscriptionsController> _logger;

    public SubscriptionsController(ILogger<SubscriptionsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Subscription list view
    /// </summary>
    [HttpGet("")]
    public IActionResult Index()
    {
        return View();
    }
}
