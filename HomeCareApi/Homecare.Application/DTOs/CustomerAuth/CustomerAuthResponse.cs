namespace Homecare.Application.DTOs.CustomerAuth;

public class CustomerAuthResponse
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
}