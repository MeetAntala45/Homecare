namespace Homecare.Application.DTOs.CustomerUser;
public class CustomerBookingListDto
{
    public int BookingId{get;set;}
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = "";
    public string ServiceType { get; set; } = "";
    public string AssignedExpert { get; set; } = "-";
    public string Address { get; set; } = "";
    public string DateTime { get; set; } = "";
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "";
    public string Status { get; set; } = "";
    public string? PartnerImage { get; set; }
     public int? PartnerId { get; set; }  
    public string? PartnerPhone { get; set; } 
    public bool IsPartnerDeleted { get; set; }  
}
