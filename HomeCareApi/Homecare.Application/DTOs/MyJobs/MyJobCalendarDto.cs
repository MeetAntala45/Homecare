namespace Homecare.Application.DTOs.MyJobs;

public class MyJobCalendarDto
{
    public int BookingId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateOnly BookingDate { get; set; }
    public TimeOnly SlotTime { get; set; }
    public string BookingStatus { get; set; } = string.Empty;
}