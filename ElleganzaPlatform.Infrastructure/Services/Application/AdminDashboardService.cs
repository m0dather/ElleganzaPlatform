using ElleganzaPlatform.Application.Services;
using ElleganzaPlatform.Application.ViewModels.Admin;
using ElleganzaPlatform.Domain.Enums;
using ElleganzaPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ElleganzaPlatform.Infrastructure.Services.Application;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly ApplicationDbContext _context;

    public AdminDashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardViewModel> GetDashboardDataAsync()
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var startOfToday = now.Date;

        // Total sales (completed orders only)
        var totalSales = await _context.Orders
            .Where(o => o.Status == OrderStatus.Delivered)
            .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

        // Total orders
        var totalOrders = await _context.Orders.CountAsync();

        // Active orders (not delivered, not cancelled, not refunded)
        var activeOrders = await _context.Orders
            .CountAsync(o => o.Status != OrderStatus.Delivered 
                          && o.Status != OrderStatus.Cancelled 
                          && o.Status != OrderStatus.Refunded);

        // Orders processed today
        var ordersProcessedToday = await _context.Orders
            .CountAsync(o => o.CreatedAt >= startOfToday);

        // Total products
        var totalProducts = await _context.Products
            .CountAsync(p => p.Status == ProductStatus.Active);

        // Total customers (users with Customer role - simplified count)
        var totalCustomers = await _context.Users
            .CountAsync(u => u.IsActive);

        // New customers this month
        var newCustomersThisMonth = await _context.Users
            .CountAsync(u => u.CreatedAt >= startOfMonth);

        return new DashboardViewModel
        {
            Metrics = new DashboardMetrics
            {
                TotalSales = totalSales,
                TotalOrders = totalOrders,
                TotalProducts = totalProducts,
                TotalCustomers = totalCustomers,
                NewCustomersThisMonth = newCustomersThisMonth,
                OrdersProcessedToday = ordersProcessedToday,
                ActiveOrders = activeOrders
            }
        };
    }
}
