using Homecare.Application.Constants;
using Homecare.Application.DTOs.Dashboard;

namespace Homecare.Application.Interfaces.Dashboard;

public interface IRevenueChartService
{
    Task<ApiResponse<RevenueChartResponseDto>> GetRevenueChartAsync(GetChartRequest request);
}
