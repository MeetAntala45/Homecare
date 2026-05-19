using Homecare.Application.Constants;
using Homecare.Application.DTOs.Dashboard;

namespace Homecare.Application.Interfaces.Dashboard;

public interface ITopServicePartnersService
{
    Task<ApiResponse<List<TopServicePartnersResponseDto>>> GetTopServicePartnersAsync();
}
