using Homecare.Domain.Enums;

namespace Homecare.Domain.Entities;

public class WalletTransaction
{
    public int Id { get; set; }
    public int WalletId { get; set; }
    public decimal Amount { get; set; }
    public WalletTransactionType Type { get; set; }
    public string Description { get; set; } = null!;
    public int? ReferenceId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public CustomerWallet Wallet { get; set; } = null!;
}