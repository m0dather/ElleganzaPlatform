using ElleganzaPlatform.Application.Services;
using ElleganzaPlatform.Application.ViewModels.Admin;
using ElleganzaPlatform.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElleganzaPlatform.Areas.Admin.Store.Controllers;

[Area("Admin")]
[Route("admin/products")]
[Authorize(Policy = AuthorizationPolicies.RequireStoreAdmin)]
/// <summary>
/// Product management controller for admin interface
/// Uses Admin theme (Metronic)
/// </summary>
public class ProductsController : Controller
{
    private readonly ILogger<ProductsController> _logger;
    private readonly IAdminProductService _productService;

    public ProductsController(
        ILogger<ProductsController> logger,
        IAdminProductService productService)
    {
        _logger = logger;
        _productService = productService;
    }

    /// <summary>
    /// Product list view
    /// </summary>
    [HttpGet("")]
    public async Task<IActionResult> Index(int page = 1)
    {
        var model = await _productService.GetProductsAsync(page);
        return View(model);
    }

    /// <summary>
    /// Create product form
    /// </summary>
    [HttpGet("create")]
    public async Task<IActionResult> Create()
    {
        var model = await _productService.GetProductFormAsync();
        return View(model);
    }

    /// <summary>
    /// Create product POST
    /// </summary>
    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model = await _productService.GetProductFormAsync();
            return View(model);
        }

        var success = await _productService.CreateProductAsync(model);
        if (!success)
        {
            ModelState.AddModelError("", "Failed to create product");
            model = await _productService.GetProductFormAsync();
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Edit product form
    /// </summary>
    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _productService.GetProductFormAsync(id);
        if (model.Id == 0)
            return NotFound();

        return View(model);
    }

    /// <summary>
    /// Edit product POST
    /// </summary>
    [HttpPost("edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductFormViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            model = await _productService.GetProductFormAsync(id);
            return View(model);
        }

        var success = await _productService.UpdateProductAsync(model);
        if (!success)
        {
            ModelState.AddModelError("", "Failed to update product");
            model = await _productService.GetProductFormAsync(id);
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }
}
