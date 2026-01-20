using ElleganzaPlatform.Application.Services;
using ElleganzaPlatform.Application.ViewModels.Store;
using ElleganzaPlatform.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElleganzaPlatform.Controllers;

/// <summary>
/// Checkout Controller - Refactored for CheckoutSession-based flow
/// New flow: Cart → CheckoutSession → Payment → Order
/// Supports both online payment and Cash On Delivery (COD)
/// </summary>
[Authorize]
public class CheckoutController : Controller
{
    private readonly ILogger<CheckoutController> _logger;
    private readonly ICheckoutService _checkoutService;
    private readonly ICheckoutSessionService _checkoutSessionService;
    private readonly ICartService _cartService;

    public CheckoutController(
        ILogger<CheckoutController> logger,
        ICheckoutService checkoutService,
        ICheckoutSessionService checkoutSessionService,
        ICartService cartService)
    {
        _logger = logger;
        _checkoutService = checkoutService;
        _checkoutSessionService = checkoutSessionService;
        _cartService = cartService;
    }

    /// <summary>
    /// GET /checkout
    /// Displays one-page checkout (NEW: Redirects to one-page flow)
    /// All checkout steps are on a single page with progressive enable/disable
    /// </summary>
    [HttpGet("/checkout")]
    public IActionResult Index()
    {
        // Redirect to one-page checkout
        return RedirectToAction(nameof(OnePageCheckout));
    }

    /// <summary>
    /// GET /checkout/multi-step
    /// Displays multi-step checkout page (Legacy flow - kept for compatibility)
    /// Step 1: Enter shipping/billing addresses
    /// </summary>
    [HttpGet("/checkout/multi-step")]
    public async Task<IActionResult> MultiStepCheckout()
    {
        // Load checkout data (cart + customer info)
        var checkoutData = await _checkoutService.GetCheckoutDataAsync();
        
        // Validation - Redirect if cart is empty or user not found
        if (checkoutData == null)
        {
            return RedirectToAction("Index", "Cart");
        }

        // Load available shipping methods
        checkoutData.AvailableShippingMethods = await _checkoutSessionService.GetAvailableShippingMethodsAsync();

        return View("Index", checkoutData);
    }

    /// <summary>
    /// POST /checkout/create-session
    /// Creates a checkout session from cart
    /// Step 2: Create checkout session
    /// </summary>
    [HttpPost("/checkout/create-session")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSession([FromForm] CreateCheckoutSessionRequest request)
    {
        if (!ModelState.IsValid)
        {
            var checkoutData = await _checkoutService.GetCheckoutDataAsync();
            if (checkoutData != null)
            {
                checkoutData.ShippingAddress = request.ShippingAddress;
                checkoutData.BillingAddress = request.BillingAddress ?? request.ShippingAddress;
                checkoutData.CustomerNotes = request.CustomerNotes;
                checkoutData.AvailableShippingMethods = await _checkoutSessionService.GetAvailableShippingMethodsAsync();
            }
            return View("Index", checkoutData);
        }

        var checkoutSession = await _checkoutSessionService.CreateCheckoutSessionAsync(request);
        
        if (checkoutSession == null)
        {
            TempData["Error"] = "Unable to create checkout session. Please try again.";
            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction(nameof(SelectShipping), new { sessionId = checkoutSession.Id });
    }

    /// <summary>
    /// GET /checkout/select-shipping/{sessionId}
    /// Displays shipping method selection
    /// Step 3: Select shipping method
    /// </summary>
    [HttpGet("/checkout/select-shipping/{sessionId}")]
    public async Task<IActionResult> SelectShipping(int sessionId)
    {
        var checkoutSession = await _checkoutSessionService.GetCheckoutSessionAsync(sessionId);
        
        if (checkoutSession == null)
        {
            return RedirectToAction(nameof(Index));
        }

        var shippingMethods = await _checkoutSessionService.GetAvailableShippingMethodsAsync();
        ViewBag.ShippingMethods = shippingMethods;
        ViewBag.SessionId = sessionId;

        return View(checkoutSession);
    }

    /// <summary>
    /// POST /checkout/update-shipping
    /// Updates shipping method for checkout session
    /// Step 4: Confirm shipping method
    /// </summary>
    [HttpPost("/checkout/update-shipping")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateShipping([FromForm] SelectShippingMethodRequest request)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Invalid shipping method selection.";
            return RedirectToAction(nameof(SelectShipping), new { sessionId = request.CheckoutSessionId });
        }

        var success = await _checkoutSessionService.UpdateShippingMethodAsync(request);
        
        if (!success)
        {
            TempData["Error"] = "Unable to update shipping method.";
            return RedirectToAction(nameof(SelectShipping), new { sessionId = request.CheckoutSessionId });
        }

        return RedirectToAction(nameof(SelectPayment), new { sessionId = request.CheckoutSessionId });
    }

    /// <summary>
    /// GET /checkout/select-payment/{sessionId}
    /// Displays payment method selection
    /// Step 5: Select payment method (Online or COD)
    /// </summary>
    [HttpGet("/checkout/select-payment/{sessionId}")]
    public async Task<IActionResult> SelectPayment(int sessionId)
    {
        var checkoutSession = await _checkoutSessionService.GetCheckoutSessionAsync(sessionId);
        
        if (checkoutSession == null)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(checkoutSession);
    }

    /// <summary>
    /// POST /checkout/update-payment
    /// Updates payment method for checkout session
    /// Step 6: Confirm payment method
    /// - If Online: Redirects to payment gateway
    /// - If COD: Creates order immediately
    /// </summary>
    [HttpPost("/checkout/update-payment")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePayment([FromForm] SelectPaymentMethodRequest request)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Invalid payment method selection.";
            return RedirectToAction(nameof(SelectPayment), new { sessionId = request.CheckoutSessionId });
        }

        var success = await _checkoutSessionService.UpdatePaymentMethodAsync(request);
        
        if (!success)
        {
            TempData["Error"] = "Unable to update payment method.";
            return RedirectToAction(nameof(SelectPayment), new { sessionId = request.CheckoutSessionId });
        }

        // Redirect to review step for final confirmation
        return RedirectToAction(nameof(ReviewOrder), new { sessionId = request.CheckoutSessionId });
    }

