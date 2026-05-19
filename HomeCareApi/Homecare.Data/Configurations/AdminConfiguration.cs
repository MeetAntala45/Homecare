using Homecare.Domain.Entities;
using Homecare.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Homecare.Data.Configurations
{
    public class AdminConfiguration : IEntityTypeConfiguration<Admin>
    {
        public void Configure(EntityTypeBuilder<Admin> builder)
        {
            builder.ToTable("admin");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id)
                   .ValueGeneratedOnAdd();

            builder.Property(a => a.Name)
                   .HasMaxLength(100);

            builder.Property(a => a.MobileNumber)
                   .HasMaxLength(15);

            builder.Property(a => a.Email)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(a => a.PasswordHash)
                   .IsRequired()
                   .HasMaxLength(300);

            builder.Property(a => a.ProfileImage)
                   .HasMaxLength(300);

            builder.Property(a => a.Address)
                   .HasColumnType("text");

            builder.Property(a => a.Role)
                   .IsRequired()
                   .HasConversion<int>()
                   .HasDefaultValue(AdminRole.Admin);

            builder.Property(a => a.IsDeleted)
                   .IsRequired()
                   .HasDefaultValue(false);

            builder.Property(a => a.CreatedAt)
                   .IsRequired();

            builder.Property(a => a.ModifiedAt);

            builder.Property(a => a.CreatedBy)
                   .IsRequired();

            builder.Property(a => a.ModifiedBy);

            builder.HasIndex(a => a.Email)
                   .IsUnique();

            builder.HasQueryFilter(a => !a.IsDeleted);
        }
    }
}