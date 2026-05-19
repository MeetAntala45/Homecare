using Homecare.Application.Constants;
using Homecare.Application.Constants.Pagination;
using Homecare.Application.DTOs.PartnerLeave;

namespace Homecare.Application.Interfaces.PartnerLeave;


public interface ILeaveService
{
   
     Task<ApiResponse<LeaveResponseDto>> ApplyLeaveAsync(
        int partnerId, ApplyLeaveRequestDto dto);

    Task<ApiResponse<PagedResult<LeaveResponseDto>>> GetMyLeavesAsync(
        int partnerId, LeaveFilterDto filter);

    Task<ApiResponse<string>> CancelLeaveAsync(
        int leaveId, int partnerId);

    Task<ApiResponse<PagedResult<AdminLeaveListDto>>> GetAllLeavesAsync(
        LeaveFilterDto filter);

    Task<ApiResponse<string>> ReviewLeaveAsync(
        int leaveId, int adminId, ReviewLeaveDto dto);
}