using ElleganzaPlatform.Application.Services;
using ElleganzaPlatform.Application.ViewModels.Store;
using Microsoft.AspNetCore.Mvc;

namespace ElleganzaPlatform.Controllers;

/// <summary>
/// Cart controller for shopping cart operations
/// Uses Store theme (Ecomus)
/// Phase 3.1.1: Hardened with CSRF protection, validation, and error handling
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
    /// Phase 3.1.1: Protected with anti-forgery token validation
    /// </summary>
    [HttpPost("/cart/add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            _logger.LogWarning("Invalid AddToCart request: {Errors}", errors);
            return BadRequest(new { success = false, message = "Invalid request" });
        }

        // Phase 3.1.1: Quantity validation
        if (request.Quantity <= 0)
        {
            return BadRequest(new { success = false, message = "Quantity must be at least 1" });
        }

        try
        {
            var success = await _cartService.AddToCartAsync(request.ProductId, request.Quantity);
            if (!success)
            {
                _logger.LogWarning("Failed to add product {ProductId} to cart (quantity: {Quantity})", 
                    request.ProductId, request.Quantity);
                return BadRequest(new { success = false, message = "Unable to add item to cart. Product may be out of stock or unavailable." });
            }

            var count = await _cartService.GetCartItemCountAsync();
            _logger.LogInformation("Product {ProductId} added to cart successfully", request.ProductId);
            return Ok(new { success = true, cartCount = count, message = "Item added to cart" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding product {ProductId} to cart", request.ProductId);
            return StatusCode(500, new { success = false, message = "An error occurred while adding the item to cart" });
        }
    }

    /// <summary>
    /// Update cart item quantity (AJAX)
    /// Phase 3.1.1: Protected with anti-forgery token validation
    /// </summary>
    [HttpPost("/cart/update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCartItem([FromBody] UpdateCartItemRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            _logger.LogWarning("Invalid UpdateCartItem request: {Errors}", errors);
            return BadRequest(new { success = false, message = "Invalid request" });
        }

        // Phase 3.1.1: Quantity validation (0 is allowed - means remove)
        if (request.Quantity < 0)
        {
            return BadRequest(new { success = false, message = "Quantity cannot be negative" });
        }

        try
        {
            var success = await _cartService.UpdateCartItemAsync(request.ProductId, request.Quantity);
            if (!success)
            {
                _logger.LogWarning("Failed to update cart item {ProductId} to quantity {Quantity}", 
                    request.ProductId, request.Quantity);
                return BadRequest(new { success = false, message = "Unable to update cart item. Please check product availability and stock." });
            }

            var cart = await _cartService.GetCartAsync();
            _logger.LogInformation("Cart item {ProductId} updated to quantity {Quantity}", 
                request.ProductId, request.Quantity);
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cart item {ProductId}", request.ProductId);
            return StatusCode(500, new { success = false, message = "An error occurred while updating the cart" });
        }
    }

    /// <summary>
    /// Remove item from cart (AJAX)
    /// Phase 3.1.1: Protected with anti-forgery token validation
    /// </summary>
    [HttpPost("/cart/remove/{productId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveFromCart(int productId)
    {
        try
        {
            var success = await _cartService.RemoveFromCartAsync(productId);
            if (!success)
            {
                _logger.LogWarning("Failed to remove product {ProductId} from cart", productId);
                return BadRequest(new { success = false, message = "Unable to remove item from cart" });
            }

            var cart = await _cartService.GetCartAsync();
            _logger.LogInformation("Product {ProductId} removed from cart", productId);
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing product {ProductId} from cart", productId);
            return StatusCode(500, new { success = false, message = "An error occurred while removing the item" });
        }
    }

    /// <summary>
    /// Get cart count (AJAX)
    /// </summary>
    [HttpGet("/cart/count")]
    public async Task<IActionResult> GetCartCount()
    {
        try
        {
            var count = await _cartService.GetCartItemCountAsync();
            return Ok(new { count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cart count");
            return StatusCode(500, new { count = 0 });
        }
    }

    /// <summary>
    /// Get mini cart data (AJAX)
    /// Returns cart items and summary for off-canvas mini cart
    /// </summary>
    [HttpGet("/cart/mini")]
    public async Task<IActionResult> GetMiniCart()
    {
        try
        {
            var cart = await _cartService.GetCartAsync();
            return Ok(new
            {
                success = true,
                items = cart.Items.Select(item => new
                {
                    productId = item.ProductId,
                    productName = item.ProductName,
                    productSlug = item.ProductSlug,
                    imageUrl = item.ImageUrl,
                    price = item.Price,
                    quantity = item.Quantity,
                    total = item.Total
                }),
                subTotal = cart.SubTotal,
                taxAmount = cart.TaxAmount,
                shippingAmount = cart.ShippingAmount,
                totalAmount = cart.TotalAmount,
                totalItems = cart.TotalItems
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting mini cart");
            return StatusCode(500, new { success = false, message = "An error occurred while loading the cart" });
        }
    }

    /// <summary>
    /// Clear cart
    /// Phase 3.1.1: Protected with anti-forgery token validation
    /// </summary>
    [HttpPost("/cart/clear")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClearCart()
    {
        try
        {
            await _cartService.ClearCartAsync();
            _logger.LogInformation("Cart cleared successfully");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cart");
            TempData["Error"] = "An error occurred while clearing the cart";
            return RedirectToAction(nameof(Index));
        }
    }
}
