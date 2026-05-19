namespace Homecare.Application.DTOs.PartnerLeave;

public class ApplyLeaveRequestDto
{
    public string FromDate { get; set; } = null!;  
    public string ToDate { get; set; } = null!;
    public string Reason { get; set; } = null!;
}