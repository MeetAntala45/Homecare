namespace Homecare.Application.Constants.AdminProfile;

public static class AdminProfileMessages
{
    public const string AdminNotFound = "Admin not found.";
    public const string Unauthorized = "Unauthorized.";
    public const string InvalidRequest = "Invalid request.";

    public const string ProfileFetched = "Profile fetched successfully.";

    public const string EmailAlreadyInUse = "This email address is already in use.";
    public const string ContactUpdated = "Contact information updated successfully.";

    public const string CurrentPasswordIncorrect = "Current password is incorrect.";
    public const string PasswordChanged = "Password changed successfully.";

    public const string NoFileProvided = "No file provided.";
    public const string InvalidFileType = "Only JPG, PNG, or WEBP images are allowed.";
    public const string FileTooLarge = "Image size must not exceed 2MB.";
    public const string PhotoUploaded = "Photo uploaded successfully.";
    
    public static readonly string[] AllowedPhotoTypes =
    ["image/jpeg", "image/png", "image/jpg", "image/webp"];
}

