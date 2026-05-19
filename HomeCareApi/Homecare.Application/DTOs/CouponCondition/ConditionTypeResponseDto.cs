using System;

namespace Homecare.Application.DTOs.CouponCondition;

public class ConditionTypeResponseDto
{
    public int Id { get; set; }
    public string Label { get; set; } = null!;
    public string ContextKey { get; set; } = null!;
    public string InputType { get; set; } = null!;
    public string DefaultOperator { get; set; } = null!;
    public string DefaultFailBehaviour { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
