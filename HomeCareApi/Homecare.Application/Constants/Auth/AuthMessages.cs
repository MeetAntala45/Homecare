namespace Homecare.Application.Constants.Auth;

public static class AuthMessages
{
    public const string InvalidCredentials = "Invalid Credentials";
    public const string LoginSuccess = "Login successful";

    public const string InvalidRefreshToken = "Invalid refresh token";
    public const string TokenRefreshed = "Token refreshed successfully";

    public const string LogoutSuccess = "Logged out successfully";

    public const string EmailNotRegistered = "Email is not registered";
    public const string ResetLinkSent = "A Password reset link has been sent.";
    public const string ForgotPasswordEmailSubject = "Reset your Password";

    public const string PasswordMismatch = "Passwords do not match";
    public const string PasswordResetSuccess = "Password has been reset successfully";

    public const string InvalidResetLink = "Invalid reset link";
    public const string ResetLinkAlreadyUsed = "Reset link already used";
    public const string ResetLinkExpired = "Reset link expired";
    public const string ResetTokenValid = "Valid";
}