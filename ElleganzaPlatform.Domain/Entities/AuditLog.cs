using ElleganzaPlatform.Domain.Common;

namespace ElleganzaPlatform.Domain.Entities;

/// <summary>
/// Audit log entity for tracking administrative actions
/// </summary>
public class AuditLog : BaseEntity
{
    /// <summary>
    /// User ID who performed the action
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// User name who performed the action
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Type of action performed (e.g., "VendorApproved", "VendorRejected", "ProductApproved")
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Entity type affected (e.g., "Vendor", "Product", "Order")
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Entity ID affected
    /// </summary>
    public int EntityId { get; set; }

    /// <summary>
    /// Additional details about the action (JSON format)
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// IP address of the user
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// When the action was performed
    /// </summary>
    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;
}
