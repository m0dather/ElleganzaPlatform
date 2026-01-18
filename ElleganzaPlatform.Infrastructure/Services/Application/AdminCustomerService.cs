using ElleganzaPlatform.Application.Services;
using ElleganzaPlatform.Application.ViewModels.Admin;
using ElleganzaPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ElleganzaPlatform.Infrastructure.Services.Application;

public class AdminCustomerService : IAdminCustomerService
{
    private readonly ApplicationDbContext _context;

    public AdminCustomerService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerListViewModel> GetCustomersAsync(int page = 1, int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var query = _context.Users
            .Where(u => u.IsActive);

        var totalCustomers = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCustomers / (double)pageSize);

        var customers = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new CustomerListItemViewModel
            {
                Id = u.Id,
                Email = u.Email ?? string.Empty,
                FirstName = u.FirstName,
                LastName = u.LastName,
                PhoneNumber = u.PhoneNumber,
                CreatedAt = u.CreatedAt,
                TotalOrders = u.Orders.Count,
                TotalSpent = u.Orders.Sum(o => o.TotalAmount),
                IsActive = u.IsActive
            })
            .ToListAsync();

        return new CustomerListViewModel
        {
            Customers = customers,
            CurrentPage = page,
            TotalPages = totalPages,
            TotalCustomers = totalCustomers
        };
    }

    public async Task<CustomerDetailsViewModel?> GetCustomerDetailsAsync(string customerId)
    {
        var customer = await _context.Users
            .Where(u => u.Id == customerId)
            .Select(u => new CustomerDetailsViewModel
            {
                Id = u.Id,
                Email = u.Email ?? string.Empty,
                FirstName = u.FirstName,
                LastName = u.LastName,
                PhoneNumber = u.PhoneNumber,
                CreatedAt = u.CreatedAt,
                IsActive = u.IsActive,
                TotalOrders = u.Orders.Count,
                TotalSpent = u.Orders.Sum(o => o.TotalAmount),
                RecentOrders = u.Orders
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(10)
                    .Select(o => new CustomerOrderViewModel
                    {
                        Id = o.Id,
                        OrderNumber = o.OrderNumber,
                        CreatedAt = o.CreatedAt,
                        Status = o.Status.ToString(),
                        TotalAmount = o.TotalAmount
                    })
            })
            .FirstOrDefaultAsync();

        return customer;
    }
}
