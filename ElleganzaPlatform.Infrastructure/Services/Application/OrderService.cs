using ElleganzaPlatform.Application.Common;
using ElleganzaPlatform.Application.Services;
using ElleganzaPlatform.Application.ViewModels.Store;
using ElleganzaPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ElleganzaPlatform.Infrastructure.Services.Application;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public OrderService(
        ApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<CustomerOrderListViewModel> GetCustomerOrdersAsync(int page = 1, int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 50) pageSize = 10;

        var userId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            return new CustomerOrderListViewModel();
        }

        var query = _context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.UserId == userId);

        var totalOrders = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalOrders / (double)pageSize);

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new CustomerOrderItemViewModel
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                OrderDate = o.CreatedAt,
                Status = o.Status,
                TotalAmount = o.TotalAmount,
                ItemCount = o.OrderItems.Count
            })
            .ToListAsync();

        return new CustomerOrderListViewModel
        {
            Orders = orders,
            CurrentPage = page,
            TotalPages = totalPages,
            TotalOrders = totalOrders
        };
    }

    public async Task<CustomerOrderDetailsViewModel?> GetOrderDetailsAsync(int orderId)
    {
        var userId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userId))
            return null;

        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.Id == orderId && o.UserId == userId)
            .Select(o => new CustomerOrderDetailsViewModel
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                OrderDate = o.CreatedAt,
                Status = o.Status,
                SubTotal = o.SubTotal,
                TaxAmount = o.TaxAmount,
                ShippingAmount = o.ShippingAmount,
                TotalAmount = o.TotalAmount,
                ShippingAddress = o.ShippingAddress,
                BillingAddress = o.BillingAddress,
                CustomerNotes = o.CustomerNotes,
                Items = o.OrderItems.Select(oi => new CustomerOrderItemDetailsViewModel
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.ProductName,
                    ProductSku = oi.ProductSku,
                    ProductImage = oi.Product.MainImage,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice
                }).ToList()
            })
            .FirstOrDefaultAsync();

        return order;
    }
}
