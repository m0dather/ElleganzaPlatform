using ElleganzaPlatform.Application.Services;
using ElleganzaPlatform.Application.ViewModels.Store;
using ElleganzaPlatform.Domain.Entities;
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

    public async Task<AddressListViewModel> GetCustomerAddressesAsync(string userId)
    {
        var addresses = await _context.CustomerAddresses
            .Where(a => a.UserId == userId && !a.IsDeleted)
            .OrderByDescending(a => a.IsDefaultShipping)
            .ThenByDescending(a => a.IsDefaultBilling)
            .ThenByDescending(a => a.CreatedAt)
            .Select(a => new CustomerAddressViewModel
            {
                Id = a.Id,
                FirstName = a.FirstName,
                LastName = a.LastName,
                Phone = a.Phone,
                AddressLine1 = a.AddressLine1,
                AddressLine2 = a.AddressLine2,
                City = a.City,
                State = a.State,
                PostalCode = a.PostalCode,
                Country = a.Country,
                IsDefaultShipping = a.IsDefaultShipping,
                IsDefaultBilling = a.IsDefaultBilling
            })
            .ToListAsync();

        return new AddressListViewModel
        {
            Addresses = addresses
        };
    }

    public async Task<CustomerAddressViewModel?> GetCustomerAddressAsync(int addressId, string userId)
    {
        var address = await _context.CustomerAddresses
            .Where(a => a.Id == addressId && a.UserId == userId && !a.IsDeleted)
            .Select(a => new CustomerAddressViewModel
            {
                Id = a.Id,
                FirstName = a.FirstName,
                LastName = a.LastName,
                Phone = a.Phone,
                AddressLine1 = a.AddressLine1,
                AddressLine2 = a.AddressLine2,
                City = a.City,
                State = a.State,
                PostalCode = a.PostalCode,
                Country = a.Country,
                IsDefaultShipping = a.IsDefaultShipping,
                IsDefaultBilling = a.IsDefaultBilling
            })
            .FirstOrDefaultAsync();

        return address;
    }

    public async Task<int> CreateCustomerAddressAsync(CustomerAddressViewModel model, string userId)
    {
        // If this is set as default shipping, unset any existing default shipping address
        if (model.IsDefaultShipping)
        {
            var existingDefaultShipping = await _context.CustomerAddresses
                .Where(a => a.UserId == userId && a.IsDefaultShipping && !a.IsDeleted)
                .ToListAsync();
            
            foreach (var addr in existingDefaultShipping)
            {
                addr.IsDefaultShipping = false;
            }
        }

        // If this is set as default billing, unset any existing default billing address
        if (model.IsDefaultBilling)
        {
            var existingDefaultBilling = await _context.CustomerAddresses
                .Where(a => a.UserId == userId && a.IsDefaultBilling && !a.IsDeleted)
                .ToListAsync();
            
            foreach (var addr in existingDefaultBilling)
            {
                addr.IsDefaultBilling = false;
            }
        }

        var address = new CustomerAddress
        {
            UserId = userId,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Phone = model.Phone,
            AddressLine1 = model.AddressLine1,
            AddressLine2 = model.AddressLine2,
            City = model.City,
            State = model.State,
            PostalCode = model.PostalCode,
            Country = model.Country,
            IsDefaultShipping = model.IsDefaultShipping,
            IsDefaultBilling = model.IsDefaultBilling
        };

        _context.CustomerAddresses.Add(address);
        await _context.SaveChangesAsync();

        return address.Id;
    }

    public async Task<bool> UpdateCustomerAddressAsync(CustomerAddressViewModel model, string userId)
    {
        var address = await _context.CustomerAddresses
            .FirstOrDefaultAsync(a => a.Id == model.Id && a.UserId == userId && !a.IsDeleted);

        if (address == null)
            return false;

        // If this is set as default shipping, unset any existing default shipping address
        if (model.IsDefaultShipping && !address.IsDefaultShipping)
        {
            var existingDefaultShipping = await _context.CustomerAddresses
                .Where(a => a.UserId == userId && a.Id != model.Id && a.IsDefaultShipping && !a.IsDeleted)
                .ToListAsync();
            
            foreach (var addr in existingDefaultShipping)
            {
                addr.IsDefaultShipping = false;
            }
        }

        // If this is set as default billing, unset any existing default billing address
        if (model.IsDefaultBilling && !address.IsDefaultBilling)
        {
            var existingDefaultBilling = await _context.CustomerAddresses
                .Where(a => a.UserId == userId && a.Id != model.Id && a.IsDefaultBilling && !a.IsDeleted)
                .ToListAsync();
            
            foreach (var addr in existingDefaultBilling)
            {
                addr.IsDefaultBilling = false;
            }
        }

        // Update address fields
        address.FirstName = model.FirstName;
        address.LastName = model.LastName;
        address.Phone = model.Phone;
        address.AddressLine1 = model.AddressLine1;
        address.AddressLine2 = model.AddressLine2;
        address.City = model.City;
        address.State = model.State;
        address.PostalCode = model.PostalCode;
        address.Country = model.Country;
        address.IsDefaultShipping = model.IsDefaultShipping;
        address.IsDefaultBilling = model.IsDefaultBilling;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCustomerAddressAsync(int addressId, string userId)
    {
        // Check if user has at least 2 active addresses (cannot delete the last one)
        var addressCount = await _context.CustomerAddresses
            .CountAsync(a => a.UserId == userId && !a.IsDeleted);

        if (addressCount <= 1)
            return false;

        var address = await _context.CustomerAddresses
            .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId && !a.IsDeleted);

        if (address == null)
            return false;

        // Soft delete
        address.IsDeleted = true;

        // If this was a default address, assign default to another address
        if (address.IsDefaultShipping || address.IsDefaultBilling)
        {
            var nextAddress = await _context.CustomerAddresses
                .Where(a => a.UserId == userId && a.Id != addressId && !a.IsDeleted)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync();

            if (nextAddress != null)
            {
                if (address.IsDefaultShipping)
                    nextAddress.IsDefaultShipping = true;
                if (address.IsDefaultBilling)
                    nextAddress.IsDefaultBilling = true;
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }
}
