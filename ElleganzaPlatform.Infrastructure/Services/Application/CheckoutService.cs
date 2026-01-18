using ElleganzaPlatform.Application.Common;
using ElleganzaPlatform.Application.Services;
using ElleganzaPlatform.Application.ViewModels.Store;
using ElleganzaPlatform.Domain.Entities;
using ElleganzaPlatform.Domain.Enums;
using ElleganzaPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ElleganzaPlatform.Infrastructure.Services.Application;

public class CheckoutService : ICheckoutService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IStoreContextService _storeContextService;
    private readonly ICartService _cartService;

    public CheckoutService(
        ApplicationDbContext context,
        ICurrentUserService currentUserService,
        IStoreContextService storeContextService,
        ICartService cartService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _storeContextService = storeContextService;
        _cartService = cartService;
    }

    public async Task<CheckoutViewModel?> GetCheckoutDataAsync()
    {
        // Only authenticated users can checkout
        if (string.IsNullOrEmpty(_currentUserService.UserId))
            return null;

        var cart = await _cartService.GetCartAsync();
        if (cart.Items.Count == 0)
            return null;

        // Get user details for pre-filling the form
        var user = await _context.Users
            .Where(u => u.Id == _currentUserService.UserId)
            .FirstOrDefaultAsync();

        if (user == null)
            return null;

        return new CheckoutViewModel
        {
            Cart = cart,
            ShippingAddress = new AddressViewModel
            {
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Phone = user.PhoneNumber ?? string.Empty
            }
        };
    }

    public async Task<OrderConfirmationViewModel?> PlaceOrderAsync(PlaceOrderRequest request)
    {
        // Only authenticated users can place orders
        if (string.IsNullOrEmpty(_currentUserService.UserId))
            return null;

        var cart = await _cartService.GetCartAsync();
        if (cart.Items.Count == 0)
            return null;

        var storeId = await _storeContextService.GetCurrentStoreIdAsync();
        if (!storeId.HasValue)
            return null;

        // Generate order number
        var orderNumber = await GenerateOrderNumberAsync();

        // Create order
        var order = new Order
        {
            StoreId = storeId.Value,
            UserId = _currentUserService.UserId,
            OrderNumber = orderNumber,
            Status = OrderStatus.Pending,
            SubTotal = cart.SubTotal,
            TaxAmount = cart.TaxAmount,
            ShippingAmount = cart.ShippingAmount,
            TotalAmount = cart.TotalAmount,
            ShippingAddress = request.ShippingAddress.FormattedAddress,
            BillingAddress = (request.BillingAddress ?? request.ShippingAddress).FormattedAddress,
            CustomerNotes = request.CustomerNotes
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Create order items
        foreach (var cartItem in cart.Items)
        {
            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                ProductId = cartItem.ProductId,
                VendorId = cartItem.VendorId,
                ProductName = cartItem.ProductName,
                ProductSku = cartItem.ProductSku,
                Quantity = cartItem.Quantity,
                UnitPrice = cartItem.UnitPrice,
                TotalPrice = cartItem.TotalPrice,
                VendorCommission = cartItem.TotalPrice * 0.15m // 15% commission (should be configurable)
            };

            _context.OrderItems.Add(orderItem);

            // Update product stock
            var product = await _context.Products.FindAsync(cartItem.ProductId);
            if (product != null)
            {
                product.StockQuantity -= cartItem.Quantity;
            }
        }

        await _context.SaveChangesAsync();

        // Clear cart after successful order
        await _cartService.ClearCartAsync();

        return new OrderConfirmationViewModel
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            OrderDate = order.CreatedAt,
            TotalAmount = order.TotalAmount,
            Status = order.Status.ToString()
        };
    }

    private async Task<string> GenerateOrderNumberAsync()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = new Random().Next(1000, 9999);
        var orderNumber = $"ORD-{timestamp}-{random}";

        // Ensure uniqueness
        while (await _context.Orders.AnyAsync(o => o.OrderNumber == orderNumber))
        {
            random = new Random().Next(1000, 9999);
            orderNumber = $"ORD-{timestamp}-{random}";
        }

        return orderNumber;
    }
}
