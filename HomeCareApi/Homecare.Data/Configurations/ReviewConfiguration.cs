using Homecare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Homecare.Data.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("reviews");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedOnAdd();

        builder.Property(r => r.BookingId)
            .IsRequired();

        builder.Property(r => r.CustomerId)
            .IsRequired();

        builder.Property(r => r.PartnerId)
            .IsRequired();

        builder.Property(r => r.Rating)
        .IsRequired()
        .HasColumnType("smallint");

        builder.ToTable(t => t.HasCheckConstraint("CK_reviews_Rating", "\"Rating\" BETWEEN 1 AND 5"));

        builder.Property(r => r.ReviewText)
            .HasMaxLength(1000);

        builder.Property(r => r.CreatedBy)
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.ModifiedBy);
        builder.Property(r => r.ModifiedAt);

        builder.Property(r => r.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasQueryFilter(r => !r.IsDeleted);

        builder.HasOne(r => r.Booking)
            .WithOne(b => b.Review)
            .HasForeignKey<Review>(r => r.BookingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Customer)
            .WithMany()
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Partner)
            .WithMany()
            .HasForeignKey(r => r.PartnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.BookingId)
            .IsUnique();

        builder.HasIndex(r => r.PartnerId);
        builder.HasIndex(r => r.CustomerId);
    }
}
