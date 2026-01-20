using ElleganzaPlatform.Application.ViewModels.Store;

namespace ElleganzaPlatform.Application.Services;

/// <summary>
/// Service for managing checkout sessions (pre-order state)
/// Handles the checkout flow before order creation
/// </summary>
public interface ICheckoutSessionService
{
    /// <summary>
    /// Creates a new checkout session from the current cart
    /// First step in the checkout flow
    /// </summary>
    /// <param name="request">Checkout session creation request with addresses</param>
    /// <returns>Created checkout session or null if cart is empty</returns>
    Task<CheckoutSessionViewModel?> CreateCheckoutSessionAsync(CreateCheckoutSessionRequest request);

    /// <summary>
    /// Gets an existing checkout session by ID
    /// Validates session belongs to current user
    /// </summary>
    /// <param name="sessionId">Checkout session ID</param>
    /// <returns>Checkout session or null if not found/unauthorized</returns>
    Task<CheckoutSessionViewModel?> GetCheckoutSessionAsync(int sessionId);

    /// <summary>
    /// Updates the shipping method and cost for a checkout session
    /// Second step in the checkout flow
    /// </summary>
    /// <param name="request">Shipping method selection request</param>
    /// <returns>True if updated successfully</returns>
    Task<bool> UpdateShippingMethodAsync(SelectShippingMethodRequest request);

    /// <summary>
    /// Updates the payment method for a checkout session
    /// Third step in the checkout flow
    /// </summary>
    /// <param name="request">Payment method selection request</param>
    /// <returns>True if updated successfully</returns>
    Task<bool> UpdatePaymentMethodAsync(SelectPaymentMethodRequest request);

    /// <summary>
    /// Creates an order from a checkout session
    /// Called after payment success (online) or immediately (COD)
    /// </summary>
    /// <param name="sessionId">Checkout session ID</param>
    /// <returns>Order confirmation or null if validation fails</returns>
    Task<OrderConfirmationViewModel?> CreateOrderFromSessionAsync(int sessionId);

    /// <summary>
    /// Updates checkout session status to Paid after successful payment
    /// Called by payment service after webhook verification
    /// </summary>
    /// <param name="paymentIntentId">Stripe PaymentIntent ID</param>
    /// <returns>True if updated successfully</returns>
    Task<bool> UpdateSessionStatusToPaidAsync(string paymentIntentId);

    /// <summary>
    /// Expires old checkout sessions that have passed their expiration time
    /// Should be called periodically via background job
    /// </summary>
    Task ExpireOldSessionsAsync();

    /// <summary>
    /// Gets available shipping methods for the current cart
    /// </summary>
    /// <returns>List of shipping method options</returns>
    Task<List<ShippingMethodOption>> GetAvailableShippingMethodsAsync();
}
