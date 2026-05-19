using System;
using Homecare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Homecare.Data.Configurations;

public class CouponConditionConfiguration : IEntityTypeConfiguration<CouponCondition>
{
    public void Configure(
            EntityTypeBuilder<CouponCondition> builder)
    {
        builder.ToTable("coupon_conditions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CouponId)
            .IsRequired();

        builder.Property(x => x.ConditionTypeId)
            .IsRequired();

        builder.Property(x => x.Operator)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Value)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.FailBehaviour)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(x => x.CouponId);

        builder.HasIndex(x => x.ConditionTypeId);

        builder.HasIndex(x => new { x.CouponId, x.ConditionTypeId })
            .IsUnique();

        builder.HasOne(x => x.ConditionType)
            .WithMany()
            .HasForeignKey(x => x.ConditionTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
