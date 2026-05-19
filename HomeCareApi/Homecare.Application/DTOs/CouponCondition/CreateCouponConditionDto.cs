using System;
using System.ComponentModel.DataAnnotations;

namespace Homecare.Application.DTOs.CouponCondition;

public class CreateCouponConditionDto
{
    [Required(ErrorMessage = "Condition type is required.")]
    public int ConditionTypeId { get; set; }

    [Required(ErrorMessage = "Operator is required.")]
    public string Operator { get; set; } = null!;

    [Required(ErrorMessage = "Value is required.")]
    public string Value { get; set; } = null!;

    [Required(ErrorMessage = "Fail behaviour is required.")]
    public string FailBehaviour { get; set; } = null!;
}
