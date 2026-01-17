using ElleganzaPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElleganzaPlatform.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(300);
            
        builder.Property(p => p.NameAr)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(p => p.Sku)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Price)
            .HasPrecision(18, 2);

        builder.Property(p => p.CompareAtPrice)
            .HasPrecision(18, 2);

        builder.HasIndex(p => p.Sku);
        builder.HasIndex(p => new { p.StoreId, p.VendorId });
    }
}
