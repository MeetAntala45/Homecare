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

public class TopServicePartnersService : ITopServicePartnersService
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;

    public TopServicePartnersService(AppDbContext context,
    IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }
    public async Task<ApiResponse<List<TopServicePartnersResponseDto>>> GetTopServicePartnersAsync()
    {
        const string cacheKey = CacheKeys.TopServicePartners;

        if (_cache.TryGetValue(cacheKey, out List<TopServicePartnersResponseDto>? cached) && cached != null)
        {
            return ApiResponse<List<TopServicePartnersResponseDto>>
                .SuccessResponse(DashboardMessages.TopServicePartnersFetchedSuccess, cached);
        }

        var servicePartners = await (
            from b in _context.Bookings
            where b.BookingStatus == BookingStatus.Completed &&
            b.PaymentStatus == PaymentStatus.Paid && b.PartnerId != null
            join p in _context.ServicePartners on b.PartnerId equals p.Id
            join st in _context.ServiceTypes on p.ServiceTypeId equals st.Id

            group b by new { p.Id, p.FullName, p.ProfileImage, ServiceTypeName = st.Name } into g

            select new TopServicePartnersResponseDto
            {
                Name = g.Key.FullName,
                ProfileImage = g.Key.ProfileImage,
                ServiceTypeName = g.Key.ServiceTypeName,
                JobsCompleted = g.Count()
            }
        )
        .OrderByDescending(x => x.JobsCompleted)
        .Take(5)
        .ToListAsync();

        _cache.Set(cacheKey, servicePartners, TimeSpan.FromMinutes(10));

        return ApiResponse<List<TopServicePartnersResponseDto>>.SuccessResponse(DashboardMessages.TopServicePartnersFetchedSuccess, servicePartners);
    }
}

