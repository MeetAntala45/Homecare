using System;

namespace Homecare.Application.Common.Models;

public class CouponCheckContext
{
    public decimal CartTotal { get; set; }
    public int UserBookingCount { get; set; }
    public int UserCouponUses { get; set; }
    public string? SlotDayOfWeek { get; set; }
    public TimeSpan SlotTime { get; set; }
    public DateTime? SlotDate { get; set; }
    public string? CustomerCity { get; set; }
    public string? CustomerState { get; set; }
    public int? ServiceSubCategoryId { get; set; }
}
