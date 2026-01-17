namespace ElleganzaPlatform.Domain.Enums;

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
