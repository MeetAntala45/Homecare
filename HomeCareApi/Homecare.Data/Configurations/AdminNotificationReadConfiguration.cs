using System;
using Homecare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Homecare.Data.Configurations;

public class AdminNotificationReadConfiguration : IEntityTypeConfiguration<AdminNotificationRead>
{
    public void Configure(EntityTypeBuilder<AdminNotificationRead> builder)
    {
        builder.ToTable("admin_notification_reads");

        builder.HasKey(r => new { r.NotificationId, r.AdminId });

        builder.Property(r => r.ReadAt)
            .IsRequired();

        builder.HasOne(r => r.Notification)
            .WithMany(n => n.ReadBy)
            .HasForeignKey(r => r.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Admin)
            .WithMany()
            .HasForeignKey(r => r.AdminId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.AdminId);
        builder.HasIndex(r => new { r.AdminId, r.NotificationId });
    }
}
