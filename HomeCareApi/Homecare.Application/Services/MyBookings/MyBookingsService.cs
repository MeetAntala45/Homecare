using Homecare.Application.Constants;
using Homecare.Application.DTOs.MyBookings;
using Homecare.Application.Interfaces.MyBookings;
using Homecare.Data;
using Microsoft.EntityFrameworkCore;

namespace Homecare.Application.Services.MasterData;

public class MyBookingsService : IMyBookingsService
{
    private readonly AppDbContext _context;
    public MyBookingsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<MyBookingResponseDto>>> GetBookingsByCustomerIdAsync(int customerId)
    {
        try
        {
            var bookings = await (
            from b in _context.Bookings
            join s in _context.Services on b.ServiceId equals s.Id
            join c in _context.Addresses on b.AddressId equals c.Id
            join sp in _context.ServicePartners on b.PartnerId equals sp.Id into spGroup
            from sp in spGroup.DefaultIfEmpty()
            where b.CustomerId == customerId
            select new MyBookingResponseDto
            {
                Id = b.Id,
                CustomerId = b.CustomerId,
                ProfileImage = sp.ProfileImage,
                ServiceName = s.Name,
                PartnerName = sp != null ? sp.FullName : null,
                BookingStatus = b.BookingStatus.ToString(),
                Price = b.TotalAmount,
                CancellationReason = b.CancellationReason!,
                MobileNumber = sp != null ? sp.MobileNumber : "",
                Duration = s.DurationMin,
                HouseFlatNo = c.HouseFlatNo,
                LandMark = c.Landmark,
                Address = c.DisplayName!,
                BookingDate = b.SlotDate,
                SlotStartTime = b.SlotStartTime,
                HasReview = _context.Reviews.Any(r => r.BookingId == b.Id && !r.IsDeleted),
                Latitude = b.Address.Latitude,
                Longitude = b.Address.Longitude,
            }
        )
        .AsNoTracking()
        .ToListAsync();

            return ApiResponse<List<MyBookingResponseDto>>.SuccessResponse(
                "Bookings fetched successfully",
                bookings
            );
        }
        catch (Exception ex)
        {
            return ApiResponse<List<MyBookingResponseDto>>.Fail($"Error: {ex.Message}");
        }
    }
}