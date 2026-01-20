using ElleganzaPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElleganzaPlatform.Infrastructure.Data.Configurations;

public class CheckoutSessionConfiguration : IEntityTypeConfiguration<CheckoutSession>
{
    public void Configure(EntityTypeBuilder<CheckoutSession> builder)
    {
        builder.HasKey(cs => cs.Id);
        
        builder.Property(cs => cs.CartSnapshot)
            .IsRequired();

        builder.Property(cs => cs.ShippingMethod)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(cs => cs.ShippingCost)
            .HasPrecision(18, 2);

        builder.Property(cs => cs.ShippingAddress)
            .IsRequired();

        builder.Property(cs => cs.BillingAddress)
            .IsRequired();

        builder.Property(cs => cs.PaymentIntentId)
            .HasMaxLength(255);

        builder.HasOne(cs => cs.Store)
            .WithMany()
            .HasForeignKey(cs => cs.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cs => cs.User)
            .WithMany()
            .HasForeignKey(cs => cs.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cs => cs.Order)
            .WithMany()
            .HasForeignKey(cs => cs.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasIndex(cs => cs.PaymentIntentId);
        builder.HasIndex(cs => new { cs.StoreId, cs.UserId, cs.Status });
        builder.HasIndex(cs => cs.ExpiresAt);
    }
}
