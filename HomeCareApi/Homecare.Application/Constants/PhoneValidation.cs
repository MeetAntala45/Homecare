using System.ComponentModel.DataAnnotations;

public class PhoneValidationAttribute : RegularExpressionAttribute
{

    public PhoneValidationAttribute()
        : base(ValidationConstants.PhoneRegex)
    {
        ErrorMessage = "Enter a valid 10 digit mobile number";
    }
}

public static class ValidationConstants
{
    public const string PhoneRegex = @"^[1-9]\d{9}$";
}