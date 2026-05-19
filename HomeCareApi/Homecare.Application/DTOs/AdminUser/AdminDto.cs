using System.ComponentModel.DataAnnotations;

namespace Homecare.Application.DTOs.AdminUser;

public class AdminDto
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(150, ErrorMessage = "Email cannot exceed 150 characters")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Mobile number is required")]
    [PhoneValidation]
    public string MobileNumber { get; set; } = string.Empty;
    public string? Password { get; set; } = null!;
    public string? ConfirmPassword { get; set; } = null!;
}
