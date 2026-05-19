using Homecare.Domain.Enums;

namespace Homecare.Domain.Entities;

public class PartnerLeave : BaseEntity
{
    public int Id { get; set; }
    public int PartnerId { get; set; }
    public ServicePartner Partner { get; set; } = null!;
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public string Reason { get; set; } = null!;
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
    public int? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? AdminRemarks { get; set; }
}