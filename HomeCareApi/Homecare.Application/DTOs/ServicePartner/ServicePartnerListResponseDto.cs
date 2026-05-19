namespace Homecare.Application.DTOs.ServicePartner;

public class ServicePartnerListResponseDto
{
    public int Id { get; set; }
    public string ServicePartnerId { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string MobileNumber { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? ResidentialAddress { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public int JobsDone { get; set; }
    public int StatusId { get; set; }
    public string Status { get; set; } = null!;
}