using System.ComponentModel.DataAnnotations;

namespace Homecare.Application.DTOs.Profile
{
    public class UpdateContactDto
    {
        [Required(ErrorMessage = "Mobile number is required.")]
        [PhoneValidation]
        public string MobileNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [MaxLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required.")]
        [MaxLength(300, ErrorMessage = "Address cannot exceed 300 characters.")]
        public string Address { get; set; } = string.Empty;
    }
}