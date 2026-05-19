using Homecare.Application.Constants;
using Homecare.Application.DTOs.CustomerProfile;

namespace Homecare.Application.Interfaces.CustomerProfile;
public interface ICustomerProfileService
{
    Task<ApiResponse<CustomerProfileDto>> GetProfileAsync(int customerId);
    Task<ApiResponse<string>> UpdateMobileAsync(int customerId, UpdateMobileDto dto);
    Task<ApiResponse<EmailChangeOtpResponseDto>> RequestEmailChangeAsync(int customerId, RequestEmailChangeDto dto);
    Task<ApiResponse<string>> VerifyEmailChangeAsync(int customerId, VerifyEmailChangeDto dto);
    Task<ApiResponse<string>> AddAddressAsync(int customerId, AddressRequestDto dto);
    Task<ApiResponse<string>> EditAddressAsync(int customerId, int addressId, AddressRequestDto dto);
    Task<ApiResponse<string>> DeleteAddressAsync(int customerId, int addressId);
    Task<ApiResponse<List<string>>> GetAddressLabelsAsync(int customerId);
    Task<ApiResponse<string>> AddRecentSearchAsync(int customerId, AddRecentSearchDto dto);
    Task<ApiResponse<List<RecentSearchDto>>> GetRecentSearchesAsync(int customerId);
}
