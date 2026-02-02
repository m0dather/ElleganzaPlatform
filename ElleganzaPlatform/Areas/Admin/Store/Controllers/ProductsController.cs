using ElleganzaPlatform.Application.Services;
using ElleganzaPlatform.Application.ViewModels.Admin;
using ElleganzaPlatform.Domain.Enums;
using ElleganzaPlatform.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElleganzaPlatform.Areas.Admin.Store.Controllers;

[Area("Admin")]
[Route("admin/products")]
[Authorize(Policy = AuthorizationPolicies.RequireStoreAdmin)]
/// <summary>
/// Product management controller for admin interface
/// Stage 4.2: Full product approval workflow with status management
/// Uses Admin theme (Metronic)
/// </summary>
public class ProductsController : Controller
{
    private readonly ILogger<ProductsController> _logger;
    private readonly IAdminProductService _productService;
    private readonly IAuditLogService _auditLogService;

    public ProductsController(
        ILogger<ProductsController> logger,
        IAdminProductService productService,
        IAuditLogService auditLogService)
    {
        _logger = logger;
        _productService = productService;
        _auditLogService = auditLogService;
    }

    /// <summary>
    /// Product list view with optional status filter
    /// Stage 4.2: Added status filter support
    /// </summary>
    [HttpGet("")]
    public async Task<IActionResult> Index(int page = 1, ProductStatus? status = null)
    {
        ProductListViewModel model;
        
        if (status.HasValue)
        {
            model = await _productService.GetProductsByStatusAsync(status.Value, page);
            ViewData["CurrentFilter"] = status;
        }
        else
        {
            model = await _productService.GetProductsAsync(page);
        }
        
        return View(model);
    }

    /// <summary>
    /// List pending products awaiting approval
    /// Stage 4.2: New endpoint for pending products
    /// </summary>
    [HttpGet("pending")]
    public async Task<IActionResult> Pending(int page = 1)
    {
        var model = await _productService.GetProductsByStatusAsync(ProductStatus.PendingApproval, page);
        ViewData["PageTitle"] = "Pending Products";
        return View("Index", model);
    }

    /// <summary>
    /// List approved products
    /// Stage 4.2: New endpoint for approved products
    /// </summary>
    [HttpGet("approved")]
    public async Task<IActionResult> Approved(int page = 1)
    {
        var model = await _productService.GetProductsByStatusAsync(ProductStatus.Active, page);
        ViewData["PageTitle"] = "Approved Products";
        return View("Index", model);
    }

    /// <summary>
    /// List rejected products
    /// Stage 4.2: New endpoint for rejected products
    /// </summary>
    [HttpGet("rejected")]
    public async Task<IActionResult> Rejected(int page = 1)
    {
        var model = await _productService.GetProductsByStatusAsync(ProductStatus.Rejected, page);
        ViewData["PageTitle"] = "Rejected Products";
        return View("Index", model);
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

    /// <summary>
    /// Approve a product
    /// Stage 4.2: Product approval workflow
    /// </summary>
    [HttpPost("{id}/approve")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var username = User.Identity?.Name ?? "Admin";
        var success = await _productService.ApproveProductAsync(id, username);
        
        if (!success)
        {
            TempData["Error"] = "Failed to approve product.";
            return RedirectToAction(nameof(Index));
        }

        // Log admin action to audit log
        await _auditLogService.LogActionAsync(
            action: "ProductApproved",
            entityType: "Product",
            entityId: id,
            details: $"Product was approved by {username}."
        );

        _logger.LogInformation("Product {ProductId} approved by {User}", id, username);

        TempData["Success"] = "Product has been approved successfully.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Reject a product
    /// Stage 4.2: Product rejection with reason
    /// </summary>
    [HttpPost("{id}/reject")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string? reason)
    {
        var username = User.Identity?.Name ?? "Admin";
        var rejectionReason = string.IsNullOrWhiteSpace(reason) ? "No reason provided" : reason;
        
        var success = await _productService.RejectProductAsync(id, username, rejectionReason);
        
        if (!success)
        {
            TempData["Error"] = "Failed to reject product.";
            return RedirectToAction(nameof(Index));
        }

        // Log admin action to audit log
        await _auditLogService.LogActionAsync(
            action: "ProductRejected",
            entityType: "Product",
            entityId: id,
            details: $"Product was rejected by {username}. Reason: {rejectionReason}"
        );

        _logger.LogInformation("Product {ProductId} rejected by {User}. Reason: {Reason}", 
            id, username, rejectionReason);

        TempData["Success"] = "Product has been rejected.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Disable a product
    /// Stage 4.2: Disable approved product
    /// </summary>
    [HttpPost("{id}/disable")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Disable(int id)
    {
        var username = User.Identity?.Name ?? "Admin";
        var success = await _productService.DisableProductAsync(id, username);
        
        if (!success)
        {
            TempData["Error"] = "Failed to disable product.";
            return RedirectToAction(nameof(Index));
        }

        // Log admin action to audit log
        await _auditLogService.LogActionAsync(
            action: "ProductDisabled",
            entityType: "Product",
            entityId: id,
            details: $"Product was disabled by {username}."
        );

        _logger.LogInformation("Product {ProductId} disabled by {User}", id, username);

        TempData["Success"] = "Product has been disabled.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Enable a disabled product
    /// Stage 4.2: Re-enable disabled product
    /// </summary>
    [HttpPost("{id}/enable")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enable(int id)
    {
        var username = User.Identity?.Name ?? "Admin";
        var success = await _productService.EnableProductAsync(id, username);
        
        if (!success)
        {
            TempData["Error"] = "Failed to enable product.";
            return RedirectToAction(nameof(Index));
        }

        // Log admin action to audit log
        await _auditLogService.LogActionAsync(
            action: "ProductEnabled",
            entityType: "Product",
            entityId: id,
            details: $"Product was enabled by {username}."
        );

        _logger.LogInformation("Product {ProductId} enabled by {User}", id, username);

        TempData["Success"] = "Product has been enabled.";
        return RedirectToAction(nameof(Index));
    }
}
