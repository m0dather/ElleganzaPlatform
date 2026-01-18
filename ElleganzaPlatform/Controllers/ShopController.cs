using ElleganzaPlatform.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ElleganzaPlatform.Controllers;

/// <summary>
/// Shop controller for storefront product browsing and shopping features
/// Uses Store theme (Ecomus)
/// </summary>
public class ShopController : Controller
{
    private readonly ILogger<ShopController> _logger;
    private readonly IStoreService _storeService;

    public ShopController(
        ILogger<ShopController> logger,
        IStoreService storeService)
    {
        _logger = logger;
        _storeService = storeService;
    }

    /// <summary>
    /// Shop index page - Shows product listings/brands
    /// </summary>
    [HttpGet("/shop")]
    public async Task<IActionResult> Index(int page = 1)
    {
        var model = await _storeService.GetShopPageDataAsync(page);
        return View(model);
    }

    /// <summary>
    /// Product details page
    /// </summary>
    [HttpGet("/shop/product/{id?}")]
    public async Task<IActionResult> Product(int? id)
    {
        if (!id.HasValue)
            return NotFound();

        var model = await _storeService.GetProductDetailsAsync(id.Value);
        if (model == null)
            return NotFound();

        return View(model);
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
