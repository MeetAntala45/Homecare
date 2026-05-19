using Homecare.Application.Constants;
using Homecare.Application.DTOs.Dashboard;
using Homecare.Application.Interfaces.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Homecare.API.Controllers.Dashboard
{
    [Route("api/dashboard")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IMetricsService _metricsService;
        private readonly IRevenueChartService _revenueService;
        private readonly ITopPerformingServicesService _topPerformingServices;
        private readonly ITopServicePartnersService _topServicePartnersService;
        private readonly ITopCityGraphService _topCityService;

        public DashboardController(IMetricsService metricsService,
        IRevenueChartService revenueService, ITopPerformingServicesService topPerformingServices, ITopServicePartnersService topServicePartnersService, ITopCityGraphService topCityService)
        {
            _metricsService = metricsService;
            _revenueService = revenueService;
            _topPerformingServices = topPerformingServices;
            _topServicePartnersService = topServicePartnersService;
            _topCityService = topCityService;
        }

        [HttpGet("metrics")]
        public async Task<ApiResponse<DashboardMetricsResponseDto>> GetMetrics()
        {
            return await _metricsService.GetAllMetricsAsync();
        }

        [HttpGet("revenue-chart")]
        public async Task<ApiResponse<RevenueChartResponseDto>> GetRevenueChart([FromQuery] GetChartRequest request)
        {
            return await _revenueService.GetRevenueChartAsync(request);
        }
        [HttpGet("top-services")]
        public async Task<ApiResponse<List<TopServiceResponseDto>>> GetTopServices([FromQuery] GetChartRequest request)
        {
            return await _topPerformingServices.GetTopServicesBooking(request);
        }
        [HttpGet("top-service-partners")]
        public async Task<ApiResponse<List<TopServicePartnersResponseDto>>> GetTopServicePartners()
        {
            return await _topServicePartnersService.GetTopServicePartnersAsync();
        }
        [HttpGet("top-cities")]
        public async Task<ApiResponse<List<TopCityDto>>> GetTopCity([FromQuery] GetChartRequest request)
        {
            return await _topCityService.GetTopCitiesAsync(request);
        }

    }
}
