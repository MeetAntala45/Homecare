using System.ComponentModel.DataAnnotations;
using Homecare.Application.Constants.Support;


namespace Homecare.Application.DTOs.Support.cs;

public class SupportCreateDto
{
    [Required(ErrorMessage = SuppportRequestConstant.FirstNameRequiredErorr)]
    [MaxLength(50, ErrorMessage = SuppportRequestConstant.FirstNameLimitError)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = SuppportRequestConstant.LastNameRequiredErorr)]
    [MaxLength(50, ErrorMessage = SuppportRequestConstant.LastNameLimitError)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = SuppportRequestConstant.MobileRequiredErorr)]
    [RegularExpression(SuppportRequestConstant.MobileRegEX, ErrorMessage = SuppportRequestConstant.MobileValidationError)]
    public string? Mobile { get; set; }

    [Required(ErrorMessage = SuppportRequestConstant.EmailRequiredErorr)]
    [MaxLength(150, ErrorMessage = SuppportRequestConstant.EmailLengthError)]
    [RegularExpression(
       @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
       ErrorMessage = SuppportRequestConstant.EmailValidationError)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = SuppportRequestConstant.DescriptionRequiredErorr)]
    [MaxLength(500, ErrorMessage = SuppportRequestConstant.DescriptionLengthError)]
    public string Description { get; set; } = string.Empty;
}
