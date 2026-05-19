using Homecare.Application.Constants;
using Homecare.Application.Constants.Pagination;
using Homecare.Application.Constants.Support;
using Homecare.Application.DTOs.Support.cs;
using Homecare.Application.Interfaces.Support;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Homecare.API.Controllers.Support
{
    [Route("api/support")]
    [ApiController]
    [Authorize]

    public class SupportController : ControllerBase
    {
        private readonly ISupportService _supportService;

        public SupportController(ISupportService supportService)
        {
            _supportService = supportService;
        }

        [HttpGet("all")]
        public async Task<ApiResponse<PagedResult<SupportResponseDto>>> GetAllContacts([FromQuery] SupportFilterRequest req)
        {
            var data = await _supportService.GetAllContactsAsync(req);

            return ApiResponse<PagedResult<SupportResponseDto>>
                .SuccessResponse(SupportConstant.SuccessFetchResponse, data);
        }

        [AllowAnonymous]
        [HttpPost("add")]
        public async Task<ApiResponse<string>> AddContact([FromBody] SupportCreateDto dto)
        {
            await _supportService.AddContactAsync(dto);

            return ApiResponse<string>
                .SuccessResponse(SupportConstant.SuccessAddResponse);
        }
    }
}
