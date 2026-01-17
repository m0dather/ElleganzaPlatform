using ElleganzaPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElleganzaPlatform.Infrastructure.Data.Configurations;

public class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.HasKey(v => v.Id);
        
        builder.Property(v => v.Name)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(v => v.NameAr)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(v => v.CommissionRate)
            .HasPrecision(5, 2);

        builder.HasMany(v => v.Products)
            .WithOne(p => p.Vendor)
            .HasForeignKey(p => p.VendorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(v => v.StoreId);
    }
}
