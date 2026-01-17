# Authorization Implementation Guide - Phase 1.1

## Overview

This document describes the **Policy-Based Authorization** system implemented in ElleganzaPlatform. The system enforces strict role and store-based access control using ASP.NET Core's authorization policies.

## Architecture

### Core Principles

1. **Policy-Based Only**: All authorization uses `[Authorize(Policy = "...")]` instead of role-based attributes
2. **Centralized Definitions**: All policies are defined in a single constants class
3. **Service-Based**: Authorization handlers use `ICurrentUserService` for user context
4. **Store-Aware**: Policies respect multi-store architecture with store isolation
5. **SuperAdmin Bypass**: SuperAdmin role has access to all resources

## Available Policies

### 1. RequireSuperAdmin

**Purpose**: Restricts access to SuperAdmin users only

**Usage**:
```csharp
[Authorize(Policy = AuthorizationPolicies.RequireSuperAdmin)]
public class SuperAdminController : Controller
{
    // Only accessible by SuperAdmin
}
```

**Requirements**:
- User must be authenticated
- User must have the `SuperAdmin` role

**Example**: Super Admin dashboard at `/super-admin`

---

### 2. RequireStoreAdmin

**Purpose**: Restricts access to StoreAdmin users with store isolation

**Usage**:
```csharp
[Authorize(Policy = AuthorizationPolicies.RequireStoreAdmin)]
public class StoreAdminController : Controller
{
    // Accessible by StoreAdmin (own store only) and SuperAdmin (all stores)
}
```

**Requirements**:
- User must be authenticated
- User must have the `StoreAdmin` role
- User must have a valid `StoreId` claim
- SuperAdmin bypass: SuperAdmin can access all stores

**Example**: Store Admin dashboard at `/admin`

---

### 3. RequireVendor

**Purpose**: Restricts access to Vendor users with vendor isolation

**Usage**:
```csharp
[Authorize(Policy = AuthorizationPolicies.RequireVendor)]
public class VendorController : Controller
{
    // Accessible by Vendor (own vendor only) and SuperAdmin (all vendors)
}
```

**Requirements**:
- User must be authenticated
- User must have the `Vendor` role
- User must have a valid `VendorId` claim
- SuperAdmin bypass: SuperAdmin can access all vendors

**Example**: Vendor dashboard at `/vendor`

---

### 4. RequireCustomer

**Purpose**: Restricts access to Customer users

**Usage**:
```csharp
[Authorize(Policy = AuthorizationPolicies.RequireCustomer)]
public class CustomerController : Controller
{
    // Only accessible by Customer users
}
```

**Requirements**:
- User must be authenticated
- User must have the `Customer` role

**Example**: Customer account at `/account`

---

### 5. RequireSameStore

**Purpose**: Ensures user belongs to the current store context

**Usage**:
```csharp
[Authorize(Policy = AuthorizationPolicies.RequireSameStore)]
public class StoreSpecificController : Controller
{
    // User's StoreId must match current store context
}
```

**Requirements**:
- User must be authenticated
- User's `StoreId` claim must match the current store context (`IStoreContextService.GetCurrentStoreIdAsync()`)
- SuperAdmin bypass: SuperAdmin can access all stores

**Use Case**: Store-specific resources that should only be accessible within the correct store context

---

## Implementation Details

### Policy Constants

All policy names are defined in `AuthorizationPolicies` class:

```csharp
public static class AuthorizationPolicies
{
    public const string RequireSuperAdmin = "RequireSuperAdmin";
    public const string RequireStoreAdmin = "RequireStoreAdmin";
    public const string RequireVendor = "RequireVendor";
    public const string RequireCustomer = "RequireCustomer";
    public const string RequireSameStore = "RequireSameStore";
}
```

### Authorization Handlers

Each policy has a corresponding requirement and handler:

