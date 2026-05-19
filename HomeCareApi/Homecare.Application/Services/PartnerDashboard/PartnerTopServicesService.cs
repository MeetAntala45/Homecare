using Homecare.Application.Constants;
using Homecare.Application.Constants.Caching;
using Homecare.Application.Constants.PartnerDashboard;
using Homecare.Application.DTOs.PartnerDashboard;
using Homecare.Application.Interfaces.PartnerDashboard;
using Homecare.Data;
using Homecare.Domain.Entities;
using Homecare.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Homecare.Application.Services.PartnerDashboard;

public class PartnerTopServicesService : IPartnerTopServicesService
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;

    public PartnerTopServicesService(AppDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<ApiResponse<List<PartnerTopServiceResponseDto>>> GetTopServicesAsync(int partnerId, GetPartnerChartRequest request)
    {
        var cacheKey = CacheKeys.PartnerTopServices(partnerId, request.Period, request.Week);

        if (_cache.TryGetValue(cacheKey, out List<PartnerTopServiceResponseDto>? cached) && cached != null)
        {
            return ApiResponse<List<PartnerTopServiceResponseDto>>
                .SuccessResponse(PartnerDashboardMessages.TopServicesFetchedSuccess, cached);
        }

        var baseQuery = _context.Bookings
            .Where(b =>
                b.PartnerId == partnerId &&
                b.BookingStatus == BookingStatus.Completed &&
                b.PaymentStatus == PaymentStatus.Paid);

        baseQuery = request.Period.ToLower() switch
        {
            "week" => ApplyWeekFilter(baseQuery, request.Week ?? "this"),
            "month" => ApplyMonthFilter(baseQuery),
            "year" => ApplyYearFilter(baseQuery),
            _ => ApplyWeekFilter(baseQuery, "this")
        };

        var result = await (
            from b in baseQuery

            join s in _context.Services
                on b.ServiceId equals s.Id
            where !s.IsDeleted

            group b by new { s.Id, s.Name } into g

            select new PartnerTopServiceResponseDto
            {
                ServiceId = g.Key.Id,
                Name = g.Key.Name,
                BookingCount = g.Count()
            }
        )
        .OrderByDescending(x => x.BookingCount)
        .ToListAsync();

        var expiry = IsCurrentPeriod(request)
            ? TimeSpan.FromMinutes(3)
            : TimeSpan.FromMinutes(20);

        _cache.Set(cacheKey, result, expiry);

        return ApiResponse<List<PartnerTopServiceResponseDto>>.SuccessResponse(
            PartnerDashboardMessages.TopServicesFetchedSuccess, result);
    }

    public bool IsCurrentPeriod(GetPartnerChartRequest request)
    {
        return request.Period switch
        {
            "week" => request.Week == "this" || request.Week == null,
            _ => true
        };
    }

    private IQueryable<Booking> ApplyWeekFilter(IQueryable<Booking> query, string week)
    {
        var startOfThisWeek = GetStartOfWeek();

        var startOfWeek = week == "last"
            ? startOfThisWeek.AddDays(-7)
            : startOfThisWeek;
        var endOfWeek = startOfWeek.AddDays(7);

        var start = DateOnly.FromDateTime(startOfWeek);
        var end = DateOnly.FromDateTime(endOfWeek);

        return query.Where(b => b.SlotDate >= start && b.SlotDate < end);
    }

    private IQueryable<Booking> ApplyMonthFilter(IQueryable<Booking> query)
    {
        var today = DateTime.Now.Date;
        var startOfMonth = new DateTime(today.Year, today.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1);

        var start = DateOnly.FromDateTime(startOfMonth);
        var end = DateOnly.FromDateTime(endOfMonth);

        return query.Where(b => b.SlotDate >= start && b.SlotDate < end);
    }

    private IQueryable<Booking> ApplyYearFilter(IQueryable<Booking> query)
    {
        int year = DateTime.Now.Year;

        var start = new DateOnly(year, 1, 1);
        var end = new DateOnly(year + 1, 1, 1);

        return query.Where(b => b.SlotDate >= start && b.SlotDate < end);
    }

    private DateTime GetStartOfWeek()
    {
        var today = DateTime.Now.Date;
        int diff = today.DayOfWeek - DayOfWeek.Monday;
        if (diff < 0) diff += 7;
        return today.AddDays(-diff);
    }
}