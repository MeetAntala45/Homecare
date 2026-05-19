using Homecare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Homecare.Data.Configurations;

public class SupportConfiguration : IEntityTypeConfiguration<Support>
{
    public void Configure(EntityTypeBuilder<Support> builder)
    {
        builder.ToTable("supports");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
               .ValueGeneratedOnAdd();

        builder.Property(x => x.FirstName)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(x => x.LastName)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(x => x.Mobile)
               .IsRequired()
               .HasMaxLength(15);

        builder.Property(x => x.Email)
               .IsRequired()
               .HasMaxLength(150);

        builder.Property(x => x.Description)
               .IsRequired()
               .HasMaxLength(500);

        builder.Property(x => x.CreatedAt)
               .IsRequired()
               .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(x => x.Email);

        builder.HasIndex(x => x.Mobile);
    }
}