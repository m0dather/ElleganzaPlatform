using ElleganzaPlatform.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElleganzaPlatform.Controllers;

/// <summary>
/// Phase 4: Payment Controller
/// Handles payment initiation for orders
/// Requires authentication to protect payment operations
/// </summary>
[Authorize]
public class PaymentController : Controller
{
    private readonly IPaymentService _paymentService;
    private readonly IOrderService _orderService;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IPaymentService paymentService,
        IOrderService orderService,
        ILogger<PaymentController> logger)
    {
        _paymentService = paymentService;
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Phase 4: POST /payment/create
    /// Initiates payment for an order
    /// Validates order belongs to current user
    /// Redirects to Stripe Checkout
    /// </summary>
    [HttpPost("/payment/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePayment(int orderId)
    {
        // Phase 4: Security - Verify order belongs to current user
        var order = await _orderService.GetOrderDetailsAsync(orderId);
        if (order == null)
        {
            _logger.LogWarning("Payment creation denied: User does not have access to Order {OrderId}", orderId);
            TempData["Error"] = "Order not found or access denied.";
            return RedirectToAction("Orders", "Account");
        }

        // Phase 4: Create payment session
        var result = await _paymentService.CreatePaymentAsync(orderId);

        // Phase 4: Handle payment creation failure
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Payment creation failed for Order {OrderId}: {Error}", orderId, result.ErrorMessage);
            TempData["Error"] = result.ErrorMessage ?? "Unable to process payment. Please try again.";
            return RedirectToAction("OrderDetails", "Account", new { id = orderId });
        }

        // Phase 4: Success - Redirect to Stripe Checkout
        _logger.LogInformation("Redirecting user to Stripe Checkout for Order {OrderId}", orderId);
        return Redirect(result.CheckoutUrl!);
    }
}
