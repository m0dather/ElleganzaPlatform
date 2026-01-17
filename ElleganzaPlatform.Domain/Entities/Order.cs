using ElleganzaPlatform.Domain.Common;
using ElleganzaPlatform.Domain.Enums;

namespace ElleganzaPlatform.Domain.Entities;

public class Order : BaseEntity
{
    public int StoreId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;
    public string? CustomerNotes { get; set; }
    
    // Navigation properties
    public Store Store { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
