using ElleganzaPlatform.Application.Services;
using ElleganzaPlatform.Application.ViewModels.Store;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElleganzaPlatform.Controllers;

/// <summary>
/// Checkout controller for order processing
/// Uses Store theme (Ecomus)
/// Requires authentication
/// </summary>
[Authorize]
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
    /// Checkout page - Requires authentication
    /// </summary>
    [HttpGet("/checkout")]
    public async Task<IActionResult> Index()
    {
        var checkoutData = await _checkoutService.GetCheckoutDataAsync();
        if (checkoutData == null)
        {
            // No items in cart or user not found
            return RedirectToAction("Index", "Cart");
        }

        return View(checkoutData);
    }

    /// <summary>
    /// Place order
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

    /// <summary>
    /// Order success page
    /// </summary>
    [HttpGet("/checkout/success/{orderId}")]
    public async Task<IActionResult> OrderSuccess(int orderId)
    {
        // Get order details to verify it belongs to current user
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
            Status = order.StatusDisplay
        };

        return View(confirmation);
    }
}
