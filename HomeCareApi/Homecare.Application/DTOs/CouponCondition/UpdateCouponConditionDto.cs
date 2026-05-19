using System;
using System.ComponentModel.DataAnnotations;

namespace Homecare.Application.DTOs.CouponCondition;

public class UpdateCouponConditionDto
{
    [Required]
    public int Id { get; set; }

    [Required]
    public int ConditionTypeId { get; set; }

    [Required]
    public string Operator { get; set; } = null!;

    [Required]
    public string Value { get; set; } = null!;

    [Required]
    public string FailBehaviour { get; set; } = null!;
}
