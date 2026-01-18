using ElleganzaPlatform.Application.Common;
using ElleganzaPlatform.Application.Services;
using ElleganzaPlatform.Application.ViewModels.Store;
using ElleganzaPlatform.Domain.Entities;
using ElleganzaPlatform.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ElleganzaPlatform.Infrastructure.Services.Application;

/// <summary>
/// Shopping cart service supporting both guest (session-based) and authenticated (database) users
/// Handles cart merge on login, store/vendor isolation, and stock validation
/// </summary>
public class CartService : ICartService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IStoreContextService _storeContextService;
    private const string CartSessionKey = "ShoppingCart";

    public CartService(
        ApplicationDbContext context,
        ICurrentUserService currentUserService,
        IHttpContextAccessor httpContextAccessor,
        IStoreContextService storeContextService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _httpContextAccessor = httpContextAccessor;
        _storeContextService = storeContextService;
    }

    public async Task<CartViewModel> GetCartAsync()
    {
        var cartItems = await GetCartItemsAsync();
        var cart = new CartViewModel { Items = cartItems };

        // Calculate totals
        cart.SubTotal = cart.Items.Sum(i => i.TotalPrice);
        cart.TaxAmount = cart.SubTotal * 0.1m; // 10% tax (should be configurable)
        cart.ShippingAmount = cart.SubTotal > 100 ? 0 : 10; // Free shipping over $100
        cart.TotalAmount = cart.SubTotal + cart.TaxAmount + cart.ShippingAmount;

        return cart;
    }

    public async Task<bool> AddToCartAsync(int productId, int quantity = 1)
    {
        if (quantity <= 0)
            return false;

        // Get current store context
        var storeId = await _storeContextService.GetCurrentStoreIdAsync();
        if (!storeId.HasValue)
            return false;

        // Get product details with store/vendor validation
        var product = await _context.Products
            .Where(p => p.Id == productId && !p.IsDeleted && p.StoreId == storeId.Value)
            .FirstOrDefaultAsync();

        if (product == null)
            return false;

        // Check stock availability
        if (product.StockQuantity < quantity)
            return false;

        var cartItems = await GetCartItemsAsync();
        var existingItem = cartItems.FirstOrDefault(i => i.ProductId == productId);

        if (existingItem != null)
        {
            // Update quantity if item already exists
            var newQuantity = existingItem.Quantity + quantity;
            if (newQuantity > product.StockQuantity)
                return false;

            existingItem.Quantity = newQuantity;
        }
        else
        {
            // Add new item with all required fields
            cartItems.Add(new CartItemViewModel
            {
                ProductId = product.Id,
                ProductName = product.Name,
                ProductSku = product.Sku,
                UnitPrice = product.Price,
                Quantity = quantity,
                ImageUrl = product.MainImage,
                VendorId = product.VendorId,
                StoreId = product.StoreId,
                StockQuantity = product.StockQuantity
            });
        }

        await SaveCartItemsAsync(cartItems);
        return true;
    }

    public async Task<bool> UpdateCartItemAsync(int productId, int quantity)
    {
        if (quantity < 0)
            return false;

        if (quantity == 0)
            return await RemoveFromCartAsync(productId);

        var cartItems = await GetCartItemsAsync();
        var item = cartItems.FirstOrDefault(i => i.ProductId == productId);

        if (item == null)
            return false;

        // Check stock availability and store context
        var storeId = await _storeContextService.GetCurrentStoreIdAsync();
        var product = await _context.Products
            .Where(p => p.Id == productId && !p.IsDeleted && p.StoreId == storeId)
            .FirstOrDefaultAsync();

        if (product == null || product.StockQuantity < quantity)
            return false;

        item.Quantity = quantity;
        await SaveCartItemsAsync(cartItems);
        return true;
    }

    public async Task<bool> RemoveFromCartAsync(int productId)
    {
        var cartItems = await GetCartItemsAsync();
        var item = cartItems.FirstOrDefault(i => i.ProductId == productId);

        if (item == null)
            return false;

        cartItems.Remove(item);
        await SaveCartItemsAsync(cartItems);
        return true;
    }

    public async Task ClearCartAsync()
    {
        // Clear based on user type
        if (!string.IsNullOrEmpty(_currentUserService.UserId))
        {
            // Clear database cart for authenticated users
            var storeId = await _storeContextService.GetCurrentStoreIdAsync();
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == _currentUserService.UserId && c.StoreId == storeId);

            if (cart != null)
            {
                _context.CartItems.RemoveRange(cart.Items);
                _context.Carts.Remove(cart);
                await _context.SaveChangesAsync();
            }
        }
        
        // Always clear session cart
        await SaveCartItemsToSessionAsync(new List<CartItemViewModel>());
    }

    /// <summary>
    /// Merges guest session cart into authenticated user's database cart after login
    /// Called after successful authentication
    /// </summary>
    public async Task MergeGuestCartAsync()
    {
        // Only merge if user is authenticated
        if (string.IsNullOrEmpty(_currentUserService.UserId))
            return;

        var storeId = await _storeContextService.GetCurrentStoreIdAsync();
        if (!storeId.HasValue)
            return;

        // Get guest cart from session
        var guestCartItems = await GetCartItemsFromSessionAsync();
        if (guestCartItems.Count == 0)
            return;

        // Get or create user's database cart
        var userCart = await GetOrCreateUserCartAsync(_currentUserService.UserId, storeId.Value);

        // Merge items: add guest cart items to user cart, avoiding duplicates
        foreach (var guestItem in guestCartItems)
        {
            // Check if product already exists in user cart
            var existingItem = userCart.Items.FirstOrDefault(i => i.ProductId == guestItem.ProductId);

            if (existingItem != null)
            {
                // Update quantity (take the maximum or sum, depending on business rules)
                // Business Rule: Sum quantities from guest and user carts
                // This allows users to continue shopping as guest and keep their progress
                var product = await _context.Products
                    .Where(p => p.Id == guestItem.ProductId && !p.IsDeleted)
                    .FirstOrDefaultAsync();

                if (product != null)
                {
                    var newQuantity = existingItem.Quantity + guestItem.Quantity;
                    // Business Rule: Never exceed available stock
                    existingItem.Quantity = Math.Min(newQuantity, product.StockQuantity);
                    existingItem.UpdatedAt = DateTime.UtcNow;
                }
            }
            else
            {
                // Add new item to user cart
                var product = await _context.Products
                    .Where(p => p.Id == guestItem.ProductId && !p.IsDeleted && p.StoreId == storeId.Value)
                    .FirstOrDefaultAsync();

                if (product != null && product.StockQuantity >= guestItem.Quantity)
                {
                    var cartItem = new CartItem
                    {
                        CartId = userCart.Id,
                        ProductId = product.Id,
                        // Business Rule: Cap quantity at available stock
                        Quantity = Math.Min(guestItem.Quantity, product.StockQuantity),
                        PriceSnapshot = product.Price,
                        VendorId = product.VendorId,
                        StoreId = product.StoreId,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = _currentUserService.UserId
                    };

                    userCart.Items.Add(cartItem);
                }
            }
        }

        // Update cart last activity
        userCart.LastActivityAt = DateTime.UtcNow;
        userCart.UpdatedAt = DateTime.UtcNow;
        userCart.UpdatedBy = _currentUserService.UserId;

        await _context.SaveChangesAsync();

        // Clear guest session cart after successful merge
        await SaveCartItemsToSessionAsync(new List<CartItemViewModel>());
    }

    public async Task<int> GetCartItemCountAsync()
    {
        var cartItems = await GetCartItemsAsync();
        return cartItems.Sum(i => i.Quantity);
    }

    #region Private Helper Methods

    /// <summary>
    /// Gets cart items based on user authentication status
    /// Authenticated: from database
    /// Guest: from session
    /// </summary>
    private async Task<List<CartItemViewModel>> GetCartItemsAsync()
    {
        // If user is authenticated, load cart from database
        if (!string.IsNullOrEmpty(_currentUserService.UserId))
        {
            return await GetCartItemsFromDatabaseAsync(_currentUserService.UserId);
        }

        // Otherwise, load from session (guest cart)
        return await GetCartItemsFromSessionAsync();
    }

    /// <summary>
    /// Gets cart items from database for authenticated users
    /// </summary>
    private async Task<List<CartItemViewModel>> GetCartItemsFromDatabaseAsync(string userId)
    {
        var storeId = await _storeContextService.GetCurrentStoreIdAsync();
        if (!storeId.HasValue)
            return new List<CartItemViewModel>();

        var cart = await _context.Carts
            .Include(c => c.Items)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId && c.StoreId == storeId.Value && !c.IsDeleted);

        if (cart == null)
            return new List<CartItemViewModel>();

        // Map CartItem entities to CartItemViewModel
        return cart.Items
            .Where(i => !i.IsDeleted)
            .Select(i => new CartItemViewModel
            {
                ProductId = i.ProductId,
                ProductName = i.Product.Name,
                ProductSku = i.Product.Sku,
                UnitPrice = i.PriceSnapshot, // Use price snapshot
                Quantity = i.Quantity,
                ImageUrl = i.Product.MainImage,
                VendorId = i.VendorId,
                StoreId = i.StoreId,
                StockQuantity = i.Product.StockQuantity
            })
            .ToList();
    }

    /// <summary>
    /// Gets cart items from session for guest users
    /// </summary>
    private async Task<List<CartItemViewModel>> GetCartItemsFromSessionAsync()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null)
            return new List<CartItemViewModel>();

        var cartJson = session.GetString(CartSessionKey);
        if (string.IsNullOrEmpty(cartJson))
            return new List<CartItemViewModel>();

        try
        {
            var items = JsonSerializer.Deserialize<List<CartItemViewModel>>(cartJson);
            return items ?? new List<CartItemViewModel>();
        }
        catch
        {
            return new List<CartItemViewModel>();
        }
    }

    /// <summary>
    /// Saves cart items based on user authentication status
    /// Authenticated: to database
    /// Guest: to session
    /// </summary>
    private async Task SaveCartItemsAsync(List<CartItemViewModel> items)
    {
        // If user is authenticated, save to database
        if (!string.IsNullOrEmpty(_currentUserService.UserId))
        {
            await SaveCartItemsToDatabaseAsync(_currentUserService.UserId, items);
        }
        else
        {
            // Otherwise, save to session (guest cart)
            await SaveCartItemsToSessionAsync(items);
        }
    }

    /// <summary>
    /// Saves cart items to database for authenticated users
    /// </summary>
    private async Task SaveCartItemsToDatabaseAsync(string userId, List<CartItemViewModel> items)
    {
        var storeId = await _storeContextService.GetCurrentStoreIdAsync();
        if (!storeId.HasValue)
            return;

        // Get or create cart
        var cart = await GetOrCreateUserCartAsync(userId, storeId.Value);

        // Remove all existing items
        var existingItems = await _context.CartItems
            .Where(ci => ci.CartId == cart.Id)
            .ToListAsync();
        _context.CartItems.RemoveRange(existingItems);

        // Add new items
        foreach (var item in items)
        {
            var cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                PriceSnapshot = item.UnitPrice,
                VendorId = item.VendorId,
                StoreId = item.StoreId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            _context.CartItems.Add(cartItem);
        }

        // Update cart last activity
        cart.LastActivityAt = DateTime.UtcNow;
        cart.UpdatedAt = DateTime.UtcNow;
        cart.UpdatedBy = userId;

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Saves cart items to session for guest users
    /// </summary>
    private async Task SaveCartItemsToSessionAsync(List<CartItemViewModel> items)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null)
            return;

        var cartJson = JsonSerializer.Serialize(items);
        session.SetString(CartSessionKey, cartJson);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Gets existing cart or creates a new one for authenticated user
    /// </summary>
    private async Task<Cart> GetOrCreateUserCartAsync(string userId, int storeId)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId && c.StoreId == storeId && !c.IsDeleted);

        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId,
                StoreId = storeId,
                LastActivityAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        return cart;
    }

    #endregion
}
