namespace ElleganzaPlatform.Infrastructure.Authorization;

public static class Policies
{
    public const string SuperAdminPolicy = "SuperAdminPolicy";
    public const string StoreAdminPolicy = "StoreAdminPolicy";
    public const string VendorPolicy = "VendorPolicy";
    public const string CustomerPolicy = "CustomerPolicy";
}

public static class Roles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string StoreAdmin = "StoreAdmin";
    public const string VendorAdmin = "VendorAdmin";
    public const string Customer = "Customer";
}

public static class ClaimTypes
{
    public const string StoreId = "StoreId";
    public const string VendorId = "VendorId";
}
