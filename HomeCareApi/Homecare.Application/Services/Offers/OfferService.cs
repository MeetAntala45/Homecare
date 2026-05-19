using Homecare.Data;
using Homecare.Domain.Entities;
using Homecare.Domain.Enums;
using Homecare.Application.Interfaces.Offers;
using Homecare.Application.DTOs.Offers;
using Homecare.Application.Constants.Pagination;
using Homecare.Application.Constants.Offers;
using Microsoft.EntityFrameworkCore;
using Homecare.Application.DTOs.CouponCondition;
using Homecare.Application.Constants;

namespace Homecare.Application.Services.Offers;

public class OfferService : IOfferService
{
    private readonly AppDbContext _context;

    public OfferService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<string>> AddOffer(CreateOfferDto dto, int adminId)
    {
        if (dto == null)
            throw new Exception(OfferMessages.InvalidRequest);

        var code = dto.OfferCode.Trim().ToUpper();

        var exists = await _context.coupons.AnyAsync(x => x.CouponCode == code);

        if (exists)
            return ApiResponse<string>.Fail(OfferMessages.OfferCodeExists);

        if (dto.Conditions != null)
        {
            var conditionError = ValidateConditions(dto.Conditions);
            if (conditionError != null)
                return ApiResponse<string>.Fail(conditionError);
        }


        var offer = new Coupon
        {
            CouponCode = code,
            Description = dto.Description?.Trim(),
            DiscountPct = dto.DiscountPct,
            Status = CouponStatus.Active,
            UsageCount = 0,
            CreatedBy = adminId,
            CreatedAt = DateTime.UtcNow,
            Conditions = (dto.Conditions ?? new()).Select(c => new CouponCondition
            {
                ConditionTypeId = c.ConditionTypeId,
                Operator = c.Operator.Trim().ToLower(),
                Value = c.Value.Trim(),
                FailBehaviour = c.FailBehaviour.Trim().ToLower(),
                CreatedAt = DateTime.UtcNow
            }).ToList()
        };

        await _context.coupons.AddAsync(offer);
        await _context.SaveChangesAsync();
        return ApiResponse<string>.SuccessResponse(OfferMessages.OfferCreated);
    }

