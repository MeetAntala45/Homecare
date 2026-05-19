using Homecare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Homecare.Data.Configurations
{
       public class ServiceConfiguration : IEntityTypeConfiguration<Service>
       {
              public void Configure(EntityTypeBuilder<Service> builder)
              {
                     builder.ToTable("services");

                     builder.HasKey(x => x.Id);

                     builder.Property(x => x.Id)
                            .HasColumnName("id")
                            .UseIdentityColumn();

                     builder.Property(x => x.SubCategoryId)
                            .HasColumnName("sub_category_id")
                            .IsRequired();

                     builder.Property(x => x.Name)
                            .HasColumnName("name")
                            .HasMaxLength(150)
                            .IsRequired();

                     builder.Property(x => x.Description)
                            .HasColumnName("description")
                            .HasMaxLength(300)
                            .IsRequired();

                     builder.Property(x => x.Price)
                            .HasColumnName("price")
                            .HasColumnType("decimal(10,2)")
                            .IsRequired();

                     builder.Property(x => x.CommissionPct)
                            .HasColumnName("commission_pct")
                            .HasColumnType("decimal(5,2)")
                            .IsRequired();

                     builder.Property(x => x.DurationMin)
                            .HasColumnName("duration_min")
                            .IsRequired();

                     builder.Property(x => x.IsAvailable)
                            .HasColumnName("is_available")
                            .IsRequired();

                     builder.Property(x => x.CreatedBy)
                            .HasColumnName("created_by")
                            .IsRequired();

                     builder.Property(x => x.CreatedOn)
                            .HasColumnName("created_on")
                            .IsRequired();

                     builder.Property(x => x.ModifiedBy)
                            .HasColumnName("modified_by");

                     builder.Property(x => x.ModifiedOn)
                            .HasColumnName("modified_on");

                     builder.Property(x => x.IsDeleted)
                       .HasColumnName("is_deleted")
                       .IsRequired();

                     builder.HasIndex(x => new { x.SubCategoryId, x.Name })
                            .IsUnique()
                            .HasDatabaseName("UX_services_subcategory_name");

                     builder.HasOne(x => x.SubCategory)
                            .WithMany(sc => sc.Services)
                            .HasForeignKey(x => x.SubCategoryId)
                            .OnDelete(DeleteBehavior.Restrict);

                     builder.HasMany(x => x.ServiceImages)
                            .WithOne(i => i.Service)
                            .HasForeignKey(i => i.ServiceId)
                            .OnDelete(DeleteBehavior.Cascade);

                     builder.HasMany(x => x.ServiceChecklists)
                            .WithOne(c => c.Service)
                            .HasForeignKey(c => c.ServiceId)
                            .OnDelete(DeleteBehavior.Cascade);
              }
       }
}