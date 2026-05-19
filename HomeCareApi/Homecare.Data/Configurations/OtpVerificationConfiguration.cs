using Homecare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Homecare.Data.Configurations;

public class OtpVerificationConfiguration : IEntityTypeConfiguration<OtpVerification>
{
    public void Configure(EntityTypeBuilder<OtpVerification> builder)
    {
        builder.ToTable("otp_verifications");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
               .ValueGeneratedOnAdd();

        builder.Property(o => o.CustomerId);

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

        builder.HasOne(o => o.Customer)
               .WithMany(c => c.OtpVerifications)
               .HasForeignKey(o => o.CustomerId)
               .OnDelete(DeleteBehavior.SetNull)
               .IsRequired(false);
    }
}

