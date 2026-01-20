using ElleganzaPlatform.Application.Services;
using ElleganzaPlatform.Domain.Enums;
using ElleganzaPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;

namespace ElleganzaPlatform.Infrastructure.Services.Payment;

/// <summary>
/// Phase 4: Stripe Payment Service Implementation
/// Handles payment processing via Stripe Checkout
/// Implements secure webhook verification
/// Ensures idempotent payment processing
/// </summary>
public class StripePaymentService : IPaymentService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripePaymentService> _logger;
    private readonly string _stripeSecretKey;
    private readonly string _stripeWebhookSecret;

    public StripePaymentService(
        ApplicationDbContext context,
        IConfiguration configuration,
        ILogger<StripePaymentService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;

        // Phase 4: Load Stripe configuration from appsettings
        // DO NOT hardcode keys - always read from configuration
        _stripeSecretKey = _configuration["Stripe:SecretKey"] 
            ?? throw new InvalidOperationException("Stripe:SecretKey not configured");
        _stripeWebhookSecret = _configuration["Stripe:WebhookSecret"] 
            ?? throw new InvalidOperationException("Stripe:WebhookSecret not configured");

        // Phase 4: Initialize Stripe API with secret key
        StripeConfiguration.ApiKey = _stripeSecretKey;
    }

    /// <summary>
    /// Phase 4: Creates Stripe Checkout session for order payment
    /// Validates order exists and is eligible for payment (status = Pending)
    /// Returns checkout URL for customer redirect
    /// </summary>
    public async Task<PaymentResult> CreatePaymentAsync(int orderId)
    {
        try
        {
            // Phase 4: Load order with validation
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            // Phase 4: Safety - Order must exist
            if (order == null)
            {
                _logger.LogWarning("Payment creation failed: Order {OrderId} not found", orderId);
                return new PaymentResult
                {
                    IsSuccess = false,
                    Provider = "Stripe",
                    ErrorMessage = "Order not found",
                    OrderId = orderId
                };
            }

            // Phase 4: Safety - Order must be in Pending status
            // Orders can only be paid once
            if (order.Status != OrderStatus.Pending)
            {
                _logger.LogWarning("Payment creation failed: Order {OrderId} status is {Status}, expected Pending", 
                    orderId, order.Status);
                return new PaymentResult
                {
                    IsSuccess = false,
                    Provider = "Stripe",
                    ErrorMessage = $"Order is not eligible for payment. Current status: {order.Status}",
                    OrderId = orderId
                };
            }

            // Phase 4: Safety - Order must not already have a transaction ID
            if (!string.IsNullOrEmpty(order.PaymentTransactionId))
            {
                _logger.LogWarning("Payment creation failed: Order {OrderId} already has transaction {TransactionId}", 
                    orderId, order.PaymentTransactionId);
                return new PaymentResult
                {
                    IsSuccess = false,
                    Provider = "Stripe",
                    ErrorMessage = "Payment already exists for this order",
                    OrderId = orderId
                };
            }

            // Phase 4: Build base URL for redirects
            // In production, this should come from configuration
            var baseUrl = _configuration["App:BaseUrl"] ?? "https://localhost:7000";

            // Phase 4: Create Stripe Checkout Session
            var options = new SessionCreateOptions
            {
                // Phase 4: Payment configuration
                PaymentMethodTypes = new List<string> { "card" },
                Mode = "payment",
                
                // Phase 4: Line items - convert order items to Stripe format
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Order {order.OrderNumber}",
                                Description = $"Payment for {order.OrderItems.Count} item(s)"
                            },
                            // Phase 4: Stripe expects amount in cents
                            UnitAmount = (long)(order.TotalAmount * 100)
                        },
                        Quantity = 1
                    }
                },
                
                // Phase 4: Success/Cancel URLs
                // Customer is redirected here after payment
                SuccessUrl = $"{baseUrl}/checkout/success/{orderId}?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{baseUrl}/checkout/success/{orderId}",
                
                // Phase 4: Metadata for order tracking
                // This helps identify the order in webhook
                Metadata = new Dictionary<string, string>
                {
                    { "order_id", orderId.ToString() },
                    { "order_number", order.OrderNumber }
                }
            };

            // Phase 4: Create session via Stripe API
            var service = new SessionService();
            var session = await service.CreateAsync(options);

            _logger.LogInformation("Stripe Checkout session created: {SessionId} for Order {OrderId}", 
                session.Id, orderId);

            // Phase 4: Return success with checkout URL
            return new PaymentResult
            {
                IsSuccess = true,
                Provider = "Stripe",
                CheckoutUrl = session.Url,
                TransactionId = session.Id,
                OrderId = orderId
            };
        }
        catch (StripeException ex)
        {
            // Phase 4: Handle Stripe-specific errors
            _logger.LogError(ex, "Stripe error creating payment for Order {OrderId}", orderId);
            return new PaymentResult
            {
                IsSuccess = false,
                Provider = "Stripe",
                ErrorMessage = $"Payment processing error: {ex.Message}",
                OrderId = orderId
            };
        }
        catch (Exception ex)
        {
            // Phase 4: Handle general errors
            _logger.LogError(ex, "Error creating payment for Order {OrderId}", orderId);
            return new PaymentResult
            {
                IsSuccess = false,
                Provider = "Stripe",
                ErrorMessage = "An unexpected error occurred while processing payment",
                OrderId = orderId
            };
        }
    }

    /// <summary>
    /// Phase 4: Verifies Stripe webhook and updates order status
    /// CRITICAL: Webhook is source of truth, NOT client redirects
    /// Implements idempotent processing to handle duplicate webhooks
    /// Validates webhook signature for security
    /// </summary>
    public async Task<PaymentResult> VerifyPaymentAsync(string payload, string signature)
    {
        try
        {
            // Phase 4: CRITICAL - Verify webhook signature
            // This prevents malicious requests from fake payment confirmations
            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    payload,
                    signature,
                    _stripeWebhookSecret
                );
            }
            catch (StripeException ex)
            {
                // Phase 4: Signature verification failed - reject webhook
                _logger.LogWarning(ex, "Webhook signature verification failed");
                return new PaymentResult
                {
                    IsSuccess = false,
                    Provider = "Stripe",
                    ErrorMessage = "Invalid webhook signature"
                };
            }

            // Phase 4: Process checkout.session.completed event
            if (stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Session;
                if (session == null)
                {
                    _logger.LogWarning("Webhook payload missing session data");
                    return new PaymentResult
                    {
                        IsSuccess = false,
                        Provider = "Stripe",
                        ErrorMessage = "Invalid webhook payload"
                    };
                }

                // Phase 4: Extract order ID from session metadata
                if (!session.Metadata.TryGetValue("order_id", out var orderIdStr) ||
                    !int.TryParse(orderIdStr, out var orderId))
                {
                    _logger.LogWarning("Webhook missing order_id in metadata");
                    return new PaymentResult
                    {
                        IsSuccess = false,
                        Provider = "Stripe",
                        ErrorMessage = "Order ID not found in payment metadata"
                    };
                }

                // Phase 4: Load order for status update
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    _logger.LogWarning("Webhook references non-existent Order {OrderId}", orderId);
                    return new PaymentResult
                    {
                        IsSuccess = false,
                        Provider = "Stripe",
                        ErrorMessage = "Order not found",
                        OrderId = orderId
                    };
                }

                // Phase 4: IDEMPOTENCY - Check if payment already processed
                // Stripe may send duplicate webhooks - ignore them safely
                if (order.Status == OrderStatus.Paid && 
                    order.PaymentTransactionId == session.PaymentIntentId)
                {
                    _logger.LogInformation("Duplicate webhook ignored for Order {OrderId}, already marked as Paid", 
                        orderId);
                    return new PaymentResult
                    {
                        IsSuccess = true,
                        Provider = "Stripe",
                        TransactionId = session.PaymentIntentId,
                        OrderId = orderId
                    };
                }

                // Phase 4: Verify payment was actually successful
                if (session.PaymentStatus == "paid")
                {
                    // Phase 4: Update order status to Paid
                    order.Status = OrderStatus.Paid;
                    order.PaymentTransactionId = session.PaymentIntentId;
                    
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Order {OrderId} marked as Paid with transaction {TransactionId}", 
                        orderId, session.PaymentIntentId);

                    return new PaymentResult
                    {
                        IsSuccess = true,
                        Provider = "Stripe",
                        TransactionId = session.PaymentIntentId,
                        OrderId = orderId
                    };
                }
                else
                {
                    // Phase 4: Payment failed or incomplete
                    _logger.LogWarning("Payment failed for Order {OrderId}, status: {PaymentStatus}", 
                        orderId, session.PaymentStatus);
                    
                    // Phase 4: Update order status to Failed
                    order.Status = OrderStatus.Failed;
                    order.PaymentTransactionId = session.PaymentIntentId;
                    
                    await _context.SaveChangesAsync();

                    return new PaymentResult
                    {
                        IsSuccess = false,
                        Provider = "Stripe",
                        ErrorMessage = $"Payment failed: {session.PaymentStatus}",
                        OrderId = orderId
                    };
                }
            }

            // Phase 4: Ignore other event types
            _logger.LogInformation("Webhook event {EventType} received but not processed", stripeEvent.Type);
            return new PaymentResult
            {
                IsSuccess = true,
                Provider = "Stripe"
            };
        }
        catch (Exception ex)
        {
            // Phase 4: Handle errors in webhook processing
            _logger.LogError(ex, "Error processing webhook");
            return new PaymentResult
            {
                IsSuccess = false,
                Provider = "Stripe",
                ErrorMessage = "Error processing webhook"
            };
        }
    }
}
