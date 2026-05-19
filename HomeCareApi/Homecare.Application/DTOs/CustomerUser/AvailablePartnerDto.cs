
namespace Homecare.Application.DTOs.CustomerUser;

public class AvailablePartnerDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string? ProfileImage { get; set; }
    public string? MobileNumber { get; set; }
    public bool IsCurrentlyAssigned { get; set; }
}