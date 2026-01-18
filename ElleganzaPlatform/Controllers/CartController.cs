using ElleganzaPlatform.Application.Services;
using ElleganzaPlatform.Application.ViewModels.Store;
using Microsoft.AspNetCore.Mvc;

namespace ElleganzaPlatform.Controllers;

/// <summary>
/// Cart controller for shopping cart operations
/// Uses Store theme (Ecomus)
/// </summary>
public class CartController : Controller
{
    private readonly ILogger<CartController> _logger;
    private readonly ICartService _cartService;

    public CartController(
        ILogger<CartController> logger,
        ICartService cartService)
    {
        _logger = logger;
        _cartService = cartService;
    }

    /// <summary>
    /// View cart page
    /// </summary>
    [HttpGet("/cart")]
    public async Task<IActionResult> Index()
    {
        var cart = await _cartService.GetCartAsync();
        return View(cart);
    }

    /// <summary>
    /// Add item to cart (AJAX)
    /// </summary>
    [HttpPost("/cart/add")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, message = "Invalid request" });

        var success = await _cartService.AddToCartAsync(request.ProductId, request.Quantity);
        if (!success)
            return BadRequest(new { success = false, message = "Unable to add item to cart. Please check product availability." });

        var count = await _cartService.GetCartItemCountAsync();
        return Ok(new { success = true, cartCount = count, message = "Item added to cart" });
    }

    /// <summary>
    /// Update cart item quantity (AJAX)
    /// </summary>
    [HttpPost("/cart/update")]
    public async Task<IActionResult> UpdateCartItem([FromBody] UpdateCartItemRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, message = "Invalid request" });

        var success = await _cartService.UpdateCartItemAsync(request.ProductId, request.Quantity);
        if (!success)
            return BadRequest(new { success = false, message = "Unable to update cart item" });

        var cart = await _cartService.GetCartAsync();
        return Ok(new 
        { 
            success = true, 
            cartCount = cart.TotalItems,
            subTotal = cart.SubTotal,
            taxAmount = cart.TaxAmount,
            shippingAmount = cart.ShippingAmount,
            totalAmount = cart.TotalAmount
        });
    }

    /// <summary>
    /// Remove item from cart (AJAX)
    /// </summary>
    [HttpPost("/cart/remove/{productId}")]
    public async Task<IActionResult> RemoveFromCart(int productId)
    {
        var success = await _cartService.RemoveFromCartAsync(productId);
        if (!success)
            return BadRequest(new { success = false, message = "Unable to remove item from cart" });

        var cart = await _cartService.GetCartAsync();
        return Ok(new 
        { 
            success = true, 
            cartCount = cart.TotalItems,
            subTotal = cart.SubTotal,
            taxAmount = cart.TaxAmount,
            shippingAmount = cart.ShippingAmount,
            totalAmount = cart.TotalAmount
        });
    }

    /// <summary>
    /// Get cart count (AJAX)
    /// </summary>
    [HttpGet("/cart/count")]
    public async Task<IActionResult> GetCartCount()
    {
        var count = await _cartService.GetCartItemCountAsync();
        return Ok(new { count });
    }

    /// <summary>
    /// Clear cart
    /// </summary>
    [HttpPost("/cart/clear")]
    public async Task<IActionResult> ClearCart()
    {
        await _cartService.ClearCartAsync();
        return RedirectToAction(nameof(Index));
    }
}
