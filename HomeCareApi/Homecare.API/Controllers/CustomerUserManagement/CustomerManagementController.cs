using System.Security.Claims;
using Homecare.Application.Constants;
using Homecare.Application.Constants.CustomerUserManagement;
using Homecare.Application.Constants.Pagination;
using Homecare.Application.DTOs.CustomerUser;
using Homecare.Application.Interfaces;
using Homecare.Application.Interfaces.CustomerUserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Homecare.API.Controllers.CustomerManagement
{
    [Route("api/customer")]
    [ApiController]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class CustomerUserManagementController : ControllerBase
    {
        private readonly ICustomerManagementService _service;
        private readonly ICurrentUserService _currentUser;

        public CustomerUserManagementController(
            ICustomerManagementService service,
            ICurrentUserService currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        [AllowAnonymous]
        [HttpGet("customer-list")]
        public async Task<ApiResponse<FilterPagedResult<CustomerListDto>>> GetCustomerList([FromQuery] CustomerListFilterDto dto)
        {
            return await _service.GetCustomerListAsync(dto);
        }

        [AllowAnonymous]
        [HttpGet("service-types")]
        public async Task<ApiResponse<List<DropdownOptionDto>>> GetServiceTypes()
        {
            return await _service.GetServiceTypesAsync();
        }

        [HttpGet("{id}")]
        public async Task<ApiResponse<CustomerListDto>> GetCustomerById(int id)
        {
            return await _service.GetCustomerByIdAsync(id);
        }

        [HttpGet("{id}/bookings")]
        public async Task<ApiResponse<PaymentPagedResult<CustomerBookingListDto>>> GetCustomerBookings(
            int id, [FromQuery] CustomerBookingFilterDto filter)
        {
            return await _service.GetCustomerBookingsAsync(id, filter);
        }

        [HttpPatch("block/{id}")]
        public async Task<ApiResponse<string>> BlockCustomer(int id)
        {
            return await _service.BlockCustomerAsync(id, _currentUser.UserId);
        }

        [HttpPatch("unblock/{id}")]
        public async Task<ApiResponse<string>> UnblockCustomer(int id)
        {
            return await _service.UnblockCustomerAsync(id, _currentUser.UserId);
        }

        [HttpPatch("active/{id}")]
        public async Task<ApiResponse<string>> ActivateCustomer(int id)
        {
            return await _service.ActivateCustomerAsync(id, _currentUser.UserId);
        }

        [HttpDelete("delete/{id}")]
        public async Task<ApiResponse<string>> DeleteCustomer(int id)
        {
            return await _service.DeleteCustomerAsync(id, _currentUser.UserId);
        }

        [HttpPost("add-customer")]
        public async Task<ApiResponse<string>> AddCustomer([FromBody] AddCustomerDto dto)
        {
            if (!ModelState.IsValid)
                return ApiResponse<string>.Fail(CustomerManagementMessages.InvalidData);
            return await _service.AddCustomerAsync(GetAdminId(), dto);
        }

        private int GetAdminId()
        {
            var claim = User.Claims.FirstOrDefault(
                c => c.Type == ClaimTypes.NameIdentifier);
            return int.TryParse(claim?.Value, out var id) ? id : 0;
        }

    }
}