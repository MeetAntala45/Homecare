namespace Homecare.Application.DTOs.Support.cs;

public class SupportResponseDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? Mobile { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
