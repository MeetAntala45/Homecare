namespace Homecare.Domain.Entities;

public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public int? AdminId { get; set; }
    public Admin? Admin { get; set; } = null!;
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; } = null!;
    public int? ServicePartnerId { get; set; } = null;
    public ServicePartner? ServicePartner { get; set; }
}
