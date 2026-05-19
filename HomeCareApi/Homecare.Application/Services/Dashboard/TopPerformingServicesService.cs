using Homecare.Application.Constants;
using Homecare.Application.Constants.Caching;
using Homecare.Application.Constants.Dashboard;
using Homecare.Application.DTOs.Dashboard;
using Homecare.Application.Interfaces.Dashboard;
using Homecare.Data;
using Homecare.Domain.Entities;
using Homecare.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Homecare.Application.Services.Dashboard;

public class TopPerformingServicesService : ITopPerformingServicesService
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;

    public TopPerformingServicesService(AppDbContext context,
    IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }
    public async Task<ApiResponse<List<TopServiceResponseDto>>> GetTopServicesBooking(GetChartRequest request)
    {
        var cacheKey = CacheKeys.TopServices(
           request.Period,
           request.Week
       );

        if (_cache.TryGetValue(cacheKey, out List<TopServiceResponseDto>? cached) && cached != null)
        {
            return ApiResponse<List<TopServiceResponseDto>>
                .SuccessResponse(DashboardMessages.TopServicesFetchedSuccess, cached);
        }

        var baseQuery = _context.Bookings
            .Where(b =>
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
            from st in _context.ServiceTypes
            where !st.IsDeleted

            join c in _context.Categories on st.Id equals c.ServiceTypeId into cJoin
            from c in cJoin.DefaultIfEmpty()

            join sc in _context.SubCategories on c.Id equals sc.CategoryId into scJoin
            from sc in scJoin.DefaultIfEmpty()

            join s in _context.Services on sc.Id equals s.SubCategoryId into sJoin
            from s in sJoin.DefaultIfEmpty()

            join b in baseQuery on s.Id equals b.ServiceId into bJoin

            select new TopServiceResponseDto
            {
                ServiceTypeId = st.Id,
                Name = st.Name,
                BookingCount = bJoin.Count()
            }
        )
        .GroupBy(x => new { x.ServiceTypeId, x.Name })
        .Select(g => new TopServiceResponseDto
        {
            ServiceTypeId = g.Key.ServiceTypeId,
            Name = g.Key.Name,
            BookingCount = g.Sum(x => x.BookingCount)
        })
        .Where(x => x.BookingCount > 0)
        .OrderByDescending(x => x.BookingCount)
        .ToListAsync();

        var expiry = IsCurrentPeriod(request)
            ? TimeSpan.FromMinutes(3)
            : TimeSpan.FromMinutes(20);

        _cache.Set(cacheKey, result, expiry);

        return ApiResponse<List<TopServiceResponseDto>>.SuccessResponse(DashboardMessages.TopServicesFetchedSuccess, result);
    }

    public bool IsCurrentPeriod(GetChartRequest request)
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



