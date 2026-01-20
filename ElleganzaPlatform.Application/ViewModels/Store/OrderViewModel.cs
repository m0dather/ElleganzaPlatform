using ElleganzaPlatform.Domain.Enums;

namespace ElleganzaPlatform.Application.ViewModels.Store;

public class CustomerOrderListViewModel
{
    public List<CustomerOrderItemViewModel> Orders { get; set; } = new();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalOrders { get; set; }
}

public class CustomerOrderItemViewModel
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public string StatusDisplay => Status.ToString();
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
    
    // Phase 4: Payment Integration
    public bool CanBePaid => Status == OrderStatus.Pending;
}

public class CustomerOrderDetailsViewModel
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public string StatusDisplay => Status.ToString();
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;
    public string? CustomerNotes { get; set; }
    public List<CustomerOrderItemDetailsViewModel> Items { get; set; } = new();
    
    // Phase 4: Payment Integration
    public string? PaymentTransactionId { get; set; }
    public bool CanBePaid => Status == OrderStatus.Pending;
}

public class CustomerOrderItemDetailsViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public string? ProductImage { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
