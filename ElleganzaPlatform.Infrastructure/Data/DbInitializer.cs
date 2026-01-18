using ElleganzaPlatform.Domain.Entities;
using ElleganzaPlatform.Domain.Enums;
using ElleganzaPlatform.Infrastructure.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using CustomClaimTypes = ElleganzaPlatform.Infrastructure.Authorization.ClaimTypes;

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
            // ==================================================
            // 1️⃣ SEED DEMO STORE
            // ==================================================
            // Check if demo store already exists to ensure idempotency
            var demoStore = await context.Stores.FirstOrDefaultAsync(s => s.Code == "demo");
            
            if (demoStore == null)
            {
                demoStore = new Store
                {
                    Code = "demo",
                    Name = "Demo Store",
                    NameAr = "متجر تجريبي",
                    Description = "The default Ecomus themed store for ElleganzaPlatform",
                    DescriptionAr = "المتجر الافتراضي بتصميم Ecomus لمنصة إليجانزا",
                    IsActive = true,
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.Stores.Add(demoStore);
                await context.SaveChangesAsync();
                logger.LogInformation("✅ Demo store seeded successfully with code: demo");
            }
            else
            {
                logger.LogInformation("ℹ️ Demo store already exists, skipping...");
            }

            // ==================================================
            // 2️⃣ SEED ROLES (ASP.NET Identity)
            // ==================================================
            var roles = new[] { Roles.SuperAdmin, Roles.StoreAdmin, Roles.Vendor, Roles.Customer };
            
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var role = new IdentityRole(roleName);
                    await roleManager.CreateAsync(role);
                    logger.LogInformation($"✅ Role '{roleName}' created successfully.");
                }
            }

            // ==================================================
            // 3️⃣ SEED USERS
            // ==================================================
            
            // --- Super Admin User ---
            const string superAdminEmail = "superadmin@elleganza.local";
            const string superAdminUsername = "superadmin";
            const string superAdminPassword = "Admin@123";

            var superAdminUser = await userManager.FindByEmailAsync(superAdminEmail);
            
            if (superAdminUser == null)
            {
                superAdminUser = new ApplicationUser
                {
                    UserName = superAdminUsername,
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
                    logger.LogInformation($"✅ Super admin user created: {superAdminEmail}");
                    logger.LogWarning($"⚠️ Demo password: {superAdminPassword}");
                }
                else
                {
                    logger.LogError($"❌ Failed to create super admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                // Ensure super admin has the SuperAdmin role
                if (!await userManager.IsInRoleAsync(superAdminUser, Roles.SuperAdmin))
                {
                    await userManager.AddToRoleAsync(superAdminUser, Roles.SuperAdmin);
                    logger.LogInformation($"✅ SuperAdmin role added to existing user: {superAdminEmail}");
                }
            }

            // --- Store Admin User ---
            const string storeAdminEmail = "admin@elleganza.local";
            const string storeAdminUsername = "admin";
            const string storeAdminPassword = "Admin@123";

            var storeAdminUser = await userManager.FindByEmailAsync(storeAdminEmail);
            
            if (storeAdminUser == null)
            {
                storeAdminUser = new ApplicationUser
                {
                    UserName = storeAdminUsername,
                    Email = storeAdminEmail,
                    EmailConfirmed = true,
                    FirstName = "Store",
                    LastName = "Admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(storeAdminUser, storeAdminPassword);
                
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(storeAdminUser, Roles.StoreAdmin);
                    
                    // Add StoreId claim for Store Admin
                    await userManager.AddClaimAsync(storeAdminUser, 
                        new Claim(CustomClaimTypes.StoreId, demoStore.Id.ToString()));
                    
                    // Create StoreAdmin entity to link user with store
                    var storeAdmin = new StoreAdmin
                    {
                        StoreId = demoStore.Id,
                        UserId = storeAdminUser.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    context.StoreAdmins.Add(storeAdmin);
                    await context.SaveChangesAsync();
                    
                    logger.LogInformation($"✅ Store admin user created: {storeAdminEmail}");
                    logger.LogWarning($"⚠️ Demo password: {storeAdminPassword}");
                }
                else
                {
                    logger.LogError($"❌ Failed to create store admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            // --- Vendor User ---
            const string vendorEmail = "vendor@elleganza.local";
            const string vendorUsername = "vendor";
            const string vendorPassword = "Vendor@123";

            var vendorUser = await userManager.FindByEmailAsync(vendorEmail);
            Vendor? demoVendor = null;
            
            if (vendorUser == null)
            {
                vendorUser = new ApplicationUser
                {
                    UserName = vendorUsername,
                    Email = vendorEmail,
                    EmailConfirmed = true,
                    FirstName = "Demo",
                    LastName = "Vendor",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(vendorUser, vendorPassword);
                
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(vendorUser, Roles.Vendor);
                    
                    // Create Vendor entity
                    demoVendor = new Vendor
                    {
                        StoreId = demoStore.Id,
                        Name = "Demo Vendor",
                        NameAr = "بائع تجريبي",
                        Description = "A demo vendor for testing purposes",
                        DescriptionAr = "بائع تجريبي لأغراض الاختبار",
                        IsActive = true,
                        CommissionRate = 15.0m, // 15% commission
                        ContactEmail = vendorEmail,
                        ContactPhone = "+1234567890",
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    context.Vendors.Add(demoVendor);
                    await context.SaveChangesAsync();
                    
                    // Create VendorAdmin entity to link user with vendor
                    var vendorAdmin = new VendorAdmin
                    {
                        VendorId = demoVendor.Id,
                        UserId = vendorUser.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    context.VendorAdmins.Add(vendorAdmin);
                    await context.SaveChangesAsync();
                    
                    // Add VendorId claim for Vendor user
                    await userManager.AddClaimAsync(vendorUser, 
                        new Claim(CustomClaimTypes.VendorId, demoVendor.Id.ToString()));
                    
                    logger.LogInformation($"✅ Vendor user created: {vendorEmail}");
                    logger.LogInformation($"✅ Vendor entity created with ID: {demoVendor.Id}");
                    logger.LogWarning($"⚠️ Demo password: {vendorPassword}");
                }
                else
                {
                    logger.LogError($"❌ Failed to create vendor user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                // Get existing vendor entity
                var vendorAdmin = await context.VendorAdmins
                    .Include(va => va.Vendor)
                    .FirstOrDefaultAsync(va => va.UserId == vendorUser.Id);
                
                if (vendorAdmin != null)
                {
                    demoVendor = vendorAdmin.Vendor;
                }
            }

            // --- Customer User ---
            const string customerEmail = "customer@elleganza.local";
            const string customerUsername = "customer";
            const string customerPassword = "Customer@123";

            var customerUser = await userManager.FindByEmailAsync(customerEmail);
            
            if (customerUser == null)
            {
                customerUser = new ApplicationUser
                {
                    UserName = customerUsername,
                    Email = customerEmail,
                    EmailConfirmed = true,
                    FirstName = "Demo",
                    LastName = "Customer",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(customerUser, customerPassword);
                
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(customerUser, Roles.Customer);
                    logger.LogInformation($"✅ Customer user created: {customerEmail}");
                    logger.LogWarning($"⚠️ Demo password: {customerPassword}");
                }
                else
                {
                    logger.LogError($"❌ Failed to create customer user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            // ==================================================
            // 5️⃣ SEED PRODUCTS
            // ==================================================
            // Only seed products if vendor exists and no products exist yet
            if (demoVendor != null && !await context.Products.AnyAsync())
            {
                var products = new[]
                {
                    new Product
                    {
                        StoreId = demoStore.Id,
                        VendorId = demoVendor.Id,
                        Name = "Classic Cotton T-Shirt",
                        NameAr = "قميص قطني كلاسيكي",
                        Description = "Comfortable and stylish cotton t-shirt perfect for everyday wear",
                        DescriptionAr = "قميص قطني مريح وأنيق مثالي للارتداء اليومي",
                        Sku = "DEMO-TSH-001",
                        Price = 29.99m,
                        CompareAtPrice = 39.99m,
                        StockQuantity = 100,
                        Status = ProductStatus.Active,
                        RequiresApproval = false,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Product
                    {
                        StoreId = demoStore.Id,
                        VendorId = demoVendor.Id,
                        Name = "Slim Fit Jeans",
                        NameAr = "جينز ضيق",
                        Description = "Modern slim fit jeans with premium denim fabric",
                        DescriptionAr = "جينز ضيق عصري بقماش دنيم فاخر",
                        Sku = "DEMO-JNS-002",
                        Price = 59.99m,
                        CompareAtPrice = 79.99m,
                        StockQuantity = 50,
                        Status = ProductStatus.Active,
                        RequiresApproval = false,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Product
                    {
                        StoreId = demoStore.Id,
                        VendorId = demoVendor.Id,
                        Name = "Leather Jacket",
                        NameAr = "سترة جلدية",
                        Description = "Premium genuine leather jacket with modern design",
                        DescriptionAr = "سترة جلدية أصلية فاخرة بتصميم عصري",
                        Sku = "DEMO-JKT-003",
                        Price = 199.99m,
                        CompareAtPrice = 299.99m,
                        StockQuantity = 20,
                        Status = ProductStatus.Active,
                        RequiresApproval = false,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Product
                    {
                        StoreId = demoStore.Id,
                        VendorId = demoVendor.Id,
                        Name = "Summer Dress",
                        NameAr = "فستان صيفي",
                        Description = "Light and breezy summer dress perfect for warm weather",
                        DescriptionAr = "فستان صيفي خفيف ومنعش مثالي للطقس الدافئ",
                        Sku = "DEMO-DRS-004",
                        Price = 49.99m,
                        CompareAtPrice = 69.99m,
                        StockQuantity = 75,
                        Status = ProductStatus.Active,
                        RequiresApproval = false,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Product
                    {
                        StoreId = demoStore.Id,
                        VendorId = demoVendor.Id,
                        Name = "Running Shoes",
                        NameAr = "أحذية رياضية",
                        Description = "High-performance running shoes with excellent cushioning",
                        DescriptionAr = "أحذية رياضية عالية الأداء مع توسيد ممتاز",
                        Sku = "DEMO-SHO-005",
                        Price = 89.99m,
                        CompareAtPrice = 119.99m,
                        StockQuantity = 60,
                        Status = ProductStatus.Active,
                        RequiresApproval = false,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                context.Products.AddRange(products);
                await context.SaveChangesAsync();
                logger.LogInformation($"✅ Seeded {products.Length} demo products");
            }

            // ==================================================
            // 6️⃣ SEED SAMPLE ORDER (Optional but Recommended)
            // ==================================================
            // Only seed order if customer and products exist, and no orders exist yet
            if (customerUser != null && demoVendor != null)
            {
                var existingOrder = await context.Orders.FirstOrDefaultAsync(o => o.UserId == customerUser.Id);
                
                if (existingOrder == null)
                {
                    // Get the first product for the order
                    var product = await context.Products
                        .Where(p => p.VendorId == demoVendor.Id)
                        .FirstOrDefaultAsync();
                    
                    if (product != null)
                    {
                        // Create sample order
                        var order = new Order
                        {
                            StoreId = demoStore.Id,
                            UserId = customerUser.Id,
                            OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-001",
                            Status = OrderStatus.Delivered, // Completed status
                            SubTotal = product.Price * 2,
                            TaxAmount = product.Price * 2 * 0.10m, // 10% tax
                            ShippingAmount = 10.00m,
                            ShippingAddress = "123 Demo Street, Demo City, DC 12345",
                            BillingAddress = "123 Demo Street, Demo City, DC 12345",
                            CustomerNotes = "This is a sample order for testing purposes",
                            CreatedAt = DateTime.UtcNow.AddDays(-7) // Order from 7 days ago
                        };
                        
                        order.TotalAmount = order.SubTotal + order.TaxAmount + order.ShippingAmount;
                        
                        context.Orders.Add(order);
                        await context.SaveChangesAsync();
                        
                        // Create order item
                        var orderItem = new OrderItem
                        {
                            OrderId = order.Id,
                            ProductId = product.Id,
                            VendorId = demoVendor.Id,
                            ProductName = product.Name,
                            ProductSku = product.Sku,
                            Quantity = 2,
                            UnitPrice = product.Price,
                            TotalPrice = product.Price * 2,
                            VendorCommission = product.Price * 2 * (demoVendor.CommissionRate / 100),
                            CreatedAt = DateTime.UtcNow.AddDays(-7)
                        };
                        
                        context.OrderItems.Add(orderItem);
                        await context.SaveChangesAsync();
                        
                        logger.LogInformation($"✅ Sample order created: {order.OrderNumber}");
                    }
                }
            }

            logger.LogInformation("🎉 Database seeding completed successfully!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ An error occurred while seeding the database.");
            throw;
        }
    }
}
