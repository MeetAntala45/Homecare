using Homecare.Application.Constants;
using Homecare.Application.DTOs.Referral;

namespace Homecare.Application.Interfaces.Referral;

public interface IReferralService
{
    Task<(bool success, string message)> ApplyReferralAtSignupAsync(
        string referralCode, int newCustomerId);

    Task ProcessReferrerRewardAsync(int completedBookingId);

    Task<decimal> GetRefereeFirstOrderDiscountAsync(
        int customerId, decimal servicePrice, decimal couponDiscount);

    Task<decimal> UseWalletAsync(int customerId, decimal servicePrice,
        decimal currentTotal, int bookingId);

    Task EnsureWalletAsync(int customerId);
    Task EnsureReferralCodeAsync(int customerId, string customerName);

    Task<WalletDto> GetWalletAsync(int customerId);
    Task<ReferralInfoDto> GetReferralInfoAsync(int customerId);

    Task<(bool isValid, string? errorMessage)> ValidateReferralCodeAsync(string referralCode);
    Task<ApiResponse<string>> SendReferralEmailAsync(int customerId, string recipientEmail);
}