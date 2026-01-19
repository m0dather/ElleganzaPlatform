using ElleganzaPlatform.Application.Common;
using ElleganzaPlatform.Application.Services;
using ElleganzaPlatform.Application.ViewModels.Store;
using ElleganzaPlatform.Domain.Entities;
using ElleganzaPlatform.Domain.Enums;
using ElleganzaPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ElleganzaPlatform.Infrastructure.Services.Application;

/// <summary>
/// Phase 3.2: Checkout Service
/// Handles the secure checkout process for authenticated users
/// Converts shopping cart into orders with proper store and vendor isolation
/// </summary>
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

    /// <summary>
    /// Phase 3.2: Retrieves checkout data for authenticated users
    /// Loads cart items and pre-fills customer information
    /// Returns null if user is not authenticated or cart is empty
    /// </summary>
    public async Task<CheckoutViewModel?> GetCheckoutDataAsync()
    {
        // Phase 3.2: Access Control - Only authenticated users can checkout
        if (string.IsNullOrEmpty(_currentUserService.UserId))
            return null;

        // Phase 3.2: Load cart using CartService (single source of truth)
        var cart = await _cartService.GetCartAsync();
        
        // Phase 3.2: Validation - Cart must have items to proceed
        if (cart.Items.Count == 0)
            return null;

        // Phase 3.2: Load user details for pre-filling the checkout form
        var user = await _context.Users
            .Where(u => u.Id == _currentUserService.UserId)
            .FirstOrDefaultAsync();

        if (user == null)
            return null;

        // Phase 3.2: Return checkout view model with cart and user data
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

    /// <summary>
    /// Phase 3.2: Places an order from the current cart
    /// Creates Order and OrderItems with proper StoreId, UserId, and VendorId isolation
    /// Validates stock availability and prevents negative stock
    /// Updates product stock quantities atomically within a transaction
    /// Clears cart after successful order creation
    /// Returns null if validation fails or stock is insufficient
    /// </summary>
    public async Task<OrderConfirmationViewModel?> PlaceOrderAsync(PlaceOrderRequest request)
    {
        // Phase 3.2: Access Control - Only authenticated users can place orders
        if (string.IsNullOrEmpty(_currentUserService.UserId))
            return null;

        // Phase 3.2: Load cart using CartService (single source of truth)
        var cart = await _cartService.GetCartAsync();
        
        // Phase 3.2: Validation - Cart must have items to place an order
        if (cart.Items.Count == 0)
            return null;

        // Phase 3.2: Store Isolation - Get current store context
        var storeId = await _storeContextService.GetCurrentStoreIdAsync();
        if (!storeId.HasValue)
            return null;

        // Phase 3.2: CRITICAL - Use transaction to ensure atomicity
        // All operations (order creation, stock update, cart clear) must succeed or fail together
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Phase 3.2: Stock Validation - Re-validate stock availability before creating order
            // This prevents overselling as stock could have changed since cart was loaded
            // Store product references to avoid duplicate database calls during stock update
            var productsMap = new Dictionary<int, Product>();
            
            foreach (var cartItem in cart.Items)
            {
                var product = await _context.Products.FindAsync(cartItem.ProductId);
                if (product == null)
                {
                    // Product no longer exists - abort order
                    return null;
                }

                // Phase 3.2: Prevent negative stock - Validate sufficient quantity available
                if (product.StockQuantity < cartItem.Quantity)
                {
                    // Insufficient stock - abort order
                    return null;
                }
                
                // Store product reference for later use
                productsMap[cartItem.ProductId] = product;
            }

            // Phase 3.2: Generate unique order number
            var orderNumber = await GenerateOrderNumberAsync();

            // Phase 3.2: Create Order entity with customer and store information
            var order = new Order
            {
                StoreId = storeId.Value,              // Store isolation
                UserId = _currentUserService.UserId,   // Customer assignment
                OrderNumber = orderNumber,
                Status = OrderStatus.Pending,          // Initial status as per requirements
                SubTotal = cart.SubTotal,
                TaxAmount = cart.TaxAmount,
                ShippingAmount = cart.ShippingAmount,
                TotalAmount = cart.TotalAmount,
                ShippingAddress = request.ShippingAddress.FormattedAddress,
                BillingAddress = (request.BillingAddress ?? request.ShippingAddress).FormattedAddress,
                CustomerNotes = request.CustomerNotes
            };

            _context.Orders.Add(order);
            
            // Phase 3.2: Flush Order to get OrderId for OrderItems
            await _context.SaveChangesAsync();

            // Phase 3.2: Create OrderItems from cart items with vendor and store isolation
            foreach (var cartItem in cart.Items)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = cartItem.ProductId,
                    VendorId = cartItem.VendorId,      // Vendor isolation per item
                    StoreId = cartItem.StoreId,        // Store isolation per item
                    ProductName = cartItem.ProductName,
                    ProductSku = cartItem.ProductSku,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.UnitPrice,
                    TotalPrice = cartItem.TotalPrice,
                    VendorCommission = cartItem.TotalPrice * 0.15m // 15% commission (configurable)
                };

                _context.OrderItems.Add(orderItem);

                // Phase 3.2: Update product stock quantity atomically
                // Reuse product reference from validation to avoid duplicate database call
                if (productsMap.TryGetValue(cartItem.ProductId, out var product))
                {
                    product.StockQuantity -= cartItem.Quantity;
                }
            }

            // Phase 3.2: Persist all changes in ONE atomic operation
            await _context.SaveChangesAsync();

            // Phase 3.2: Commit transaction - All operations successful
            await transaction.CommitAsync();

            // Phase 3.2: Clear cart after successful order (outside transaction)
            // Cart clearing is non-critical - order is already persisted
            // If this fails, user can manually clear cart later
            try
            {
                await _cartService.ClearCartAsync();
            }
            catch
            {
                // Log error but don't fail the order - order is already created
                // User can still view and manage their order
            }

            // Phase 3.2: Return order confirmation for success page
            return new OrderConfirmationViewModel
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                OrderDate = order.CreatedAt,
                TotalAmount = order.TotalAmount,
                Status = order.Status.ToString()
            };
        }
        catch (Exception)
        {
            // Phase 3.2: Rollback transaction on any error
            // Ensures no partial orders or incorrect stock levels
            await transaction.RollbackAsync();
            return null;
        }
    }

    /// <summary>
    /// Phase 3.2: Generates a unique order number
    /// Format: ORD-{timestamp}-{random}
    /// Ensures uniqueness by checking against existing orders
    /// </summary>
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
