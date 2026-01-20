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

/// <summary>
/// Phase 4: Order status for payment integration
/// Pending: Order created, awaiting payment
/// Paid: Payment successful, ready for processing
/// Processing: Order being prepared
/// Shipped: Order dispatched to customer
/// Delivered: Order received by customer
/// Cancelled: Order cancelled
/// Failed: Payment failed
/// Refunded: Payment refunded
/// </summary>
public enum OrderStatus
{
    Pending = 1,
    Paid = 2,
    Processing = 3,
    Shipped = 4,
    Delivered = 5,
    Cancelled = 6,
    Failed = 7,
    Refunded = 8
}

public enum ProductStatus
{
    Draft = 1,
    PendingApproval = 2,
    Active = 3,
    Inactive = 4,
    OutOfStock = 5
}
