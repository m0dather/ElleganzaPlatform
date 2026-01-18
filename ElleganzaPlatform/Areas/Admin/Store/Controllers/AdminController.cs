using ElleganzaPlatform.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElleganzaPlatform.Areas.Admin.Store.Controllers;

[Area("Admin")]
[Route("admin")]
[Authorize(Policy = AuthorizationPolicies.RequireStoreAdmin)]
/// <summary>
/// Store Admin dashboard controller.
/// Enforces RequireStoreAdmin policy - accessible to StoreAdmin (own store) and SuperAdmin (all stores).
/// </summary>
public class AdminController : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        // Redirect to Dashboard
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpGet("Settings")]
    public IActionResult Settings()
    {
        return View();
    }

    [HttpGet("Vendors")]
    public IActionResult Vendors()
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
