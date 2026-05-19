using Homecare.Application.Constants;
using Homecare.Application.DTOs.PartnerDashboard;

namespace Homecare.Application.Interfaces.PartnerDashboard;

public interface IPartnerMetricsService
{
    Task<ApiResponse<PartnerDashboardMetricsResponseDto>> GetAllMetricsAsync(int partnerId);
}