using Homecare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Homecare.Data.Configurations
{
       public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
       {
              public void Configure(EntityTypeBuilder<RefreshToken> builder)
              {
                     builder.ToTable("refresh_tokens");

                     builder.HasKey(r => r.Id);

                     builder.Property(r => r.Id)
                            .ValueGeneratedOnAdd();

                     builder.Property(r => r.Token)
                            .IsRequired()
                            .HasMaxLength(500);

                     builder.HasIndex(r => r.Token)
                            .IsUnique();

                     builder.Property(r => r.ExpiresAt)
                            .IsRequired();

                     builder.Property(r => r.IsRevoked)
                            .IsRequired()
                            .HasDefaultValue(false);

                     builder.Property(r => r.CreatedAt)
                            .IsRequired();

                     builder.HasOne(r => r.Admin)
                            .WithMany(a => a.RefreshTokens)
                            .HasForeignKey(r => r.AdminId)
                            .OnDelete(DeleteBehavior.Cascade)
                            .IsRequired(false);

                     builder.HasOne(r => r.Customer)
                            .WithMany(c => c.RefreshTokens)
                            .HasForeignKey(r => r.CustomerId)
                            .OnDelete(DeleteBehavior.Cascade)
                            .IsRequired(false);

                     builder.HasOne(r => r.ServicePartner)
                            .WithMany(p => p.RefreshTokens)
                            .HasForeignKey(r => r.ServicePartnerId)
                            .OnDelete(DeleteBehavior.Cascade)
                            .IsRequired(false);

                     builder.ToTable(t => t.HasCheckConstraint(
                            "chk_refresh_token_owner",
                            @"(
                            (""AdminId"" IS NOT NULL AND ""CustomerId"" IS NULL AND ""ServicePartnerId"" IS NULL)
                            OR (""AdminId"" IS NULL AND ""CustomerId"" IS NOT NULL AND ""ServicePartnerId"" IS NULL)
                            OR (""AdminId"" IS NULL AND ""CustomerId"" IS NULL AND ""ServicePartnerId"" IS NOT NULL)
                            )"
                     ));
              }
       }
}