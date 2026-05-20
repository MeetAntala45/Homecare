namespace Homecare.Application.DTOs.Referral;

public class WalletDto
{
    public decimal Balance { get; set; }
    public List<WalletTransactionDto> Transactions { get; set; } = new();
}

public class WalletTransactionDto
{
    public decimal  Amount      { get; set; }
    public string   Type        { get; set; } = null!; 
    public string   Description { get; set; } = null!;
    public DateTime CreatedAt   { get; set; }
}