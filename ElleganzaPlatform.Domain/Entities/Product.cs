using ElleganzaPlatform.Domain.Common;
using ElleganzaPlatform.Domain.Enums;

namespace ElleganzaPlatform.Domain.Entities;

public class Product : BaseEntity
{
    public int StoreId { get; set; }
    public int VendorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public int StockQuantity { get; set; }
    public ProductStatus Status { get; set; }
    public string? MainImage { get; set; }
    public bool RequiresApproval { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectedBy { get; set; }
    
    // Navigation properties
    public Store Store { get; set; } = null!;
    public Vendor Vendor { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
