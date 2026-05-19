
using Homecare.Domain.Enums;

namespace Homecare.Domain.Entities;

public class AdminSystemNotification
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public AdminSystemNotifType Type { get; set; }
    public bool IsRead { get; set; } = false;
    public int? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
    public int? FromPartnerId { get; set; }
    public string? FromPartnerName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}