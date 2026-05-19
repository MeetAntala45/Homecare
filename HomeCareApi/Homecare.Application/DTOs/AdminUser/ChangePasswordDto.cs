using System.ComponentModel.DataAnnotations;

namespace Homecare.Application.DTOs.AdminUser;

public class ChangePasswordDto
{
    public int Id { get; set; }

    [RegularExpression(
            @"^(?=.*[a-zA-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,15}$",
            ErrorMessage = "Password must be 8-15 characters with letter, number and special character")]
    public string NewPassword { get; set; } = null!;

    [Required(ErrorMessage = "Confirm password is required")]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = null!;
}
