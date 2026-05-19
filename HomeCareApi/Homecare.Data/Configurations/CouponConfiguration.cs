using Homecare.Domain.Entities;
using Homecare.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Homecare.Data.Configurations
{
    public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
    {
        public void Configure(EntityTypeBuilder<Coupon> builder)
        {
            builder.ToTable("coupons");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.CouponCode)
                .IsRequired()
                .HasMaxLength(30);

            builder.HasIndex(c => c.CouponCode)
                .IsUnique();

            builder.Property(c => c.Description)
                .HasMaxLength(255);

            builder.Property(c => c.DiscountPct)
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            builder.Property(c => c.Status)
                .IsRequired()
                .HasDefaultValue(CouponStatus.Active);

            builder.Property(c => c.CreatedAt)
              .IsRequired();

            builder.Property(c => c.ModifiedAt);

            builder.Property(c => c.CreatedBy)
                   .IsRequired();
            
            builder.Property(a => a.IsDeleted)
                   .IsRequired()
                   .HasDefaultValue(false);

            builder.Property(c => c.UsageCount)
                .IsRequired()
                .HasDefaultValue(0);
                
            builder.Property(c => c.ModifiedBy);

            builder.HasMany(c => c.Conditions)
                .WithOne(cc => cc.Coupon)
                .HasForeignKey(cc => cc.CouponId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}