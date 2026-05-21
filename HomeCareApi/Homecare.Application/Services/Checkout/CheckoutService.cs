using Homecare.Application.Common.Models;
using Homecare.Application.Constants;
using Homecare.Application.Constants.Checkout;
using Homecare.Application.DTOs.Checkout;
using Homecare.Application.Interfaces.Checkout;
using Homecare.Application.Interfaces.Referral;
using Homecare.Application.Services.Offers;
using Homecare.Data;
using Homecare.Domain.Entities;
using Homecare.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Homecare.Application.Services.Checkout;

public class CheckoutService : ICheckoutService
{
    private readonly AppDbContext _context;
    private readonly RuleEngine _ruleEngine;

    private readonly IReferralService _referralService;

    public CheckoutService(AppDbContext context, RuleEngine ruleEngine,
        IReferralService referralService)
    {
        _context = context;
        _ruleEngine = ruleEngine;
        _referralService = referralService;
    }
    public async Task<ApiResponse<List<AvailableCouponDto>>> GetAvailableCouponsAsync(
        AvailableCouponRequestDto dto, int customerId)
    {
        var coupons = await _context.coupons
            .AsNoTracking()
            .Where(c => !c.IsDeleted && c.Status == CouponStatus.Active)
            .Include(c => c.Conditions)
                .ThenInclude(cc => cc.ConditionType)
            .ToListAsync();

        var available = new List<AvailableCouponDto>();

        foreach (var coupon in coupons)
        {
            var isVisible = true;
            var isEligible = true;
            if (!coupon.Conditions.Any())
            {
                available.Add(MapToAvailableDto(coupon));
                continue;
            }

            var ctx = await BuildContextAsync(
               customerId, dto.ServiceId, dto.AddressId, dto.SlotDate, dto.SlotStartTime, coupon.CouponCode);

            foreach (var condition in coupon.Conditions)
            {
                if (condition.ConditionType == null)
                    continue;
                var pass = _ruleEngine.Evaluate(condition, ctx);

                if (!pass)
                {
                    if (condition.FailBehaviour == "hide")
                    {
                        isVisible = false;
                        break;
                    }
                    isEligible = false;
                }
            }

            if (!isVisible) continue;

            var mappedDto = MapToAvailableDto(coupon);
            mappedDto.IsEligible = isEligible;
            mappedDto.IneligibleReason = isEligible
                ? null
                : BuildIneligibleReason(coupon.Conditions
                    .FirstOrDefault(c => !_ruleEngine.Evaluate(c, ctx)), ctx);

            available.Add(mappedDto);
        }

        return ApiResponse<List<AvailableCouponDto>>
            .SuccessResponse(CheckoutSummaryMessages.Coupon.LoadSuccess, available
            .OrderByDescending(x => x.IsEligible)
            .ThenByDescending(x => x.DiscountPct)
            .ToList()
            );
    }

    public async Task<ApiResponse<ApplyCouponResponseDto>> ApplyCouponAsync(
        ApplyCouponRequestDto dto, int customerId)
    {
        var code = dto.CouponCode.Trim().ToUpper();

        var coupon = await _context.coupons
            .Where(c => c.CouponCode == code
                     && !c.IsDeleted
                     && c.Status == CouponStatus.Active)
            .Include(c => c.Conditions)
                .ThenInclude(cc => cc.ConditionType)
            .FirstOrDefaultAsync();

        if (coupon == null)
            return ApiResponse<ApplyCouponResponseDto>.Fail(CheckoutSummaryMessages.Coupon.NotFound);

        var ctx = await BuildContextAsync(
            customerId, dto.ServiceId, dto.AddressId, dto.SlotDate, dto.SlotStartTime, code);

        foreach (var condition in coupon.Conditions)
        {
            if (condition.ConditionType == null) continue;

            var pass = _ruleEngine.Evaluate(condition, ctx);
            if (!pass)
            {
                var reason = BuildIneligibleReason(condition, ctx);
                return ApiResponse<ApplyCouponResponseDto>.Fail(reason);
            }
        }

        var servicePrice = ctx.CartTotal;
        var discountAmount = Math.Round(servicePrice * coupon.DiscountPct / 100, 2);

        discountAmount = Math.Min(discountAmount, servicePrice);

        return ApiResponse<ApplyCouponResponseDto>.SuccessResponse(
            CheckoutSummaryMessages.Coupon.Applied,
            new ApplyCouponResponseDto
            {
                CouponCode = coupon.CouponCode,
                DiscountAmount = discountAmount,
                Message = $"{coupon.DiscountPct}% discount applied.",
            });
    }

