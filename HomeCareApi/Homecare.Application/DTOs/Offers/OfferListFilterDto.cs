using Homecare.Domain.Enums;

namespace Homecare.Application.DTOs.Offers;

public class GetOfferListFilterDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public string? SortBy { get; set; } = "id";
    public string SortOrder { get; set; } = "desc";

    public string? CouponCode { get; set; }
    public int? MinDiscount { get; set; }
    public int? MaxDiscount { get; set; }
    public int? MinUsage { get; set; }
    public int? MaxUsage { get; set; }
    public CouponStatus? Status { get; set; }
}