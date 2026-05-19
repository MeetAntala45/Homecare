namespace Homecare.Application.DTOs.CouponAddvertisement;

public class CouponAdvertisementDto
{
    public string CouponCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DiscountPct { get; set; }
    public List<CouponAdvertisementConditionDto> Conditions { get; set; } = new();
}

public class CouponAdvertisementConditionDto
{
    public string Label { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
}