    public async Task<ApiResponse<CheckoutSummaryResponseDto>> GetSummaryAsync(
        CheckoutSummaryRequestDto dto, int customerId)
    {
        var service = await _context.Services
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == dto.ServiceId);

        if (service == null)
            return ApiResponse<CheckoutSummaryResponseDto>.Fail(CheckoutSummaryMessages.Service.NotFound);

        var servicePrice = service.Price;
        var taxPct = TaxConstants.TaxPct;
        var taxAmount = Math.Round(servicePrice * taxPct / 100, 2);
        var discountAmount = 0m;
        string? appliedCode = null;

        if (!string.IsNullOrWhiteSpace(dto.CouponCode) && dto.SlotDate.HasValue
    && dto.SlotStartTime.HasValue)
        {
            var applyResult = await ApplyCouponAsync(new ApplyCouponRequestDto
            {
                CouponCode = dto.CouponCode,
                ServiceId = dto.ServiceId,
                SlotDate = dto.SlotDate.Value,
                SlotStartTime = dto.SlotStartTime.Value,
            }, customerId);

            if (applyResult.Success && applyResult.Data != null)
            {
                discountAmount = applyResult.Data.DiscountAmount;
                appliedCode = applyResult.Data.CouponCode;
            }
        }

        var total = servicePrice + taxAmount - discountAmount;

        decimal refereeDiscount = 0;
        bool isRefereeFirstOrder = false;

        if (customerId > 0)
        {
            refereeDiscount = await _referralService.GetRefereeFirstOrderDiscountAsync(
                customerId, servicePrice, discountAmount);
            isRefereeFirstOrder = refereeDiscount > 0;
        }

        decimal walletBalance = 0;
        decimal walletCap = Math.Round(servicePrice * ReferralConstants.WalletUsageCapPct / 100, 2);

        if (customerId > 0)
        {
            var wallet = await _context.CustomerWallets
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.CustomerId == customerId);

