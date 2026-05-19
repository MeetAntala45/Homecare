using Homecare.Application.Constants;
using Homecare.Application.DTOs.PartnerDashboard;

namespace Homecare.Application.Interfaces.PartnerDashboard;

public interface IPartnerRevenueChartService
{
    Task<ApiResponse<PartnerRevenueChartResponseDto>> GetRevenueChartAsync(int partnerId, GetPartnerChartRequest request);
}