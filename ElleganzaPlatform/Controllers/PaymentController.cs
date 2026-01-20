using ElleganzaPlatform.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElleganzaPlatform.Controllers;

/// <summary>
/// Payment Controller - Updated for CheckoutSession-based flow
/// Handles payment initiation for checkout sessions
/// Requires authentication to protect payment operations
/// </summary>
[Authorize]
public class PaymentController : Controller
{
    private readonly IPaymentService _paymentService;
    private readonly IOrderService _orderService;
    private readonly ICheckoutSessionService _checkoutSessionService;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IPaymentService paymentService,
        IOrderService orderService,
        ICheckoutSessionService checkoutSessionService,
        ILogger<PaymentController> logger)
    {
        _paymentService = paymentService;
        _orderService = orderService;
        _checkoutSessionService = checkoutSessionService;
        _logger = logger;
    }

    /// <summary>
    /// POST /payment/create
    /// Initiates payment for a checkout session
    /// Validates session belongs to current user
    /// Redirects to Stripe Checkout
    /// </summary>
    [HttpPost("/payment/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePayment(int checkoutSessionId)
    {
        // Security - Verify checkout session belongs to current user
        var checkoutSession = await _checkoutSessionService.GetCheckoutSessionAsync(checkoutSessionId);
        if (checkoutSession == null)
        {
            _logger.LogWarning("Payment creation denied: User does not have access to CheckoutSession {CheckoutSessionId}", checkoutSessionId);
            TempData["Error"] = "Checkout session not found or access denied.";
            return RedirectToAction("Index", "Checkout");
        }

        // Create payment session
        var result = await _paymentService.CreatePaymentAsync(checkoutSessionId);

        // Handle payment creation failure
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Payment creation failed for CheckoutSession {CheckoutSessionId}: {Error}", 
                checkoutSessionId, result.ErrorMessage);
            TempData["Error"] = result.ErrorMessage ?? "Unable to process payment. Please try again.";
            return RedirectToAction("SelectPayment", "Checkout", new { sessionId = checkoutSessionId });
        }

        // Success - Redirect to Stripe Checkout
        _logger.LogInformation("Redirecting user to Stripe Checkout for CheckoutSession {CheckoutSessionId}", checkoutSessionId);
        return Redirect(result.CheckoutUrl!);
    }

    /// <summary>
    /// Legacy endpoint - Kept for backward compatibility
    /// POST /payment/create-order
    /// Initiates payment for an order (old flow)
    /// </summary>
    [HttpPost("/payment/create-order")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePaymentForOrder(int orderId)
    {
        // Security - Verify order belongs to current user
        var order = await _orderService.GetOrderDetailsAsync(orderId);
        if (order == null)
        {
            _logger.LogWarning("Payment creation denied: User does not have access to Order {OrderId}", orderId);
            TempData["Error"] = "Order not found or access denied.";
            return RedirectToAction("Orders", "Account");
        }

        // Create payment session
        var result = await _paymentService.CreatePaymentForOrderAsync(orderId);

        // Handle payment creation failure
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Payment creation failed for Order {OrderId}: {Error}", orderId, result.ErrorMessage);
            TempData["Error"] = result.ErrorMessage ?? "Unable to process payment. Please try again.";
            return RedirectToAction("OrderDetails", "Account", new { id = orderId });
        }

        // Success - Redirect to Stripe Checkout
        _logger.LogInformation("Redirecting user to Stripe Checkout for Order {OrderId}", orderId);
        return Redirect(result.CheckoutUrl!);
    }
}
