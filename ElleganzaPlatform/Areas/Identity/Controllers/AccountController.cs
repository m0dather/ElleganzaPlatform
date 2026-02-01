using ElleganzaPlatform.Application.Common;
using ElleganzaPlatform.Application.Services;
using ElleganzaPlatform.Areas.Identity.Models;
using ElleganzaPlatform.Domain.Entities;
using ElleganzaPlatform.Infrastructure.Authorization;
using ElleganzaPlatform.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AuthClaimTypes = ElleganzaPlatform.Infrastructure.Authorization.ClaimTypes;
using SystemClaim = System.Security.Claims.Claim;

namespace ElleganzaPlatform.Areas.Identity.Controllers;

[Area("Identity")]
public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPostLoginRedirectService _redirectService;
    private readonly IStoreContextService _storeContextService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IPostLoginRedirectService redirectService,
        IStoreContextService storeContextService,
        ApplicationDbContext context,
        ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _redirectService = redirectService;
        _storeContextService = storeContextService;
        _context = context;
        _logger = logger;
    }

    #region Login

    [HttpGet("/login")]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        // Redirect authenticated users to storefront (/)
        // Using explicit Redirect("/") to avoid area resolution issues
        if (User.Identity?.IsAuthenticated == true)
        {
            return Redirect("/");
        }

        var model = new LoginViewModel { ReturnUrl = returnUrl };
        return View(model);
    }

    [HttpPost("/login")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Find user by username or email
        ApplicationUser? user = null;
        
        if (model.UserNameOrEmail.Contains('@'))
        {
            user = await _userManager.FindByEmailAsync(model.UserNameOrEmail);
        }
        else
        {
            user = await _userManager.FindByNameAsync(model.UserNameOrEmail);
        }

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        // Check if user is active
        if (!user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Your account has been deactivated. Please contact support.");
            return View(model);
        }

        // Attempt sign in
        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            _logger.LogInformation("User {Username} logged in successfully", user.UserName);

            // Add custom claims
            await AddCustomClaimsAsync(user);

            // Merge guest cart into user cart after successful login
            var cartService = HttpContext.RequestServices.GetRequiredService<ICartService>();
            await cartService.MergeGuestCartAsync();

            // Get redirect URL based on role using PostLoginRedirectService
            // This ensures centralized, role-based redirect logic
            var redirectUrl = await _redirectService.GetRedirectUrlAsync(user.Id);

            // IMPORTANT: Only use ReturnUrl if it's explicitly provided AND safe
            // Ignore any Identity default paths that may come from ASP.NET Identity internals
            if (!string.IsNullOrEmpty(model.ReturnUrl) && 
                Url.IsLocalUrl(model.ReturnUrl) &&
                !model.ReturnUrl.StartsWith("/Identity", StringComparison.OrdinalIgnoreCase) &&
                !model.ReturnUrl.StartsWith("/Account/Login", StringComparison.OrdinalIgnoreCase))
            {
                return Redirect(model.ReturnUrl);
            }

            // Always redirect to role-based dashboard, never to Identity routes
            return Redirect(redirectUrl);
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User {Username} account locked out", user.UserName);
            ModelState.AddModelError(string.Empty, "Account locked out. Please try again later.");
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(model);
    }

    #endregion

    #region Customer Registration

    [HttpGet("/register")]
    [AllowAnonymous]
    public IActionResult RegisterCustomer()
    {
        // Redirect authenticated users to storefront (/)
        // Using explicit Redirect("/") to avoid area resolution issues
        if (User.Identity?.IsAuthenticated == true)
        {
            return Redirect("/");
        }

        return View();
    }

    [HttpPost("/register")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterCustomer(CustomerRegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.UserName,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            PhoneNumber = model.PhoneNumber,
            EmailConfirmed = true, // Auto-confirm for now
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            // Assign Customer role
            await _userManager.AddToRoleAsync(user, Roles.Customer);

            _logger.LogInformation("New customer registered: {Username}", user.UserName);

            // Sign in the user
            await _signInManager.SignInAsync(user, isPersistent: false);

            // Merge guest cart into user cart after registration
            var cartService = HttpContext.RequestServices.GetRequiredService<ICartService>();
            await cartService.MergeGuestCartAsync();

            // Redirect to customer dashboard
            return Redirect("/account");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    #endregion

    #region Vendor Registration

    [HttpGet("/register/vendor")]
    [AllowAnonymous]
    public IActionResult RegisterVendor()
    {
        // Redirect authenticated users to storefront (/)
        // Using explicit Redirect("/") to avoid area resolution issues
        if (User.Identity?.IsAuthenticated == true)
        {
            return Redirect("/");
        }

        return View();
    }

    [HttpPost("/register/vendor")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterVendor(VendorRegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Get current store (demo store)
        var storeId = await _storeContextService.GetCurrentStoreIdAsync();
        if (!storeId.HasValue)
        {
            ModelState.AddModelError(string.Empty, "Store context not found. Please try again.");
            return View(model);
        }

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Create user
            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                EmailConfirmed = true, // Auto-confirm for now
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            // Assign Vendor role
            await _userManager.AddToRoleAsync(user, Roles.Vendor);

            // Create vendor entity
            var vendor = new Domain.Entities.Vendor
            {
                StoreId = storeId.Value,
                Name = model.VendorName,
                NameAr = model.VendorNameAr,
                Description = model.Description ?? string.Empty,
                DescriptionAr = model.DescriptionAr ?? string.Empty,
                ContactEmail = model.ContactEmail,
                ContactPhone = model.ContactPhone,
                IsActive = false, // Stage 4.1: Vendors require admin approval before activation
                CommissionRate = 0.15m, // Default 15% commission
                CreatedAt = DateTime.UtcNow,
                CreatedBy = user.Id
            };

            _context.Vendors.Add(vendor);
            await _context.SaveChangesAsync();

            // Create VendorAdmin association
            var vendorAdmin = new VendorAdmin
            {
                VendorId = vendor.Id,
                UserId = user.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = user.Id
            };

            _context.VendorAdmins.Add(vendorAdmin);
            await _context.SaveChangesAsync();

            // Add VendorId claim
            await _userManager.AddClaimAsync(user, new SystemClaim(AuthClaimTypes.VendorId, vendor.Id.ToString()));

            await transaction.CommitAsync();

            _logger.LogInformation("New vendor registered: {VendorName} by user {Username}", model.VendorName, user.UserName);

            // Sign in the user
            await _signInManager.SignInAsync(user, isPersistent: false);

            // Merge guest cart into user cart after registration
            var cartService = HttpContext.RequestServices.GetRequiredService<ICartService>();
            await cartService.MergeGuestCartAsync();

            // Redirect to vendor dashboard
            return Redirect("/vendor");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error occurred during vendor registration");
            ModelState.AddModelError(string.Empty, "An error occurred during registration. Please try again.");
            return View(model);
        }
    }

    #endregion

    #region Logout

    [HttpPost("/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        // Sign out the user using SignInManager (clears authentication cookie)
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out");
        
        // ALWAYS redirect to storefront (/) after logout
        // Using explicit Redirect("/") to avoid area resolution issues
        // This ensures users are redirected to the public storefront, not Identity routes
        return Redirect("/");
    }

    #endregion

    #region Access Denied

    [HttpGet("/access-denied")]
    public IActionResult AccessDenied()
    {
        return View();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Adds custom claims (StoreId, VendorId) to the user after sign in
    /// </summary>
    private async Task AddCustomClaimsAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        // Add StoreId claim for StoreAdmin
        if (roles.Contains(Roles.StoreAdmin))
        {
            var storeAdmin = await _context.StoreAdmins
                .Where(sa => sa.UserId == user.Id && sa.IsActive)
                .FirstOrDefaultAsync();

            if (storeAdmin != null)
            {
                var existingClaim = (await _userManager.GetClaimsAsync(user))
                    .FirstOrDefault(c => c.Type == AuthClaimTypes.StoreId);

                if (existingClaim == null)
                {
                    await _userManager.AddClaimAsync(user, new SystemClaim(AuthClaimTypes.StoreId, storeAdmin.StoreId.ToString()));
                }
            }
        }

        // Add VendorId claim for Vendor role
        // Note: VendorAdmin entity is a join table, separate from the Vendor role
        if (roles.Contains(Roles.Vendor))
        {
            var vendorAdmin = await _context.VendorAdmins
                .Where(va => va.UserId == user.Id && va.IsActive)
                .FirstOrDefaultAsync();

            if (vendorAdmin != null)
            {
                var existingClaim = (await _userManager.GetClaimsAsync(user))
                    .FirstOrDefault(c => c.Type == AuthClaimTypes.VendorId);

                if (existingClaim == null)
                {
                    await _userManager.AddClaimAsync(user, new SystemClaim(AuthClaimTypes.VendorId, vendorAdmin.VendorId.ToString()));
                }
            }
        }
    }

    #endregion
}
