using Homecare.Application.DTOs.CouponCondition;
using Homecare.Domain.Enums;

namespace Homecare.Application.DTOs.Offers;

public class OfferResponseDto
{
    public int Id { get; set; }

    public string OfferCode { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Discount { get; set; } = string.Empty;

    public int TimesApplied { get; set; }

    public CouponStatus Status { get; set; }

    public List<CouponConditionResponseDto> Conditions { get; set; } = new();
}