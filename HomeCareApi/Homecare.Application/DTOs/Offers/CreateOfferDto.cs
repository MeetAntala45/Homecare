using System.ComponentModel.DataAnnotations;
using Homecare.Application.DTOs.CouponCondition;

namespace Homecare.Application.DTOs.Offers;

public class CreateOfferDto
{
    [Required]
    [MaxLength(50)]
    public string OfferCode { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Description { get; set; }

    [Range(1, 100)]
    public decimal DiscountPct { get; set; }

    public List<CreateCouponConditionDto>? Conditions { get; set; } = new();
}