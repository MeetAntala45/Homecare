using Homecare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Homecare.Data.Configurations;

public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
{
    public void Configure(EntityTypeBuilder<WalletTransaction> builder)
    {
        builder.ToTable("wallet_transactions");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedOnAdd();
        builder.Property(t => t.WalletId).IsRequired();
        builder.Property(t => t.Amount).IsRequired().HasColumnType("decimal(10,2)");
        builder.Property(t => t.Type).IsRequired();
        builder.Property(t => t.Description).IsRequired().HasMaxLength(300);
        builder.Property(t => t.ReferenceId);
        builder.Property(t => t.CreatedAt).IsRequired();
        builder.HasIndex(t => t.WalletId);
    }
}