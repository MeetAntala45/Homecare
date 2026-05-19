using Homecare.Application.Constants;
using Homecare.Application.DTOs.MyBookings;
using Homecare.Application.DTOs.ReviewListing;
using Homecare.Application.Interfaces;
using Homecare.Application.Interfaces.MyBookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Homecare.API.Controllers.MyBookings
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly ICurrentUserService _currentUser;

        public ReviewController(IReviewService reviewService, ICurrentUserService currentUser)
        {
            _reviewService = reviewService;
            _currentUser = currentUser;
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<ApiResponse<ReviewResponseDto>> CreateReview([FromBody] CreateReviewDto dto)
        {
            return await _reviewService.CreateReviewAsync(dto, _currentUser.UserId);
        }

        [HttpGet("booking/{bookingId:int}")]
        [Authorize(Roles = "Customer")]
        public async Task<ApiResponse<ReviewResponseDto>> GetReviewByBookingId(int bookingId)
        {
            return await _reviewService.GetReviewByBookingIdAsync(bookingId, _currentUser.UserId);
        }

        [HttpGet("service/{serviceId:int}")]
        [Authorize(Roles = "Customer")]
        [AllowAnonymous]
        public async Task<ApiResponse<ServiceReviewSummaryDto>> GetServiceReviews(int serviceId)
        {
            return await _reviewService.GetReviewsByServiceIdAsync(serviceId);
        }

        [HttpGet("my-reviews")]
        [Authorize(Roles = "ServicePartner")]
        public async Task<ApiResponse<PartnerReviewSummaryDto>> GetMyReviews()
        {
            return await _reviewService.GetPartnerReviewsAsync(_currentUser.UserId);
        }
    }
}
