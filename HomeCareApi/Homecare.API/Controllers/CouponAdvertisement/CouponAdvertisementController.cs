using Homecare.Application.Interfaces.CouponAdvertisement;
using Microsoft.AspNetCore.Mvc;

namespace Homecare.API.Controllers.CouponAdvertisement
{
    [Route("api/coupon-advertisement")]
    [ApiController]
    public class CouponAdvertisementController : ControllerBase
    {
        private readonly ICouponAdvertisementService _service;

        public CouponAdvertisementController(ICouponAdvertisementService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _service.GetBestAdvertisementAsync();
            return Ok(result);
        }
    }
}