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
/// Pending: Order created, awaiting payment (deprecated - use PendingPayment)
/// PendingPayment: Order created with COD, awaiting payment on delivery
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
    Refunded = 8,
    PendingPayment = 9
}

/// <summary>
/// Payment method options for checkout
/// Online: Online payment via payment gateway (e.g., Stripe)
/// CashOnDelivery: Pay cash when order is delivered
/// </summary>
public enum PaymentMethod
{
    Online = 1,
    CashOnDelivery = 2
}

/// <summary>
/// Checkout session status
/// Draft: Session created, customer filling out details
/// Paid: Online payment completed successfully
/// COD: Cash on delivery selected, ready to create order
/// Expired: Session expired due to timeout
/// Completed: Order created from this session
/// </summary>
public enum CheckoutSessionStatus
{
    Draft = 1,
    Paid = 2,
    COD = 3,
    Expired = 4,
    Completed = 5
}

public enum ProductStatus
{
    Draft = 1,
    PendingApproval = 2,
    Active = 3,
    Inactive = 4,
    OutOfStock = 5,
    Rejected = 6,
    Disabled = 7
}

/// <summary>
/// Vendor approval status for marketplace control
/// Pending: Awaiting admin approval
/// Approved: Active and can publish products
/// Rejected: Rejected by admin
/// Suspended: Temporarily suspended by admin
/// </summary>
public enum VendorStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Suspended = 4
}
