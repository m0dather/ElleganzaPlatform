using System.ComponentModel.DataAnnotations;

namespace ElleganzaPlatform.Application.ViewModels.Store;

/// <summary>
/// View model for customer address management
/// Used for creating and editing customer addresses
/// </summary>
public class CustomerAddressViewModel
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public string FirstName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    public string LastName { get; set; } = string.Empty;
    
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
    
    public bool IsDefaultShipping { get; set; }
    public bool IsDefaultBilling { get; set; }
    
    /// <summary>
    /// Formats the address into a display string
    /// </summary>
    public string FormattedAddress =>
        $"{AddressLine1}, {(string.IsNullOrEmpty(AddressLine2) ? "" : AddressLine2 + ", ")}{City}, {State} {PostalCode}, {Country}";
}

/// <summary>
/// View model for the address listing page
/// Contains all customer addresses with metadata
/// </summary>
public class AddressListViewModel
{
    public IEnumerable<CustomerAddressViewModel> Addresses { get; set; } = new List<CustomerAddressViewModel>();
    public bool HasAddresses => Addresses.Any();
    public int TotalAddresses => Addresses.Count();
    public bool CanDeleteAddresses => TotalAddresses > 1;
}
