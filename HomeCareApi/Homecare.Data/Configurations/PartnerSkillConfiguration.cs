using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Homecare.Domain.Entities;

public class PartnerSkillConfiguration : IEntityTypeConfiguration<PartnerSkill>
{
    public void Configure(EntityTypeBuilder<PartnerSkill> builder)
    {
        builder.ToTable("partner_skills");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CategoryId)
               .IsRequired();

        builder.Property(x => x.PartnerId)
               .IsRequired();

        builder.HasOne(x => x.Partner)
               .WithMany(p => p.Skills)
               .HasForeignKey(x => x.PartnerId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Category)
               .WithMany()
               .HasForeignKey(x => x.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}