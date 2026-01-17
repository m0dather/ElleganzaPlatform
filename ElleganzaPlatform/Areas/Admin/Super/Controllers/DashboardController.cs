using ElleganzaPlatform.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElleganzaPlatform.Areas.Admin.Super.Controllers;

[Area("Admin")]
[Route("super-admin")]
[Authorize(Policy = AuthorizationPolicies.RequireSuperAdmin)]
public class DashboardController : Controller
{
    [HttpGet("")]
    [HttpGet("Dashboard")]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("Stores")]
    public IActionResult Stores()
    {
        return View();
    }

    [HttpGet("Vendors")]
    public IActionResult Vendors()
    {
        return View();
    }

    [HttpGet("Users")]
    public IActionResult Users()
    {
        return View();
    }

    [HttpGet("Reports")]
    public IActionResult Reports()
    {
        return View();
    }
}
