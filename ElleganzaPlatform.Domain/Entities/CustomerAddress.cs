using ElleganzaPlatform.Domain.Common;

namespace ElleganzaPlatform.Domain.Entities;

/// <summary>
/// Represents a customer's saved address for shipping or billing.
/// Each customer can have multiple addresses with one default shipping and one default billing.
/// </summary>
public class CustomerAddress : BaseEntity
{
    /// <summary>
    /// Foreign key to the ApplicationUser (Customer)
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    
    /// <summary>
    /// Indicates if this is the default shipping address for the customer
    /// </summary>
    public bool IsDefaultShipping { get; set; }
    
    /// <summary>
    /// Indicates if this is the default billing address for the customer
    /// </summary>
    public bool IsDefaultBilling { get; set; }
    
    // Navigation property
    public ApplicationUser User { get; set; } = null!;
}
