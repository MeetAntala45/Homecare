using System;

namespace Homecare.Application.DTOs.MyBookings;

public class ReviewResponseDto
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public byte Rating { get; set; }
    public string? ReviewText { get; set; }
    public DateTime CreatedAt { get; set; }
}
