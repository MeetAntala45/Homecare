using Homecare.Domain.Entities;
using Homecare.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Homecare.Data.Configurations
{
    public class PartnerLeaveConfiguration : IEntityTypeConfiguration<PartnerLeave>
    {
        public void Configure(EntityTypeBuilder<PartnerLeave> builder)
        {
            // Table Name
            builder.ToTable("partner_leaves");

            // Primary Key
            builder.HasKey(pl => pl.Id);

            // Properties
            builder.Property(pl => pl.FromDate)
                .IsRequired();

            builder.Property(pl => pl.ToDate)
                .IsRequired();

            builder.Property(pl => pl.Reason)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(pl => pl.Status)
                .IsRequired()
                .HasDefaultValue(LeaveStatus.Pending);

            builder.Property(pl => pl.AdminRemarks)
                .HasMaxLength(500);

    
            builder.HasOne(pl => pl.Partner)
                .WithMany()
                .HasForeignKey(pl => pl.PartnerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(pl => pl.ReviewedBy)
                .IsRequired(false);

            builder.Property(pl => pl.ReviewedAt)
                .IsRequired(false);
        }
    }
}