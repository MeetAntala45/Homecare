namespace Homecare.Application.DTOs;
public class ServiceFilterDto
{
    public int? SubCategoryId { get; set; }
    public decimal? PriceMin { get; set; }
    public decimal? PriceMax { get; set; }
    public bool? IsAvailable { get; set; }
    public decimal? CommissionPct { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; } = "asc";
}