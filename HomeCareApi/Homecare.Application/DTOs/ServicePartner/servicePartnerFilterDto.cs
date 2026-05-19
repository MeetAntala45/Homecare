namespace Homecare.Application.DTOs.ServicePartner;

public class servicePartnerFilterDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "id";
    public string SortOrder { get; set; } = "desc";
    public string? PartnerName { get; set; }
    public int? ServiceTypeId { get; set; }
    public int? MinJob { get; set; }
    public int? MaxJob { get; set; }
    public int? StatusId { get; set; }

}