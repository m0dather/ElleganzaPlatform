# ElleganzaPlatform - Authentication Flow Diagram

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         AUTHENTICATION FLOW                              │
└─────────────────────────────────────────────────────────────────────────┘

                              ┌──────────────┐
                              │  Guest User  │
                              └──────┬───────┘
                                     │
                    ┌────────────────┼────────────────┐
                    │                │                │
           ┌────────▼────────┐  ┌───▼───────┐  ┌───▼──────────┐
           │  Want to Login  │  │  Customer │  │   Vendor     │
           │                 │  │  Register │  │   Register   │
           └────────┬────────┘  └─────┬─────┘  └──────┬───────┘
                    │                 │                │
                    │                 │                │
        ┌───────────▼────────────┐    │                │
        │  /Identity/Account/    │    │                │
        │       Login            │    │                │
        │                        │    │                │
        │  ┌──────────────────┐ │    │                │
        │  │ Username/Email   │ │    │                │
        │  │ Password         │ │    │                │
        │  └──────────────────┘ │    │                │
        └───────────┬────────────┘    │                │
                    │                 │                │
              [Submit Login]          │                │
                    │                 │                │
        ┌───────────▼────────────┐    │                │
        │  AccountController     │    │                │
        │  .Login(POST)          │    │                │
        │                        │    │                │
        │  1. Find User          │    │                │
        │  2. Validate Password  │    │                │
        │  3. Sign In            │    │                │
        │  4. Add Claims         │    │                │
        └───────────┬────────────┘    │                │
                    │                 │                │
        ┌───────────▼────────────┐    │                │
        │ PostLoginRedirect      │    │                │
        │ Service                │    │                │
        │                        │    │                │
        │ Check Role Priority:   │    │                │
        │ SuperAdmin > Store >   │    │                │
        │ Vendor > Customer      │    │                │
        └───────────┬────────────┘    │                │
                    │                 │                │
         ┌──────────┼─────────────────┼────────────────┼──────────┐
         │          │                 │                │          │
    ┌────▼─────┐ ┌─▼────────┐ ┌──────▼──────┐ ┌──────▼──────┐   │
    │SuperAdmin│ │StoreAdmin│ │ VendorAdmin │ │  Customer   │   │
    │          │ │          │ │             │ │             │   │
    │/super-   │ │  /admin  │ │   /vendor   │ │  /account   │   │
    │ admin    │ │          │ │             │ │             │   │
    └──────────┘ └──────────┘ └─────────────┘ └─────────────┘   │
                                                                  │
         ┌────────────────────────────────────────────────────────┘
         │
         │  CUSTOMER REGISTRATION FLOW
         │
         ▼
    ┌─────────────────────────────┐
    │ /Identity/Account/          │
    │  RegisterCustomer           │
    │                             │
    │ ┌─────────────────────────┐ │
    │ │ Personal Info           │ │
    │ │ - Name, Email           │ │
    │ │ - Username, Password    │ │
    │ └─────────────────────────┘ │
    └──────────────┬──────────────┘
                   │
                   ▼
    ┌─────────────────────────────┐
    │ AccountController           │
    │ .RegisterCustomer(POST)     │
    │                             │
    │ 1. Create User              │
    │ 2. Assign Customer Role     │
    │ 3. Auto Sign-In             │
    │ 4. Redirect to /account     │
    └─────────────────────────────┘

         VENDOR REGISTRATION FLOW
         
    ┌─────────────────────────────┐
    │ /Identity/Account/          │
    │  RegisterVendor             │
    │                             │
    │ ┌─────────────────────────┐ │
    │ │ Personal Info           │ │
    │ │ - Name, Email           │ │
    │ │ - Username, Password    │ │
    │ │                         │ │
    │ │ Vendor Info             │ │
    │ │ - Store Name (EN/AR)    │ │
    │ │ - Description           │ │
    │ │ - Contact Info          │ │
    │ └─────────────────────────┘ │
    └──────────────┬──────────────┘
                   │
                   ▼
    ┌─────────────────────────────┐
    │ AccountController           │
    │ .RegisterVendor(POST)       │
    │                             │
    │ 1. Get Demo Store           │
    │ 2. Create User              │
    │ 3. Assign VendorAdmin Role  │
    │ 4. Create Vendor Entity     │
    │ 5. Create VendorAdmin Link  │
    │ 6. Add VendorId Claim       │
    │ 7. Auto Sign-In             │
    │ 8. Redirect to /vendor      │
    └─────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│                       AUTHORIZATION FLOW                                 │
└─────────────────────────────────────────────────────────────────────────┘

    User tries to access protected route (e.g., /vendor)
                        │
                        ▼
    ┌─────────────────────────────────────┐
    │  [Authorize(Policy="VendorPolicy")] │
    │  Attribute on Controller            │
    └─────────────┬───────────────────────┘
                  │
                  ▼
    ┌─────────────────────────────────────┐
    │  Authorization Middleware           │
    │  - Check if authenticated           │
    │  - If not → Redirect to Login       │
    └─────────────┬───────────────────────┘
                  │
                  ▼
    ┌─────────────────────────────────────┐
    │  VendorAuthorizationHandler         │
    │                                     │
    │  1. Check if SuperAdmin → Allow     │
    │  2. Check if VendorAdmin role       │
    │  3. Extract VendorId claim          │
    │  4. Validate claim exists           │
    │  5. Allow or Deny                   │
    └─────────────┬───────────────────────┘
                  │
         ┌────────┴────────┐
         │                 │
    ┌────▼─────┐    ┌─────▼──────┐
    │ Allowed  │    │  Denied    │
    │ Access   │    │ Redirect   │
    │ Resource │    │ /Identity/ │
    │          │    │ Account/   │
    │          │    │ Access-    │
    │          │    │ Denied     │
    └──────────┘    └────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│                      MULTI-STORE ARCHITECTURE                            │
