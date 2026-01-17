# Authentication System Testing Guide

## Overview
This guide provides instructions for testing the newly implemented authentication and multi-role dashboard system.

## Prerequisites
1. .NET 8.0 SDK installed
2. SQL Server or SQL Server LocalDB installed
3. Entity Framework Core tools installed (`dotnet tool install --global dotnet-ef`)

## Database Setup

### 1. Update Connection String (if needed)
Edit `ElleganzaPlatform/appsettings.json` and update the connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ElleganzaPlatform;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

For SQL Server, use:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ElleganzaPlatform;User Id=sa;Password=YourPassword;TrustServerCertificate=True"
  }
}
```

### 2. Apply Migrations
```bash
cd ElleganzaPlatform
dotnet ef database update --project ../ElleganzaPlatform.Infrastructure
```

### 3. Verify Database Seeding
The application will automatically seed:
- **4 Roles**: SuperAdmin, StoreAdmin, VendorAdmin, Customer
- **Demo Store**: Code "demo", marked as default
- **Super Admin User**: 
  - Email: `superadmin@elleganza.com`
  - Password: `SuperAdmin@123`

## Testing Scenarios

### Test 1: Super Admin Login & Redirect
1. Navigate to `/Identity/Account/Login` or click "Login" in the header
2. Enter credentials:
   - Username/Email: `superadmin@elleganza.com`
   - Password: `SuperAdmin@123`
3. Click "Login"
4. **Expected Result**: Redirected to `/super-admin` dashboard
5. **Header Check**: Should show "Super Admin Dashboard" link

### Test 2: Customer Registration & Auto-Login
1. Navigate to `/Identity/Account/RegisterCustomer` or click "Register" in header
2. Fill in the registration form:
   - First Name: `John`
   - Last Name: `Doe`
   - Email: `john.doe@example.com`
   - Username: `johndoe`
   - Password: `Customer@123`
   - Confirm Password: `Customer@123`
3. Click "Create Account"
4. **Expected Result**: 
   - User created with Customer role
   - Automatically logged in
   - Redirected to `/account` (Customer Dashboard)
5. **Header Check**: Should show "My Account" link

### Test 3: Vendor Registration & Auto-Activation
1. Logout (if logged in)
2. Navigate to `/Identity/Account/RegisterVendor`
3. Fill in the registration form:
   - **Personal Information**:
     - First Name: `Jane`
     - Last Name: `Smith`
     - Email: `jane.smith@example.com`
     - Username: `janesmith`
     - Password: `Vendor@123`
     - Confirm Password: `Vendor@123`
   - **Vendor Information**:
     - Vendor/Store Name: `Jane's Fashion`
     - Vendor/Store Name (Arabic): `أزياء جين`
     - Description: `Quality fashion items`
     - Description (Arabic): `عناصر أزياء عالية الجودة`
     - Contact Email: `contact@janesfashion.com`
     - Contact Phone: `+1234567890`
4. Click "Create Vendor Account"
5. **Expected Result**:
   - User created with VendorAdmin role
   - Vendor entity created and linked to demo store
   - VendorAdmin association created
   - VendorId claim added to user
   - Vendor is **immediately active** (no approval required)
   - Automatically logged in
   - Redirected to `/vendor` (Vendor Dashboard)
6. **Header Check**: Should show "Vendor Dashboard" link

### Test 4: Role-Based Navigation
1. Login as different roles
2. Check the header navigation:
   - **Guest**: Shows "Login" and "Register"
   - **Customer**: Shows "My Account" and "Logout"
   - **VendorAdmin**: Shows "Vendor Dashboard" and "Logout"
   - **StoreAdmin**: Shows "Admin Dashboard" and "Logout"
   - **SuperAdmin**: Shows "Super Admin Dashboard" and "Logout"

### Test 5: Authorization Protection
1. While logged out, try to access:
   - `/super-admin` → Redirected to Login
   - `/admin` → Redirected to Login
   - `/vendor` → Redirected to Login
   - `/account` → Redirected to Login

2. Login as Customer, try to access:
   - `/super-admin` → Access Denied (403)
   - `/admin` → Access Denied (403)
   - `/vendor` → Access Denied (403)
   - `/account` → ✅ Accessible

3. Login as VendorAdmin, try to access:
   - `/super-admin` → Access Denied (403)
   - `/admin` → Access Denied (403)
   - `/vendor` → ✅ Accessible
   - `/account` → Access Denied (403)

### Test 6: Store Context & Multi-Store Readiness
1. Check that the demo store exists:
   ```sql
   SELECT * FROM Stores WHERE Code = 'demo';
   ```
