using ElleganzaPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElleganzaPlatform.Infrastructure.Data.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.HasKey(c => c.Id);
        
        // UserId is nullable (for guest carts)
        builder.Property(c => c.UserId)
            .HasMaxLength(450);
        
        // SessionId is nullable (for authenticated user carts)
        builder.Property(c => c.SessionId)
            .HasMaxLength(200);

        builder.Property(c => c.StoreId)
            .IsRequired();

        builder.Property(c => c.LastActivityAt)
            .IsRequired();

        // One-to-many relationship with CartItems
        builder.HasMany(c => c.Items)
            .WithOne(ci => ci.Cart)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship with ApplicationUser (optional - for authenticated users only)
        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship with Store (required)
        builder.HasOne(c => c.Store)
            .WithMany()
            .HasForeignKey(c => c.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes for performance
        builder.HasIndex(c => c.UserId);
        builder.HasIndex(c => c.SessionId);
        builder.HasIndex(c => new { c.StoreId, c.UserId });
        builder.HasIndex(c => c.LastActivityAt);
    }
}