└─────────────────────────────────────────────────────────────────────────┘

    ┌─────────────────────────────────────┐
    │  Request to any URL                 │
    └─────────────┬───────────────────────┘
                  │
                  ▼
    ┌─────────────────────────────────────┐
    │  StoreContextService                │
    │                                     │
    │  1. Check domain/subdomain          │
    │  2. Check path segments             │
    │  3. Fallback to default store       │
    │  4. Return StoreId                  │
    └─────────────┬───────────────────────┘
                  │
                  ▼
    ┌─────────────────────────────────────┐
    │  Current Store: "demo"              │
    │  - Code: demo                       │
    │  - IsDefault: true                  │
    │  - IsActive: true                   │
    └─────────────┬───────────────────────┘
                  │
         ┌────────┴────────┐
         │                 │
    ┌────▼─────┐    ┌─────▼──────┐
    │ Vendors  │    │  Products  │
    │ scoped   │    │  scoped    │
    │ to demo  │    │  to demo   │
    │ store    │    │  store     │
    └──────────┘    └────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│                      SERVICE LAYER ARCHITECTURE                          │
└─────────────────────────────────────────────────────────────────────────┘

    ┌──────────────────────────────────────────────────┐
    │         Application Layer (Interfaces)           │
    ├──────────────────────────────────────────────────┤
    │  IPostLoginRedirectService                       │
    │  IStoreContextService                            │
    │  ICurrentUserService                             │
    └─────────────────┬────────────────────────────────┘
                      │
                      ▼
    ┌──────────────────────────────────────────────────┐
    │      Infrastructure Layer (Implementations)      │
    ├──────────────────────────────────────────────────┤
    │  PostLoginRedirectService                        │
    │  ├─ Check user roles                             │
    │  ├─ Apply role priority                          │
    │  └─ Return dashboard URL                         │
    │                                                  │
    │  StoreContextService                             │
    │  ├─ Resolve store by domain/path                 │
    │  ├─ Cache store ID                               │
    │  └─ Return store context                         │
    │                                                  │
    │  CurrentUserService                              │
    │  ├─ Get UserId from claims                       │
    │  ├─ Get StoreId from claims                      │
    │  ├─ Get VendorId from claims                     │
    │  └─ Role check helpers                           │
    └──────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│                    ROLE HIERARCHY & PRIORITY                             │
└─────────────────────────────────────────────────────────────────────────┘

            ┌─────────────────────────┐
            │      SuperAdmin         │
            │                         │
            │  ✓ Platform Owner       │
            │  ✓ All Stores Access    │
            │  ✓ All Users Management │
            │  ✓ System Configuration │
            └───────────┬─────────────┘
                        │
                        ▼
            ┌─────────────────────────┐
            │      StoreAdmin         │
            │                         │
            │  ✓ Single Store Access  │
            │  ✓ Store Configuration  │
            │  ✓ Vendor Management    │
            │  ✓ Product Approval     │
            │  ✓ Has StoreId Claim    │
            └───────────┬─────────────┘
                        │
                        ▼
            ┌─────────────────────────┐
            │     VendorAdmin         │
            │                         │
            │  ✓ Vendor Scope Access  │
            │  ✓ Own Products Only    │
            │  ✓ Own Orders View      │
            │  ✓ Vendor Reports       │
            │  ✓ Has VendorId Claim   │
            └───────────┬─────────────┘
                        │
                        ▼
            ┌─────────────────────────┐
            │       Customer          │
            │                         │
            │  ✓ Browse Products      │
            │  ✓ Place Orders         │
            │  ✓ View Own Orders      │
            │  ✓ Manage Wishlist      │
            │  ✓ Profile Management   │
            └─────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│                         NAVIGATION MENU LOGIC                            │
└─────────────────────────────────────────────────────────────────────────┘

    User State: Authenticated?
           │
           ├─ NO (Guest)
           │    ├─ Show: Login
           │    └─ Show: Register
           │
           └─ YES (Authenticated)
                │
                ├─ Role: SuperAdmin
                │    ├─ Show: Super Admin Dashboard
                │    └─ Show: Logout
                │
                ├─ Role: StoreAdmin
                │    ├─ Show: Admin Dashboard
                │    └─ Show: Logout
                │
                ├─ Role: VendorAdmin
                │    ├─ Show: Vendor Dashboard
                │    └─ Show: Logout
                │
                └─ Role: Customer
                     ├─ Show: My Account
                     └─ Show: Logout

═══════════════════════════════════════════════════════════════════════════

                        IMPLEMENTATION COMPLETE ✅

═══════════════════════════════════════════════════════════════════════════
```
