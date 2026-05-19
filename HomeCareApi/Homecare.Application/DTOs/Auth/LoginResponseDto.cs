using System.ComponentModel.DataAnnotations;
using Homecare.Domain.Enums;

namespace Homecare.Application.DTOs.Auth;

public class LoginResponseDto
{
    [Required]
    public string AccessToken { get; set; } = string.Empty;

    [Required]
    public string RefreshToken { get; set; } = string.Empty;

    [Required]
    public DateTime RefreshTokenExpiry { get; set; }
    public AdminRole Role { get; set; }
}
