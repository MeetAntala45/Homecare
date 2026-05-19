namespace Homecare.Application.Constants.CustomerProfile;

public static class CustomerProfileMessages
{
    public const string CustomerNotFound = "Customer not found.";
    public const string InvalidData = "Invalid data.";

    public const string ProfileFetched = "Profile fetched successfully.";

    public const string MobileUpdated = "Mobile number updated successfully.";

    public const string EmailSameAsCurrent = "New email must be different from current email.";
    public const string EmailAlreadyRegistered = "This email is already registered with another account.";
    public const string EmailOtpSent = "OTP sent to your new email address.";
    public const string EmailOtpSubject = "Verify your new email - HomeCare";
    public const string EmailOtpExpiredOrInvalid = "OTP expired or invalid. Please request a new one.";
    public const string EmailOtpIncorrect = "Incorrect OTP. Please try again.";
    public const string EmailUpdated = "Email updated successfully.";

    public const string AddressNotFound = "Address not found.";
    public const string AddressAdded = "Address added successfully.";
    public const string AddressUpdated = "Address updated successfully.";
    public const string AddressDeleted = "Address deleted successfully.";
    public const string LabelsFetched = "Labels fetched successfully.";
    public const string AddressAlreadyExists = "This address is already saved.";
    public const string AddressDeletionNotPermitted = "This address cannot be deleted as it is linked to an existing booking.";



    public const string RecentSearchSaved = "Recent search saved successfully.";
    public const string RecentSearchesFetched = "Recent searches fetched successfully.";
}