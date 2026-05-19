using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Homecare.Domain.Entities;

public class PartnerLanguageConfiguration : IEntityTypeConfiguration<PartnerLanguage>
{
    public void Configure(EntityTypeBuilder<PartnerLanguage> builder)
    {
        builder.ToTable("partner_languages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Language)
            .IsRequired();

        builder.Property(x => x.Proficiency)
            .IsRequired();
    }
}