using Homecare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Homecare.Data.Configurations;

public class PartnerOtpVerificationConfiguration : IEntityTypeConfiguration<PartnerOtpVerification>
{
    public void Configure(EntityTypeBuilder<PartnerOtpVerification> builder)
    {
        builder.ToTable("partner_otp_verifications");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
               .ValueGeneratedOnAdd();

        builder.Property(o => o.ServicePartnerId)
               .IsRequired();

        builder.Property(o => o.Email)
               .IsRequired()
               .HasMaxLength(150);

        builder.Property(o => o.OtpCode)
               .IsRequired()
               .HasMaxLength(4)
               .IsFixedLength();

        builder.Property(o => o.IsUsed)
               .IsRequired()
               .HasDefaultValue(false);

        builder.Property(o => o.IsRevoked)
               .IsRequired()
               .HasDefaultValue(false);

        builder.Property(o => o.CreatedAt)
               .IsRequired();

        builder.Property(o => o.ExpiresAt)
               .IsRequired();

        builder.HasOne(o => o.ServicePartner)
               .WithMany(p => p.PartnerOtpVerifications)
               .HasForeignKey(o => o.ServicePartnerId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
