using Homecare.Domain.Enums;

namespace Homecare.Domain.Entities;

public class Booking
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int ServiceId { get; set; }
    public int AddressId { get; set; }
    public int? PartnerId { get; set; }
    public int? CouponId { get; set; }
    public DateOnly SlotDate { get; set; }
    public TimeOnly SlotStartTime { get; set; }
    public TimeOnly SlotEndTime { get; set; }
    public decimal ServicePrice { get; set; }
    public decimal TaxPct { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public decimal TotalAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public BookingStatus BookingStatus { get; set; } = BookingStatus.Pending;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public string? CancellationReason { get; set; }


    public Customer Customer { get; set; } = null!;
    public Address Address { get; set; } = null!;
    public Coupon? Coupon { get; set; }
    public ServicePartner? Partner { get; set; }
    public Payment? Payment { get; set; }
    public Service Service { get; set; } = null!; 
    public Review? Review { get; set; }
}