            walletBalance = wallet?.Balance ?? 0;
        }

        decimal previewTotal = total;
        if (refereeDiscount > 0 && previewTotal - refereeDiscount > 0.01m)
            previewTotal -= refereeDiscount;

        return ApiResponse<CheckoutSummaryResponseDto>.SuccessResponse(
            CheckoutSummaryMessages.Summary.Loaded,
            new CheckoutSummaryResponseDto
            {
                ServiceName = service.Name,
                ServicePrice = servicePrice,
                TaxPct = taxPct,
                TaxAmount = taxAmount,
                DiscountAmount = discountAmount,
                AppliedCouponCode = appliedCode,
                TotalAmount = previewTotal,
                RefereeDiscount = refereeDiscount,
                IsRefereeFirstOrder = isRefereeFirstOrder,
                WalletBalance = walletBalance,
                WalletCap = walletCap,
            });
    }

    private async Task<CouponCheckContext> BuildContextAsync(
        int customerId, int serviceId, int addressId,
        DateOnly slotDate, TimeOnly slotStartTime,
        string? couponCode)
    {

        var service = await _context.Services
        .AsNoTracking()
        .FirstOrDefaultAsync(s => s.Id == serviceId);

        var userBookingCount = await _context.Bookings
            .CountAsync(b => b.CustomerId == customerId);

        var userCouponUses = 0;
        if (!string.IsNullOrWhiteSpace(couponCode))
        {
            var couponId = await _context.coupons
                .Where(c => c.CouponCode == couponCode.ToUpper())
                .Select(c => c.Id)
                .FirstOrDefaultAsync();

            userCouponUses = await _context.Bookings
                .CountAsync(b => b.CustomerId == customerId
                              && b.CouponId == couponId);
        }

        var address = await _context.Addresses.Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.Id)
            .FirstOrDefaultAsync();

        return new CouponCheckContext
        {
            CartTotal = service?.Price ?? 0,
            UserBookingCount = userBookingCount,
            UserCouponUses = userCouponUses,
            SlotDayOfWeek = slotDate.DayOfWeek.ToString().ToLower(),
            SlotTime = slotStartTime.ToTimeSpan(),
            SlotDate = slotDate.ToDateTime(TimeOnly.MinValue),
            CustomerCity = address?.DisplayName?.ToLower(),
            CustomerState = address?.DisplayName?.ToLower(),
            ServiceSubCategoryId = service?.SubCategoryId
        };
    }

    private static AvailableCouponDto MapToAvailableDto(Coupon c) => new()
    {
        Id = c.Id,
        CouponCode = c.CouponCode,
        Description = c.Description,
        Discount = c.DiscountPct.ToString("0.##") + "% Off",
        DiscountPct = c.DiscountPct
    };

    private string BuildIneligibleReason(
    CouponCondition? condition, CouponCheckContext ctx)
    {
        if (condition == null) return "Condition not met.";

        if (condition.ConditionType == null) return "Condition not met.";

        return condition.ConditionType.ContextKey switch
        {
            "cart_total" =>
                $"Add ${decimal.Parse(condition.Value) - ctx.CartTotal:0} more to unlock.",

            "user_booking_count" =>
                condition.Operator == "eq" && condition.Value == "0"
                    ? "Only for first-time users."
                    : $"Requires {condition.Value} bookings.",

            "user_coupon_uses" =>
                "You have already used this coupon the maximum number of times.",

            "slot_day_of_week" =>
                condition.Operator == "in"
                    ? FormatDaysList(condition.Value)
                    : $"Valid on {FormatDay(condition.Value)}.",

            "slot_time" =>
                condition.Operator == "between"
                    ? FormatTimeRange(condition.Value)
                    : $"Valid at {FormatTime(condition.Value)}.",

            "slot_date" =>
                condition.Operator == "between"
                ? BuildDateRangeReason(condition.Value)
                : $"Only valid on {DateTime.Parse(condition.Value):dd MMM yyyy}.",

            "service_sub_category_id" =>
                BuildSubcategoryReason(condition.Value),

            _ => $"Condition not met ({condition.ConditionType.ContextKey})."
        };
    }

    private string BuildDateRangeReason(string value)
    {
        var parts = value.Split(',', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2)
            return "Valid for selected dates only.";

        var start = DateTime.Parse(parts[0].Trim());
        var end = DateTime.Parse(parts[1].Trim());

        return $"Valid between {start:dd MMM yyyy} and {end:dd MMM yyyy}.";
    }

    private string FormatTime(string time24)
    {
        if (!TimeSpan.TryParse(time24, out var ts))
            return time24;

        var hours = ts.Hours;
        var minutes = ts.Minutes;

        var period = hours >= 12 ? "PM" : "AM";
        var hours12 = hours % 12;
        if (hours12 == 0) hours12 = 12;

        return $"{hours12:D2}:{minutes:D2} {period}";
    }

    private string FormatTimeRange(string value)
    {
        var parts = value.Split(',', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2)
            return "Valid during selected time.";

        var fromTime = FormatTime(parts[0].Trim());
        var toTime = FormatTime(parts[1].Trim());

        return $"Valid between {fromTime} and {toTime}.";
    }

    private string FormatDay(string day)
    {
        if (string.IsNullOrEmpty(day)) return day;
        return char.ToUpper(day[0]) + day.Substring(1).ToLower();
    }

    private string FormatDaysList(string value)
    {
        var days = value.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(d => FormatDay(d.Trim()))
            .ToList();

        if (days.Count == 0) return "Valid on selected days.";
        if (days.Count == 1) return $"Valid on {days[0]}s only.";
        if (days.Count == 2) return $"Valid on {days[0]}s and {days[1]}s.";

        var allButLast = string.Join(", ", days.Take(days.Count - 1));
        return $"Valid on {allButLast}, and {days.Last()}s.";
    }

    private string BuildSubcategoryReason(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Valid on selected services only.";

        var ids = value
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => int.Parse(x.Trim()))
            .ToList();

        var names = _context.SubCategories
            .Where(sc => ids.Contains(sc.Id))
            .Select(sc => sc.Name)
            .ToList();

        if (!names.Any())
            return "Valid on selected services only.";

        if (names.Count == 1)
            return $"Valid only for {names[0]} services.";

        if (names.Count == 2)
            return $"Valid only for {names[0]} and {names[1]} services.";

        var allButLast = string.Join(", ", names.Take(names.Count - 1));

        return $"Valid only for {allButLast}, and {names.Last()} services.";
    }
}