namespace ElleganzaPlatform.Application.ViewModels.Store;

public class CustomerAccountViewModel
{
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public int TotalOrders { get; set; }
    public int WishlistCount { get; set; }
}

public class CustomerOrdersViewModel
{
    public IEnumerable<OrderSummaryViewModel> Orders { get; set; } = new List<OrderSummaryViewModel>();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalOrders { get; set; }
}

public class OrderSummaryViewModel
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
}

public class OrderDetailsViewModel
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;
    public IEnumerable<OrderItemViewModel> Items { get; set; } = new List<OrderItemViewModel>();
}

public class OrderItemViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
