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

public class PartnerMetricsService : IPartnerMetricsService
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;

    public PartnerMetricsService(AppDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<ApiResponse<PartnerDashboardMetricsResponseDto>> GetAllMetricsAsync(int partnerId)
    {
        var cacheKey = CacheKeys.PartnerSummaryCards(partnerId);

        if (_cache.TryGetValue(cacheKey, out PartnerDashboardMetricsResponseDto? cached) && cached != null)
        {
            return ApiResponse<PartnerDashboardMetricsResponseDto>.SuccessResponse(
                PartnerDashboardMessages.MetricsFetchedSuccess, cached);
        }

        var baseQuery = _context.Bookings
            .Where(b =>
                b.PartnerId == partnerId &&
                b.BookingStatus == BookingStatus.Completed &&
                b.PaymentStatus == PaymentStatus.Paid);

        var date = GetDateRanges();

        var data = new PartnerDashboardMetricsResponseDto
        {
            TotalBookings = await GetTotalBookings(baseQuery, date),
            UniqueCustomers = await GetUniqueCustomers(baseQuery, date),
            AverageRating = await GetAverageRating(partnerId, date),
            TotalRevenue = await GetTotalRevenue(baseQuery, date)
        };

        _cache.Set(cacheKey, data, TimeSpan.FromMinutes(3));

        return ApiResponse<PartnerDashboardMetricsResponseDto>.SuccessResponse(
            PartnerDashboardMessages.MetricsFetchedSuccess, data);
    }

    private async Task<MetricCardResponseDto> GetTotalBookings(
        IQueryable<Booking> query,
        DateRange date)
    {
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

    private async Task<MetricCardResponseDto> GetUniqueCustomers(
        IQueryable<Booking> query,
        DateRange date)
    {
        var total = await query
            .Select(b => b.CustomerId)
            .Distinct()
            .CountAsync();

        var current = await query
            .Where(b => b.SlotDate >= date.SlotCurrentStart)
            .Select(b => b.CustomerId)
            .Distinct()
            .CountAsync();

        var previous = await query
            .Where(b => b.SlotDate >= date.SlotPreviousStart &&
                        b.SlotDate < date.SlotPreviousEnd)
            .Select(b => b.CustomerId)
            .Distinct()
            .CountAsync();

        return BuildMetric(total, current, previous);
    }

    private async Task<MetricCardResponseDto> GetAverageRating(
        int partnerId,
        DateRange date)
    {
        var reviewQuery = _context.Reviews
            .Where(r => r.PartnerId == partnerId && !r.IsDeleted);

        var total = await reviewQuery.AverageAsync(r => (decimal?)r.Rating) ?? 0;

        var current = await reviewQuery
            .Where(r => r.CreatedAt >= date.CurrentStart)
            .AverageAsync(r => (decimal?)r.Rating) ?? 0;

        var previous = await reviewQuery
            .Where(r => r.CreatedAt >= date.PreviousStart &&
                        r.CreatedAt < date.PreviousEnd)
            .AverageAsync(r => (decimal?)r.Rating) ?? 0;

        return BuildMetric(
            Math.Round(total, 1),
            Math.Round(current, 1),
            Math.Round(previous, 1));
    }

    private async Task<MetricCardResponseDto> GetTotalRevenue(
        IQueryable<Booking> query,
        DateRange date)
    {

        var revenueQuery = from b in query
                           join s in _context.Services on b.ServiceId equals s.Id
                           select new
                           {
                               NetRevenue = b.TotalAmount - b.TaxAmount - (b.ServicePrice * s.CommissionPct / 100m),
                               b.SlotDate
                           };

        var total = await revenueQuery.SumAsync(x => x.NetRevenue);

        var current = await revenueQuery
            .Where(x => x.SlotDate >= date.SlotCurrentStart)
            .SumAsync(x => x.NetRevenue);

        var previous = await revenueQuery
            .Where(x => x.SlotDate >= date.SlotPreviousStart &&
                        x.SlotDate < date.SlotPreviousEnd)
            .SumAsync(x => x.NetRevenue);

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