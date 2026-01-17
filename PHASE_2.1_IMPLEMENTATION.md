# Phase 2.1: Theme Resolution Engine Implementation

## Overview
This document describes the implementation of the Theme Resolution Engine for the ElleganzaPlatform ASP.NET Core 8 MVC application. The engine enables automatic theme switching based on request paths without modifying controllers or views.

## Architecture

### Components Implemented

#### 1. IThemeContext Interface
**Location:** `ElleganzaPlatform.Application/Common/IThemeContext.cs`

Defines the contract for theme resolution:
- `ThemeType` enum: Store, Admin
- `ThemeType` property: Gets the current theme type
- `ThemeName` property: Gets the current theme name

```csharp
public enum ThemeType
{
    Store,  // Customer-facing storefront
    Admin   // Back-office management
}

public interface IThemeContext
{
    ThemeType ThemeType { get; }
    string ThemeName { get; }
}
```

#### 2. ThemeContext Service
**Location:** `ElleganzaPlatform.Infrastructure/Services/ThemeContext.cs`

Request-scoped service that implements theme resolution logic:

**Theme Selection Rules:**
- **Admin Theme (Metronic):** Paths starting with `/admin`, `/vendor`, or `/super-admin`
- **Store Theme (Ecomus):** All other paths (default)

**Performance Optimization:**
- Caches theme resolution per request
- Lazy evaluation on first access

**Key Features:**
- Uses `IHttpContextAccessor` to access request path
- Thread-safe per-request caching
- No static state or global variables

#### 3. ThemeViewLocationExpander
**Location:** `ElleganzaPlatform/Infrastructure/ThemeViewLocationExpander.cs`

Implements `IViewLocationExpander` to extend Razor view search locations:

**View Search Order:**
1. Theme-specific controller views: `/Themes/{ThemeType}/{ThemeName}/Views/{Controller}/{View}.cshtml`
2. Theme-specific shared views: `/Themes/{ThemeType}/{ThemeName}/Views/Shared/{View}.cshtml`
3. Default MVC locations (fallback)

**Example Paths:**
- Admin: `/Themes/Admin/Metronic/Views/Dashboard/Index.cshtml`
- Store: `/Themes/Store/Ecomus/Views/Home/Index.cshtml`

**Caching Strategy:**
- Uses theme type and name as cache key
- Efficient view location caching by ASP.NET Core

#### 4. Static File Configuration
**Location:** `ElleganzaPlatform/Program.cs`

Configured `CompositeFileProvider` to serve static assets from multiple theme directories:

**Static File Locations:**
- Default wwwroot: `/wwwroot`
- Admin theme assets: `/Themes/Admin/Metronic/wwwroot`
- Store theme assets: `/Themes/Store/Ecomus/wwwroot`

**Implementation:**
```csharp
var themeDirectories = new[]
{
    Path.Combine(themesPath, "Store", "Ecomus", "wwwroot"),
    Path.Combine(themesPath, "Admin", "Metronic", "wwwroot")
};

var compositeProvider = new CompositeFileProvider(fileProviders);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = compositeProvider
});
```

## Dependency Injection Configuration

### Services Registration
**Location:** `ElleganzaPlatform.Infrastructure/DependencyInjection.cs`

```csharp
services.AddScoped<IThemeContext, ThemeContext>();
```

Registered as **scoped** to ensure:
- One instance per HTTP request
- Request path available throughout the request lifecycle
- Proper cleanup after request completion

### View Engine Configuration
**Location:** `ElleganzaPlatform/Program.cs`

```csharp
builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.ViewLocationExpanders.Add(new ThemeViewLocationExpander());
});
```

## Theme Structure

### Directory Layout
```
/Themes
├── Admin
│   └── Metronic
│       ├── Views
│       │   ├── Dashboard
│       │   │   └── Index.cshtml
│       │   └── Shared
│       │       ├── _Layout.cshtml
│       │       ├── _Navbar.cshtml
│       │       ├── _Sidebar.cshtml
│       │       └── _Footer.cshtml
│       └── wwwroot
│           └── assets
│               └── (theme-specific assets)
└── Store
    └── Ecomus
        ├── Views
        │   ├── Home
        │   │   └── Index.cshtml
        │   ├── Product
        │   │   └── Index.cshtml
        │   └── Shared
        │       └── _Layout.cshtml
        └── wwwroot
            ├── css
            ├── js
            ├── images
            └── fonts
```

## How It Works

### Request Flow

1. **HTTP Request Arrives**
   - Request path: `/admin/Dashboard` or `/products`

