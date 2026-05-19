using Homecare.Application.DTOs.MyJobs;
using Homecare.Application.Constants.Pagination;

namespace Homecare.Application.Interfaces.MyJobs;

public interface IMyJobsService
{
    Task<PaymentPagedResult<MyJobsDto>> GetBookingsByPartnerIdAsync(MyJobRequestDto req, int userId);
    Task<List<MyJobCalendarDto>> GetCalendarJobsAsync(MyJobCalendarRequestDto req, int userId);
    Task<int> GetBookingPageAsync(int partnerId, int bookingId, int pageSize, string status);
}
