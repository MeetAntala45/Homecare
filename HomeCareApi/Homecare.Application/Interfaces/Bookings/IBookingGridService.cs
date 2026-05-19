using Homecare.Application.Constants;
using Homecare.Application.DTOs.BookingManagement;
using Homecare.Application.DTOs.Bookings;

namespace Homecare.Application.Interfaces.Bookings;

public interface IBookingGridService
{
    Task<ApiResponse<CustomerBookingSummaryPagedDto>> GetCustomerBookingSummariesAsync(
        BookingGridRequestDto request);

    Task<ApiResponse<List<DropdownOptionDto>>> GetServiceTypesAsync();

    Task<int> GetCustomerPositionAsync(int customerId, int paymentMethod, int pageSize);
    Task<ApiResponse<bool>> DeleteCustomerBookingsAsync(int customerId, int PaymentMethodValue);

    Task<ApiResponse<CustomerBookingDetailPagedDto>> GetCustomerBookingDetailsAsync(
        CustomerBookingDetailRequestDto request);
}