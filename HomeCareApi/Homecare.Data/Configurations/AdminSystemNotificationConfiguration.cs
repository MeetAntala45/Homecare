
using Homecare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Homecare.Data.Configurations;

public class AdminSystemNotificationConfiguration
    : IEntityTypeConfiguration<AdminSystemNotification>
{
    public void Configure(EntityTypeBuilder<AdminSystemNotification> builder)
    {
        builder.ToTable("admin_system_notifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .UseIdentityColumn();

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Message)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.Type)
            .IsRequired();

        builder.Property(x => x.IsRead)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.ReferenceType)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(x => x.ReferenceId)
            .IsRequired(false);

        builder.Property(x => x.FromPartnerName)
            .HasMaxLength(150)
            .IsRequired(false);

        builder.Property(x => x.FromPartnerId)
            .IsRequired(false);

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.HasIndex(x => x.IsRead);
        builder.HasIndex(x => x.Type);
        builder.HasIndex(x => x.CreatedAt);
    }
}