namespace Homecare.Application.Constants.CustomerAuth;

public static class CustomerAuthMessages
{
    public const string Blocked = "You are blocked by admin";
    public const string Deleted = "You are deleted by admin";
    public const string OtpSent = "OTP sent Successfully. Please check your mail";
    public const string OtpSubject = "Your OTP Code for Login Request";
    public const string InvalidOtp = "Invalid OTP";
    public const string OtpExpired = "OTP has expired. Please request a new one.";
    public const string SignUpSuccess = "Sign Up Successfull";
    public const string LoginSuccess = "Login Successfull";
    public const string TokenRefreshed = "Token refreshed successfully.";
    public const string LogoutSuccess = "Logged out successfully";
    public const string InvalidRefreshToken = "Invalid refresh token.";
    public const string RefreshTokenExpired = "Refresh token has expired. Please login again.";
    public const string RefreshTokenMissing = "Refresh token missing.";
    public const string OtpRateLimited = "Too many OTP requests. Please try again after 30 minutes.";

    public static string OtpCooldown(int seconds) =>
        $"Please wait {seconds} seconds before requesting a new OTP.";
}