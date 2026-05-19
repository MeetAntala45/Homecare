using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Homecare.Domain.Entities;

public class PartnerServiceOfferedConfiguration : IEntityTypeConfiguration<PartnerServiceOffered>
{
    public void Configure(EntityTypeBuilder<PartnerServiceOffered> builder)
    {
        builder.ToTable("partner_services_offered");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SubCategoryId)
               .IsRequired();

        builder.Property(x => x.PartnerId)
               .IsRequired();

        builder.HasOne(x => x.Partner)
               .WithMany(p => p.ServicesOffered)
               .HasForeignKey(x => x.PartnerId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.SubCategory)
               .WithMany()
               .HasForeignKey(x => x.SubCategoryId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}