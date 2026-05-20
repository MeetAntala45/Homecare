namespace Homecare.Domain.Entities;

public class CustomerWallet
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public decimal Balance { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }

    public Customer Customer { get; set; } = null!;
    public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
}