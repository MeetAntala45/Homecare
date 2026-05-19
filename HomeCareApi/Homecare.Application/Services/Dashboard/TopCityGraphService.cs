using Homecare.Application.Constants;
using Homecare.Application.Constants.Caching;
using Homecare.Application.Constants.Dashboard;
using Homecare.Application.DTOs.Dashboard;
using Homecare.Application.Interfaces.Dashboard;
using Homecare.Data;
using Homecare.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Homecare.Application.Services.Dashboard;

public class TopCityGraphService : ITopCityGraphService
{

    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;

    public TopCityGraphService(AppDbContext context,
    IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }
    public async Task<ApiResponse<List<TopCityDto>>> GetTopCitiesAsync(GetChartRequest request)
    {
        var cacheKey = CacheKeys.TopCities(
            request.Period,
            request.Week
        );

        if (_cache.TryGetValue(cacheKey, out List<TopCityDto>? cached) && cached != null)
        {
            return ApiResponse<List<TopCityDto>>
                .SuccessResponse(DashboardMessages.TopCitiesFetchedSuccess, cached);
        }

        var baseQuery = from b in _context.Bookings.Where(b =>
                   b.BookingStatus == BookingStatus.Completed &&
                   b.PaymentStatus == PaymentStatus.Paid)
                        join a in _context.Addresses
                            on b.AddressId equals a.Id
                        select new BookingCityDataDto
                        {
                            City = a.City,
                            SlotDate = b.SlotDate
                        };

        List<TopCityDto> data;

        switch (request.Period.ToLower())
        {
            case "week":
                data = await GetWeeklyCityData(baseQuery, request.Week ?? "this");
                break;

            case "month":
                data = await GetMonthlyCityData(baseQuery);
                break;

            case "year":
                data = await GetYearlyCityData(baseQuery);
                break;

            default:
                data = await GetWeeklyCityData(baseQuery, "this");
                break;
        }

        var expiry = IsCurrentPeriod(request)
            ? TimeSpan.FromMinutes(3)
            : TimeSpan.FromMinutes(20);

        _cache.Set(cacheKey, data, expiry);

        return new ApiResponse<List<TopCityDto>>
        {
            Success = true,
            Data = data,
            Message = DashboardMessages.TopCitiesFetchedSuccess
        };
    }

    public bool IsCurrentPeriod(GetChartRequest request)
    {
        return request.Period switch
        {
            "week" => request.Week == "this" || request.Week == null,
            _ => true
        };
    }

    private async Task<List<TopCityDto>> GetWeeklyCityData(
    IQueryable<BookingCityDataDto> query,
    string week)
    {
        var startOfThisWeek = GetStartOfWeek();

        var startOfWeek = week == "last"
            ? startOfThisWeek.AddDays(-7)
            : startOfThisWeek;

        var endOfWeek = startOfWeek.AddDays(7);

        var start = DateOnly.FromDateTime(startOfWeek);
        var end = DateOnly.FromDateTime(endOfWeek);

        var filtered = query
            .Where(x => x.SlotDate >= start && x.SlotDate < end);

        var allCityData = await filtered
            .GroupBy(x => new { x.City, Day = x.SlotDate.DayOfWeek })
            .Select(g => new
            {
                g.Key.City,
                g.Key.Day,
                Count = g.Count()
            })
            .ToListAsync();

        var topCities = allCityData
            .GroupBy(x => x.City)
            .Select(g => new { City = g.Key, Count = g.Sum(x => x.Count) })
            .OrderByDescending(x => x.Count)
            .Take(2)
            .Select(x => x.City)
            .ToList();

        var orderedDays = new[]
        {
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
        DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
    };

        return topCities.Select(city => new TopCityDto
        {
            City = city,
            Data = orderedDays
                .Select(day =>
                    allCityData.FirstOrDefault(x => x.City == city && x.Day == day)?.Count ?? 0
                )
                .ToList()
        }).ToList();
    }

    private async Task<List<TopCityDto>> GetMonthlyCityData(
    IQueryable<BookingCityDataDto> query)
    {
        var today = DateTime.UtcNow.Date;
        var startOfMonth = new DateTime(today.Year, today.Month, 1);

        var start = DateOnly.FromDateTime(startOfMonth);
        var end = DateOnly.FromDateTime(today);

        var filtered = query
            .Where(x => x.SlotDate >= start && x.SlotDate <= end);

        var allCityData = await filtered
            .GroupBy(x => new { x.City, Date = x.SlotDate.Day })
            .Select(g => new
            {
                g.Key.City,
                g.Key.Date,
                Count = g.Count()
            })
            .ToListAsync();

        var topCities = allCityData
            .GroupBy(x => x.City)
            .Select(g => new { City = g.Key, Count = g.Sum(x => x.Count) })
            .OrderByDescending(x => x.Count)
            .Take(2)
            .Select(x => x.City)
            .ToList();

        int totalDays = DateTime.DaysInMonth(today.Year, today.Month);

        return topCities.Select(city => new TopCityDto
        {
            City = city,
            Data = Enumerable.Range(1, totalDays)
                .Select(day =>
                {
                    var date = new DateTime(today.Year, today.Month, day);
                    return allCityData.FirstOrDefault(x => x.City == city && x.Date == day)?.Count ?? 0;
                })
                .ToList()
        }).ToList();
    }

    private async Task<List<TopCityDto>> GetYearlyCityData(
    IQueryable<BookingCityDataDto> query)
    {
        int year = DateTime.UtcNow.Year;

        var start = new DateOnly(year, 1, 1);
        var end = new DateOnly(year + 1, 1, 1);

        var filtered = query
            .Where(x => x.SlotDate >= start && x.SlotDate < end);

        var allCityData = await filtered
            .GroupBy(x => new { x.City, Month = x.SlotDate.Month })
            .Select(g => new
            {
                g.Key.City,
                g.Key.Month,
                Count = g.Count()
            })
            .ToListAsync();

        var topCities = allCityData
            .GroupBy(x => x.City)
            .Select(g => new { City = g.Key, Count = g.Sum(x => x.Count) })
            .OrderByDescending(x => x.Count)
            .Take(2)
            .Select(x => x.City)
            .ToList();

        return topCities.Select(city => new TopCityDto
        {
            City = city,
            Data = Enumerable.Range(1, 12)
                .Select(month =>
                    allCityData.FirstOrDefault(x => x.City == city && x.Month == month)?.Count ?? 0
                )
                .ToList()
        }).ToList();
    }

    private DateTime GetStartOfWeek()
    {
        var today = DateTime.Now.Date;

        int diff = today.DayOfWeek - DayOfWeek.Monday;
        if (diff < 0) diff += 7;

        return today.AddDays(-diff);
    }

}

