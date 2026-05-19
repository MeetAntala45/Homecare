using System.ComponentModel.DataAnnotations;

namespace Homecare.Application.DTOs.CustomerProfile;

public class UpdateMobileDto
{
    [Required(ErrorMessage = "Mobile number is required.")]
    [PhoneValidation]
    public string MobileNumber { get; set; } = null!;
}