namespace Homecare.Application.DTOs.PartnerLeave;

public class LeaveResponseDto
{
    public int Id { get; set; }
    public string FromDate { get; set; } = null!;
    public string ToDate { get; set; } = null!;
    public int TotalDays { get; set; }
    public string Reason { get; set; } = null!;
    public int StatusId { get; set; }
    public string Status { get; set; } = null!;
    public string? AdminRemarks { get; set; }
    public string AppliedOn { get; set; } = null!;
    public string? ReviewedAt { get; set; }
}