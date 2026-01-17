using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElleganzaPlatform.Areas.Admin.Store.Controllers;

[Area("Admin")]
[Route("admin")]
[Authorize(Policy = "StoreAdminPolicy")]
public class DashboardController : Controller
{
    [HttpGet("")]
    [HttpGet("Dashboard")]
    public IActionResult Index()
    {
        return View();
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
