using System;
using Homecare.Application.Constants;
using Homecare.Application.DTOs.CouponCondition;

namespace Homecare.Application.Interfaces.CouponCondition;

public interface IConditionTypeService
{
    Task<ApiResponse<List<ConditionTypeResponseDto>>> GetAllActiveAsync();
    Task<ApiResponse<List<ConditionTypeResponseDto>>> GetAllAsync();
    Task<ApiResponse<string>> CreateAsync(CreateConditionTypeDto dto, int adminId);
    Task<ApiResponse<List<ServiceTypeGroupDto>>> GetServiceTypeHierarchyAsync();
    ApiResponse<List<string>> GetContextKeys();

    ApiResponse<List<string>> GetAllowedOperators(string inputType);

    ApiResponse<List<string>> GetAllowedInputTypes(string contextKey);
}
