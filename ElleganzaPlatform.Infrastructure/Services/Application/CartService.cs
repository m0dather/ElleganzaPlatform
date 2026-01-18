using ElleganzaPlatform.Application.Common;
using ElleganzaPlatform.Application.Services;
using ElleganzaPlatform.Application.ViewModels.Store;
using ElleganzaPlatform.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ElleganzaPlatform.Infrastructure.Services.Application;

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

        // Get product details
        var product = await _context.Products
            .Where(p => p.Id == productId && !p.IsDeleted)
            .FirstOrDefaultAsync();

        if (product == null || product.StockQuantity < quantity)
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
            // Add new item
            cartItems.Add(new CartItemViewModel
            {
                ProductId = product.Id,
                ProductName = product.Name,
                ProductSku = product.Sku,
                UnitPrice = product.Price,
                Quantity = quantity,
                ImageUrl = product.MainImage,
                VendorId = product.VendorId,
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

        // Check stock availability
        var product = await _context.Products
            .Where(p => p.Id == productId && !p.IsDeleted)
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
        await SaveCartItemsAsync(new List<CartItemViewModel>());
    }

    public async Task MergeGuestCartAsync()
    {
        // This is called after login to merge guest session cart with user cart
        // For simplicity, we'll keep using session-based cart even for authenticated users
        // In a full implementation, you might want to persist cart to database for logged-in users
        await Task.CompletedTask;
    }

    public async Task<int> GetCartItemCountAsync()
    {
        var cartItems = await GetCartItemsAsync();
        return cartItems.Sum(i => i.Quantity);
    }

    private async Task<List<CartItemViewModel>> GetCartItemsAsync()
    {
        // For this implementation, we use session storage for both guest and authenticated users
        // This keeps the implementation simple and stateless
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

    private async Task SaveCartItemsAsync(List<CartItemViewModel> items)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null)
            return;

        var cartJson = JsonSerializer.Serialize(items);
        session.SetString(CartSessionKey, cartJson);
        await Task.CompletedTask;
    }
}
