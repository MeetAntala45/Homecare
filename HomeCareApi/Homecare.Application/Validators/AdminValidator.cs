using FluentValidation;
using Homecare.Application.DTOs.AdminUser;

namespace Homecare.Application.Validators;

public class AdminValidator : AbstractValidator<AdminDto>
{
    public AdminValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid admin ID")
            .When(x => x.Id.HasValue);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .Matches(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&]).{8,15}$")
            .WithMessage("Password must be 8-15 characters with letter, number and special character")
            .When(x => !x.Id.HasValue);

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required")
            .Equal(x => x.Password).WithMessage("Passwords do not match")
            .When(x => !x.Id.HasValue);

        RuleFor(x => x.MobileNumber)
            .Matches(@"^[1-9]\d{9}$")
            .WithMessage("Mobile number must be 10 digits)")
            .When(x => !string.IsNullOrWhiteSpace(x.MobileNumber));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format")
            .When(x => x.Id.HasValue && !string.IsNullOrWhiteSpace(x.Email));
    }
}