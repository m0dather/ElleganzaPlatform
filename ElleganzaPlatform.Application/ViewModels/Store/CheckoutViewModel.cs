namespace ElleganzaPlatform.Application.ViewModels.Store;

public class CheckoutViewModel
{
    public CartViewModel Cart { get; set; } = new();
    public AddressViewModel ShippingAddress { get; set; } = new();
    public AddressViewModel BillingAddress { get; set; } = new();
    public bool UseSameAddressForBilling { get; set; } = true;
    public string? CustomerNotes { get; set; }
}

public class AddressViewModel
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    public string FormattedAddress =>
        $"{AddressLine1}, {(string.IsNullOrEmpty(AddressLine2) ? "" : AddressLine2 + ", ")}{City}, {State} {PostalCode}, {Country}";
}

public class PlaceOrderRequest
{
    public AddressViewModel ShippingAddress { get; set; } = new();
    public AddressViewModel? BillingAddress { get; set; }
    public string? CustomerNotes { get; set; }
}

public class OrderConfirmationViewModel
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}
