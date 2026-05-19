using System;

namespace Homecare.Application.Constants.Offers;

public class ContextKeys
{
    public const string CartTotal = "cart_total";
    public const string UserBookingCount = "user_booking_count";
    public const string UserCouponUses = "user_coupon_uses";
    public const string SlotDayOfWeek = "slot_day_of_week";
    public const string SlotTime = "slot_time";
    public const string SlotDate = "slot_date";
    public const string CustomerCity = "customer_city";
    public const string CustomerState = "customer_state";
    public const string ServiceSubCategory = "service_sub_category_id";

    public static readonly IReadOnlyList<string> All = new[]
    {
        CartTotal,
        UserBookingCount,
        UserCouponUses,
        SlotDayOfWeek,
        SlotTime,
        SlotDate,
        CustomerCity,
        CustomerState,
        ServiceSubCategory
    };
    public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> AllowedInputTypes =
    new Dictionary<string, IReadOnlyList<string>>
    {
        { CartTotal, new[] { "number" } },
        { UserBookingCount, new[] { "number" } },
        { UserCouponUses, new[] { "number" } },
        { SlotDayOfWeek, new[] { "days" } },
        { SlotTime, new[] { "time" } },
        { SlotDate, new[] { "date" } },
        { CustomerCity, new[] { "text" } },
        { CustomerState, new[] { "text" } },
        { ServiceSubCategory, new[] { "subcategory" } },
    };
}