1. **SuperAdminRequirement** → `SuperAdminAuthorizationHandler`
2. **StoreAdminRequirement** → `StoreAdminAuthorizationHandler`
3. **VendorRequirement** → `VendorAuthorizationHandler`
4. **CustomerRequirement** → `CustomerAuthorizationHandler`
5. **SameStoreRequirement** → `SameStoreAuthorizationHandler`

### Handler Dependencies

All handlers depend on `ICurrentUserService` which provides:
- `UserId`: Current user's ID
- `StoreId`: User's store ID (for StoreAdmin)
- `VendorId`: User's vendor ID (for Vendor)
- `IsSuperAdmin`: Quick check for SuperAdmin role
- `IsStoreAdmin`: Quick check for StoreAdmin role
- `IsCustomer`: Quick check for Customer role
- `IsInRole(string role)`: Generic role check

The `SameStoreAuthorizationHandler` also uses `IStoreContextService` to get the current store context.

### Registration

Policies are registered in `Program.cs`:

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthorizationPolicies.RequireSuperAdmin, policy =>
        policy.AddRequirements(new SuperAdminRequirement()));

    options.AddPolicy(AuthorizationPolicies.RequireStoreAdmin, policy =>
        policy.AddRequirements(new StoreAdminRequirement()));

    options.AddPolicy(AuthorizationPolicies.RequireVendor, policy =>
        policy.AddRequirements(new VendorRequirement()));

    options.AddPolicy(AuthorizationPolicies.RequireCustomer, policy =>
        policy.AddRequirements(new CustomerRequirement()));

    options.AddPolicy(AuthorizationPolicies.RequireSameStore, policy =>
        policy.AddRequirements(new SameStoreRequirement()));
});
```

Authorization handlers are registered in `DependencyInjection.cs` (Infrastructure layer):

```csharp
services.AddScoped<IAuthorizationHandler, SuperAdminAuthorizationHandler>();
services.AddScoped<IAuthorizationHandler, StoreAdminAuthorizationHandler>();
services.AddScoped<IAuthorizationHandler, VendorAuthorizationHandler>();
services.AddScoped<IAuthorizationHandler, CustomerAuthorizationHandler>();
services.AddScoped<IAuthorizationHandler, SameStoreAuthorizationHandler>();
```

## Role Model

### Identity Roles

The system uses ASP.NET Identity roles:

```csharp
public static class Roles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string StoreAdmin = "StoreAdmin";
    public const string Vendor = "Vendor";
    public const string Customer = "Customer";
}
```

### Role Assignment

Roles are assigned during:
1. **Database Seeding**: SuperAdmin user is created
2. **User Registration**: Customer and Vendor roles assigned during registration
3. **Manual Assignment**: StoreAdmin role can be assigned by SuperAdmin

**Important Note**: The `VendorAdmin` entity in the database is a join table that links users to vendors. This is separate from the `Vendor` **role** used for authorization. A user with the Vendor role will have a corresponding entry in the VendorAdmins table that stores their VendorId.

### Custom Claims

Additional claims for store and vendor isolation:

- **StoreId**: Assigned to StoreAdmin users
- **VendorId**: Assigned to Vendor users

Claims are added after successful login in `AccountController.AddCustomClaimsAsync()`.

## Best Practices

### ✅ DO

- Use policy constants: `AuthorizationPolicies.RequireSuperAdmin`
- Apply policies at controller or action level
- Rely on handlers for authorization logic
- Use `ICurrentUserService` to check user context
- Document why a specific policy is used

### ❌ DON'T

- Use `[Authorize(Roles = "...")]` attributes
- Hardcode role checks in controllers or views
- Implement authorization logic in controllers
- Check roles directly in Razor views
- Bypass policies with custom authorization logic

## Example Controller

```csharp
using ElleganzaPlatform.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElleganzaPlatform.Areas.Admin.Super.Controllers;

