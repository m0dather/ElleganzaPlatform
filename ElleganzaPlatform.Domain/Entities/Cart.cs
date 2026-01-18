using ElleganzaPlatform.Domain.Common;

namespace ElleganzaPlatform.Domain.Entities;

/// <summary>
/// Represents a shopping cart for both guest and authenticated users
/// Guest carts: identified by SessionId
/// Authenticated carts: linked to UserId and store-scoped
/// </summary>
public class Cart : BaseEntity
{
    /// <summary>
    /// User ID for authenticated users (null for guest carts)
    /// </summary>
    public string? UserId { get; set; }
    
    /// <summary>
    /// Session ID for guest carts (null for authenticated user carts)
    /// </summary>
    public string? SessionId { get; set; }
    
    /// <summary>
    /// Store ID for multi-store isolation
    /// </summary>
    public int StoreId { get; set; }
    
    /// <summary>
    /// Last activity timestamp (used for cart cleanup/expiration)
    /// </summary>
    public DateTime LastActivityAt { get; set; }
    
    // Navigation properties
    public ApplicationUser? User { get; set; }
    public Store Store { get; set; } = null!;
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}
