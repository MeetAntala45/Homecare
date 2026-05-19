using Homecare.Application.Constants;
using Homecare.Application.DTOs.Dashboard;

namespace Homecare.Application.Interfaces.Dashboard;

public interface ITopPerformingServicesService
{
    Task<ApiResponse<List<TopServiceResponseDto>>> GetTopServicesBooking(GetChartRequest request);
}
