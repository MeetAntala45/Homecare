
namespace Homecare.Application.DTOs.PartnerLeave;

public class AdminSystemNotifDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public int TypeId { get; set; }
    public string Type { get; set; } = null!;
    public bool IsRead { get; set; }
    public int? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
    public int? FromPartnerId { get; set; }
    public string? FromPartnerName { get; set; }
    public string CreatedAt { get; set; } = null!;
    public string TimeAgo { get; set; } = null!;
}