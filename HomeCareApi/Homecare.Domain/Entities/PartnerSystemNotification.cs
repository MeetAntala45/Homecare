
using Homecare.Domain.Enums;

namespace Homecare.Domain.Entities;

public class PartnerSystemNotification
{
    public int Id { get; set; }
    public int PartnerId { get; set; }
    public ServicePartner Partner { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public PartnerSystemNotifType Type { get; set; }
    public bool IsRead { get; set; } = false;
    public int? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}