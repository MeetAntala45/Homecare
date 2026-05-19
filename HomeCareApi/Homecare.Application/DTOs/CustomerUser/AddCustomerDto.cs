using System.ComponentModel.DataAnnotations;

namespace Homecare.Application.DTOs.CustomerUser;

public class AddCustomerDto
{
    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [MaxLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Mobile number is required.")]
    [PhoneValidation] 
    public string MobileNumber { get; set; } = null!;
}
