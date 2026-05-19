
using Homecare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Homecare.Data.Configurations;

public class PartnerSystemNotificationConfiguration
    : IEntityTypeConfiguration<PartnerSystemNotification>
{
    public void Configure(EntityTypeBuilder<PartnerSystemNotification> builder)
    {
        builder.ToTable("partner_system_notifications");

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

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.HasOne(x => x.Partner)
            .WithMany()
            .HasForeignKey(x => x.PartnerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.PartnerId);
        builder.HasIndex(x => new { x.PartnerId, x.IsRead });
        builder.HasIndex(x => x.CreatedAt);
    }
}