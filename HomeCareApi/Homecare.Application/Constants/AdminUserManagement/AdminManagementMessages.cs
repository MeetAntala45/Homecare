namespace Homecare.Application.Constants.AdminUserManagement;

public static class AdminManagementMessages
{
    public const string AdminNotFound = "Admin not found";
    public const string AdminDoesNotExist = "Admin doesn't exist";
    public const string AdminAlreadyExists = "Admin already exists";
    public const string AdminAlreadyDeleted = "Admin is already deleted";
    public const string EmailAlreadyExists = "Email already exists";
    public const string AdminUpdated = "Admin updated successfully.";
    public const string AdminDeleted = "Admin deleted successfully";
    public const string AdminsFetched = "Admins Fetched";
    public const string AdminFetched = "Admin";
    public const string AdminCreated = "Admin created successfully. Email will be sent regarding password info.";
    public const string AdminRestoredWithEmail = "Admin restored. Email will be sent regarding password info.";
    public const string AdminRestored = "Admin restored successfully.";

    public const string PasswordMismatch = "Password and confirm password must match";
    public const string PasswordChanged = "Password Changed successfully. Email will be sent.";

    public const string CredentialsEmailSubject = "Your HomeCare Admin Credentials";
    public const string PasswordChangedEmailSubject = "Your HomeCare Admin Password Has Been Changed";
}