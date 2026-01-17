using ElleganzaPlatform.Domain.Common;

namespace ElleganzaPlatform.Domain.Entities;

public class VendorAdmin : BaseEntity
{
    public int VendorId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    
    // Navigation properties
    public Vendor Vendor { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
