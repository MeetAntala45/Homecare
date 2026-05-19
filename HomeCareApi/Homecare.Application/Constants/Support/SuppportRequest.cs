namespace Homecare.Application.Constants.Support;

public class SuppportRequestConstant
{
    public const string FirstNameRequiredErorr = "First Name is required.";
    public const string LastNameRequiredErorr = "Last Name is required.";
    public const string MobileRequiredErorr = "Mobile Number is required.";
    public const string EmailRequiredErorr = "Email Address is required.";
    public const string DescriptionRequiredErorr = "Description is required.";
    public const string FirstNameLimitError = "First name cannot exceed 50 characters.";
    public const string LastNameLimitError = "Last name cannot exceed 50 characters.";
    public const string MobileValidationError = "Enter a valid 10 digit mobile number";
    public const string EmailValidationError =  "Enter a valid email address.";
    public const string EmailLengthError =  "Email cannot exceed 150 characters.";
    public const string DescriptionLengthError =   "Description cannot exceed 500 characters.";
    public const string MobileRegEX =   @"^[1-9]\d{9}$";
}
