namespace ElleganzaPlatform.Application.Services;

/// <summary>
/// Phase 4: Payment Service Interface
/// Provides abstraction for payment processing to support multiple payment providers
/// Enables secure payment processing and webhook verification
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Phase 4: Creates a payment session for an order
    /// Validates order is eligible for payment (status = Pending)
    /// Returns checkout URL for customer to complete payment
    /// </summary>
    /// <param name="orderId">The order ID to create payment for</param>
    /// <returns>PaymentResult with checkout URL or error</returns>
    Task<PaymentResult> CreatePaymentAsync(int orderId);

    /// <summary>
    /// Phase 4: Verifies payment webhook callback from payment provider
    /// Validates webhook signature for security
    /// Updates order status based on payment outcome
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
    /// Order ID associated with this payment
    /// </summary>
    public int OrderId { get; set; }
}
