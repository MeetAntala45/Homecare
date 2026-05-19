// Homecare.Application/DTOs/Leave/LeaveFilterDto.cs
namespace Homecare.Application.DTOs.PartnerLeave;

public class LeaveFilterDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int? StatusId { get; set; }
    public string? PartnerName { get; set; }
    public string? FromDate { get; set; }
    public string? ToDate { get; set; }
}