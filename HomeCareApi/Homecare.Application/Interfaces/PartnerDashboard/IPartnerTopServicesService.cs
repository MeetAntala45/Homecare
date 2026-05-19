using Homecare.Application.Constants;
using Homecare.Application.DTOs.PartnerDashboard;

namespace Homecare.Application.Interfaces.PartnerDashboard;

public interface IPartnerTopServicesService
{
    Task<ApiResponse<List<PartnerTopServiceResponseDto>>> GetTopServicesAsync(int partnerId, GetPartnerChartRequest request);
}