using ElleganzaPlatform.Domain.Enums;

namespace ElleganzaPlatform.Application.ViewModels.Admin;

public class OrderListViewModel
{
    public IEnumerable<OrderListItemViewModel> Orders { get; set; } = new List<OrderListItemViewModel>();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalOrders { get; set; }
}

public class OrderListItemViewModel
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public string StatusDisplay => Status.ToString();
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
}

public class AdminOrderDetailsViewModel
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public OrderStatus Status { get; set; }
    public string StatusDisplay => Status.ToString();
    public string CustomerEmail { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;
    public string? CustomerNotes { get; set; }
    public IEnumerable<AdminOrderItemViewModel> Items { get; set; } = new List<AdminOrderItemViewModel>();
}

public class AdminOrderItemViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
