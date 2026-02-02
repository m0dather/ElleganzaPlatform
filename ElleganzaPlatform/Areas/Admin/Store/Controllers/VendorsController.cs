using ElleganzaPlatform.Application.Services;
using ElleganzaPlatform.Domain.Enums;
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
/// Stage 4.2: Full vendor lifecycle management with approval workflow.
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
    /// List all vendors with optional status filter
    /// Stage 4.2: Added status filter support
    /// </summary>
    [HttpGet("")]
    public async Task<IActionResult> Index(VendorStatus? status = null)
    {
        var query = _context.Vendors
            .Include(v => v.Store)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(v => v.Status == status.Value);
        }

        var vendors = await query
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();

        ViewData["CurrentFilter"] = status;
        return View(vendors);
    }

    /// <summary>
    /// List pending vendors awaiting approval
    /// Stage 4.2: New endpoint for pending vendors
    /// </summary>
    [HttpGet("pending")]
    public async Task<IActionResult> Pending()
    {
        var vendors = await _context.Vendors
            .Include(v => v.Store)
            .Where(v => v.Status == VendorStatus.Pending)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();

        return View("Index", vendors);
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
            .Include(v => v.Products)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vendor == null)
            return NotFound();

        return View(vendor);
    }

    /// <summary>
    /// Approve a vendor
    /// Stage 4.2: Updates vendor status to Approved
    /// </summary>
    [HttpPost("{id}/approve")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var vendor = await _context.Vendors.FindAsync(id);
        if (vendor == null)
            return NotFound();

        if (vendor.Status == VendorStatus.Approved)
        {
            TempData["Warning"] = "Vendor is already approved.";
            return RedirectToAction(nameof(Details), new { id });
        }

        vendor.Status = VendorStatus.Approved;
        vendor.IsActive = true;
        vendor.ApprovedAt = DateTime.UtcNow;
        vendor.ApprovedBy = User.Identity?.Name;
        vendor.RejectionReason = null; // Clear any previous rejection reason
        vendor.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Log admin action to audit log
        await _auditLogService.LogActionAsync(
            action: "VendorApproved",
            entityType: "Vendor",
            entityId: vendor.Id,
            details: $"Vendor '{vendor.Name}' was approved and activated."
        );

        _logger.LogInformation("Vendor {VendorId} ({VendorName}) approved by {User}", 
            vendor.Id, vendor.Name, User.Identity?.Name);

        TempData["Success"] = $"Vendor '{vendor.Name}' has been approved successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>
    /// Reject a vendor
    /// Stage 4.2: Updates vendor status to Rejected with reason
    /// </summary>
    [HttpPost("{id}/reject")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string? reason)
    {
        var vendor = await _context.Vendors.FindAsync(id);
        if (vendor == null)
            return NotFound();

        if (vendor.Status == VendorStatus.Rejected)
        {
            TempData["Warning"] = "Vendor is already rejected.";
            return RedirectToAction(nameof(Details), new { id });
        }

        vendor.Status = VendorStatus.Rejected;
        vendor.IsActive = false;
        vendor.RejectionReason = reason ?? "No reason provided";
        vendor.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Log admin action to audit log
        var details = $"Vendor '{vendor.Name}' was rejected.";
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

        _logger.LogInformation("Vendor {VendorId} ({VendorName}) rejected by {User}. Reason: {Reason}", 
            vendor.Id, vendor.Name, User.Identity?.Name, reason ?? "No reason provided");

        TempData["Success"] = $"Vendor '{vendor.Name}' has been rejected.";
        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>
    /// Suspend a vendor
    /// Stage 4.2: Temporarily suspend a vendor
    /// </summary>
    [HttpPost("{id}/suspend")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Suspend(int id, string? reason)
    {
        var vendor = await _context.Vendors.FindAsync(id);
        if (vendor == null)
            return NotFound();

        if (vendor.Status == VendorStatus.Suspended)
        {
            TempData["Warning"] = "Vendor is already suspended.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var previousStatus = vendor.Status;
        vendor.Status = VendorStatus.Suspended;
        vendor.IsActive = false;
        vendor.SuspendedAt = DateTime.UtcNow;
        vendor.SuspendedBy = User.Identity?.Name;
        vendor.SuspensionReason = reason ?? "No reason provided";
        vendor.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Log admin action to audit log
        var details = $"Vendor '{vendor.Name}' was suspended (previous status: {previousStatus}).";
        if (!string.IsNullOrWhiteSpace(reason))
        {
            details += $" Reason: {reason}";
        }

        await _auditLogService.LogActionAsync(
            action: "VendorSuspended",
            entityType: "Vendor",
            entityId: vendor.Id,
            details: details
        );

        _logger.LogInformation("Vendor {VendorId} ({VendorName}) suspended by {User}. Reason: {Reason}", 
            vendor.Id, vendor.Name, User.Identity?.Name, reason ?? "No reason provided");

        TempData["Success"] = $"Vendor '{vendor.Name}' has been suspended.";
        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>
    /// Reactivate a suspended or rejected vendor
    /// Stage 4.2: Restore vendor to approved status
    /// </summary>
    [HttpPost("{id}/reactivate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reactivate(int id)
    {
        var vendor = await _context.Vendors.FindAsync(id);
        if (vendor == null)
            return NotFound();

        if (vendor.Status == VendorStatus.Approved)
        {
            TempData["Warning"] = "Vendor is already active.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var previousStatus = vendor.Status;
        vendor.Status = VendorStatus.Approved;
        vendor.IsActive = true;
        vendor.SuspensionReason = null;
        vendor.RejectionReason = null;
        vendor.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Log admin action to audit log
        await _auditLogService.LogActionAsync(
            action: "VendorReactivated",
            entityType: "Vendor",
            entityId: vendor.Id,
            details: $"Vendor '{vendor.Name}' was reactivated (previous status: {previousStatus})."
        );

        _logger.LogInformation("Vendor {VendorId} ({VendorName}) reactivated by {User}", 
            vendor.Id, vendor.Name, User.Identity?.Name);

        TempData["Success"] = $"Vendor '{vendor.Name}' has been reactivated.";
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
                .ThenInclude(o => o.User)
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
