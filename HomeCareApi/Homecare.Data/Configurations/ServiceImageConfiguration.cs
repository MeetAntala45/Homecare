using Homecare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Homecare.Data.Configurations
{
    public class ServiceImageConfiguration : IEntityTypeConfiguration<ServiceImage>
    {
        public void Configure(EntityTypeBuilder<ServiceImage> builder)
        {
            builder.ToTable("service_images");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                   .HasColumnName("id")
                   .UseIdentityColumn();

            builder.Property(x => x.ServiceId)
                   .HasColumnName("service_id")
                   .IsRequired();

            builder.Property(x => x.ImagePath)
                   .HasColumnName("image_path")
                   .HasMaxLength(300)
                   .IsRequired();

            builder.Property(x => x.CreatedOn)
                   .HasColumnName("created_on")
                   .IsRequired();
              
            builder.Property(x => x.CreatedBy)
              .HasColumnName("created_by")
              .IsRequired();

            builder.Property(x => x.IsDeleted)
                     .HasColumnName("is_deleted")
                     .IsRequired();

            builder.Property(x => x.ModifiedBy)
                     .HasColumnName("modified_by");

            builder.Property(x => x.ModifiedOn)
                     .HasColumnName("modified_on");
        }
    }
}