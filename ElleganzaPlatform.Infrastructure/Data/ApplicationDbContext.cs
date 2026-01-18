using ElleganzaPlatform.Application.Common;
using ElleganzaPlatform.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace ElleganzaPlatform.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly ICurrentUserService? _currentUserService;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUserService)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<Store> Stores => Set<Store>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<StoreAdmin> StoreAdmins => Set<StoreAdmin>();
    public DbSet<VendorAdmin> VendorAdmins => Set<VendorAdmin>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Global query filters for multi-tenancy
        builder.Entity<Store>().HasQueryFilter(e => !e.IsDeleted);
        
        builder.Entity<Vendor>().HasQueryFilter(e => 
            !e.IsDeleted && 
            (_currentUserService == null || 
             _currentUserService.IsSuperAdmin || 
             _currentUserService.StoreId == null || 
             e.StoreId == _currentUserService.StoreId));

        builder.Entity<Product>().HasQueryFilter(e => 
            !e.IsDeleted && 
            (_currentUserService == null || 
             _currentUserService.IsSuperAdmin || 
             (_currentUserService.StoreId == null || e.StoreId == _currentUserService.StoreId) &&
             (_currentUserService.VendorId == null || e.VendorId == _currentUserService.VendorId)));

        builder.Entity<Order>().HasQueryFilter(e => 
            !e.IsDeleted && 
            (_currentUserService == null || 
             _currentUserService.IsSuperAdmin || 
             _currentUserService.StoreId == null || 
             e.StoreId == _currentUserService.StoreId));

        builder.Entity<Cart>().HasQueryFilter(e => 
            !e.IsDeleted && 
            (_currentUserService == null || 
             _currentUserService.IsSuperAdmin || 
             _currentUserService.StoreId == null || 
             e.StoreId == _currentUserService.StoreId));

        builder.Entity<CartItem>().HasQueryFilter(e => 
            !e.IsDeleted && 
            (_currentUserService == null || 
             _currentUserService.IsSuperAdmin || 
             _currentUserService.StoreId == null || 
             e.StoreId == _currentUserService.StoreId));
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Domain.Common.BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy = _currentUserService?.UserId;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = _currentUserService?.UserId;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