2. **ThemeContext Resolution**
   - `IHttpContextAccessor` provides access to request
   - Path is analyzed to determine theme type
   - Theme name is resolved based on type
   - Result is cached for the request duration

3. **View Resolution**
   - Controller returns `View()`
   - Razor engine calls `ThemeViewLocationExpander.PopulateValues()`
   - Theme context is stored in view location context
   - `ExpandViewLocations()` adds theme-specific paths
   - View engine searches theme folders first, then defaults

4. **Static File Serving**
   - Static file middleware uses `CompositeFileProvider`
   - Searches theme wwwroot folders
   - Falls back to default wwwroot if not found

### Example Scenarios

#### Scenario 1: Admin Dashboard
**Request:** `GET /admin/Dashboard`
- ThemeType: `Admin`
- ThemeName: `Metronic`
- View Path: `/Themes/Admin/Metronic/Views/Dashboard/Index.cshtml`
- Static Files: `/Themes/Admin/Metronic/wwwroot/assets/*`

#### Scenario 2: Store Homepage
**Request:** `GET /`
- ThemeType: `Store`
- ThemeName: `Ecomus`
- View Path: `/Themes/Store/Ecomus/Views/Home/Index.cshtml`
- Static Files: `/Themes/Store/Ecomus/wwwroot/images/*`

#### Scenario 3: Vendor Dashboard
**Request:** `GET /vendor/products`
- ThemeType: `Admin`
- ThemeName: `Metronic`
- View Path: `/Themes/Admin/Metronic/Views/{Controller}/{View}.cshtml`
- Static Files: `/Themes/Admin/Metronic/wwwroot/assets/*`

## Key Design Decisions

### 1. Request-Scoped Service
- **Why:** Theme determination is per-request and depends on HTTP context
- **Benefit:** Clean, testable, thread-safe implementation

### 2. Per-Request Caching
- **Why:** Multiple accesses to ThemeType/ThemeName during single request
- **Benefit:** Performance optimization without global state

### 3. Path-Based Theme Selection
- **Why:** Simple, predictable, doesn't require authentication context
- **Benefit:** Works for both authenticated and anonymous users

### 4. CompositeFileProvider
- **Why:** Multiple theme directories need to serve static files
- **Benefit:** No manual file copying or build-time complexity

### 5. IViewLocationExpander
- **Why:** Standard ASP.NET Core extension point for view resolution
- **Benefit:** Integrates seamlessly with existing MVC pipeline

## Testing

### Unit Tests Performed
All theme resolution logic tested with the following scenarios:
1. ✅ Admin path (`/admin/*`) → Admin theme
2. ✅ Vendor path (`/vendor/*`) → Admin theme
3. ✅ Super-admin path (`/super-admin/*`) → Admin theme
4. ✅ Store root path (`/`) → Store theme
5. ✅ Store product path (`/products`) → Store theme

**Test Results:** All tests passed ✅

## Extensibility

### Adding New Themes

To add a new theme:

1. Create theme directory structure:
   ```
   /Themes/{ThemeType}/{ThemeName}
       ├── Views
       └── wwwroot
   ```

2. Update `ThemeContext.cs` to include new theme name logic

3. Update `Program.cs` static file configuration to include new theme path

### Supporting Multi-Store Themes

Future enhancement (Phase 2.2+):
- Extend `IThemeContext` with `StoreId` property
- Modify `ThemeContext` to resolve theme based on store
- Update view location expander to include store-specific paths

## Benefits Achieved

✅ **Zero Controller Changes:** Controllers remain theme-agnostic  
✅ **Centralized Logic:** All theme resolution in one service  
✅ **Maintainable:** Clear separation of concerns  
✅ **Testable:** DI-based, mockable dependencies  
✅ **Performant:** Request-scoped caching  
✅ **Extensible:** Easy to add new themes or logic  
✅ **Production-Ready:** No shortcuts, proper architecture  

## Next Steps

### Phase 2.2: Theme UI Binding
- Bind theme-specific layouts to views
- Implement theme-specific ViewStart files
- Add theme-specific ViewImports

### Phase 2.3: Multi-Store Theme Support
- Extend theme resolution to support store-specific themes
- Database-driven theme configuration
- Runtime theme switching based on store context

## Conclusion

The Theme Resolution Engine successfully implements Phase 2.1 requirements:
- ✅ Resolves Razor views from theme folders
- ✅ Serves static files from theme wwwroot folders
- ✅ Automatically switches between Store and Admin themes
- ✅ Does not modify controllers or views
- ✅ Centralized, extensible architecture
- ✅ Clean, well-commented, production-ready code
- ✅ All builds pass with zero warnings/errors
