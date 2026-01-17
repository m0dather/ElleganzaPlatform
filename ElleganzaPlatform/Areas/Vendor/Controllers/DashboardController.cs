using ElleganzaPlatform.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElleganzaPlatform.Areas.Vendor.Controllers;

[Area("Vendor")]
[Route("vendor")]
[Authorize(Policy = AuthorizationPolicies.RequireVendor)]
public class DashboardController : Controller
{
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
    public IActionResult Orders()
    {
        return View();
    }

    [HttpGet("Reports")]
    public IActionResult Reports()
    {
        return View();
    }
}
