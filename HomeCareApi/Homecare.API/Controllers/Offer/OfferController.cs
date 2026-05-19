using Microsoft.AspNetCore.Mvc;
using Homecare.Application.Interfaces.Offers;
using Homecare.Application.DTOs.Offers;
using Microsoft.AspNetCore.Authorization;
using Homecare.Application.Constants;
using Homecare.Application.Constants.Offers;
using Homecare.Application.Constants.Pagination;
using Homecare.Application.Interfaces;

namespace Homecare.API.Controllers;

[ApiController]
[Route("api/offer")]
[Authorize]
public class OfferController : ControllerBase
{
    private readonly IOfferService _services;

    private readonly ICurrentUserService _currentUser;

    public OfferController(IOfferService services, ICurrentUserService currentUser)
    {
        _services = services;
        _currentUser = currentUser;
    }

    [HttpPost("create")]
    public async Task<ApiResponse<string>> AddOffer([FromBody] CreateOfferDto dto)
    {
        return  await _services.AddOffer(dto, _currentUser.UserId);
    }

    [HttpGet("get")]
    public async Task<ApiResponse<FilterPagedResult<OfferResponseDto>>> GetAll([FromQuery] GetOfferListFilterDto filter)
    {
        return await _services.GetAll(filter);
    }

    [HttpPut("edit")]
    public async Task<ApiResponse<string>> Update([FromBody] UpdateOfferDto dto)
    {
        return await _services.UpdateOffer(dto, _currentUser.UserId);
    }

    [HttpDelete("delete/{id}")]
    public async Task<ApiResponse<string>> Delete(int id)
    {
        return await _services.DeleteOffer(id, _currentUser.UserId);
    }
}