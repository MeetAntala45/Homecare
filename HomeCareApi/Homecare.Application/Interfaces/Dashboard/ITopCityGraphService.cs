using System;
using Homecare.Application.Constants;
using Homecare.Application.DTOs.Dashboard;

namespace Homecare.Application.Interfaces.Dashboard;

public interface ITopCityGraphService
{
    Task<ApiResponse<List<TopCityDto>>> GetTopCitiesAsync(GetChartRequest request);
}
