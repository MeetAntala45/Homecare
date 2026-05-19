using Homecare.Application.Constants;
using Homecare.Application.Constants.Pagination;
using Homecare.Application.DTOs.CustomerUser;
using Homecare.Domain.Entities;
using static Homecare.Application.DTOs.CustomerUser.CustomerBookingListDto;

namespace Homecare.Application.Interfaces.CustomerUserManagement;

public interface ICustomerManagementService
{
    Task<ApiResponse<FilterPagedResult<CustomerListDto>>> GetCustomerListAsync(CustomerListFilterDto filter);
    Task<ApiResponse<string>> AddCustomerAsync(int adminId, AddCustomerDto dto);
    Task<ApiResponse<string>> BlockCustomerAsync(int customerId, int adminId);
    Task<ApiResponse<string>> DeleteCustomerAsync(int customerId, int adminId);
    Task<ApiResponse<CustomerListDto>> GetCustomerByIdAsync(int id);
    Task<ApiResponse<string>> UnblockCustomerAsync(int customerId, int adminId);
    Task<ApiResponse<PaymentPagedResult<CustomerBookingListDto>>> GetCustomerBookingsAsync(int customerId, CustomerBookingFilterDto filter);
    Task<ApiResponse<List<DropdownOptionDto>>> GetServiceTypesAsync();
    Task<ApiResponse<List<AvailablePartnerDto>>> GetAvailablePartnersAsync(int bookingId);
    Task<ApiResponse<string>> ChangeExpertAsync(int bookingId, int newPartnerId, int adminId);
    Task<ApiResponse<string>> CompleteBookingAsync(int bookingId, int adminId);
    Task<ApiResponse<string>> CancelBookingAsync(int bookingId, string? reason, int adminId);
    Task<ApiResponse<string>> DeleteBookingAsync(int bookingId, int adminId);
    Task<ApiResponse<string>> ActivateCustomerAsync(int bookingId, int adminId);
}