[Area("Admin")]
[Route("super-admin")]
[Authorize(Policy = AuthorizationPolicies.RequireSuperAdmin)]
public class DashboardController : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        // Only SuperAdmin users can access this
        return View();
    }

    [HttpGet("stores")]
    public IActionResult Stores()
    {
        // Only SuperAdmin users can access this
        return View();
    }
}
```

## Testing Authorization

### Manual Testing

1. **SuperAdmin Access**:
   - Login as `superadmin@elleganza.com` / `SuperAdmin@123`
   - Access `/super-admin` - Should succeed
   - Access `/admin` - Should succeed (SuperAdmin bypass)
   - Access `/vendor` - Should succeed (SuperAdmin bypass)

2. **StoreAdmin Access**:
   - Login as a StoreAdmin user
   - Access `/admin` - Should succeed
   - Access `/super-admin` - Should fail (403 Forbidden)

3. **Vendor Access**:
   - Login as a Vendor user
   - Access `/vendor` - Should succeed
   - Access `/admin` - Should fail (403 Forbidden)

4. **Customer Access**:
   - Login as a Customer user
   - Access `/account` - Should succeed
   - Access `/admin` - Should fail (403 Forbidden)

### Expected Behavior

- **Authorized**: User sees the requested page
- **Unauthorized**: User is redirected to `/Identity/Account/Login`
- **Forbidden**: User is redirected to `/Identity/Account/AccessDenied`

## Future Enhancements (Phase 1.2+)

- **Redirect Engine**: Intelligent post-authorization redirects
- **Role Priority**: Handle multiple roles with priority
- **Resource-Based Authorization**: Check ownership of specific resources
- **Dynamic Policies**: Runtime policy configuration
- **Audit Logging**: Track authorization decisions

## Files Modified/Created

### Created
- `/Infrastructure/Authorization/SameStoreAuthorizationHandler.cs`

### Modified
- `/Infrastructure/Authorization/AuthorizationConstants.cs` - Added policy constants
- `/Infrastructure/Authorization/SuperAdminAuthorizationHandler.cs` - Uses ICurrentUserService
- `/Infrastructure/Authorization/StoreAdminAuthorizationHandler.cs` - Uses ICurrentUserService
- `/Infrastructure/Authorization/VendorAuthorizationHandler.cs` - Uses ICurrentUserService, updated to Vendor role
- `/Infrastructure/Authorization/CustomerAuthorizationHandler.cs` - Uses ICurrentUserService
- `/Infrastructure/DependencyInjection.cs` - Registered SameStoreAuthorizationHandler
- `/Infrastructure/Data/DbInitializer.cs` - Updated to use Vendor role
- `/Infrastructure/Services/CurrentUserService.cs` - Updated to check Vendor role
- `/Infrastructure/Services/PostLoginRedirectService.cs` - Updated to use Vendor role
- `/ElleganzaPlatform/Program.cs` - Updated policy registration
- `/ElleganzaPlatform/Areas/Identity/Controllers/AccountController.cs` - Updated to use Vendor role
- `/ElleganzaPlatform/Areas/Admin/Super/Controllers/DashboardController.cs` - Uses new policy names
- `/ElleganzaPlatform/Areas/Admin/Store/Controllers/DashboardController.cs` - Uses new policy names
- `/ElleganzaPlatform/Areas/Vendor/Controllers/DashboardController.cs` - Uses new policy names
- `/ElleganzaPlatform/Areas/Customer/Controllers/AccountController.cs` - Uses new policy names

## Summary

Phase 1.1 successfully implements a **strict policy-based authorization system** that:

✅ Supports multiple roles per user
✅ Enforces store-aware authorization
✅ Provides centralized policy definitions
✅ Eliminates role checks in Razor Views
✅ Eliminates role-based redirects in Controllers
✅ Uses only policy-based authorization
✅ Is extensible and maintainable

The system is ready for Phase 1.2 (Redirect Engine) and future enhancements.
