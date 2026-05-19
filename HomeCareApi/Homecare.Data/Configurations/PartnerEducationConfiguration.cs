using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Homecare.Domain.Entities;

public class PartnerEducationConfiguration : IEntityTypeConfiguration<PartnerEducation>
{
    public void Configure(EntityTypeBuilder<PartnerEducation> builder)
    {
        builder.ToTable("partner_education");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.InstituteName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.PassingYear)
            .IsRequired();

        builder.Property(x => x.MarksPercentage)
            .HasColumnType("decimal(5,2)");
    }
}