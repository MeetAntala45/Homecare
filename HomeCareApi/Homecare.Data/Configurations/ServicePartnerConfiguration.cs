using Homecare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Homecare.Data.Configurations;

public class ServicePartnerConfiguration : IEntityTypeConfiguration<ServicePartner>
{
    public void Configure(EntityTypeBuilder<ServicePartner> builder)
    {
        builder.ToTable("service_partners");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.MobileNumber)
            .IsRequired()
            .HasMaxLength(15);

        builder.Property(x => x.ProfileImage)
            .HasMaxLength(300);

        builder.Property(x => x.DateOfBirth)
            .IsRequired();

        builder.Property(x => x.Gender)
            .IsRequired();

        builder.Property(x => x.ServiceTypeId)
            .IsRequired();

        builder.Property(x => x.PermanentAddress)
            .IsRequired();

        builder.Property(x => x.ResidentialAddress)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.JobsCompletedCount)
            .HasDefaultValue(0)
            .IsRequired();

        builder.HasIndex(x => x.Email);

        builder.HasIndex(x => x.MobileNumber);

        builder.HasMany(x => x.Educations)
            .WithOne(x => x.Partner)
            .HasForeignKey(x => x.PartnerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Experiences)
            .WithOne(x => x.Partner)
            .HasForeignKey(x => x.PartnerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Skills)
            .WithOne(x => x.Partner)
            .HasForeignKey(x => x.PartnerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.ServicesOffered)
            .WithOne(x => x.Partner)
            .HasForeignKey(x => x.PartnerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Languages)
            .WithOne(x => x.Partner)
            .HasForeignKey(x => x.PartnerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Documents)
            .WithOne(x => x.Partner)
            .HasForeignKey(x => x.PartnerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}