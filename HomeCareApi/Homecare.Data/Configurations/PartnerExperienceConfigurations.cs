using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Homecare.Domain.Entities;

public class PartnerExperienceConfiguration : IEntityTypeConfiguration<PartnerExperience>
{
    public void Configure(EntityTypeBuilder<PartnerExperience> builder)
    {
        builder.ToTable("partner_experience");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CompanyName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Role)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.FromDate)
            .IsRequired();
    }
}