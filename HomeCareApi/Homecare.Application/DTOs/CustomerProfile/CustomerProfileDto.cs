namespace Homecare.Application.DTOs.CustomerProfile;

public class CustomerProfileDto
{
    public string Email { get; set; } = null!;
    public string? MobileNumber { get; set; }
    public List<CustomerAddressDto> Addresses { get; set; } = new();
    public string? ReferralCode { get; set; }
    public decimal WalletBalance { get; set; }
}