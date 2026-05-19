using Homecare.Application.Constants;
using Homecare.Application.Constants.Offers;
using Homecare.Application.DTOs.CouponCondition;
using Homecare.Application.Interfaces;
using Homecare.Application.Interfaces.CouponCondition;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Homecare.API.Controllers.CouponCondition
{
    [Route("api/offers/condition-types")]
    [ApiController]
    public class ConditionTypeController : ControllerBase
    {
        private readonly IConditionTypeService _service;
        private readonly ICurrentUserService _currentUser;

        public ConditionTypeController(IConditionTypeService service,
        ICurrentUserService currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        [HttpGet("active-conditions")]
        public async Task<ApiResponse<List<ConditionTypeResponseDto>>> GetActive()
        {

            return await _service.GetAllActiveAsync();
        }

        [HttpGet("all-conditions")]
        public async Task<ApiResponse<List<ConditionTypeResponseDto>>> GetAll()
        {
            return await _service.GetAllAsync();
        }

        [HttpPost("add-condition")]
        public async Task<ApiResponse<string>> Create(
            [FromBody] CreateConditionTypeDto dto)
        {
            return await _service.CreateAsync(dto, _currentUser.UserId);
        }

         [HttpGet("context-keys")]
        public ApiResponse<List<string>> GetContextKeys()
        {
            return _service.GetContextKeys();
        }

        [HttpGet("allowed-operators/{inputType}")]
        public ApiResponse<List<string>> GetAllowedOperators(string inputType)
        {
            return _service.GetAllowedOperators(inputType);
        }

        [HttpGet("allowed-input-types/{contextKey}")]
        public ApiResponse<List<string>> GetAllowedInputTypes(string contextKey)
        {
            return _service.GetAllowedInputTypes(contextKey);
        }

        [HttpGet("service-type-hierarchy")]
        public async Task<ApiResponse<List<ServiceTypeGroupDto>>> GetServiceTypeHierarchy()
        {
            return await _service.GetServiceTypeHierarchyAsync();
        }
    }
}
