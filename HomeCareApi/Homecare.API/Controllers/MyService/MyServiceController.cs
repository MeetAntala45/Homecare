using Homecare.Application.DTOs.MyServices;
using Homecare.Application.Interfaces;
using Homecare.Application.Interfaces.MyService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Homecare.API.Controllers.ServicePartner
{
    [ApiController]
    [Route("api/service-partner")]
    [Authorize]
    public class MyServiceController : ControllerBase
    {
        private readonly IMyService _myService;
        private readonly ICurrentUserService _currentUser;


        public MyServiceController(IMyService myService, ICurrentUserService currentUser)
        {
            _myService = myService;
            _currentUser = currentUser;
        }

        [HttpGet("service-hierarchy")]
        public async Task<IActionResult> GetPartnerServiceHierarchy()
        {

            var response = await _myService.GetPartnerServiceHierarchyAsync(_currentUser.UserId);

            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }

        [Authorize]
        [HttpPost("add-skill-service")]
        public async Task<IActionResult> AddSkillAndService([FromBody] AddPartnerSkillAndServiceRequestDto request)
        {

            var userId = _currentUser.UserId;

            var response = await _myService.AddSkillAndServiceAsync(userId, request);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [Authorize]
        [HttpDelete("remove-skill-service/{subCategoryId}")]
        public async Task<IActionResult> RemoveSkillService(int subCategoryId)
        {
            var userId = _currentUser.UserId;

            var response = await _myService.RemoveSkillServiceAsync(userId, subCategoryId);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
    }
}