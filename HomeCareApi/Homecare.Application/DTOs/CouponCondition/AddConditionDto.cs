using System;
using System.ComponentModel.DataAnnotations;

namespace Homecare.Application.DTOs.CouponCondition;

public class AddConditionDto
{
    [Required]
    public int CouponId { get; set; }

    [Required]
    public List<CreateCouponConditionDto> Conditions { get; set; } = new();
}
