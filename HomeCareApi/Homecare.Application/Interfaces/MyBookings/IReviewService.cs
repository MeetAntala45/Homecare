using Homecare.Application.Constants;
using Homecare.Application.DTOs.MyBookings;
using Homecare.Application.DTOs.ReviewListing;

namespace Homecare.Application.Interfaces.MyBookings;

public interface IReviewService
{
    Task<ApiResponse<ReviewResponseDto>> CreateReviewAsync(CreateReviewDto dto, int customerId);
    Task<ApiResponse<ReviewResponseDto>> GetReviewByBookingIdAsync(int bookingId, int customerId);
    Task<ApiResponse<ServiceReviewSummaryDto>> GetReviewsByServiceIdAsync(int serviceId);
    public Task<ApiResponse<PartnerReviewSummaryDto>> GetPartnerReviewsAsync(int partnerId);
}
