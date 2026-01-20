using ElleganzaPlatform.Application.Services;
using ElleganzaPlatform.Application.ViewModels.Store;
using ElleganzaPlatform.Domain.Enums;
using ElleganzaPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ElleganzaPlatform.Infrastructure.Services.Application;

public class CustomerService : ICustomerService
{
    private readonly ApplicationDbContext _context;

    public CustomerService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerAccountViewModel?> GetCustomerAccountAsync(string userId)
    {
        var user = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => new CustomerAccountViewModel
            {
                Email = u.Email ?? string.Empty,
                FirstName = u.FirstName,
                LastName = u.LastName,
                PhoneNumber = u.PhoneNumber,
                TotalOrders = u.Orders.Count,
                WishlistCount = 0 // Placeholder for future wishlist implementation
            })
            .FirstOrDefaultAsync();

        return user;
    }

    public async Task<CustomerOrdersViewModel> GetCustomerOrdersAsync(string userId, int page = 1, int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var query = _context.Orders
            .Where(o => o.UserId == userId);

        var totalOrders = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalOrders / (double)pageSize);

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OrderSummaryViewModel
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CreatedAt = o.CreatedAt,
                Status = o.Status.ToString(),
                TotalAmount = o.TotalAmount,
                ItemCount = o.OrderItems.Count,
                CanBePaid = o.Status == OrderStatus.Pending  // Phase 4: Payment integration
            })
            .ToListAsync();

        return new CustomerOrdersViewModel
        {
            Orders = orders,
            CurrentPage = page,
            TotalPages = totalPages,
            TotalOrders = totalOrders
        };
    }

    public async Task<OrderDetailsViewModel?> GetOrderDetailsAsync(int orderId, string userId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.Id == orderId && o.UserId == userId)
            .Select(o => new OrderDetailsViewModel
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CreatedAt = o.CreatedAt,
                Status = o.Status.ToString(),
                SubTotal = o.SubTotal,
                TaxAmount = o.TaxAmount,
                ShippingAmount = o.ShippingAmount,
                TotalAmount = o.TotalAmount,
                ShippingAddress = o.ShippingAddress,
                BillingAddress = o.BillingAddress,
                PaymentTransactionId = o.PaymentTransactionId,  // Phase 4: Payment integration
                CanBePaid = o.Status == OrderStatus.Pending,  // Phase 4: Payment integration
                Items = o.OrderItems.Select(oi => new OrderItemViewModel
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.ProductName,
                    ProductSku = oi.ProductSku,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice
                })
            })
            .FirstOrDefaultAsync();

        return order;
    }
}
