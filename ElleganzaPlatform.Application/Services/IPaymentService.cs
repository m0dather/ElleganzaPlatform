namespace ElleganzaPlatform.Application.Services;

/// <summary>
/// Phase 4: Payment Service Interface
/// Provides abstraction for payment processing to support multiple payment providers
/// Enables secure payment processing and webhook verification
/// Updated to support CheckoutSession-based flow
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Creates a payment session for a checkout session
    /// Validates checkout session is eligible for payment
    /// Returns checkout URL for customer to complete payment
    /// </summary>
    /// <param name="checkoutSessionId">The checkout session ID to create payment for</param>
    /// <returns>PaymentResult with checkout URL or error</returns>
    Task<PaymentResult> CreatePaymentAsync(int checkoutSessionId);
    
    /// <summary>
    /// Legacy method for backward compatibility
    /// Creates a payment session for an order (deprecated - use checkout session instead)
    /// </summary>
    /// <param name="orderId">The order ID to create payment for</param>
    /// <returns>PaymentResult with checkout URL or error</returns>
    Task<PaymentResult> CreatePaymentForOrderAsync(int orderId);

    /// <summary>
    /// Phase 4: Verifies payment webhook callback from payment provider
    /// Validates webhook signature for security
    /// Updates checkout session status based on payment outcome
    /// Implements idempotent processing to handle duplicate webhooks
    /// </summary>
    /// <param name="payload">Raw webhook payload from payment provider</param>
    /// <param name="signature">Webhook signature header for verification</param>
    /// <returns>PaymentResult with verification outcome</returns>
    Task<PaymentResult> VerifyPaymentAsync(string payload, string signature);
}

/// <summary>
/// Phase 4: Payment Result
/// Contains outcome of payment operations
/// Used for both payment creation and verification
/// </summary>
public class PaymentResult
{
    /// <summary>
    /// Indicates if the payment operation was successful
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Payment provider identifier (e.g., "Stripe", "PayPal")
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Unique transaction ID from payment provider
    /// Used for reconciliation and refunds
    /// </summary>
    public string? TransactionId { get; set; }

    /// <summary>
    /// Error message if payment failed
    /// Null if IsSuccess = true
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Checkout URL for payment creation
    /// Customer is redirected here to complete payment
    /// </summary>
    public string? CheckoutUrl { get; set; }

    /// <summary>
    /// Order ID associated with this payment (optional, used for legacy flow)
    /// </summary>
    public int? OrderId { get; set; }
    
    /// <summary>
    /// Checkout session ID associated with this payment
    /// </summary>
    public int? CheckoutSessionId { get; set; }
}
