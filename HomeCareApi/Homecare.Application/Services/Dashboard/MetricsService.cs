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

public class MetricsService : IMetricsService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IMemoryCache _cache;

    public MetricsService(IDbContextFactory<AppDbContext> contextFactory,
    IMemoryCache cache)
    {
        _contextFactory = contextFactory;
        _cache = cache;
    }

    public async Task<ApiResponse<DashboardMetricsResponseDto>> GetAllMetricsAsync()
    {
        var cacheKey = CacheKeys.SummaryCards();

        if (_cache.TryGetValue(cacheKey, out DashboardMetricsResponseDto? cached) && cached != null)
        {
            return ApiResponse<DashboardMetricsResponseDto>
                .SuccessResponse(DashboardMessages.MetricsFetchedSuccess, cached);
        }

        var date = GetDateRanges();

        var bookingsTask = GetTotalBookingsAsync(date);
        var customersTask = GetActiveCustomersAsync(date);
        var partnersTask = GetActivePartnersAsync(date);
        var revenueTask = GetTotalRevenueAsync(date);

        await Task.WhenAll(bookingsTask, customersTask, partnersTask, revenueTask);

        var data = new DashboardMetricsResponseDto
        {
            TotalBookings = bookingsTask.Result,
            ActiveCustomers = customersTask.Result,
            ActivePartners = partnersTask.Result,
            TotalRevenue = revenueTask.Result
        };

        _cache.Set(cacheKey, data, TimeSpan.FromMinutes(3));

        return ApiResponse<DashboardMetricsResponseDto>
            .SuccessResponse(DashboardMessages.MetricsFetchedSuccess, data);
    }

    private async Task<MetricCardResponseDto> GetTotalBookingsAsync(DateRange date)
    {
        await using var context = _contextFactory.CreateDbContext();

        var query = context.Bookings
            .Where(b => b.BookingStatus == BookingStatus.Completed &&
                        b.PaymentStatus == PaymentStatus.Paid &&
                        !b.IsDeleted);

        var total = await query.CountAsync();

        var current = await query
            .Where(b => b.SlotDate >= date.SlotCurrentStart)
            .CountAsync();

        var previous = await query
            .Where(b => b.SlotDate >= date.SlotPreviousStart &&
                        b.SlotDate < date.SlotPreviousEnd)
            .CountAsync();

        return BuildMetric(total, current, previous);
    }

    private async Task<MetricCardResponseDto> GetActiveCustomersAsync(DateRange date)
    {
        await using var context = _contextFactory.CreateDbContext();

        var query = context.Customers
            .Where(c => c.Status == UserStatus.Active);

        var total = await query
            .Select(c => c.Id)
            .Distinct()
            .CountAsync();

        var current = await query
            .Where(c => c.CreatedAt >= date.CurrentStart)
            .Select(c => c.Id)
            .Distinct()
            .CountAsync();

        var previous = await query
            .Where(c => c.CreatedAt >= date.PreviousStart &&
                        c.CreatedAt < date.PreviousEnd)
            .Select(c => c.Id)
            .Distinct()
            .CountAsync();

        return BuildMetric(total, current, previous);
    }

    private async Task<MetricCardResponseDto> GetActivePartnersAsync(DateRange date)
    {
        await using var context = _contextFactory.CreateDbContext();

        var query = context.ServicePartners
            .Where(sp => !sp.IsDeleted);

        var total = await query
            .Select(sp => sp.Id)
            .Distinct()
            .CountAsync();

        var current = await query
            .Where(sp => sp.ApprovedOn >= date.CurrentStart)
            .Select(sp => sp.Id)
            .Distinct()
            .CountAsync();

        var previous = await query
            .Where(sp => sp.ApprovedOn >= date.PreviousStart &&
                         sp.ApprovedOn < date.PreviousEnd)
            .Select(sp => sp.Id)
            .Distinct()
            .CountAsync();

        return BuildMetric(total, current, previous);
    }

    private async Task<MetricCardResponseDto> GetTotalRevenueAsync(DateRange date)
    {
        await using var context = _contextFactory.CreateDbContext();

        var query = context.Bookings
            .Where(b => b.BookingStatus == BookingStatus.Completed &&
                        b.PaymentStatus == PaymentStatus.Paid &&
                        !b.IsDeleted);

        var total = await query.SumAsync(b => b.TotalAmount);

        var current = await query
            .Where(b => b.SlotDate >= date.SlotCurrentStart)
            .SumAsync(b => b.TotalAmount);

        var previous = await query
            .Where(b => b.SlotDate >= date.SlotPreviousStart &&
                        b.SlotDate < date.SlotPreviousEnd)
            .SumAsync(b => b.TotalAmount);

        return BuildMetric(total, current, previous);
    }

    private static MetricCardResponseDto BuildMetric(
        decimal total,
        decimal current,
        decimal previous)
    {
        var change = previous == 0
            ? 100
            : Math.Round((current - previous) / previous * 100, 1);

        return new MetricCardResponseDto
        {
            Value = total,
            Change = change,
            IsIncrease = current >= previous
        };
    }

    private static DateRange GetDateRanges()
    {
        var today = DateTime.UtcNow;

        return new DateRange
        {
            CurrentStart = today.AddDays(-7),
            PreviousStart = today.AddDays(-14),
            PreviousEnd = today.AddDays(-7)
        };
    }

    private class DateRange
    {
        public DateTime CurrentStart { get; set; }
        public DateTime PreviousStart { get; set; }
        public DateTime PreviousEnd { get; set; }

        public DateOnly SlotCurrentStart => DateOnly.FromDateTime(CurrentStart);
        public DateOnly SlotPreviousStart => DateOnly.FromDateTime(PreviousStart);
        public DateOnly SlotPreviousEnd => DateOnly.FromDateTime(PreviousEnd);
    }
}
