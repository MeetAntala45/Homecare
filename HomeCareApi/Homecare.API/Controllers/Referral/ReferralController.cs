using Homecare.Application.Constants;
using Homecare.Application.DTOs.Referral;
using Homecare.Application.Interfaces;
using Homecare.Application.Interfaces.Referral;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Homecare.API.Controllers.Referral;

[ApiController]
[Route("api/referral")]
[Authorize]
public class ReferralController : ControllerBase
{
    private readonly IReferralService _referralService;
    private readonly ICurrentUserService _currentUser;

    public ReferralController(
        IReferralService referralService,
        ICurrentUserService currentUser)
    {
        _referralService = referralService;
        _currentUser = currentUser;
    }


    [HttpGet("wallet")]
    public async Task<ActionResult<ApiResponse<WalletDto>>> GetWallet()
    {
        var wallet = await _referralService.GetWalletAsync(_currentUser.UserId);
        return Ok(ApiResponse<WalletDto>.SuccessResponse("Wallet fetched successfully.", wallet));
    }

    [HttpGet("info")]
    public async Task<ActionResult<ApiResponse<ReferralInfoDto>>> GetReferralInfo()
    {
        var info = await _referralService.GetReferralInfoAsync(_currentUser.UserId);
        return Ok(ApiResponse<ReferralInfoDto>.SuccessResponse("Referral info fetched successfully.", info));
    }
    [HttpPost("share")]
    public async Task<ActionResult<ApiResponse<string>>> ShareReferral(
        [FromBody] SendReferralEmailDto dto)
    {
        var result = await _referralService.SendReferralEmailAsync(
            _currentUser.UserId, dto.RecipientEmail);

        return result.Success ? Ok(result) : BadRequest(result);
    }
}