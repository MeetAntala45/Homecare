using Homecare.Application.Common.Models;
using Homecare.Application.Constants;
using Homecare.Application.Constants.Offers;
using Homecare.Application.DTOs.CouponAddvertisement;
using Homecare.Application.Interfaces.CouponAdvertisement;
using Homecare.Data;
using Homecare.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Homecare.Application.Services.Offers;

public class CouponAdvertisementService : ICouponAdvertisementService
{
    private readonly AppDbContext _context;
    private readonly RuleEngine _ruleEngine;

    public CouponAdvertisementService(AppDbContext context, RuleEngine ruleEngine)
    {
        _context = context;
        _ruleEngine = ruleEngine;
    }

    public async Task<ApiResponse<CouponAdvertisementDto?>> GetBestAdvertisementAsync()
    {
        var indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById(
    OperatingSystem.IsWindows() ? "India Standard Time" : "Asia/Kolkata"
);
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indiaTimeZone);

        var ctx = new CouponCheckContext
        {
            SlotDayOfWeek = now.DayOfWeek.ToString().ToLowerInvariant(),
            SlotTime = now.TimeOfDay,
            SlotDate = now.Date,
            CartTotal = 0,
            UserBookingCount = 0,
            UserCouponUses = 0,
            CustomerCity = null,
            CustomerState = null,
            ServiceSubCategoryId = null
        };

        var advertisableKeys = new HashSet<string>
        {
            ContextKeys.SlotDayOfWeek,
            ContextKeys.SlotTime,
            ContextKeys.SlotDate
        };

        var coupons = await _context.coupons
            .AsNoTracking()
            .Where(c => !c.IsDeleted && c.Status == CouponStatus.Active)
            .Include(c => c.Conditions)
                .ThenInclude(cc => cc.ConditionType)
            .ToListAsync();

        var best = coupons
            .Where(coupon =>
            {
                var conditions = coupon.Conditions
                    .Where(c => c.ConditionType != null)
                    .ToList();

                if (!conditions.Any()) return false;

                bool hasUserContextCondition = conditions.Any(c =>
                    !advertisableKeys.Contains(c.ConditionType!.ContextKey));

                if (hasUserContextCondition) return false;

                return conditions.All(c => _ruleEngine.Evaluate(c, ctx));
            })
            .OrderByDescending(c => c.DiscountPct)
            .FirstOrDefault();

        if (best is null)
            return ApiResponse<CouponAdvertisementDto?>.SuccessResponse(
                "No active advertisement.", null);

        return ApiResponse<CouponAdvertisementDto?>.SuccessResponse(
            "Advertisement fetched.", MapToDto(best));
    }


    private static CouponAdvertisementDto MapToDto(Domain.Entities.Coupon coupon) => new()
    {
        CouponCode = coupon.CouponCode,
        Description = coupon.Description,
        DiscountPct = coupon.DiscountPct,
        Conditions = coupon.Conditions
            .Where(c => c.ConditionType != null)
            .Select(c => new CouponAdvertisementConditionDto
            {
                Label = c.ConditionType!.Label,
                Summary = BuildConditionSummary(c)
            })
            .ToList()
    };

    private static string BuildConditionSummary(Domain.Entities.CouponCondition c)
    {
        return c.ConditionType!.ContextKey switch
        {
            ContextKeys.SlotDayOfWeek when c.Operator == "in" =>
                "Valid on: " + string.Join(", ",
                    c.Value.Split(',').Select(d => Capitalize(d.Trim()))),

            ContextKeys.SlotDayOfWeek =>
                "Valid on " + Capitalize(c.Value),

            ContextKeys.SlotTime when c.Operator == "between" =>
                "Valid between " + FormatTimeRange(c.Value),

            ContextKeys.SlotDate when c.Operator == "between" =>
                "Valid " + FormatDateRange(c.Value),

            ContextKeys.SlotDate =>
                "Valid on " + DateTime.Parse(c.Value).ToString("dd MMM yyyy"),

            _ => c.Value
        };
    }

    private static string Capitalize(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s[1..].ToLower();

    private static string FormatTimeRange(string value)
    {
        var parts = value.Split(',');
        if (parts.Length != 2) return value;
        return $"{FormatTime(parts[0].Trim())} – {FormatTime(parts[1].Trim())}";
    }

    private static string FormatTime(string t)
    {
        if (!TimeSpan.TryParse(t, out var ts)) return t;
        var h = ts.Hours % 12 == 0 ? 12 : ts.Hours % 12;
        return $"{h}:{ts.Minutes:D2} {(ts.Hours >= 12 ? "PM" : "AM")}";
    }

    private static string FormatDateRange(string value)
    {
        var parts = value.Split(',');
        if (parts.Length != 2) return value;
        var from = DateTime.Parse(parts[0].Trim()).ToString("dd MMM yyyy");
        var to = DateTime.Parse(parts[1].Trim()).ToString("dd MMM yyyy");
        return $"{from} – {to}";
    }
}