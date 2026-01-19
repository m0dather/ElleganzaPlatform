using ElleganzaPlatform.Domain.Entities;
using ElleganzaPlatform.Infrastructure;
using ElleganzaPlatform.Infrastructure.Authorization;
using ElleganzaPlatform.Infrastructure.Data;
using ElleganzaPlatform.Resources;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.FileProviders;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();

// Phase 3.1.1: Configure Anti-Forgery for AJAX requests
// This allows anti-forgery tokens to be sent via custom headers from JavaScript
builder.Services.AddAntiforgery(options =>
{
    // Read token from header for AJAX requests
    options.HeaderName = "RequestVerificationToken";
});

// Configure Razor view engine to use theme-based view locations
builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.ViewLocationExpanders.Add(new ThemeViewLocationExpander());
});

// Localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// Session support for cart
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var supportedCultures = new[]
{
    new CultureInfo("en"),
    new CultureInfo("ar")
};

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new CookieRequestCultureProvider(),
        new AcceptLanguageHeaderRequestCultureProvider()
    };
});

// Infrastructure layer (Database, Repositories, Services)
builder.Services.AddInfrastructure(builder.Configuration);

// Identity
// NOTE: This project uses custom authentication controllers and views,
// NOT the default ASP.NET Identity UI (no AddDefaultUI or MapRazorPages).
// All authentication routes are explicitly defined in AccountController
// using route attributes (/login, /logout, /register, etc.)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    // SuperAdmin policy - requires SuperAdmin role
    options.AddPolicy(AuthorizationPolicies.RequireSuperAdmin, policy =>
        policy.AddRequirements(new SuperAdminRequirement()));

    // StoreAdmin policy - requires StoreAdmin role with store isolation
    options.AddPolicy(AuthorizationPolicies.RequireStoreAdmin, policy =>
        policy.AddRequirements(new StoreAdminRequirement()));

    // Vendor policy - requires Vendor role with vendor isolation
    options.AddPolicy(AuthorizationPolicies.RequireVendor, policy =>
        policy.AddRequirements(new VendorRequirement()));

    // Customer policy - requires Customer role
    options.AddPolicy(AuthorizationPolicies.RequireCustomer, policy =>
        policy.AddRequirements(new CustomerRequirement()));

    // Same Store policy - requires user's StoreId to match current store context
    options.AddPolicy(AuthorizationPolicies.RequireSameStore, policy =>
        policy.AddRequirements(new SameStoreRequirement()));
});

// Configure cookie settings
// CRITICAL: These settings override ASP.NET Identity defaults to prevent
// automatic redirects to /Identity/* routes. The application uses custom
// authentication routes, not the default Identity UI.
builder.Services.ConfigureApplicationCookie(options =>
{
    // Custom login path - overrides Identity default (/Account/Login)
    options.LoginPath = "/login";
    
    // Custom logout path - overrides Identity default (/Account/Logout)
    options.LogoutPath = "/logout";
    
    // Custom access denied path - overrides Identity default (/Account/AccessDenied)
    options.AccessDeniedPath = "/access-denied";
});

// HttpContext for CurrentUserService
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DbInitializer.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Configure static files from default wwwroot
app.UseStaticFiles();

// Configure static files from theme folders
// This allows serving assets from:
// - /Themes/Store/Ecomus/wwwroot
// - /Themes/Admin/Metronic/wwwroot
var themesPath = Path.Combine(builder.Environment.ContentRootPath, "Themes");
if (Directory.Exists(themesPath))
{
    var themeDirectories = new[]
    {
        Path.Combine(themesPath, "Store", "Ecomus", "wwwroot"),
        Path.Combine(themesPath, "Admin", "Metronic", "wwwroot")
    };

    var fileProviders = themeDirectories
        .Where(Directory.Exists)
        .Select(path => new PhysicalFileProvider(path) as IFileProvider)
        .ToList();

    if (fileProviders.Any())
    {
        var compositeProvider = new CompositeFileProvider(fileProviders);
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = compositeProvider
        });
    }
}

// Localization middleware
app.UseRequestLocalization();

// Session middleware (must be before routing)
app.UseSession();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Route Configuration
// NOTE: No MapRazorPages() is called, so ASP.NET Identity UI routes are NOT registered.
// All authentication routes are handled by custom controllers with explicit route attributes.
// This prevents any /Identity/* or /Account/* default routes from being accessible.

// Area routes
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
