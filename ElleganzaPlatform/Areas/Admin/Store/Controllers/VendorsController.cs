using ElleganzaPlatform.Application.Services;
using ElleganzaPlatform.Infrastructure.Authorization;
using ElleganzaPlatform.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElleganzaPlatform.Areas.Admin.Store.Controllers;

[Area("Admin")]
[Route("admin/vendors")]
[Authorize(Policy = AuthorizationPolicies.RequireStoreAdmin)]
/// <summary>
/// Store Admin vendor management controller.
/// Provides vendor approval/rejection functionality with audit logging.
/// Enforces RequireStoreAdmin policy - accessible to StoreAdmin (own store) and SuperAdmin (all stores).
/// </summary>
public class VendorsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<VendorsController> _logger;

    public VendorsController(
        ApplicationDbContext context,
        IAuditLogService auditLogService,
        ILogger<VendorsController> logger)
    {
        _context = context;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    /// <summary>
    /// List all vendors in the store
    /// </summary>
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var vendors = await _context.Vendors
            .Include(v => v.Store)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();

        return View(vendors);
    }

    /// <summary>
    /// View vendor details
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> Details(int id)
    {
        var vendor = await _context.Vendors
            .Include(v => v.Store)
            .Include(v => v.VendorAdmins)
            .ThenInclude(va => va.User)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vendor == null)
            return NotFound();

        return View(vendor);
    }

    /// <summary>
    /// Approve a vendor (activate)
    /// Logs the approval action to audit log
    /// </summary>
    [HttpPost("{id}/approve")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var vendor = await _context.Vendors.FindAsync(id);
        if (vendor == null)
            return NotFound();

        if (vendor.IsActive)
        {
            TempData["Warning"] = "Vendor is already approved.";
            return RedirectToAction(nameof(Details), new { id });
        }

        vendor.IsActive = true;
        vendor.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Log admin action to audit log
        await _auditLogService.LogActionAsync(
            action: "VendorApproved",
            entityType: "Vendor",
            entityId: vendor.Id,
            details: $"Vendor '{vendor.Name}' was approved and activated."
        );

        _logger.LogInformation("Vendor {VendorId} ({VendorName}) approved", vendor.Id, vendor.Name);

        TempData["Success"] = $"Vendor '{vendor.Name}' has been approved successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>
    /// Reject/deactivate a vendor
    /// Logs the rejection action to audit log
    /// </summary>
    [HttpPost("{id}/reject")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string? reason)
    {
        var vendor = await _context.Vendors.FindAsync(id);
        if (vendor == null)
            return NotFound();

        if (!vendor.IsActive)
        {
            TempData["Warning"] = "Vendor is already rejected/inactive.";
            return RedirectToAction(nameof(Details), new { id });
        }

        vendor.IsActive = false;
        vendor.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Log admin action to audit log
        var details = $"Vendor '{vendor.Name}' was rejected and deactivated.";
        if (!string.IsNullOrWhiteSpace(reason))
        {
            details += $" Reason: {reason}";
        }

        await _auditLogService.LogActionAsync(
            action: "VendorRejected",
            entityType: "Vendor",
            entityId: vendor.Id,
            details: details
        );

        _logger.LogInformation("Vendor {VendorId} ({VendorName}) rejected. Reason: {Reason}", 
            vendor.Id, vendor.Name, reason ?? "No reason provided");

        TempData["Success"] = $"Vendor '{vendor.Name}' has been rejected successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>
    /// Read-only preview of vendor dashboard
    /// Allows admins to view vendor's dashboard without edit permissions
    /// </summary>
    [HttpGet("{id}/preview-dashboard")]
    public async Task<IActionResult> PreviewDashboard(int id)
    {
        var vendor = await _context.Vendors
            .Include(v => v.Store)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vendor == null)
            return NotFound();

        // Store vendor context for preview
        ViewData["VendorId"] = id;
        ViewData["VendorName"] = vendor.Name;
        ViewData["IsPreviewMode"] = true;

        return View("PreviewDashboard", vendor);
    }

    /// <summary>
    /// Read-only preview of vendor products
    /// Allows admins to view vendor's products without edit permissions
    /// </summary>
    [HttpGet("{id}/preview-products")]
    public async Task<IActionResult> PreviewProducts(int id)
    {
        var vendor = await _context.Vendors
            .Include(v => v.Store)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vendor == null)
            return NotFound();

        var products = await _context.Products
            .Where(p => p.VendorId == id)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        // Store vendor context for preview
        ViewData["VendorId"] = id;
        ViewData["VendorName"] = vendor.Name;
        ViewData["IsPreviewMode"] = true;

        return View("PreviewProducts", products);
    }

    /// <summary>
    /// Read-only preview of vendor orders
    /// Allows admins to view vendor's orders without edit permissions
    /// </summary>
    [HttpGet("{id}/preview-orders")]
    public async Task<IActionResult> PreviewOrders(int id)
    {
        var vendor = await _context.Vendors
            .Include(v => v.Store)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vendor == null)
            return NotFound();

        var orders = await _context.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.Product)
            .Where(oi => oi.VendorId == id)
            .Select(oi => oi.Order)
            .Distinct()
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        // Store vendor context for preview
        ViewData["VendorId"] = id;
        ViewData["VendorName"] = vendor.Name;
        ViewData["IsPreviewMode"] = true;

        return View("PreviewOrders", orders);
    }
}
