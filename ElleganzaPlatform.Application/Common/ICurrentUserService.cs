namespace ElleganzaPlatform.Application.Common;

public interface ICurrentUserService
{
    string? UserId { get; }
    int? StoreId { get; }
    int? VendorId { get; }
    bool IsInRole(string role);
    bool IsSuperAdmin { get; }
    bool IsStoreAdmin { get; }
    bool IsVendorAdmin { get; }
    bool IsCustomer { get; }
}
