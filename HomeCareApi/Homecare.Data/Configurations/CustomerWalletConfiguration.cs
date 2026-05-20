using Homecare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Homecare.Data.Configurations;

public class CustomerWalletConfiguration : IEntityTypeConfiguration<CustomerWallet>
{
    public void Configure(EntityTypeBuilder<CustomerWallet> builder)
    {
        builder.ToTable("customer_wallets");
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id).ValueGeneratedOnAdd();
        builder.Property(w => w.CustomerId).IsRequired();
        builder.Property(w => w.Balance)
            .IsRequired()
            .HasColumnType("decimal(10,2)")
            .HasDefaultValue(0);
        builder.Property(w => w.CreatedAt).IsRequired();
        builder.Property(w => w.ModifiedAt);

        builder.HasIndex(w => w.CustomerId).IsUnique();

        builder.HasOne(w => w.Customer)
            .WithOne()
            .HasForeignKey<CustomerWallet>(w => w.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.Transactions)
            .WithOne(t => t.Wallet)
            .HasForeignKey(t => t.WalletId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}