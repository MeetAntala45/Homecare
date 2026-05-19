using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Homecare.Domain.Entities;

public class PartnerDocumentConfiguration : IEntityTypeConfiguration<PartnerDocument>
{
    public void Configure(EntityTypeBuilder<PartnerDocument> builder)
    {
        builder.ToTable("partner_documents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DocumentName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.FilePath)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(x => x.FileType)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.FileSizeKb)
            .IsRequired();
    }
}