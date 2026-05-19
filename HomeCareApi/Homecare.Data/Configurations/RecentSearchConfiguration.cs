using Homecare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Homecare.Data.Configurations;

public class RecentSearchConfiguration : IEntityTypeConfiguration<RecentSearch>
{
    public void Configure(EntityTypeBuilder<RecentSearch> builder)
    {
        builder.ToTable("recent_searches");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
               .ValueGeneratedOnAdd();

        builder.Property(r => r.CustomerId)
               .IsRequired();

        builder.Property(r => r.DisplayName)
               .IsRequired()
               .HasMaxLength(300);

        builder.Property(r => r.Latitude)
               .IsRequired()
               .HasColumnType("decimal(10,7)");

        builder.Property(r => r.Longitude)
               .IsRequired()
               .HasColumnType("decimal(10,7)");

        builder.Property(r => r.CreatedAt)
               .IsRequired()
               .HasDefaultValueSql("now()");


        builder.HasOne<Customer>()
                .WithMany()
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
    }
}