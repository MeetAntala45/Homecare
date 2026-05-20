using Homecare.Domain.Entities;
using Homecare.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Homecare.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
       public void Configure(EntityTypeBuilder<Customer> builder)
       {
              builder.ToTable("customers");

              builder.HasKey(c => c.Id);

              builder.Property(c => c.Id)
                     .ValueGeneratedOnAdd();

              builder.Property(c => c.Name)
                     .IsRequired()
                     .HasMaxLength(100);

              builder.Property(c => c.Email)
                     .IsRequired()
                     .HasMaxLength(150);

              builder.Property(c => c.MobileNumber)
                     .HasMaxLength(15);

              builder.Property(c => c.Status)
                     .IsRequired()
                     .HasConversion<string>()
                     .HasDefaultValue(UserStatus.Active);

              builder.Property(c => c.PendingBookings)
                     .IsRequired()
                     .HasDefaultValue(0);

              builder.Property(c => c.TotalBookings)
                     .IsRequired()
                     .HasDefaultValue(0);

              builder.Property(c => c.CreatedAt)
                     .IsRequired();

              builder.Property(c => c.ModifiedAt);

              builder.Property(c => c.CreatedBy);

              builder.Property(c => c.ModifiedBy);

              builder.HasIndex(c => c.Email)
                     .IsUnique();
              builder.Property(c => c.ReferralCode)
                  .HasMaxLength(20);

              builder.HasIndex(c => c.ReferralCode)
                  .IsUnique()
                  .HasFilter("\"ReferralCode\" IS NOT NULL");

              builder.Property(c => c.ReferralUseCount)
                  .IsRequired()
                  .HasDefaultValue(0);

              builder.HasMany(c => c.Addresses)
                     .WithOne(a => a.Customer)
                     .HasForeignKey(a => a.CustomerId)
                     .OnDelete(DeleteBehavior.Cascade);

              builder.HasMany(c => c.OtpVerifications)
                     .WithOne(o => o.Customer)
                     .HasForeignKey(o => o.CustomerId)
                     .OnDelete(DeleteBehavior.SetNull);

              builder.HasMany(c => c.RefreshTokens)
                     .WithOne(r => r.Customer)
                     .HasForeignKey(r => r.CustomerId)
                     .OnDelete(DeleteBehavior.Cascade);
       }
}
