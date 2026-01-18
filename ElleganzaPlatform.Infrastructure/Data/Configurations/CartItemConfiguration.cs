using ElleganzaPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElleganzaPlatform.Infrastructure.Data.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.HasKey(ci => ci.Id);
        
        builder.Property(ci => ci.CartId)
            .IsRequired();

        builder.Property(ci => ci.ProductId)
            .IsRequired();

        builder.Property(ci => ci.Quantity)
            .IsRequired();

        builder.Property(ci => ci.PriceSnapshot)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(ci => ci.VendorId)
            .IsRequired();

        builder.Property(ci => ci.StoreId)
            .IsRequired();

        // Relationship with Cart (required)
        builder.HasOne(ci => ci.Cart)
            .WithMany(c => c.Items)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship with Product (required)
        builder.HasOne(ci => ci.Product)
            .WithMany()
            .HasForeignKey(ci => ci.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship with Vendor (required)
        builder.HasOne(ci => ci.Vendor)
            .WithMany()
            .HasForeignKey(ci => ci.VendorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship with Store (required)
        builder.HasOne(ci => ci.Store)
            .WithMany()
            .HasForeignKey(ci => ci.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes for performance
        builder.HasIndex(ci => ci.CartId);
        builder.HasIndex(ci => ci.ProductId);
        builder.HasIndex(ci => new { ci.CartId, ci.ProductId });
    }
}
