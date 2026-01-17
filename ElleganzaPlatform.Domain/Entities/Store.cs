using ElleganzaPlatform.Domain.Common;

namespace ElleganzaPlatform.Domain.Entities;

public class Store : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public string? Logo { get; set; }
    public string? Domain { get; set; }
    public bool IsActive { get; set; }
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }
    public string? SeoKeywords { get; set; }
    
    // Navigation properties
    public ICollection<Vendor> Vendors { get; set; } = new List<Vendor>();
    public ICollection<StoreAdmin> StoreAdmins { get; set; } = new List<StoreAdmin>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
