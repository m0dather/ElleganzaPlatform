using Microsoft.AspNetCore.Mvc;

namespace ElleganzaPlatform.Controllers;

/// <summary>
/// Shop controller for storefront product browsing and shopping features
/// Uses Store theme (Ecomus)
/// </summary>
public class ShopController : Controller
{
    private readonly ILogger<ShopController> _logger;

    public ShopController(ILogger<ShopController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Shop index page - Shows product listings/brands
    /// </summary>
    [HttpGet("/shop")]
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Product details page
    /// </summary>
    [HttpGet("/shop/product/{id?}")]
    public IActionResult Product(int? id)
    {
        return View();
    }

    /// <summary>
    /// Product comparison page
    /// </summary>
    [HttpGet("/shop/compare")]
    public IActionResult Compare()
    {
        return View();
    }

    /// <summary>
    /// Checkout page
    /// </summary>
    [HttpGet("/shop/checkout")]
    public IActionResult Checkout()
    {
        return View();
    }
}
