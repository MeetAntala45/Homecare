// using Homecare.Application.Constants;
// using Homecare.Application.DTOs.Tracking;
// using Homecare.Application.Hubs;
// using Homecare.Application.Interfaces.Tracking;
// using Homecare.Data;
// using Homecare.Domain.Entities;
// using Homecare.Domain.Enums;
// using Microsoft.AspNetCore.SignalR;
// using Microsoft.EntityFrameworkCore;

// namespace Homecare.Application.Services.Tracking;

// public class TrackingService : ITrackingService
// {
//     private readonly AppDbContext _context;
//     private readonly IHubContext<BookingHub> _hub;

//     public TrackingService(AppDbContext context, IHubContext<BookingHub> hub)
//     {
//         _context = context;
//         _hub = hub;
//     }

//     public async Task<ApiResponse<string>> UpdateLocationAsync(UpdateLocationDto dto, int partnerId)
//     {
//         var booking = await _context.Bookings
//             .FirstOrDefaultAsync(b =>
//                 b.Id == dto.BookingId &&
//                 b.PartnerId == partnerId &&
//                 (b.BookingStatus == BookingStatus.Pending ||
//                  b.BookingStatus == BookingStatus.InProgress));

//         if (booking is null)
//             return ApiResponse<string>.Fail("Booking not found or not eligible for tracking.");

//         var existing = await _context.PartnerLocations
//             .FirstOrDefaultAsync(p => p.PartnerId == partnerId && p.BookingId == dto.BookingId);

//         if (existing is null)
//         {
//             _context.PartnerLocations.Add(new PartnerLocation
//             {
//                 PartnerId = partnerId,
//                 BookingId = dto.BookingId,
//                 Latitude = dto.Latitude,
//                 Longitude = dto.Longitude,
//                 UpdatedAt = DateTime.UtcNow
//             });
//         }
//         else
//         {
//             existing.Latitude = dto.Latitude;
//             existing.Longitude = dto.Longitude;
//             existing.UpdatedAt = DateTime.UtcNow;
//         }

//         await _context.SaveChangesAsync();

//         await _hub.Clients.Group($"Tracking_{dto.BookingId}")
//             .SendAsync("PartnerLocationUpdated", new
//             {
//                 bookingId = dto.BookingId,
//                 latitude = dto.Latitude,
//                 longitude = dto.Longitude,
//                 updatedAt = DateTime.UtcNow
//             });

//         return ApiResponse<string>.SuccessResponse("Location updated.");
//     }

//     public async Task<ApiResponse<LocationResponseDto>> GetLastLocationAsync(int bookingId, int customerId)
//     {

//         var booking = await _context.Bookings
//             .FirstOrDefaultAsync(b => b.Id == bookingId && b.CustomerId == customerId);

//         if (booking is null)
//             return ApiResponse<LocationResponseDto>.Fail("Booking not found.");

//         var location = await _context.PartnerLocations
//             .FirstOrDefaultAsync(p => p.BookingId == bookingId);

//         if (location is null)
//             return ApiResponse<LocationResponseDto>.Fail("Partner location not available yet.");

//         return ApiResponse<LocationResponseDto>.SuccessResponse("Location fetched.", new LocationResponseDto
//         {
//             Latitude = location.Latitude,
//             Longitude = location.Longitude,
//             UpdatedAt = location.UpdatedAt
//         });
//     }

// }
