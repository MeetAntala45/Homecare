using Homecare.API.Model;
using Homecare.Application.Constants;
using Homecare.Application.Constants.MasterData;
using Homecare.Application.DTOs.MasterData;
using Homecare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Homecare.API.Controllers
{
    [Authorize]
    [Route("api/admin/service-types")]
    [ApiController]
    public class ServiceTypeController : ControllerBase
    {

        private readonly IServiceType _serviceTypeService;
        private readonly ICloudinaryService _cloudinary;


        public ServiceTypeController(IServiceType serviceTypeService, ICloudinaryService cloudinary)
        {
            _serviceTypeService = serviceTypeService;
            _cloudinary = cloudinary;
        }

        [AllowAnonymous]
        [HttpGet("all")]
        public async Task<ApiResponse<IList<ServiceTypeDto>>> GetAllServiceType()
        {
            var data = await _serviceTypeService.GetAllServiceTypes();

            return ApiResponse<IList<ServiceTypeDto>>
                .SuccessResponse(ServiceTypeConstant.GetAllServiceTypeResponse, data);
        }

        [HttpPost("add")]
        public async Task<ApiResponse<string>> AddServiceType([FromForm] ServiceTypeCreateRequest request)
        {
            string imageUrl = string.Empty;

            if (request.Image != null)
                imageUrl = await _cloudinary.UploadImageAsync(request.Image, "service-types");

            var dto = new ServiceTypeDto { Name = request.Name, ImagePath = imageUrl };
            await _serviceTypeService.AddServiceType(dto);
            return ApiResponse<string>.SuccessResponse(ServiceTypeConstant.AddServiceResponse);
        }

        [HttpPut("edit/{id}")]
        public async Task<ApiResponse<string>> UpdateServiceType(int id, [FromForm] ServiceTypeCreateRequest request)
        {
            string? imageUrl = null;

            if (request.Image != null)
                imageUrl = await _cloudinary.UploadImageAsync(request.Image, "service-types");

            var dto = new ServiceTypeDto { Name = request.Name, ImagePath = imageUrl! };
            await _serviceTypeService.UpdateServiceType(id, dto);
            return ApiResponse<string>.SuccessResponse(ServiceTypeConstant.UpdateServiceResponse);
        }

        [HttpDelete("delete/{id}")]
        public async Task<ApiResponse<string>> DeleteServiceType(int id)
        {
            await _serviceTypeService.DeleteServiceType(id);

            return ApiResponse<string>
                .SuccessResponse(ServiceTypeConstant.DeleteServiceResponse);
        }

    }


}
