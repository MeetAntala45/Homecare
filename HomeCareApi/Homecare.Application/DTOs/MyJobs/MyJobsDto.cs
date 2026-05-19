namespace Homecare.Application.DTOs.MyJobs
{
    public class MyJobsDto
    {
        public int BookingId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateOnly BookingDate { get; set; }
        public TimeOnly SlotTime { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentMethod { get; set; }
        public string BookingStatus { get; set; } = string.Empty;

    }
}