using Homecare.Application.Constants;
using Homecare.Application.DTOs.MyBookings;
using Homecare.Application.DTOs.ReviewListing;
using Homecare.Application.Interfaces;
using Homecare.Application.Interfaces.MyBookings;
using Homecare.Data;
using Homecare.Domain.Entities;
using Homecare.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Homecare.Application.Services.MyBookings;

public class ReviewService : IReviewService
{
    private readonly AppDbContext _context;

    public ReviewService(AppDbContext context)
    {
        _context = context;
    }
    public async Task<ApiResponse<ReviewResponseDto>> CreateReviewAsync(CreateReviewDto dto, int customerId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Review)
            .FirstOrDefaultAsync(b => b.Id == dto.BookingId && b.CustomerId == customerId);

        if (booking == null)
            return ApiResponse<ReviewResponseDto>.Fail("Booking not found.");

        if (booking.BookingStatus != BookingStatus.Completed)
            return ApiResponse<ReviewResponseDto>.Fail("Reviews can only be submitted for completed bookings.");

        if (booking.PartnerId is null)
            return ApiResponse<ReviewResponseDto>.Fail("No service partner assigned to this booking.");

        if (booking.Review is not null)
            return ApiResponse<ReviewResponseDto>.Fail("A review has already been submitted for this booking.");

        if (dto.Rating < 1 || dto.Rating > 5)
            return ApiResponse<ReviewResponseDto>.Fail("Rating must be between 1 and 5.");

        var review = new Review
        {
            BookingId = dto.BookingId,
            PartnerId = booking.PartnerId!.Value,
            CustomerId = customerId,
            Rating = dto.Rating,
            ReviewText = dto.ReviewText,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = customerId
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        return ApiResponse<ReviewResponseDto>.SuccessResponse("Review Created successfully", MapToDto(review));
    }

    public async Task<ApiResponse<ReviewResponseDto>> GetReviewByBookingIdAsync(int bookingId, int customerId)
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.BookingId == bookingId && r.CustomerId == customerId);

        if (review != null)
        {
            return ApiResponse<ReviewResponseDto>.SuccessResponse("Review fetched successfully", MapToDto(review));
        }

        return ApiResponse<ReviewResponseDto>.Fail("Review doesn't exist");
    }

    private static ReviewResponseDto MapToDto(Review review) => new()
    {
        Id = review.Id,
        BookingId = review.BookingId,
        Rating = review.Rating,
        ReviewText = review.ReviewText,
        CreatedAt = review.CreatedAt
    };

    public async Task<ApiResponse<ServiceReviewSummaryDto>> GetReviewsByServiceIdAsync(int serviceId)
    {
        var reviews = await _context.Reviews
            .Include(r => r.Booking)
            .Include(r => r.Customer)
            .Where(r => r.Booking.ServiceId == serviceId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        if (!reviews.Any())
            return ApiResponse<ServiceReviewSummaryDto>.SuccessResponse(
                "No reviews yet",
                new ServiceReviewSummaryDto()
            );

        var summary = new ServiceReviewSummaryDto
        {
            AverageRating = Math.Round((decimal)reviews.Average(r => r.Rating), 1),
            TotalReviews = reviews.Count,
            Reviews = reviews.Select(r => new ServiceReviewItemDto
            {
                CustomerName = r.Customer.Name,
                CustomerProfileImage = null,
                Rating = r.Rating,
                ReviewText = r.ReviewText,
                CreatedAt = r.CreatedAt
            }).ToList()
        };

        return ApiResponse<ServiceReviewSummaryDto>.SuccessResponse("Reviews fetched", summary);
    }

    public async Task<ApiResponse<PartnerReviewSummaryDto>> GetPartnerReviewsAsync(int partnerId)
    {
        var reviews = await _context.Reviews
            .Include(r => r.Customer)
            .Include(r => r.Booking)
                .ThenInclude(b => b.Service)
            .Where(r => r.PartnerId == partnerId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        if (!reviews.Any())
            return ApiResponse<PartnerReviewSummaryDto>.SuccessResponse("No reviews yet",
                new PartnerReviewSummaryDto());

        var breakdown = new int[5];
        foreach (var r in reviews) breakdown[r.Rating - 1]++;

        var summary = new PartnerReviewSummaryDto
        {
            AverageRating = Math.Round((decimal)reviews.Average(r => r.Rating), 1),
            TotalReviews = reviews.Count,
            RatingBreakdown = breakdown,
            Reviews = reviews.Select(r => new PartnerReviewItemDto
            {
                CustomerName = r.Customer.Name,
                Rating = r.Rating,
                ReviewText = r.ReviewText,
                CustomerProfileImage = null,
                CreatedAt = r.CreatedAt,
                ServiceName = r.Booking.Service.Name
            }).ToList()
        };

        return ApiResponse<PartnerReviewSummaryDto>.SuccessResponse("Reviews fetched", summary);
    }
}
