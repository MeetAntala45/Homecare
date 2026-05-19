using Homecare.Domain.Entities;
using Homecare.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Homecare.Data.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("bookings");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .ValueGeneratedOnAdd();

        builder.Property(b => b.CustomerId)
            .IsRequired();

        builder.Property(b => b.ServiceId)
            .IsRequired();

        builder.Property(b => b.AddressId)
            .IsRequired();

        builder.Property(b => b.PartnerId);

        builder.Property(b => b.CouponId);

        builder.Property(b => b.SlotDate)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(b => b.SlotStartTime)
            .IsRequired()
            .HasColumnType("time");

        builder.Property(b => b.SlotEndTime)
            .IsRequired()
            .HasColumnType("time");

        builder.Property(b => b.ServicePrice)
            .IsRequired()
            .HasColumnType("decimal(10,2)");

        builder.Property(b => b.TaxPct)
            .IsRequired();

        builder.Property(b => b.TaxAmount)
            .IsRequired()
            .HasColumnType("decimal(10,2)");

        builder.Property(b => b.DiscountAmount)
            .IsRequired()
            .HasColumnType("decimal(10,2)")
            .HasDefaultValue(0);

        builder.Property(b => b.TotalAmount)
            .IsRequired()
            .HasColumnType("decimal(10,2)");

        builder.Property(p => p.PaymentMethod)
            .IsRequired();

        builder.Property(b => b.BookingStatus)
            .IsRequired()
            .HasDefaultValue(BookingStatus.Pending);

        builder.Property(b => b.PaymentStatus)
            .IsRequired()
            .HasDefaultValue(PaymentStatus.Pending);

        builder.Property(a => a.CreatedBy)
                           .IsRequired();
        builder.Property(b => b.CreatedAt)
            .IsRequired();

        builder.Property(b => b.ModifiedBy);

        builder.Property(b => b.ModifiedAt);
        builder.Property(b => b.IsDeleted)
             .IsRequired()
            .HasDefaultValue(false);

        builder.Property(b => b.CancellationReason)
            .HasMaxLength(500);

        builder.HasQueryFilter(b => !b.IsDeleted);

        builder.HasOne(b => b.Customer)
            .WithMany()
            .HasForeignKey(b => b.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Address)
            .WithMany()
            .HasForeignKey(b => b.AddressId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Coupon)
            .WithMany()
            .HasForeignKey(b => b.CouponId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(b => b.Partner)
            .WithMany()
            .HasForeignKey(b => b.PartnerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(b => b.Payment)
            .WithOne(p => p.Booking)
            .HasForeignKey<Payment>(p => p.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(b => b.CustomerId);
        builder.HasIndex(b => b.BookingStatus);
        builder.HasIndex(b => b.SlotDate);
    }
}