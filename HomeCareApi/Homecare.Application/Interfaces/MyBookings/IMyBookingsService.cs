using Homecare.Application.Constants;
using Homecare.Application.DTOs.MyBookings;

namespace Homecare.Application.Interfaces.MyBookings;

public interface IMyBookingsService
{
    Task<ApiResponse<List<MyBookingResponseDto>>> GetBookingsByCustomerIdAsync(int customerId);
}