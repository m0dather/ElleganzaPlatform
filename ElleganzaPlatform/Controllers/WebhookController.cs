using ElleganzaPlatform.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ElleganzaPlatform.Controllers;

/// <summary>
/// Webhook Controller - Updated for CheckoutSession-based flow
/// Handles payment webhook callbacks from Stripe
/// CRITICAL: This is the source of truth for payment status
/// Does NOT require authentication - webhooks come from Stripe servers
/// Security is provided by webhook signature verification
/// </summary>
[ApiController]
[Route("payments/webhook")]
public class WebhookController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ICheckoutSessionService _checkoutSessionService;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(
        IPaymentService paymentService,
        ICheckoutSessionService checkoutSessionService,
        ILogger<WebhookController> logger)
    {
        _paymentService = paymentService;
        _checkoutSessionService = checkoutSessionService;
        _logger = logger;
    }

    /// <summary>
    /// POST /payments/webhook/stripe
    /// Receives webhook events from Stripe
    /// Verifies webhook signature (CRITICAL for security)
    /// Updates checkout session or order status based on payment outcome
    /// Implements idempotent processing for duplicate webhooks
    /// Automatically creates order from paid checkout session
    /// </summary>
    [HttpPost("stripe")]
    public async Task<IActionResult> StripeWebhook()
    {
        // Read raw request body
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync();

        // Get webhook signature from header
        var signature = Request.Headers["Stripe-Signature"].ToString();

        // Validate signature is present
        if (string.IsNullOrEmpty(signature))
        {
            _logger.LogWarning("Webhook received without signature - rejecting");
            return BadRequest("Missing signature");
        }

        // Verify webhook and process payment
        var result = await _paymentService.VerifyPaymentAsync(payload, signature);

        // Handle verification failure
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Webhook verification failed: {Error}", result.ErrorMessage);
            return BadRequest(result.ErrorMessage);
        }

        // If this was a checkout session payment, create the order
        if (result.CheckoutSessionId.HasValue)
        {
            _logger.LogInformation("Creating order from paid CheckoutSession {CheckoutSessionId}", 
                result.CheckoutSessionId.Value);
            
            try
            {
                var orderConfirmation = await _checkoutSessionService.CreateOrderFromSessionAsync(
                    result.CheckoutSessionId.Value);
                
                if (orderConfirmation != null)
                {
                    _logger.LogInformation("Order {OrderId} created from CheckoutSession {CheckoutSessionId}", 
                        orderConfirmation.OrderId, result.CheckoutSessionId.Value);
                }
                else
                {
                    _logger.LogWarning("Failed to create order from CheckoutSession {CheckoutSessionId}", 
                        result.CheckoutSessionId.Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order from CheckoutSession {CheckoutSessionId}", 
                    result.CheckoutSessionId.Value);
                // Don't fail the webhook - order creation can be retried
            }
        }

        // Success - Return 200 OK to Stripe
        if (result.OrderId.HasValue && result.OrderId > 0)
        {
            _logger.LogInformation("Webhook processed successfully for Order {OrderId}", result.OrderId.Value);
        }
        else if (result.CheckoutSessionId.HasValue)
        {
            _logger.LogInformation("Webhook processed successfully for CheckoutSession {CheckoutSessionId}", 
                result.CheckoutSessionId.Value);
        }
        else
        {
            _logger.LogInformation("Webhook processed successfully (no order/session associated)");
        }
        
        return Ok();
    }
}
