using Homecare.Application.Constants;
using Homecare.Application.DTOs.Payments;
using Homecare.Application.Interfaces;
using Homecare.Application.Interfaces.Payments;
using Homecare.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Homecare.API.Controllers.Payment;

[ApiController]
[Route("api/payment")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly AppDbContext _context;
    private readonly IInvoiceService _invoiceService;

    private readonly ICurrentUserService _currentUser;
    private int userId => _currentUser.UserId;
    private readonly IHttpClientFactory _httpClientFactory;


    public PaymentController(
    IPaymentService paymentService,
    IInvoiceService invoiceService,
    AppDbContext context,
    ICurrentUserService currentUser,
    IHttpClientFactory httpClientFactory)   // ← ADD
    {
        _paymentService = paymentService;
        _invoiceService = invoiceService;
        _currentUser = currentUser;
        _context = context;
        _httpClientFactory = httpClientFactory;  // ← ADD
    }

    [Authorize]
    [HttpPost("create-checkout-session")]
    public async Task<ApiResponse<CheckoutSessionResponseDto>> CreateCheckoutSession(
        [FromBody] CheckoutRequestDto dto)
    {
        return await _paymentService.CreateCheckoutSessionAsync(dto.BookingId);
    }

    [AllowAnonymous]
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook()
    {

        Request.EnableBuffering();
        Request.Body.Position = 0;
        using var ms = new MemoryStream();
        await Request.Body.CopyToAsync(ms);
        var json = System.Text.Encoding.UTF8.GetString(ms.ToArray());

        var signature = Request.Headers["Stripe-Signature"].ToString();

        var result = await _paymentService.HandleWebhookAsync(json, signature);
        return result.Success ? Ok() : BadRequest(result.Message);
    }

    [AllowAnonymous]
    [HttpPost("expire-timed-out")]
    public async Task<ApiResponse<bool>> ExpireTimedOut()
    {
        return await _paymentService.ExpireTimedOutBookingsAsync();
    }


    [Authorize]
    [HttpGet("booking/{bookingId:int}/success-details")]
    public async Task<ApiResponse<BookingSuccessResponseDto>> GetBookingSuccessDetails(int bookingId)
    {
        return await _paymentService.GetBookingSuccessDetailsAsync(bookingId, userId);
    }

    [Authorize]
    [HttpGet("booking/{bookingId:int}/invoice")]
    public async Task<IActionResult> DownloadInvoice(int bookingId)
    {
        try
        {
            var cloudinaryUrl = await _invoiceService.GetInvoicePathAsync(bookingId);

            return Redirect(cloudinaryUrl);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<string>.Fail(ex.Message));
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(ApiResponse<string>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
    }
}