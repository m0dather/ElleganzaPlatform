using System.ComponentModel.DataAnnotations;
using ElleganzaPlatform.Domain.Enums;

namespace ElleganzaPlatform.Application.ViewModels.Store;

/// <summary>
/// View model for checkout session
/// Used to display and manage checkout session state
/// </summary>
public class CheckoutSessionViewModel
{
    public int Id { get; set; }
    public CheckoutSessionStatus Status { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string ShippingMethod { get; set; } = string.Empty;
    public decimal ShippingCost { get; set; }
    public DateTime ExpiresAt { get; set; }
    public CartViewModel Cart { get; set; } = new();
}

/// <summary>
/// Request model for creating a checkout session
/// First step in the new checkout flow
/// </summary>
public class CreateCheckoutSessionRequest
{
    [Required(ErrorMessage = "Shipping address is required")]
    public AddressViewModel ShippingAddress { get; set; } = new();
    
    public AddressViewModel? BillingAddress { get; set; }
    
    [StringLength(1000, ErrorMessage = "Customer notes cannot exceed 1000 characters")]
    public string? CustomerNotes { get; set; }
}

/// <summary>
/// Request model for selecting shipping method
/// Second step in the checkout flow
/// </summary>
public class SelectShippingMethodRequest
{
    [Required(ErrorMessage = "Checkout session ID is required")]
    public int CheckoutSessionId { get; set; }
    
    [Required(ErrorMessage = "Shipping method is required")]
    [StringLength(100, ErrorMessage = "Shipping method cannot exceed 100 characters")]
    public string ShippingMethod { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Shipping cost is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Shipping cost must be positive")]
    public decimal ShippingCost { get; set; }
}

/// <summary>
/// Request model for selecting payment method
/// Third step in the checkout flow
/// </summary>
public class SelectPaymentMethodRequest
{
    [Required(ErrorMessage = "Checkout session ID is required")]
    public int CheckoutSessionId { get; set; }
    
    [Required(ErrorMessage = "Payment method is required")]
    public PaymentMethod PaymentMethod { get; set; }
}

/// <summary>
/// Request model for creating order from checkout session
/// Final step for COD orders or after online payment success
/// </summary>
public class CreateOrderFromSessionRequest
{
    [Required(ErrorMessage = "Checkout session ID is required")]
    public int CheckoutSessionId { get; set; }
}

/// <summary>
/// Shipping method option
/// Used to display available shipping methods to customer
/// </summary>
public class ShippingMethodOption
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public string EstimatedDelivery { get; set; } = string.Empty;
}
