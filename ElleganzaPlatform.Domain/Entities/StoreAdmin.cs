using ElleganzaPlatform.Domain.Common;

namespace ElleganzaPlatform.Domain.Entities;

public class StoreAdmin : BaseEntity
{
    public int StoreId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    
    // Navigation properties
    public Store Store { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
