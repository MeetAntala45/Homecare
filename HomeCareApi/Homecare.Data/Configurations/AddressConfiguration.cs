using Homecare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Homecare.Data.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
       public void Configure(EntityTypeBuilder<Address> builder)
       {
              builder.ToTable("addresses");

              builder.HasKey(a => a.Id);

              builder.Property(a => a.Id)
                     .ValueGeneratedOnAdd();

              builder.Property(a => a.CustomerId)
                     .IsRequired();

              builder.Property(a => a.DisplayName)
                     .HasMaxLength(500)
                     .IsRequired(false);

              builder.Property(a => a.HouseFlatNo)
                     .IsRequired()
                     .HasMaxLength(100);

              builder.Property(a => a.Landmark)
                     .IsRequired()
                     .HasMaxLength(150);

              builder.Property(a => a.Latitude)
                     .HasColumnType("decimal(10,7)");

              builder.Property(a => a.Longitude)
                     .HasColumnType("decimal(10,7)");

              builder.Property(a => a.Label)
                     .IsRequired()
                     .HasMaxLength(100)
                     .HasDefaultValue("Home");

              builder.Property(a => a.CreatedAt)
                     .IsRequired()
                     .HasDefaultValueSql("now()");

              builder.Property(a => a.City)
                     .IsRequired()
                     .HasMaxLength(50)
                     .HasDefaultValue("");

              builder.Property(a => a.ModifiedAt);

              builder.HasOne(a => a.Customer)
                     .WithMany(a => a.Addresses)
                     .HasForeignKey(a => a.CustomerId)
                     .OnDelete(DeleteBehavior.Cascade);
       }
}