using ElleganzaPlatform.Application.Common;
using ElleganzaPlatform.Application.Services;
using ElleganzaPlatform.Application.ViewModels.Admin;
using ElleganzaPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ElleganzaPlatform.Infrastructure.Services.Application;

public class VendorOrderService : IVendorOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public VendorOrderService(
        ApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<OrderListViewModel> GetVendorOrdersAsync(int page = 1, int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var vendorId = _currentUserService.VendorId;
        if (!vendorId.HasValue)
        {
            return new OrderListViewModel();
        }

        // Get orders that have items from this vendor
        var query = _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .Where(o => o.OrderItems.Any(oi => oi.VendorId == vendorId.Value));

        var totalOrders = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalOrders / (double)pageSize);

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OrderListItemViewModel
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CreatedAt = o.CreatedAt,
                CustomerEmail = o.User.Email ?? string.Empty,
                CustomerName = (o.User.FirstName ?? "") + " " + (o.User.LastName ?? ""),
                Status = o.Status,
                TotalAmount = o.OrderItems
                    .Where(oi => oi.VendorId == vendorId.Value)
                    .Sum(oi => oi.TotalPrice),
                ItemCount = o.OrderItems.Count(oi => oi.VendorId == vendorId.Value)
            })
            .ToListAsync();

        return new OrderListViewModel
        {
            Orders = orders,
            CurrentPage = page,
            TotalPages = totalPages,
            TotalOrders = totalOrders
        };
    }

    public async Task<AdminOrderDetailsViewModel?> GetVendorOrderDetailsAsync(int orderId)
    {
        var vendorId = _currentUserService.VendorId;
        if (!vendorId.HasValue)
            return null;

        var order = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems.Where(oi => oi.VendorId == vendorId.Value))
                .ThenInclude(oi => oi.Vendor)
            .Where(o => o.Id == orderId && o.OrderItems.Any(oi => oi.VendorId == vendorId.Value))
            .Select(o => new AdminOrderDetailsViewModel
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CreatedAt = o.CreatedAt,
                Status = o.Status,
                CustomerEmail = o.User.Email ?? string.Empty,
                CustomerName = (o.User.FirstName ?? "") + " " + (o.User.LastName ?? ""),
                // Only show vendor's items
                SubTotal = o.OrderItems
                    .Where(oi => oi.VendorId == vendorId.Value)
                    .Sum(oi => oi.TotalPrice),
                TaxAmount = 0, // Vendor doesn't see full tax breakdown
                ShippingAmount = 0, // Vendor doesn't see shipping
                TotalAmount = o.OrderItems
                    .Where(oi => oi.VendorId == vendorId.Value)
                    .Sum(oi => oi.TotalPrice),
                ShippingAddress = o.ShippingAddress,
                BillingAddress = o.BillingAddress,
                CustomerNotes = o.CustomerNotes,
                Items = o.OrderItems
                    .Where(oi => oi.VendorId == vendorId.Value)
                    .Select(oi => new AdminOrderItemViewModel
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.ProductName,
                        ProductSku = oi.ProductSku,
                        VendorName = oi.Vendor.Name,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.TotalPrice
                    })
            })
            .FirstOrDefaultAsync();

        return order;
    }
}
