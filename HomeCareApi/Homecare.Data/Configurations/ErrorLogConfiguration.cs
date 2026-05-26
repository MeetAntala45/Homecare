using Homecare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Homecare.Data.Configurations;

public class ErrorLogConfiguration : IEntityTypeConfiguration<ErrorLog>
{
    public void Configure(EntityTypeBuilder<ErrorLog> builder)
    {
        builder.ToTable("error_logs");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.ExceptionType)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Message)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(e => e.StackTrace);

        builder.Property(e => e.Path)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.HttpMethod)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(e => e.StatusCode).IsRequired();
        builder.Property(e => e.UserId);

        builder.Property(e => e.UserRole)
            .HasMaxLength(50);

        builder.Property(e => e.OccurredAt).IsRequired();

        // Indexes for common filter/sort patterns
        builder.HasIndex(e => e.OccurredAt);
        builder.HasIndex(e => e.StatusCode);
        builder.HasIndex(e => e.ExceptionType);
    }
}