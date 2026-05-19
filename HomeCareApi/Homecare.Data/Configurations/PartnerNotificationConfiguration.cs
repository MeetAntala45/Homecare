using Homecare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Homecare.Data.Configurations;

public class PartnerNotificationConfiguration : IEntityTypeConfiguration<PartnerNotification>
{
public void Configure(EntityTypeBuilder<PartnerNotification> builder)
    {
        builder.ToTable("partner_notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Id)
            .ValueGeneratedOnAdd();

        builder.Property(n => n.BookingId)
            .IsRequired();

        builder.Property(n => n.CustomerId)
            .IsRequired();

        builder.Property(n => n.CustomerName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(n => n.ServiceName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(n => n.PaymentMethod)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(n => n.PaymentMethodValue)
            .IsRequired();

        builder.Property(n => n.SlotDate)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(n => n.SlotTime)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(n => n.Amount)
            .IsRequired()
            .HasColumnType("decimal(10,2)");

        builder.Property(n => n.Message)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(n => n.CreatedAt)
            .IsRequired();

        builder.HasOne(n => n.Booking)
            .WithMany()
            .HasForeignKey(n => n.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(n => n.CreatedAt);
        builder.HasIndex(n => n.CustomerId);
        builder.HasIndex(n => n.PartnerId);
    }
}
