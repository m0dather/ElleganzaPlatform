using ElleganzaPlatform.Domain.Common;

namespace ElleganzaPlatform.Domain.Entities;

public class Vendor : BaseEntity
{
    public int StoreId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public string? Logo { get; set; }
    public bool IsActive { get; set; }
    public decimal CommissionRate { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    
    // Navigation properties
    public Store Store { get; set; } = null!;
    public ICollection<VendorAdmin> VendorAdmins { get; set; } = new List<VendorAdmin>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