2. Verify IsDefault = 1 and IsActive = 1
3. Create a vendor - should be linked to StoreId of demo store
4. Verify store context is resolved correctly in services

### Test 7: Claims-Based Authorization
1. Login as VendorAdmin (created in Test 3)
2. Check user claims (can use browser dev tools or debugging):
   - Should have `VendorId` claim with the vendor ID
3. Create a StoreAdmin user (manually via database or future UI):
   - Should have `StoreId` claim with the store ID

## URLs Reference

| Role | Dashboard URL | Description |
|------|---------------|-------------|
| Guest | `/` | Public storefront (Ecomus theme) |
| Customer | `/account` | Customer account dashboard |
| VendorAdmin | `/vendor` | Vendor management dashboard |
| StoreAdmin | `/admin` | Store administration dashboard |
| SuperAdmin | `/super-admin` | Platform super admin dashboard |

## Authentication Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/Identity/Account/Login` | GET | Display login form |
| `/Identity/Account/Login` | POST | Process login |
| `/Identity/Account/RegisterCustomer` | GET | Display customer registration form |
| `/Identity/Account/RegisterCustomer` | POST | Process customer registration |
| `/Identity/Account/RegisterVendor` | GET | Display vendor registration form |
| `/Identity/Account/RegisterVendor` | POST | Process vendor registration |
| `/Identity/Account/Logout` | POST | Logout user |
| `/Identity/Account/AccessDenied` | GET | Access denied page |

## Verification Checklist

- [ ] Demo store seeded with code "demo"
- [ ] Super admin user created
- [ ] All 4 roles created (SuperAdmin, StoreAdmin, VendorAdmin, Customer)
- [ ] Login accepts username OR email
- [ ] Login redirects based on role priority (SuperAdmin > StoreAdmin > VendorAdmin > Customer)
- [ ] Customer registration creates Customer role user
- [ ] Customer registration auto-logs in and redirects to /account
- [ ] Vendor registration creates VendorAdmin role user
- [ ] Vendor registration creates Vendor entity
- [ ] Vendor registration creates VendorAdmin association
- [ ] Vendor is immediately active (no approval)
- [ ] Vendor registration auto-logs in and redirects to /vendor
- [ ] Role-based navigation shows correct links in header
- [ ] Authorization policies protect dashboard routes
- [ ] Logout works and redirects to home
- [ ] Access denied page displays for unauthorized access

## Common Issues & Solutions

### Issue: Database does not exist
**Solution**: Run `dotnet ef database update --project ../ElleganzaPlatform.Infrastructure`

### Issue: Migration fails
**Solution**: Ensure connection string is correct and SQL Server is running

### Issue: Super admin not seeded
**Solution**: Check application logs during startup. The seeding happens automatically.

### Issue: Login redirects to wrong dashboard
**Solution**: Check user roles in database. Role priority is: SuperAdmin > StoreAdmin > VendorAdmin > Customer

### Issue: Vendor not created during registration
**Solution**: Check logs for errors. Ensure transaction is completing successfully.

## Next Steps

1. **Theme Integration**: Replace basic Bootstrap views with Ecomus theme
2. **Email Confirmation**: Enable email confirmation for registrations
3. **Password Reset**: Implement forgot password functionality
4. **Two-Factor Authentication**: Add 2FA support
5. **Store Admin Registration**: Create UI for Store Admin creation (SuperAdmin only)
6. **Profile Management**: Allow users to update their profile information
7. **Store Selection**: When multiple stores exist, allow vendor/admin to select store context

## Architecture Notes

### Key Services

1. **IPostLoginRedirectService**: Determines redirect URL after login based on user roles
2. **IStoreContextService**: Resolves current store context (currently returns demo store)
3. **ICurrentUserService**: Provides user context throughout application

### Policy-Based Authorization

All authorization uses policies, NOT role attributes:
- `SuperAdminPolicy` → checks SuperAdmin role
- `StoreAdminPolicy` → checks StoreAdmin role + validates StoreId claim
- `VendorPolicy` → checks VendorAdmin role + validates VendorId claim
- `CustomerPolicy` → checks Customer role

### Multi-Store Architecture

- Store entity has `Code` and `IsDefault` fields
- First store is "demo" with IsDefault = true
- Future: Store context can be resolved by domain/subdomain
- Vendors are scoped to stores via `StoreId` foreign key
- Global query filters ensure data isolation

## Screenshots

After testing, capture screenshots of:
1. Login page
2. Customer registration page
3. Vendor registration page
4. Customer dashboard
5. Vendor dashboard
6. Admin dashboard
7. Super Admin dashboard
8. Header navigation for each role
9. Access denied page
