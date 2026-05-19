namespace Homecare.Domain.Entities;

public class Review
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public int CustomerId { get; set; }
    public int PartnerId { get; set; }
    public byte Rating { get; set; }     
    public string? ReviewText { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public bool IsDeleted { get; set; } = false;

    public Booking Booking { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public ServicePartner Partner { get; set; } = null!;
}
