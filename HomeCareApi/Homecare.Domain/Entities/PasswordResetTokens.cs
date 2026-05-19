namespace Homecare.Domain.Entities;

public class PasswordResetToken
{
    public int Id { get; set; }
    public int AdminId { get; set; }
    public Admin Admin { get; set; } = null!;
    public string Token {get; set;} = null!;
    public DateTime ExpiresAt {get; set;}
    public bool IsUsed {get; set;} = false;
    public DateTime CreatedAt {get; set;}
}
