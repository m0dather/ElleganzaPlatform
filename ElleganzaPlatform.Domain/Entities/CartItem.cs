using ElleganzaPlatform.Domain.Common;

namespace ElleganzaPlatform.Domain.Entities;

/// <summary>
/// Represents an item in a shopping cart
/// Includes price snapshot to handle price changes
/// Enforces VendorId and StoreId for proper isolation
/// </summary>
public class CartItem : BaseEntity
{
    /// <summary>
    /// Foreign key to Cart
    /// </summary>
    public int CartId { get; set; }
    
    /// <summary>
    /// Product ID
    /// </summary>
    public int ProductId { get; set; }
    
    /// <summary>
    /// Quantity of the product in cart
    /// </summary>
    public int Quantity { get; set; }
    
    /// <summary>
    /// Price snapshot at the time of adding to cart
    /// Prevents price changes from affecting cart items
    /// </summary>
    public decimal PriceSnapshot { get; set; }
    
    /// <summary>
    /// Vendor ID for isolation enforcement
    /// </summary>
    public int VendorId { get; set; }
    
    /// <summary>
    /// Store ID for multi-store isolation
    /// </summary>
    public int StoreId { get; set; }
    
    // Navigation properties
    public Cart Cart { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public Vendor Vendor { get; set; } = null!;
    public Store Store { get; set; } = null!;
}
