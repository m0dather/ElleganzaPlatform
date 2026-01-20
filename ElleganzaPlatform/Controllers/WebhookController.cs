using ElleganzaPlatform.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ElleganzaPlatform.Controllers;

/// <summary>
/// Phase 4: Webhook Controller
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
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(
        IPaymentService paymentService,
        ILogger<WebhookController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <summary>
    /// Phase 4: POST /payments/webhook/stripe
    /// Receives webhook events from Stripe
    /// Verifies webhook signature (CRITICAL for security)
    /// Updates order status based on payment outcome
    /// Implements idempotent processing for duplicate webhooks
    /// </summary>
    [HttpPost("stripe")]
    public async Task<IActionResult> StripeWebhook()
    {
        // Phase 4: Read raw request body
        // Signature verification requires the exact raw payload
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync();

        // Phase 4: Get webhook signature from header
        // Stripe sends signature in Stripe-Signature header
        var signature = Request.Headers["Stripe-Signature"].ToString();

        // Phase 4: Validate signature is present
        if (string.IsNullOrEmpty(signature))
        {
            _logger.LogWarning("Webhook received without signature - rejecting");
            return BadRequest("Missing signature");
        }

        // Phase 4: Verify webhook and process payment
        var result = await _paymentService.VerifyPaymentAsync(payload, signature);

        // Phase 4: Handle verification failure
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Webhook verification failed: {Error}", result.ErrorMessage);
            return BadRequest(result.ErrorMessage);
        }

        // Phase 4: Success - Return 200 OK to Stripe
        // This tells Stripe we successfully processed the webhook
        if (result.OrderId > 0)
        {
            _logger.LogInformation("Webhook processed successfully for Order {OrderId}", result.OrderId);
        }
        else
        {
            _logger.LogInformation("Webhook processed successfully (no order associated)");
        }
        return Ok();
    }
}
