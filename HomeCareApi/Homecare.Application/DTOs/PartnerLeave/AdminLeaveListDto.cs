namespace Homecare.Application.DTOs.PartnerLeave;
public class AdminLeaveListDto : LeaveResponseDto
{
    public int PartnerId { get; set; }
    public string PartnerName { get; set; } = null!;
    public string PartnerEmail { get; set; } = null!;
    public string? ProfileImage { get; set; }
}