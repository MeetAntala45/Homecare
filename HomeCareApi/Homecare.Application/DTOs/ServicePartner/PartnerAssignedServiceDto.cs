namespace Homecare.Application.DTOs.ServicePartner;
public class PartnerAssignedServiceDto
{
    public int    BookingId      { get; set; }
    public int    CustomerId      { get; set; }
    public string ServiceName    { get; set; } = string.Empty;
    public string CustomerName   { get; set; } = string.Empty;
    public string DateTime       { get; set; } = string.Empty;
    public string ServiceAddress { get; set; } = string.Empty;
    public int    StatusId       { get; set; }
    public string Status         { get; set; } = string.Empty;
}