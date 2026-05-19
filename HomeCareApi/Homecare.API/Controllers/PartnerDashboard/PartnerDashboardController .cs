using Homecare.Application.Constants;
using Homecare.Application.DTOs.PartnerDashboard;
using Homecare.Application.Interfaces.PartnerDashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Homecare.API.Controllers.PartnerDashboard
{
    [Route("api/partner/dashboard")]
    [Authorize]
    [ApiController]
    public class PartnerDashboardController : ControllerBase
    {
        private readonly IPartnerMetricsService _metricsService;
        private readonly IPartnerRevenueChartService _revenueService;
        private readonly IPartnerTopServicesService _topServicesService;

        public PartnerDashboardController(
            IPartnerMetricsService metricsService,
            IPartnerRevenueChartService revenueService,
            IPartnerTopServicesService topServicesService)
        {
            _metricsService = metricsService;
            _revenueService = revenueService;
            _topServicesService = topServicesService;
        }

        private int GetPartnerId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue("sub")
                        ?? User.FindFirstValue("partnerId");
            return int.Parse(claim!);
        }

        [HttpGet("metrics")]
        public async Task<ApiResponse<PartnerDashboardMetricsResponseDto>> GetMetrics()
        {
            return await _metricsService.GetAllMetricsAsync(GetPartnerId());
        }

        [HttpGet("revenue-chart")]
        public async Task<ApiResponse<PartnerRevenueChartResponseDto>> GetRevenueChart([FromQuery] GetPartnerChartRequest request)
        {
            return await _revenueService.GetRevenueChartAsync(GetPartnerId(), request);
        }

        [HttpGet("top-services")]
        public async Task<ApiResponse<List<PartnerTopServiceResponseDto>>> GetTopServices([FromQuery] GetPartnerChartRequest request)
        {
            return await _topServicesService.GetTopServicesAsync(GetPartnerId(), request);
        }
    }
}