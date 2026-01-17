using ElleganzaPlatform.Application.Common;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ElleganzaPlatform.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public int? StoreId
    {
        get
        {
            var storeIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("StoreId");
            return int.TryParse(storeIdClaim, out var storeId) ? storeId : null;
        }
    }

    public int? VendorId
    {
        get
        {
            var vendorIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("VendorId");
            return int.TryParse(vendorIdClaim, out var vendorId) ? vendorId : null;
        }
    }

    public bool IsInRole(string role)
    {
        return _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
    }

    public bool IsSuperAdmin => IsInRole("SuperAdmin");

    public bool IsStoreAdmin => IsInRole("StoreAdmin");

    public bool IsVendorAdmin => IsInRole("Vendor");

    public bool IsCustomer => IsInRole("Customer");
}
