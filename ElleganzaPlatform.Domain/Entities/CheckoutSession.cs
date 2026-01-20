using ElleganzaPlatform.Domain.Common;
using ElleganzaPlatform.Domain.Enums;

namespace ElleganzaPlatform.Domain.Entities;

/// <summary>
/// CheckoutSession (PreOrder)
/// Represents a checkout session before order creation
/// Stores cart snapshot and payment information
/// Order is created only after payment decision (Paid or COD)
/// </summary>
public class CheckoutSession : BaseEntity
{
    public int StoreId { get; set; }
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// JSON snapshot of cart at checkout time
    /// Preserves cart state even if products/prices change
    /// </summary>
    public string CartSnapshot { get; set; } = string.Empty;
    
    /// <summary>
    /// Shipping method selected by customer (e.g., "Standard", "Express")
    /// </summary>
    public string ShippingMethod { get; set; } = string.Empty;
    
    /// <summary>
    /// Shipping cost based on selected method
    /// </summary>
    public decimal ShippingCost { get; set; }
    
    /// <summary>
    /// Payment method selected by customer
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; }
    
    /// <summary>
    /// Stripe PaymentIntent ID (only for online payments)
    /// Used to verify payment completion
    /// </summary>
    public string? PaymentIntentId { get; set; }
    
    /// <summary>
    /// Current status of the checkout session
    /// </summary>
    public CheckoutSessionStatus Status { get; set; }
    
    /// <summary>
    /// Session expiration time
    /// Prevents stale sessions from creating orders
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// Shipping address (formatted string)
    /// </summary>
    public string ShippingAddress { get; set; } = string.Empty;
    
    /// <summary>
    /// Billing address (formatted string)
    /// </summary>
    public string BillingAddress { get; set; } = string.Empty;
    
    /// <summary>
    /// Customer notes/instructions
    /// </summary>
    public string? CustomerNotes { get; set; }
    
    /// <summary>
    /// Order ID created from this session (nullable until order is created)
    /// </summary>
    public int? OrderId { get; set; }
    
    // Navigation properties
    public Store Store { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
    public Order? Order { get; set; }
}
