using System.ComponentModel.DataAnnotations;

namespace Homecare.Application.DTOs.CustomerProfile;

public class RequestEmailChangeDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string NewEmail { get; set; } = null!;
}