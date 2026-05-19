using Homecare.Application.Constants;
using Homecare.Application.Constants.MasterData;
using Homecare.Application.DTOs.MasterData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Homecare.API.Controllers
{
    [Authorize]
    [Route("api/admin/sub-categories")]
    [ApiController]
    public class SubCategoryController : ControllerBase
    {
        private readonly ISubCategory _subCategoryService;

        public SubCategoryController(ISubCategory subCategoryService)
        {
            _subCategoryService = subCategoryService;
        }

        [AllowAnonymous]
        [HttpGet("category/{categoryId}")]
        public async Task<ApiResponse<IList<SubCategoryDto>>> GetAllSubCategory(int categoryId)
        {
            var data = await _subCategoryService.GetAllSubCategory(categoryId);

            return ApiResponse<IList<SubCategoryDto>>
                .SuccessResponse(SubCategoryConstant.GetAllSubCategoryResponse, data);
        }

        [HttpPost("add")]
        public async Task<ApiResponse<string>> AddSubCategory([FromBody] SubCategoryDto dto)
        {
            await _subCategoryService.AddSubCategory(dto);

            return ApiResponse<string>
                .SuccessResponse(SubCategoryConstant.AddSubCategoryResponse);
        }

        [HttpDelete("delete/{id}")]
        public async Task<ApiResponse<string>> DeleteSubCategory(int id)
        {
            await _subCategoryService.DeleteSubCategory(id);

            return ApiResponse<string>
                .SuccessResponse(SubCategoryConstant.DeleteSubCategoryResponse);
        }

        [HttpPut("edit/{id}")]
        public async Task<ApiResponse<string>> EditSubCategory(int id, [FromBody] SubCategoryDto dto)
        {
            await _subCategoryService.UpdateSubCategory(id, dto);

            return ApiResponse<string>
                .SuccessResponse(SubCategoryConstant.UpdateSubCategoryResponse);
        }
    }
}