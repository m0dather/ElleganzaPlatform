using ElleganzaPlatform.Application.Services;
using ElleganzaPlatform.Application.ViewModels.Admin;
using ElleganzaPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ElleganzaPlatform.Infrastructure.Services.Application;

public class AdminOrderService : IAdminOrderService
{
    private readonly ApplicationDbContext _context;

    public AdminOrderService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OrderListViewModel> GetOrdersAsync(int page = 1, int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var query = _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems);

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
                TotalAmount = o.TotalAmount,
                ItemCount = o.OrderItems.Count
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

    public async Task<AdminOrderDetailsViewModel?> GetOrderDetailsAsync(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Vendor)
            .Where(o => o.Id == orderId)
            .Select(o => new AdminOrderDetailsViewModel
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CreatedAt = o.CreatedAt,
                Status = o.Status,
                CustomerEmail = o.User.Email ?? string.Empty,
                CustomerName = (o.User.FirstName ?? "") + " " + (o.User.LastName ?? ""),
                SubTotal = o.SubTotal,
                TaxAmount = o.TaxAmount,
                ShippingAmount = o.ShippingAmount,
                TotalAmount = o.TotalAmount,
                ShippingAddress = o.ShippingAddress,
                BillingAddress = o.BillingAddress,
                CustomerNotes = o.CustomerNotes,
                Items = o.OrderItems.Select(oi => new AdminOrderItemViewModel
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
