using ElleganzaPlatform.Application.Services;
using ElleganzaPlatform.Application.ViewModels.Store;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElleganzaPlatform.Controllers;

/// <summary>
/// Phase 3.2: Checkout Controller
/// Handles secure checkout process for authenticated users
/// Uses Store theme (Ecomus)
/// Requires authentication - guest users redirected to /login
/// </summary>
[Authorize]  // Phase 3.2: Access Control - Requires authentication
public class CheckoutController : Controller
{
    private readonly ILogger<CheckoutController> _logger;
    private readonly ICheckoutService _checkoutService;
    private readonly ICartService _cartService;

    public CheckoutController(
        ILogger<CheckoutController> logger,
        ICheckoutService checkoutService,
        ICartService cartService)
    {
        _logger = logger;
        _checkoutService = checkoutService;
        _cartService = cartService;
    }

    /// <summary>
    /// Phase 3.2: GET /checkout
    /// Displays checkout page with cart summary and address form
    /// Requires authentication - unauthenticated users redirected to /login
    /// Empty cart redirects to cart page
    /// </summary>
    [HttpGet("/checkout")]
    public async Task<IActionResult> Index()
    {
        // Phase 3.2: Load checkout data (cart + customer info)
        var checkoutData = await _checkoutService.GetCheckoutDataAsync();
        
        // Phase 3.2: Validation - Redirect if cart is empty or user not found
        if (checkoutData == null)
        {
            // No items in cart or user not found
            return RedirectToAction("Index", "Cart");
        }

        return View(checkoutData);
    }

    /// <summary>
    /// Phase 3.2: POST /checkout/place-order
    /// Processes order placement from checkout form
    /// Creates Order and OrderItems with proper isolation
    /// Clears cart on success
    /// Redirects to success page with order ID
    /// </summary>
    [HttpPost("/checkout/place-order")]
    [ValidateAntiForgeryToken]  // Phase 3.2: Security - CSRF protection
    public async Task<IActionResult> PlaceOrder([FromForm] PlaceOrderRequest request)
    {
        // Phase 3.2: Validation - Check model state
        if (!ModelState.IsValid)
        {
            // Re-populate checkout data to show validation errors
            var checkoutData = await _checkoutService.GetCheckoutDataAsync();
            if (checkoutData != null)
            {
                checkoutData.ShippingAddress = request.ShippingAddress;
                checkoutData.BillingAddress = request.BillingAddress ?? request.ShippingAddress;
                checkoutData.CustomerNotes = request.CustomerNotes;
            }
            return View("Index", checkoutData);
        }

        // Phase 3.2: Place order (creates Order, OrderItems, clears cart)
        var confirmation = await _checkoutService.PlaceOrderAsync(request);
        
        // Phase 3.2: Handle failure
        if (confirmation == null)
        {
            TempData["Error"] = "Unable to place order. Please try again.";
            return RedirectToAction(nameof(Index));
        }

        // Phase 3.2: Success - Redirect to order success page
        return RedirectToAction(nameof(OrderSuccess), new { orderId = confirmation.OrderId });
    }

    /// <summary>
    /// Phase 3.2: GET /checkout/success/{orderId}
    /// Displays order confirmation page
    /// Verifies order belongs to current user for security
    /// </summary>
    [HttpGet("/checkout/success/{orderId}")]
    public async Task<IActionResult> OrderSuccess(int orderId)
    {
        // Phase 3.2: Security - Verify order belongs to current user
        var orderService = HttpContext.RequestServices.GetRequiredService<IOrderService>();
        var order = await orderService.GetOrderDetailsAsync(orderId);

        // Phase 3.2: Access Control - Order not found or doesn't belong to user
        if (order == null)
        {
            return RedirectToAction("Index", "Home");
        }

        // Phase 3.2: Display order confirmation
        var confirmation = new OrderConfirmationViewModel
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            Status = order.StatusDisplay,
            CanBePaid = order.CanBePaid  // Phase 4: Payment integration
        };

        return View(confirmation);
    }
}
