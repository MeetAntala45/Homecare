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

public class RevenueChartService : IRevenueChartService
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;

    public RevenueChartService(AppDbContext context,
    IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<ApiResponse<RevenueChartResponseDto>> GetRevenueChartAsync(GetChartRequest request)
    {
        var cacheKey = CacheKeys.RevenueChart(
            request.Period,
            request.Week
        );

        if (_cache.TryGetValue(cacheKey, out RevenueChartResponseDto? cached) && cached != null)
        {
            return ApiResponse<RevenueChartResponseDto>
                .SuccessResponse(DashboardMessages.RevenueFetchedSuccess, cached);
        }
        var baseQuery = _context.Bookings
            .Where(b =>
                b.BookingStatus == BookingStatus.Completed &&
                b.PaymentStatus == PaymentStatus.Paid);

        List<RevenueChartDataPointDto> data;

        switch (request.Period.ToLower())
        {
            case "week":
                data = await GetWeeklyRevenue(baseQuery, request.Week ?? "this");
                break;
            case "month":
                data = await GetMonthlyRevenue(baseQuery);
                break;
            case "year":
                data = await GetYearlyRevenue(baseQuery);
                break;
            default:
                data = await GetWeeklyRevenue(baseQuery, "this");
                break;
        }

        var expiry = IsCurrentPeriod(request)
           ? TimeSpan.FromMinutes(3)
           : TimeSpan.FromMinutes(20);

        _cache.Set(cacheKey, data, expiry);

        return ApiResponse<RevenueChartResponseDto>.SuccessResponse(DashboardMessages.RevenueFetchedSuccess,
            new RevenueChartResponseDto
            {
                Period = request.Period,
                Data = data
            });
    }

    public bool IsCurrentPeriod(GetChartRequest request)
    {
        return request.Period switch
        {
            "week" => request.Week == "this" || request.Week == null,
            _ => true
        };
    }

    private async Task<List<RevenueChartDataPointDto>> GetWeeklyRevenue(
    IQueryable<Booking> query,
    string week)
    {
        var startOfThisWeek = GetStartOfWeek();

        var startOfWeek = week == "last"
            ? startOfThisWeek.AddDays(-7)
            : startOfThisWeek;
        var endOfWeek = startOfWeek.AddDays(7);

        var start = DateOnly.FromDateTime(startOfWeek);
        var end = DateOnly.FromDateTime(endOfWeek);

        var dbData = await query
            .Where(b => b.SlotDate >= start && b.SlotDate < end)
            .GroupBy(b => b.SlotDate.DayOfWeek)
            .Select(g => new
            {
                Day = g.Key,
                Revenue = g.Sum(x => x.TotalAmount)
            })
            .ToListAsync();

        var orderedDays = new[]
        {
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
            DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
        };

        return orderedDays.Select(day =>
        {
            var match = dbData.FirstOrDefault(x => x.Day == day);
            return new RevenueChartDataPointDto
            {
                Label = day.ToString().Substring(0, 3),
                Value = match?.Revenue ?? 0
            };
        }).ToList();
    }

    private async Task<List<RevenueChartDataPointDto>> GetMonthlyRevenue(
     IQueryable<Booking> query)
    {
        var today = DateTime.UtcNow.Date;

        var startOfMonth = new DateTime(today.Year, today.Month, 1);

        var start = DateOnly.FromDateTime(startOfMonth);
        var end = DateOnly.FromDateTime(today);

        var dbData = await query
           .Where(b => b.SlotDate >= start &&
                       b.SlotDate <= end)
           .GroupBy(b => b.SlotDate.Day)
           .Select(g => new
           {
               Date = g.Key,
               Revenue = g.Sum(x => x.TotalAmount)
           })
           .ToListAsync();

        int totalDays = DateTime.DaysInMonth(today.Year, today.Month);
        var allDates = Enumerable.Range(1, totalDays)
            .Select(day => new DateTime(today.Year, today.Month, day));

        return allDates.Select(date =>
        {
            var match = dbData.FirstOrDefault(x => x.Date == date.Day);

            return new RevenueChartDataPointDto
            {
                Label = date.Day.ToString(),
                Value = match?.Revenue ?? 0
            };
        }).ToList();
    }

    private async Task<List<RevenueChartDataPointDto>> GetYearlyRevenue(
        IQueryable<Booking> query)
    {
        int year = DateTime.UtcNow.Year;

        var dbData = await query
            .Where(b => b.SlotDate.Year == year)
            .GroupBy(b => b.SlotDate.Month)
            .Select(g => new
            {
                Month = g.Key,
                Revenue = g.Sum(x => x.TotalAmount)
            })
            .ToListAsync();

        return Enumerable.Range(1, 12)
            .Select(month =>
            {
                var match = dbData.FirstOrDefault(x => x.Month == month);

                return new RevenueChartDataPointDto
                {
                    Label = new DateTime(year, month, 1).ToString("MMM"),
                    Value = match?.Revenue ?? 0
                };
            }).ToList();
    }

    private DateTime GetStartOfWeek()
    {
        var today = DateTime.Now.Date;

        int diff = today.DayOfWeek - DayOfWeek.Monday;

        if (diff < 0)
            diff += 7;

        return today.AddDays(-diff);
    }

}

