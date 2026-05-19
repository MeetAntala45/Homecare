namespace Homecare.Application.DTOs.ServicePartner;

public class PartnerAssignedServiceFilterDto
{
    public int    PageNumber  { get; set; } = 1;
    public int    PageSize    { get; set; } = 5;
    public string? SortBy     { get; set; }
    public string  SortOrder  { get; set; } = "desc";
    public string? Date        { get; set; }
    public string? Time        { get; set; }
    public string? Status      { get; set; }
}
