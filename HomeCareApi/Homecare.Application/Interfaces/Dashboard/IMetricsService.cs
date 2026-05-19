using System;
using Homecare.Application.Constants;
using Homecare.Application.DTOs.Dashboard;

namespace Homecare.Application.Interfaces.Dashboard;

public interface IMetricsService
{
    Task<ApiResponse<DashboardMetricsResponseDto>> GetAllMetricsAsync();
}
