using Homecare.Application.Constants;
using Homecare.Application.Constants.Caching;
using Homecare.Application.Constants.PartnerDashboard;
using Homecare.Application.DTOs.PartnerDashboard;
using Homecare.Application.Interfaces.PartnerDashboard;
using Homecare.Data;
using Homecare.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Homecare.Application.Services.PartnerDashboard;

public class PartnerRevenueChartService : IPartnerRevenueChartService
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;

    public PartnerRevenueChartService(AppDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<ApiResponse<PartnerRevenueChartResponseDto>> GetRevenueChartAsync(
        int partnerId,
        GetPartnerChartRequest request)
    {
        var cacheKey = CacheKeys.PartnerRevenueChart(partnerId, request.Period, request.Week);

        if (_cache.TryGetValue(cacheKey, out PartnerRevenueChartResponseDto? cached) && cached != null)
        {
            return ApiResponse<PartnerRevenueChartResponseDto>
                .SuccessResponse(PartnerDashboardMessages.RevenueFetchedSuccess, cached);
        }

        var baseQuery = _context.Bookings
            .Where(b =>
                b.PartnerId == partnerId &&
                b.BookingStatus == BookingStatus.Completed &&
                b.PaymentStatus == PaymentStatus.Paid)
            .Join(
                _context.Services,
                b => b.ServiceId,
                s => s.Id,
                (b, s) => new PartnerRevenueProjectionDto
                {
                    SlotDate = b.SlotDate,
                    NetRevenue = b.TotalAmount - b.TaxAmount - (b.ServicePrice * s.CommissionPct / 100m)
                });

        List<PartnerRevenueChartDataPointDto> data = request.Period.ToLower() switch
        {
            "week" => await GetWeeklyRevenue(baseQuery, request.Week ?? "this"),
            "month" => await GetMonthlyRevenue(baseQuery),
            "year" => await GetYearlyRevenue(baseQuery),
            _ => await GetWeeklyRevenue(baseQuery, "this")
        };

        var result = new PartnerRevenueChartResponseDto
        {
            Period = request.Period,
            Data = data
        };

        var expiry = IsCurrentPeriod(request)
            ? TimeSpan.FromMinutes(3)
            : TimeSpan.FromMinutes(20);

        _cache.Set(cacheKey, result, expiry);

        return ApiResponse<PartnerRevenueChartResponseDto>
            .SuccessResponse(PartnerDashboardMessages.RevenueFetchedSuccess, result);
    }

    private async Task<List<PartnerRevenueChartDataPointDto>> GetWeeklyRevenue(
        IQueryable<PartnerRevenueProjectionDto> query,
        string week)
    {
        var startOfThisWeek = GetStartOfWeek();

        var startOfWeek = week == "last"
            ? startOfThisWeek.AddDays(-7)
            : startOfThisWeek;

        var start = DateOnly.FromDateTime(startOfWeek);
        var end = DateOnly.FromDateTime(startOfWeek.AddDays(7));

        var dbData = await query
            .Where(x => x.SlotDate >= start && x.SlotDate < end)
            .GroupBy(x => x.SlotDate.DayOfWeek)
            .Select(g => new
            {
                Day = g.Key,
                Revenue = g.Sum(x => x.NetRevenue)
            })
            .ToListAsync();

        var orderedDays = new[]
        {
            DayOfWeek.Monday,    DayOfWeek.Tuesday, DayOfWeek.Wednesday,
            DayOfWeek.Thursday,  DayOfWeek.Friday,  DayOfWeek.Saturday,
            DayOfWeek.Sunday
        };

        return orderedDays.Select(day =>
        {
            var match = dbData.FirstOrDefault(x => x.Day == day);
            return new PartnerRevenueChartDataPointDto
            {
                Label = day.ToString().Substring(0, 3),
                Value = match?.Revenue ?? 0
            };
        }).ToList();
    }

    private async Task<List<PartnerRevenueChartDataPointDto>> GetMonthlyRevenue(
        IQueryable<PartnerRevenueProjectionDto> query)
    {
        var today = DateTime.UtcNow.Date;
        var startOfMonth = new DateTime(today.Year, today.Month, 1);

        var start = DateOnly.FromDateTime(startOfMonth);
        var end = DateOnly.FromDateTime(today);

        var dbData = await query
            .Where(x => x.SlotDate >= start && x.SlotDate <= end)
            .GroupBy(x => x.SlotDate.Day)
            .Select(g => new
            {
                Day = g.Key,
                Revenue = g.Sum(x => x.NetRevenue)
            })
            .ToListAsync();

        int totalDays = DateTime.DaysInMonth(today.Year, today.Month);

        return Enumerable.Range(1, totalDays).Select(day =>
        {
            var match = dbData.FirstOrDefault(x => x.Day == day);
            return new PartnerRevenueChartDataPointDto
            {
                Label = day.ToString(),
                Value = match?.Revenue ?? 0
            };
        }).ToList();
    }

    private async Task<List<PartnerRevenueChartDataPointDto>> GetYearlyRevenue(
        IQueryable<PartnerRevenueProjectionDto> query)
    {
        int year = DateTime.UtcNow.Year;

        var dbData = await query
            .Where(x => x.SlotDate.Year == year)
            .GroupBy(x => x.SlotDate.Month)
            .Select(g => new
            {
                Month = g.Key,
                Revenue = g.Sum(x => x.NetRevenue)
            })
            .ToListAsync();

        return Enumerable.Range(1, 12).Select(month =>
        {
            var match = dbData.FirstOrDefault(x => x.Month == month);
            return new PartnerRevenueChartDataPointDto
            {
                Label = new DateTime(year, month, 1).ToString("MMM"),
                Value = match?.Revenue ?? 0
            };
        }).ToList();
    }

    private static bool IsCurrentPeriod(GetPartnerChartRequest request) =>
        request.Period switch
        {
            "week" => request.Week is null or "this",
            _ => true
        };

    private static DateTime GetStartOfWeek()
    {
        var today = DateTime.Now.Date;
        int diff = today.DayOfWeek - DayOfWeek.Monday;
        if (diff < 0) diff += 7;
        return today.AddDays(-diff);
    }
}