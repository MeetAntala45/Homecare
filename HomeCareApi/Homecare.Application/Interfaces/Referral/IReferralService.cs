using Homecare.Application.Constants;
using Homecare.Application.DTOs.Referral;

namespace Homecare.Application.Interfaces.Referral;

public interface IReferralService
{
    Task<(bool success, string message)> ApplyReferralAtSignupAsync(
        string referralCode, int newCustomerId);

    Task ProcessReferrerRewardAsync(int completedBookingId);

    Task<decimal> GetRefereeFirstBookingDiscountAsync(int customerId);

    Task<decimal> UseWalletAsync(int customerId, decimal bookingTotal, int bookingId);

    Task EnsureWalletAsync(int customerId);

    Task EnsureReferralCodeAsync(int customerId, string customerName);

    Task<WalletDto> GetWalletAsync(int customerId);
    Task<ReferralInfoDto> GetReferralInfoAsync(int customerId);
}