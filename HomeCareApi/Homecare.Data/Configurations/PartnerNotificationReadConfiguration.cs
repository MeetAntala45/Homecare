using Homecare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Homecare.Data.Configurations;

public class PartnerNotificationReadConfiguration : IEntityTypeConfiguration<PartnerNotificationRead>
{
    public void Configure(EntityTypeBuilder<PartnerNotificationRead> builder)
    {
        builder.ToTable("partner_notification_reads");

        builder.HasKey(r => new { r.NotificationId, r.PartnerId });

        builder.Property(r => r.ReadAt)
            .IsRequired();

        builder.HasOne(r => r.Notification)
            .WithMany(n => n.ReadBy)
            .HasForeignKey(r => r.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Partner)
            .WithMany()
            .HasForeignKey(r => r.PartnerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.PartnerId);
        builder.HasIndex(r => new { r.PartnerId, r.NotificationId });
    }
}