    /// <summary>
    /// GET /checkout/review/{sessionId}
    /// Displays order review page with all details before final confirmation
    /// Step 4: Review & Confirm
    /// </summary>
    [HttpGet("/checkout/review/{sessionId}")]
    public async Task<IActionResult> ReviewOrder(int sessionId)
    {
        var checkoutSession = await _checkoutSessionService.GetCheckoutSessionAsync(sessionId);
        
        if (checkoutSession == null)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(checkoutSession);
    }

    /// <summary>
    /// POST /checkout/confirm-order
    /// Confirms the order and proceeds to payment (Online) or creates order (COD)
    /// Final step: Execute payment or create order
    /// </summary>
    [HttpPost("/checkout/confirm-order")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmOrder(int checkoutSessionId)
    {
        var checkoutSession = await _checkoutSessionService.GetCheckoutSessionAsync(checkoutSessionId);
        
        if (checkoutSession == null)
        {
            TempData["Error"] = "Invalid checkout session.";
            return RedirectToAction(nameof(Index));
        }

        // Branch based on payment method
        if (checkoutSession.PaymentMethod == PaymentMethod.CashOnDelivery)
        {
            // COD: Create order immediately without payment
            return RedirectToAction(nameof(ConfirmCOD), new { sessionId = checkoutSessionId });
        }
        else
        {
            // Online: Redirect to payment
            return RedirectToAction("CreatePayment", "Payment", new { checkoutSessionId = checkoutSessionId });
        }
    }

    /// <summary>
    /// GET /checkout/confirm-cod/{sessionId}
    /// Creates order for Cash On Delivery
    /// </summary>
    [HttpGet("/checkout/confirm-cod/{sessionId}")]
    public async Task<IActionResult> ConfirmCOD(int sessionId)
    {
        var confirmation = await _checkoutSessionService.CreateOrderFromSessionAsync(sessionId);
        
        if (confirmation == null)
        {
            TempData["Error"] = "Unable to create order. Please try again.";
            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction(nameof(OrderSuccess), new { orderId = confirmation.OrderId });
    }

    /// <summary>
    /// GET /checkout/payment-success
    /// Called after successful online payment
    /// Creates order from paid checkout session
    /// </summary>
    [HttpGet("/checkout/payment-success")]
    public async Task<IActionResult> PaymentSuccess(string session_id)
    {
        if (string.IsNullOrEmpty(session_id))
        {
            TempData["Error"] = "Invalid payment session.";
            return RedirectToAction(nameof(Index));
        }

        // Find checkout session by payment intent ID
        // The payment service should have already updated the session status to Paid
        // Now we create the order from the paid session
        
        // For now, redirect to a generic success page
        // The order creation will happen via webhook
        TempData["Message"] = "Payment successful! Your order is being processed.";
        return View("PaymentSuccess");
    }

    /// <summary>
    /// GET /checkout/payment-cancelled
    /// Called when payment is cancelled
    /// </summary>
    [HttpGet("/checkout/payment-cancelled")]
    public IActionResult PaymentCancelled()
    {
        TempData["Error"] = "Payment was cancelled. Your cart is still available.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// GET /checkout/success/{orderId}
    /// Displays order confirmation page
    /// </summary>
    [HttpGet("/checkout/success/{orderId}")]
    public async Task<IActionResult> OrderSuccess(int orderId)
    {
        var orderService = HttpContext.RequestServices.GetRequiredService<IOrderService>();
        var order = await orderService.GetOrderDetailsAsync(orderId);

        if (order == null)
        {
            return RedirectToAction("Index", "Home");
        }

        var confirmation = new OrderConfirmationViewModel
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            Status = order.StatusDisplay,
            CanBePaid = order.CanBePaid
        };

        return View(confirmation);
    }

    /// <summary>
    /// Legacy endpoint - Kept for backward compatibility
    /// POST /checkout/place-order
    /// Creates order directly (old flow)
    /// </summary>
    [HttpPost("/checkout/place-order")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder([FromForm] PlaceOrderRequest request)
    {
        if (!ModelState.IsValid)
        {
            var checkoutData = await _checkoutService.GetCheckoutDataAsync();
            if (checkoutData != null)
            {
                checkoutData.ShippingAddress = request.ShippingAddress;
                checkoutData.BillingAddress = request.BillingAddress ?? request.ShippingAddress;
                checkoutData.CustomerNotes = request.CustomerNotes;
            }
            return View("Index", checkoutData);
        }

        var confirmation = await _checkoutService.PlaceOrderAsync(request);
        
        if (confirmation == null)
        {
            TempData["Error"] = "Unable to place order. Please try again.";
            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction(nameof(OrderSuccess), new { orderId = confirmation.OrderId });
    }

    // ========================================
    // ONE-PAGE CHECKOUT AJAX ENDPOINTS
    // ========================================

    /// <summary>
    /// POST /checkout/save-address
    /// AJAX endpoint to save address and create/update checkout session
    /// Returns JSON with session ID and available shipping methods
    /// </summary>
    [HttpPost("/checkout/save-address")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveAddress([FromForm] CreateCheckoutSessionRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            
            return Json(new { success = false, errors = errors });
        }

        var checkoutSession = await _checkoutSessionService.CreateCheckoutSessionAsync(request);
        
        if (checkoutSession == null)
        {
            return Json(new { success = false, errors = new[] { "Unable to create checkout session. Please try again." } });
        }

        var shippingMethods = await _checkoutSessionService.GetAvailableShippingMethodsAsync();

        return Json(new 
        { 
            success = true, 
            sessionId = checkoutSession.Id,
            shippingMethods = shippingMethods.Select(m => new 
            {
                name = m.Name,
                description = m.Description,
                cost = m.Cost,
                estimatedDelivery = m.EstimatedDelivery
            }).ToList()
        });
    }

    /// <summary>
    /// POST /checkout/select-shipping-ajax
    /// AJAX endpoint to update shipping method in checkout session
    /// Returns JSON with updated totals
    /// </summary>
    [HttpPost("/checkout/select-shipping-ajax")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SelectShippingAjax([FromForm] SelectShippingMethodRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            
            return Json(new { success = false, errors = errors });
        }

        var success = await _checkoutSessionService.UpdateShippingMethodAsync(request);
        
        if (!success)
        {
            return Json(new { success = false, errors = new[] { "Unable to update shipping method." } });
        }

        var checkoutSession = await _checkoutSessionService.GetCheckoutSessionAsync(request.CheckoutSessionId);
        
        if (checkoutSession == null)
        {
            return Json(new { success = false, errors = new[] { "Checkout session not found." } });
        }

        return Json(new 
        { 
            success = true,
            shippingCost = checkoutSession.ShippingCost,
            taxAmount = checkoutSession.Cart.TaxAmount,
            totalAmount = checkoutSession.TotalAmount
        });
    }

    /// <summary>
    /// POST /checkout/select-payment-ajax
    /// AJAX endpoint to update payment method in checkout session
    /// Returns JSON with success status
    /// </summary>
    [HttpPost("/checkout/select-payment-ajax")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SelectPaymentAjax([FromForm] SelectPaymentMethodRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            
            return Json(new { success = false, errors = errors });
        }

        var success = await _checkoutSessionService.UpdatePaymentMethodAsync(request);
        
        if (!success)
        {
            return Json(new { success = false, errors = new[] { "Unable to update payment method." } });
        }

        return Json(new { success = true });
    }

    /// <summary>
    /// GET /checkout/one-page
    /// Displays the one-page checkout view
    /// All steps are on a single page with progressive enable/disable
    /// </summary>
    [HttpGet("/checkout/one-page")]
    public async Task<IActionResult> OnePageCheckout()
    {
        // Load checkout data (cart + customer info)
        var checkoutData = await _checkoutService.GetCheckoutDataAsync();
        
        // Validation - Redirect if cart is empty or user not found
        if (checkoutData == null)
        {
            return RedirectToAction("Index", "Cart");
        }

        // Load available shipping methods
        checkoutData.AvailableShippingMethods = await _checkoutSessionService.GetAvailableShippingMethodsAsync();

        return View("OnePageCheckout", checkoutData);
    }
}
