using System;

namespace Homecare.Application.DTOs.CouponCondition;

public class CouponConditionResponseDto
{
    public int Id { get; set; }
    public int ConditionTypeId { get; set; }
    public string ConditionType { get; set; } = null!;
    public string Operator { get; set; } = null!;
    public string Value { get; set; } = null!;
    public string FailBehaviour { get; set; } = null!;
}