    public async Task<ApiResponse<FilterPagedResult<OfferResponseDto>>> GetAll(GetOfferListFilterDto filter)
    {
        if (filter.PageNumber <= 0) filter.PageNumber = 1;
        if (filter.PageSize <= 0) filter.PageSize = 10;

        var baseQuery = _context.coupons
            .Where(x => !x.IsDeleted)
            .Select(x => new
            {
                Coupon = x,
                TimesApplied = _context.Bookings.Count(b =>
                    b.CouponId == x.Id &&
                    b.PaymentStatus != PaymentStatus.Failed &&
                    b.BookingStatus != BookingStatus.Cancelled)
            });

        var usageRange = await baseQuery
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Min = g.Min(x => (int?)x.TimesApplied) ?? 0,
                Max = g.Max(x => (int?)x.TimesApplied) ?? 0
            })
            .FirstOrDefaultAsync();

        var query = baseQuery
            .Where(x => !filter.Status.HasValue || x.Coupon.Status == filter.Status.Value);

        if (filter.MinDiscount.HasValue)
            query = query.Where(x => x.Coupon.DiscountPct >= filter.MinDiscount.Value);

        if (filter.MaxDiscount.HasValue)
            query = query.Where(x => x.Coupon.DiscountPct <= filter.MaxDiscount.Value);

        if (filter.MinUsage.HasValue)
            query = query.Where(x => x.TimesApplied >= filter.MinUsage.Value);

        if (filter.MaxUsage.HasValue)
            query = query.Where(x => x.TimesApplied <= filter.MaxUsage.Value);

        if (!string.IsNullOrWhiteSpace(filter.CouponCode))
        {
            var normalisedCode = filter.CouponCode.Trim().ToLower();
            query = query.Where(x => x.Coupon.CouponCode.ToLower().Contains(normalisedCode));
        }

        bool isDesc = filter.SortOrder?.ToLower() != "asc";

        query = !string.IsNullOrWhiteSpace(filter.SortBy)
            ? filter.SortBy.ToLower() switch
            {
                "id" => isDesc
                    ? query.OrderByDescending(x => x.Coupon.Id)
                    : query.OrderBy(x => x.Coupon.Id),
                "offercode" => isDesc
                    ? query.OrderByDescending(x => x.Coupon.CouponCode)
                    : query.OrderBy(x => x.Coupon.CouponCode),
                "discount" => isDesc
                    ? query.OrderByDescending(x => x.Coupon.DiscountPct)
                    : query.OrderBy(x => x.Coupon.DiscountPct),
                "timesappliedformatted" => isDesc
                    ? query.OrderByDescending(x => x.TimesApplied)
                    : query.OrderBy(x => x.TimesApplied),
                _ => query.OrderByDescending(x => x.Coupon.CreatedAt)
            } : query.OrderByDescending(x => x.Coupon.CreatedAt);

        if (!string.IsNullOrWhiteSpace(filter.CouponCode))
        {
            var normalisedName = filter.CouponCode.Trim().ToLower();

            query = query.Where(x =>
                x.Coupon.CouponCode.ToLower().Contains(normalisedName)
            );
        }

        var totalCount = await query.CountAsync();

        var data = await query
            .AsNoTracking()
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(x => new OfferResponseDto
            {
                Id = x.Coupon.Id,
                OfferCode = x.Coupon.CouponCode,
                Description = x.Coupon.Description,
                Discount = x.Coupon.DiscountPct.ToString("0.##") + "% Off",
                TimesApplied = x.TimesApplied,
                Status = x.Coupon.Status,
                Conditions = x.Coupon.Conditions.Select(c => new CouponConditionResponseDto
                {
                    Id = c.Id,
                    ConditionTypeId = c.ConditionTypeId,
                    ConditionType = c.ConditionType.Label,
                    Operator = c.Operator,
                    Value = c.Value,
                    FailBehaviour = c.FailBehaviour
                }).ToList()
            })
            .ToListAsync();

        return ApiResponse<FilterPagedResult<OfferResponseDto>>.SuccessResponse(
            OfferMessages.OffersFetched,
            new FilterPagedResult<OfferResponseDto>
            {
                Data = data,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Min = usageRange?.Min ?? 0,
                Max = usageRange?.Max ?? 0
            });
    }

    public async Task<ApiResponse<string>> UpdateOffer(UpdateOfferDto dto, int adminId)
    {
        var offer = await _context.coupons
            .Include(x => x.Conditions)
            .FirstOrDefaultAsync(x => x.Id == dto.Id);

        if (offer == null)
            return ApiResponse<string>.Fail(OfferMessages.OfferNotFound);

        var code = dto.OfferCode.Trim().ToUpper();

        var duplicate = await _context.coupons
            .AnyAsync(x => x.CouponCode == code && x.Id != dto.Id);

        if (duplicate)
            return ApiResponse<string>.Fail(OfferMessages.OfferCodeExists);

        var conditionError = ValidateConditions(dto.Conditions ?? new());
        if (conditionError != null)
            return ApiResponse<string>.Fail(conditionError);

        offer.CouponCode = code;
        offer.Description = dto.Description?.Trim();
        offer.DiscountPct = dto.DiscountPct;
        offer.Status = dto.Status;
        offer.ModifiedAt = DateTime.UtcNow;
        offer.ModifiedBy = adminId;

        _context.CouponConditions.RemoveRange(offer.Conditions);
        await _context.SaveChangesAsync();

        offer.Conditions = (dto.Conditions ?? new()).Select(c => new CouponCondition
        {
            CouponId = offer.Id,
            ConditionTypeId = c.ConditionTypeId,
            Operator = c.Operator.Trim().ToLower(),
            Value = c.Value.Trim(),
            FailBehaviour = c.FailBehaviour.Trim().ToLower(),
            CreatedAt = DateTime.UtcNow
        }).ToList();

        await _context.SaveChangesAsync();
        return ApiResponse<string>.SuccessResponse(OfferMessages.OfferUpdated);
    }

    public async Task<ApiResponse<string>> DeleteOffer(int id, int adminId)
    {
        var offer = await _context.coupons.FindAsync(id);

        if (offer == null)
            return ApiResponse<string>.Fail(OfferMessages.OfferNotFound);

        offer.Status = CouponStatus.Inactive;
        offer.IsDeleted = true;
        offer.ModifiedAt = DateTime.UtcNow;
        offer.ModifiedBy = adminId;

        await _context.SaveChangesAsync();
        return ApiResponse<string>.SuccessResponse(OfferMessages.OfferDeleted);
    }

    private string? ValidateConditions(List<CreateCouponConditionDto> conditions)
    {
        if (!conditions.Any()) return null;

        var duplicateType = conditions
            .GroupBy(x => x.ConditionTypeId)
            .FirstOrDefault(g => g.Count() > 1);

        if (duplicateType != null)
            return $"Condition type '{duplicateType.Key}' is used more than once. Each condition type can only be applied once per coupon.";

        return null;
    }

}