using ElleganzaPlatform.Application.Common;
using ElleganzaPlatform.Application.Services;
using ElleganzaPlatform.Application.ViewModels.Store;
using ElleganzaPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ElleganzaPlatform.Infrastructure.Services.Application;

/// <summary>
/// Phase 3.2: Order Service for customer-facing order operations
/// Handles order retrieval for authenticated customers
/// Ensures customers can only access their own orders
/// </summary>
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

    /// <summary>
    /// Phase 3.2: Retrieves paginated list of customer orders
    /// Only returns orders belonging to the current authenticated user
    /// Orders are sorted by creation date (newest first)
    /// </summary>
    public async Task<CustomerOrderListViewModel> GetCustomerOrdersAsync(int page = 1, int pageSize = 10)
    {
        // Phase 3.2: Validation - Ensure page parameters are valid
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 50) pageSize = 10;

        // Phase 3.2: Access Control - Get current user ID
        var userId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            return new CustomerOrderListViewModel();
        }

        // Phase 3.2: Query orders for current user only (security isolation)
        var query = _context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.UserId == userId);

        var totalOrders = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalOrders / (double)pageSize);

        // Phase 3.2: Retrieve paginated orders
        var orders = await query
            .OrderByDescending(o => o.CreatedAt)  // Newest orders first
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

    /// <summary>
    /// Phase 3.2: Retrieves detailed information for a specific order
    /// Only returns order if it belongs to the current authenticated user (security)
    /// Includes order items with product information
    /// </summary>
    public async Task<CustomerOrderDetailsViewModel?> GetOrderDetailsAsync(int orderId)
    {
        // Phase 3.2: Access Control - Get current user ID
        var userId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userId))
            return null;

        // Phase 3.2: Query order with security check (user must own the order)
        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.Id == orderId && o.UserId == userId)  // Security: user isolation
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
                PaymentTransactionId = o.PaymentTransactionId,  // Phase 4: Include payment transaction ID
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
