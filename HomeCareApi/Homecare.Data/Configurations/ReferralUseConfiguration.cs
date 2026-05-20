using Homecare.Domain.Entities;
using Homecare.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Homecare.Data.Configurations;

public class ReferralUseConfiguration : IEntityTypeConfiguration<ReferralUse>
{
    public void Configure(EntityTypeBuilder<ReferralUse> builder)
    {
        builder.ToTable("referral_uses");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedOnAdd();
        builder.Property(r => r.ReferrerId).IsRequired();
        builder.Property(r => r.RefereeId).IsRequired();
        builder.Property(r => r.ReferralCode).IsRequired().HasMaxLength(20);
        builder.Property(r => r.Status).IsRequired().HasDefaultValue(ReferralStatus.Pending);
        builder.Property(r => r.RewardBookingId);
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.RewardedAt);

        builder.HasIndex(r => r.RefereeId).IsUnique();
        builder.HasIndex(r => r.ReferrerId);
        builder.HasIndex(r => r.ReferralCode);

        builder.HasOne(r => r.Referrer)
            .WithMany()
            .HasForeignKey(r => r.ReferrerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Referee)
            .WithMany()
            .HasForeignKey(r => r.RefereeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}