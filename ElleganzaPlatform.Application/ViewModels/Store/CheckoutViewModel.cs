using System.ComponentModel.DataAnnotations;
using ElleganzaPlatform.Domain.Enums;

namespace ElleganzaPlatform.Application.ViewModels.Store;

/// <summary>
/// View model for the checkout page
/// Contains cart summary and customer address information
/// </summary>
public class CheckoutViewModel
{
    public CartViewModel Cart { get; set; } = new();
    public AddressViewModel ShippingAddress { get; set; } = new();
    public AddressViewModel BillingAddress { get; set; } = new();
    public bool UseSameAddressForBilling { get; set; } = true;
    public string? CustomerNotes { get; set; }
    
    // New checkout flow properties
    public List<ShippingMethodOption> AvailableShippingMethods { get; set; } = new();
    public string? SelectedShippingMethod { get; set; }
    public decimal ShippingCost { get; set; }
    public PaymentMethod? SelectedPaymentMethod { get; set; }
    public int? CheckoutSessionId { get; set; }
}

/// <summary>
/// View model for customer address (shipping or billing)
/// Includes validation attributes for required fields
/// </summary>
public class AddressViewModel
{
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public string FirstName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    public string LastName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Phone number is required")]
    [RegularExpression(@"^\+?[\d\s\-\(\)]{10,20}$", ErrorMessage = "Please enter a valid phone number (10-20 characters including formatting)")]
    public string Phone { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Address is required")]
    [StringLength(200, MinimumLength = 10, ErrorMessage = "Address must be between 10 and 200 characters")]
    public string AddressLine1 { get; set; } = string.Empty;
    
    [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
    public string? AddressLine2 { get; set; }
    
    [Required(ErrorMessage = "City is required")]
    [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
    public string City { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "State/Province is required")]
    [StringLength(100, ErrorMessage = "State/Province cannot exceed 100 characters")]
    public string State { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Postal code is required")]
    [StringLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
    public string PostalCode { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Country is required")]
    [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Formats the address into a single line string for storage
    /// </summary>
    public string FormattedAddress =>
        $"{AddressLine1}, {(string.IsNullOrEmpty(AddressLine2) ? "" : AddressLine2 + ", ")}{City}, {State} {PostalCode}, {Country}";
}

/// <summary>
/// Request model for placing an order
/// Contains shipping/billing addresses and optional customer notes
/// </summary>
public class PlaceOrderRequest
{
    [Required(ErrorMessage = "Shipping address is required")]
    public AddressViewModel ShippingAddress { get; set; } = new();
    
    public AddressViewModel? BillingAddress { get; set; }
    
    [StringLength(1000, ErrorMessage = "Customer notes cannot exceed 1000 characters")]
    public string? CustomerNotes { get; set; }
}

/// <summary>
/// View model for order confirmation page
/// Displays order summary after successful checkout
/// </summary>
public class OrderConfirmationViewModel
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    
    // Phase 4: Payment Integration
    public bool CanBePaid { get; set; }
}
