using ElleganzaPlatform.Domain.Entities;

namespace ElleganzaPlatform.Application.Services;

/// <summary>
/// Service interface for audit logging
/// </summary>
public interface IAuditLogService
{
    /// <summary>
    /// Log an administrative action
    /// </summary>
    /// <param name="action">Action type (e.g., "VendorApproved", "VendorRejected")</param>
    /// <param name="entityType">Entity type (e.g., "Vendor", "Product")</param>
    /// <param name="entityId">Entity ID</param>
    /// <param name="details">Additional details (optional)</param>
    Task LogActionAsync(string action, string entityType, int entityId, string? details = null);

    /// <summary>
    /// Get audit logs with pagination
    /// </summary>
    Task<List<AuditLog>> GetAuditLogsAsync(int page = 1, int pageSize = 50);

    /// <summary>
    /// Get audit logs for a specific entity
    /// </summary>
    Task<List<AuditLog>> GetEntityAuditLogsAsync(string entityType, int entityId);
}
