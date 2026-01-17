# ElleganzaPlatform v1.2

## Overview

ElleganzaPlatform is a secure, scalable, multi-store, multi-role marketplace platform built with ASP.NET Core 8, following Clean Architecture principles and implementing enterprise-grade authorization.

## Architecture

### Clean Architecture Layers

```
ElleganzaPlatform/
├── ElleganzaPlatform.Domain/          # Core business entities and interfaces
├── ElleganzaPlatform.Application/     # Business logic and use cases
├── ElleganzaPlatform.Infrastructure/  # Data access and external services
└── ElleganzaPlatform/                 # Presentation layer (Web UI)
```

### Key Features

✅ **Clean Architecture** with strict layer boundaries  
✅ **Policy-Based Authorization** with role and claim-based access control  
✅ **Multi-Store Architecture** with data isolation  
✅ **Multi-Language Support** (English & Arabic)  
✅ **Entity Framework Core** with global query filters  
✅ **ASP.NET Core Identity** for authentication  
✅ **Store/Vendor Scoping** by design  

## Authorization Model

### Roles & Permissions

#### 1️⃣ SuperAdmin (Platform Owner)
- **Dashboard**: `/Admin/Super`
- **Permissions**: Full system access
  - Manage all stores, vendors, and users
  - Platform-wide configuration
  - Global reports and analytics
- **Policy**: `SuperAdminPolicy`

#### 2️⃣ StoreAdmin (Store Manager)
- **Dashboard**: `/Admin/Store`
- **Permissions**: Store-scoped access
  - Manage store settings and vendors
  - Product approval workflows
  - Store-level orders and reports
- **Policy**: `StoreAdminPolicy` (requires `StoreId` claim)

#### 3️⃣ VendorAdmin (Merchant/Seller)
- **Dashboard**: `/Vendor/Dashboard`
- **Permissions**: Vendor-scoped access
  - Manage own products and inventory
  - View vendor-specific orders
  - Vendor sales reports
- **Policy**: `VendorPolicy` (requires `VendorId` claim)

#### 4️⃣ Customer (End User)
- **Pages**: `/Account`, `/MyOrders`, `/Wishlist`
- **Permissions**: Customer features
  - Browse and purchase products
  - Manage wishlist and orders
  - Profile management
- **Policy**: `CustomerPolicy`

## Data Isolation

### Global Query Filters

The system implements automatic data scoping through EF Core global query filters:

- **Stores**: Soft delete filter
- **Vendors**: Scoped by StoreId for StoreAdmin
- **Products**: Scoped by StoreId and VendorId
- **Orders**: Scoped by StoreId

SuperAdmin can bypass filters to access all data.

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/m0dather/ElleganzaPlatform.git
   cd ElleganzaPlatform
   ```

2. **Update connection string**
   
   Edit `ElleganzaPlatform/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Your-Connection-String-Here"
     }
   }
   ```

3. **Apply migrations**
   ```bash
   cd ElleganzaPlatform
   dotnet ef database update --project ../ElleganzaPlatform.Infrastructure
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

### Default Credentials

After first run, use these credentials to login as SuperAdmin:

- **Email**: `superadmin@elleganza.com`
- **Password**: `SuperAdmin@123`

⚠️ **IMPORTANT**: Change the default password immediately after first login!

## Project Structure

### Domain Layer
- **Entities**: Store, Vendor, Product, Order, ApplicationUser
- **Interfaces**: IRepository<T>, IUnitOfWork
- **Enums**: UserRole, OrderStatus, ProductStatus

### Application Layer
- **Common**: Result pattern, ICurrentUserService
- **Services**: Business logic interfaces
- **DTOs**: Data transfer objects

### Infrastructure Layer
- **Data**: ApplicationDbContext, Configurations, Migrations
- **Authorization**: Policy handlers and requirements
- **Repositories**: Generic repository implementation
- **Services**: CurrentUserService implementation

### Presentation Layer
- **Areas**:
  - `Admin/Super`: SuperAdmin dashboard
  - `Admin/Store`: StoreAdmin dashboard
  - `Vendor`: Vendor dashboard
  - `Customer`: Customer pages
- **Resources**: Localization files (.resx)

## Localization

The platform supports multiple languages:

- English (en) - Default
- Arabic (ar)

Language selection is handled via:
1. Cookie preference
2. Accept-Language header
3. Default fallback

All UI strings use `IStringLocalizer<SharedResource>` for internationalization.

## Security Features

### Authorization
- Policy-based authorization (not just role-based)
- Claim-based scoping (StoreId, VendorId)
- Automatic data filtering based on user context

### Data Protection
- Soft delete for audit trails
- Automatic audit fields (CreatedBy, UpdatedBy, timestamps)
- Global query filters prevent unauthorized data access

### Identity
- ASP.NET Core Identity with custom ApplicationUser
- Strong password requirements
- Email confirmation support

## Development Guidelines

### Adding New Entities
1. Create entity in `Domain/Entities`
2. Add DbSet in `ApplicationDbContext`
3. Create configuration in `Infrastructure/Data/Configurations`
4. Add migration: `dotnet ef migrations add YourMigrationName`

### Adding New Areas
1. Create controller with `[Area("AreaName")]` attribute
2. Apply appropriate authorization policy
3. Create views in `Areas/AreaName/Views`
4. Add localization strings to resource files

### Testing Authorization
- SuperAdmin: Can access all areas
- StoreAdmin: Can only access their store's data
- VendorAdmin: Can only access their vendor's data
- Customer: Can only access customer features

## Contributing

1. Follow Clean Architecture principles
2. Use policy-based authorization (not just roles)
3. Add localization for all UI strings
4. Write unit tests for business logic
5. Document significant changes

## License

Copyright © 2024 ElleganzaPlatform. All rights reserved.

## Support

For issues and questions, please create an issue in the GitHub repository.
