using ElleganzaPlatform.Application.Common;
using ElleganzaPlatform.Application.Services;
using ElleganzaPlatform.Application.ViewModels.Store;
using ElleganzaPlatform.Domain.Entities;
using ElleganzaPlatform.Domain.Enums;
using ElleganzaPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ElleganzaPlatform.Infrastructure.Services.Application;

/// <summary>
/// Service for managing checkout sessions (pre-order state)
/// Implements the new checkout flow: Cart → CheckoutSession → Payment → Order
/// </summary>
public class CheckoutSessionService : ICheckoutSessionService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IStoreContextService _storeContextService;
    private readonly ICartService _cartService;
    private readonly ILogger<CheckoutSessionService> _logger;

    public CheckoutSessionService(
        ApplicationDbContext context,
        ICurrentUserService currentUserService,
        IStoreContextService storeContextService,
        ICartService cartService,
        ILogger<CheckoutSessionService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _storeContextService = storeContextService;
        _cartService = cartService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new checkout session from the current cart
    /// </summary>
    public async Task<CheckoutSessionViewModel?> CreateCheckoutSessionAsync(CreateCheckoutSessionRequest request)
    {
        // Access Control - Only authenticated users can checkout
        if (string.IsNullOrEmpty(_currentUserService.UserId))
            return null;

        // Load cart using CartService (single source of truth)
        var cart = await _cartService.GetCartAsync();
        
        // Validation - Cart must have items to proceed
        if (cart.Items.Count == 0)
            return null;

        // Store Isolation - Get current store context
        var storeId = await _storeContextService.GetCurrentStoreIdAsync();
        if (!storeId.HasValue)
            return null;

        // Create cart snapshot (JSON)
        var cartSnapshot = new
        {
            Items = cart.Items.Select(i => new
            {
                i.ProductId,
                i.ProductName,
                i.ProductSku,
                i.UnitPrice,
                i.Quantity,
                i.TotalPrice,
                i.VendorId,
                i.StoreId
            }).ToList(),
            cart.SubTotal,
            cart.TaxAmount,
            ShippingAmount = 0m, // Will be set later
            cart.TotalAmount
        };

        var cartSnapshotJson = JsonSerializer.Serialize(cartSnapshot);

        // Create checkout session
        var checkoutSession = new CheckoutSession
        {
            StoreId = storeId.Value,
            UserId = _currentUserService.UserId,
            CartSnapshot = cartSnapshotJson,
            ShippingMethod = "Standard", // Default, will be updated
            ShippingCost = 0m, // Will be updated when shipping method is selected
            PaymentMethod = Domain.Enums.PaymentMethod.Online, // Default
            Status = CheckoutSessionStatus.Draft,
            ExpiresAt = DateTime.UtcNow.AddHours(2), // 2 hour expiration
            ShippingAddress = request.ShippingAddress.FormattedAddress,
            BillingAddress = (request.BillingAddress ?? request.ShippingAddress).FormattedAddress,
            CustomerNotes = request.CustomerNotes
        };

        _context.CheckoutSessions.Add(checkoutSession);
        await _context.SaveChangesAsync();

        _logger.LogInformation("CheckoutSession {CheckoutSessionId} created for User {UserId}", 
            checkoutSession.Id, _currentUserService.UserId);

        return new CheckoutSessionViewModel
        {
            Id = checkoutSession.Id,
            Status = checkoutSession.Status,
            PaymentMethod = checkoutSession.PaymentMethod,
            ShippingMethod = checkoutSession.ShippingMethod,
            ShippingCost = checkoutSession.ShippingCost,
            ExpiresAt = checkoutSession.ExpiresAt,
            Cart = cart,
            ShippingAddress = checkoutSession.ShippingAddress,
            BillingAddress = checkoutSession.BillingAddress,
            CustomerNotes = checkoutSession.CustomerNotes
        };
    }

    /// <summary>
    /// Gets an existing checkout session by ID
    /// </summary>
    public async Task<CheckoutSessionViewModel?> GetCheckoutSessionAsync(int sessionId)
    {
        if (string.IsNullOrEmpty(_currentUserService.UserId))
            return null;

        var checkoutSession = await _context.CheckoutSessions
            .FirstOrDefaultAsync(cs => cs.Id == sessionId && cs.UserId == _currentUserService.UserId);

        if (checkoutSession == null)
            return null;

        // Deserialize cart snapshot
        var cartSnapshot = JsonSerializer.Deserialize<CartSnapshotData>(checkoutSession.CartSnapshot);
        if (cartSnapshot == null)
            return null;

        // Convert cart snapshot to view model
        var cart = new CartViewModel
        {
            Items = cartSnapshot.Items.Select(i => new CartItemViewModel
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                ProductSku = i.ProductSku,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
                VendorId = i.VendorId,
                StoreId = i.StoreId
            }).ToList(),
            SubTotal = cartSnapshot.SubTotal,
            TaxAmount = cartSnapshot.TaxAmount,
            ShippingAmount = checkoutSession.ShippingCost,
            TotalAmount = cartSnapshot.TotalAmount + checkoutSession.ShippingCost
        };

        return new CheckoutSessionViewModel
        {
            Id = checkoutSession.Id,
            Status = checkoutSession.Status,
            PaymentMethod = checkoutSession.PaymentMethod,
            ShippingMethod = checkoutSession.ShippingMethod,
            ShippingCost = checkoutSession.ShippingCost,
            ExpiresAt = checkoutSession.ExpiresAt,
            Cart = cart,
            ShippingAddress = checkoutSession.ShippingAddress,
            BillingAddress = checkoutSession.BillingAddress,
            CustomerNotes = checkoutSession.CustomerNotes
        };
    }

    /// <summary>
    /// Updates the shipping method and cost for a checkout session
    /// </summary>
    public async Task<bool> UpdateShippingMethodAsync(SelectShippingMethodRequest request)
    {
        if (string.IsNullOrEmpty(_currentUserService.UserId))
            return false;

        var checkoutSession = await _context.CheckoutSessions
            .FirstOrDefaultAsync(cs => cs.Id == request.CheckoutSessionId && 
                                     cs.UserId == _currentUserService.UserId &&
                                     cs.Status == CheckoutSessionStatus.Draft);

        if (checkoutSession == null)
            return false;

        checkoutSession.ShippingMethod = request.ShippingMethod;
        checkoutSession.ShippingCost = request.ShippingCost;

        await _context.SaveChangesAsync();

        _logger.LogInformation("CheckoutSession {CheckoutSessionId} shipping method updated to {ShippingMethod} ({ShippingCost})", 
            request.CheckoutSessionId, request.ShippingMethod, request.ShippingCost);

        return true;
    }

    /// <summary>
    /// Updates the payment method for a checkout session
    /// </summary>
    public async Task<bool> UpdatePaymentMethodAsync(SelectPaymentMethodRequest request)
    {
        if (string.IsNullOrEmpty(_currentUserService.UserId))
            return false;

        var checkoutSession = await _context.CheckoutSessions
            .FirstOrDefaultAsync(cs => cs.Id == request.CheckoutSessionId && 
                                     cs.UserId == _currentUserService.UserId &&
                                     cs.Status == CheckoutSessionStatus.Draft);

        if (checkoutSession == null)
            return false;

        checkoutSession.PaymentMethod = request.PaymentMethod;

        // If COD selected, mark session as COD status
        if (request.PaymentMethod == Domain.Enums.PaymentMethod.CashOnDelivery)
        {
            checkoutSession.Status = CheckoutSessionStatus.COD;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("CheckoutSession {CheckoutSessionId} payment method updated to {PaymentMethod}", 
            request.CheckoutSessionId, request.PaymentMethod);

        return true;
    }

    /// <summary>
    /// Creates an order from a checkout session
    /// Called after payment success (online) or immediately (COD)
    /// </summary>
    public async Task<OrderConfirmationViewModel?> CreateOrderFromSessionAsync(int sessionId)
    {
        if (string.IsNullOrEmpty(_currentUserService.UserId))
            return null;

        var checkoutSession = await _context.CheckoutSessions
            .FirstOrDefaultAsync(cs => cs.Id == sessionId && cs.UserId == _currentUserService.UserId);

        if (checkoutSession == null)
            return null;

        // Validate session is in correct status
        if (checkoutSession.Status != CheckoutSessionStatus.Paid && 
            checkoutSession.Status != CheckoutSessionStatus.COD)
        {
            _logger.LogWarning("Cannot create order from CheckoutSession {CheckoutSessionId} with status {Status}", 
                sessionId, checkoutSession.Status);
            return null;
        }

        // Prevent duplicate order creation
        if (checkoutSession.OrderId.HasValue)
        {
            _logger.LogWarning("CheckoutSession {CheckoutSessionId} already has Order {OrderId}", 
                sessionId, checkoutSession.OrderId.Value);
            
            // Return existing order confirmation
            var existingOrder = await _context.Orders.FindAsync(checkoutSession.OrderId.Value);
            if (existingOrder != null)
            {
                return new OrderConfirmationViewModel
                {
                    OrderId = existingOrder.Id,
                    OrderNumber = existingOrder.OrderNumber,
                    OrderDate = existingOrder.CreatedAt,
                    TotalAmount = existingOrder.TotalAmount,
                    Status = existingOrder.Status.ToString()
                };
            }
        }

        // Deserialize cart snapshot
        var cartSnapshot = JsonSerializer.Deserialize<CartSnapshotData>(checkoutSession.CartSnapshot);
        if (cartSnapshot == null)
            return null;

        // Use transaction to ensure atomicity
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Validate stock availability
            var productsMap = new Dictionary<int, Product>();
            
            foreach (var item in cartSnapshot.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null)
                {
                    _logger.LogWarning("Product {ProductId} no longer exists", item.ProductId);
                    return null;
                }

                if (product.StockQuantity < item.Quantity)
                {
                    _logger.LogWarning("Insufficient stock for Product {ProductId}", item.ProductId);
                    return null;
                }
                
                productsMap[item.ProductId] = product;
            }

            // Generate unique order number
            var orderNumber = await GenerateOrderNumberAsync();

            // Create Order entity
            var order = new Order
            {
                StoreId = checkoutSession.StoreId,
                UserId = checkoutSession.UserId,
                OrderNumber = orderNumber,
                // Set status based on payment method
                Status = checkoutSession.Status == CheckoutSessionStatus.Paid 
                    ? OrderStatus.Paid 
                    : OrderStatus.PendingPayment,
                SubTotal = cartSnapshot.SubTotal,
                TaxAmount = cartSnapshot.TaxAmount,
                ShippingAmount = checkoutSession.ShippingCost,
                TotalAmount = cartSnapshot.TotalAmount + checkoutSession.ShippingCost,
                ShippingAddress = checkoutSession.ShippingAddress,
                BillingAddress = checkoutSession.BillingAddress,
                CustomerNotes = checkoutSession.CustomerNotes,
                PaymentTransactionId = checkoutSession.PaymentIntentId
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Create OrderItems
            foreach (var item in cartSnapshot.Items)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    VendorId = item.VendorId,
                    StoreId = item.StoreId,
                    ProductName = item.ProductName,
                    ProductSku = item.ProductSku,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice,
                    VendorCommission = item.TotalPrice * 0.15m
                };

                _context.OrderItems.Add(orderItem);

                // Update product stock
                if (productsMap.TryGetValue(item.ProductId, out var product))
                {
                    product.StockQuantity -= item.Quantity;
                }
            }

            // Update checkout session
            checkoutSession.OrderId = order.Id;
            checkoutSession.Status = CheckoutSessionStatus.Completed;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Clear cart after successful order
            try
            {
                await _cartService.ClearCartAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to clear cart after order creation");
            }

            _logger.LogInformation("Order {OrderId} created from CheckoutSession {CheckoutSessionId}", 
                order.Id, sessionId);

            return new OrderConfirmationViewModel
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                OrderDate = order.CreatedAt,
                TotalAmount = order.TotalAmount,
                Status = order.Status.ToString()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order from CheckoutSession {CheckoutSessionId}", sessionId);
            await transaction.RollbackAsync();
            return null;
        }
    }

    /// <summary>
    /// Updates checkout session status to Paid after successful payment
    /// </summary>
    public async Task<bool> UpdateSessionStatusToPaidAsync(string paymentIntentId)
    {
        var checkoutSession = await _context.CheckoutSessions
            .FirstOrDefaultAsync(cs => cs.PaymentIntentId == paymentIntentId);

        if (checkoutSession == null)
            return false;

        checkoutSession.Status = CheckoutSessionStatus.Paid;
        await _context.SaveChangesAsync();

        _logger.LogInformation("CheckoutSession {CheckoutSessionId} marked as Paid", checkoutSession.Id);

        return true;
    }

    /// <summary>
    /// Expires old checkout sessions
    /// </summary>
    public async Task ExpireOldSessionsAsync()
    {
        var expiredSessions = await _context.CheckoutSessions
            .Where(cs => cs.ExpiresAt < DateTime.UtcNow && 
                        cs.Status == CheckoutSessionStatus.Draft)
            .ToListAsync();

        foreach (var session in expiredSessions)
        {
            session.Status = CheckoutSessionStatus.Expired;
        }

        if (expiredSessions.Any())
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Expired {Count} checkout sessions", expiredSessions.Count);
        }
    }

    /// <summary>
    /// Gets available shipping methods
    /// </summary>
    public async Task<List<ShippingMethodOption>> GetAvailableShippingMethodsAsync()
    {
        // TODO: In production, this should come from configuration or database
        return await Task.FromResult(new List<ShippingMethodOption>
        {
            new ShippingMethodOption
            {
                Name = "Standard",
                Description = "5-7 business days",
                Cost = 5.00m,
                EstimatedDelivery = "5-7 business days"
            },
            new ShippingMethodOption
            {
                Name = "Express",
                Description = "2-3 business days",
                Cost = 15.00m,
                EstimatedDelivery = "2-3 business days"
            },
            new ShippingMethodOption
            {
                Name = "Next Day",
                Description = "1 business day",
                Cost = 25.00m,
                EstimatedDelivery = "1 business day"
            }
        });
    }

    private async Task<string> GenerateOrderNumberAsync()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper();
        var orderNumber = $"ORD-{timestamp}-{random}";

        // Ensure uniqueness (very unlikely with GUID-based suffix)
        while (await _context.Orders.AnyAsync(o => o.OrderNumber == orderNumber))
        {
            random = Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper();
            orderNumber = $"ORD-{timestamp}-{random}";
        }

        return orderNumber;
    }
}

/// <summary>
/// Helper class for cart snapshot deserialization
/// </summary>
public class CartSnapshotData
{
    public List<CartSnapshotItem> Items { get; set; } = new();
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
}

public class CartSnapshotItem
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public int VendorId { get; set; }
    public int StoreId { get; set; }
}
