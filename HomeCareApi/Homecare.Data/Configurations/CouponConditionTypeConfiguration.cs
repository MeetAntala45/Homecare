using System;
using Homecare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Homecare.Data.Configurations;

public class CouponConditionTypeConfiguration
: IEntityTypeConfiguration<CouponConditionType>
{
    public void Configure(EntityTypeBuilder<CouponConditionType> builder)
    {
        builder.ToTable("coupon_condition_types");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Label)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ContextKey)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.InputType)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.DefaultOperator)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.DefaultFailBehaviour)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("disable");

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedBy)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(x => x.ModifiedBy);
        builder.Property(x => x.ModifiedAt);

    }
}
