using ElleganzaPlatform.Domain.Entities;
using ElleganzaPlatform.Infrastructure.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ElleganzaPlatform.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = serviceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            // Seed default store
            if (!await context.Stores.AnyAsync())
            {
                var demoStore = new Store
                {
                    Code = "demo",
                    Name = "Elleganza Demo Store",
                    NameAr = "متجر إليجانزا التجريبي",
                    Description = "The default Ecomus themed store for ElleganzaPlatform",
                    DescriptionAr = "المتجر الافتراضي بتصميم Ecomus لمنصة إليجانزا",
                    IsActive = true,
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.Stores.Add(demoStore);
                await context.SaveChangesAsync();
                logger.LogInformation("Demo store seeded successfully with code: demo");
            }

            // Seed roles
            var roles = new[] { Roles.SuperAdmin, Roles.StoreAdmin, Roles.Vendor, Roles.Customer };
            
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var role = new IdentityRole(roleName);
                    await roleManager.CreateAsync(role);
                    logger.LogInformation($"Role '{roleName}' created successfully.");
                }
            }

            // Seed super admin user
            const string superAdminEmail = "superadmin@elleganza.com";
            const string superAdminPassword = "SuperAdmin@123";

            var superAdminUser = await userManager.FindByEmailAsync(superAdminEmail);
            
            if (superAdminUser == null)
            {
                superAdminUser = new ApplicationUser
                {
                    UserName = superAdminEmail,
                    Email = superAdminEmail,
                    EmailConfirmed = true,
                    FirstName = "Super",
                    LastName = "Admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(superAdminUser, superAdminPassword);
                
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(superAdminUser, Roles.SuperAdmin);
                    logger.LogInformation($"Super admin user created with email: {superAdminEmail}");
                    logger.LogWarning($"Default password is: {superAdminPassword} - Please change it immediately!");
                }
                else
                {
                    logger.LogError($"Failed to create super admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                // Ensure super admin has the SuperAdmin role
                if (!await userManager.IsInRoleAsync(superAdminUser, Roles.SuperAdmin))
                {
                    await userManager.AddToRoleAsync(superAdminUser, Roles.SuperAdmin);
                    logger.LogInformation($"SuperAdmin role added to existing user: {superAdminEmail}");
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}
