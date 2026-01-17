using ElleganzaPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElleganzaPlatform.Infrastructure.Data.Configurations;

public class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(s => s.NameAr)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(s => s.Description)
            .HasMaxLength(2000);
            
        builder.Property(s => s.DescriptionAr)
            .HasMaxLength(2000);

        builder.HasMany(s => s.Vendors)
            .WithOne(v => v.Store)
            .HasForeignKey(v => v.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Products)
            .WithOne(p => p.Store)
            .HasForeignKey(p => p.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Orders)
            .WithOne(o => o.Store)
            .HasForeignKey(o => o.StoreId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
