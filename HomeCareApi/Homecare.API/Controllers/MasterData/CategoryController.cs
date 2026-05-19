using Homecare.Application.Constants;
using Homecare.Application.Constants.MasterData;
using Homecare.Application.DTOs.MasterData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Homecare.API.Controllers
{
    [Authorize]
    [Route("api/admin/categories")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategory _categoryService;

        public CategoryController(ICategory categoryService)
        {
            _categoryService = categoryService;
        }

        [AllowAnonymous]
        [HttpGet("service-type/{serviceTypeId}")]
        public async Task<ApiResponse<IList<CategoryDto>>> GetAllCategory(int serviceTypeId)
        {
            var data = await _categoryService.GetAllCategory(serviceTypeId);

            return ApiResponse<IList<CategoryDto>>
                .SuccessResponse("Categories fetched successfully", data);
        }

        [HttpPost("add")]
        public async Task<ApiResponse<string>> AddCategory([FromBody] CategoryDto dto)
        {
            await _categoryService.AddCategory(dto);

            return ApiResponse<string>
                .SuccessResponse(CategoryConstant.AddCategoryResponse);
        }

        [HttpPut("edit/{id}")]
        public async Task<ApiResponse<string>> EditCategory(int id, [FromBody] CategoryDto dto)
        {
            await _categoryService.UpdateCategory(id, dto);

            return ApiResponse<string>
                .SuccessResponse(CategoryConstant.UpdateCategoryResponse);
        }

        [HttpDelete("delete/{id}")]
        public async Task<ApiResponse<string>> DeleteCategory(int id)
        {
            await _categoryService.DeleteCategory(id);

            return ApiResponse<string>
                .SuccessResponse(CategoryConstant.DeleteCategoryResponse);
        }
    }
}