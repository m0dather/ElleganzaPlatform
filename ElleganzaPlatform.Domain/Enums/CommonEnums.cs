namespace ElleganzaPlatform.Domain.Enums;

/// <summary>
/// Represents the primary role of a user for redirect and authorization purposes.
/// Priority: SuperAdmin > StoreAdmin > Vendor > Customer
/// </summary>
public enum PrimaryRole
{
    /// <summary>
    /// Super administrator with access to all stores and resources
    /// </summary>
    SuperAdmin = 1,

    /// <summary>
    /// Store administrator with access to their assigned store
    /// </summary>
    StoreAdmin = 2,

    /// <summary>
    /// Vendor with access to their vendor-specific resources
    /// </summary>
    Vendor = 3,

    /// <summary>
    /// Customer with access to their account and shopping features
    /// </summary>
    Customer = 4,

    /// <summary>
    /// No role assigned or anonymous user
    /// </summary>
    None = 0
}

public enum UserRole
{
    SuperAdmin = 1,
    StoreAdmin = 2,
    VendorAdmin = 3,
    Customer = 4
}

public enum OrderStatus
{
    Pending = 1,
    Processing = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5,
    Refunded = 6
}

public enum ProductStatus
{
    Draft = 1,
    PendingApproval = 2,
    Active = 3,
    Inactive = 4,
    OutOfStock = 5
}
