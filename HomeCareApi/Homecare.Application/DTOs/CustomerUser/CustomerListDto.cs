namespace Homecare.Application.DTOs.CustomerUser;

public class CustomerListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? MobileNumber { get; set; }
    public int TotalBookings { get; set; }
    public int PendingBookings { get; set; }
    public string Status { get; set; } = null!;
    
}
