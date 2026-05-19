using Homecare.Application.Constants;
using Homecare.Application.Constants.Pagination;
using Homecare.Application.DTOs.PaymentsAndTransactions;
using Homecare.Application.Interfaces.PaymentsAndTransactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Homecare.API.Controllers.PaymentsAndTransactions
{
    [Authorize]
    [ApiController]
    [Route("api/admin/payment")]
    public class PaymentTransactionController : ControllerBase
    {
        private readonly IPaymentTransactionService _paymentTransactionService;

        public PaymentTransactionController(IPaymentTransactionService paymentTransactionService)
        {
            _paymentTransactionService = paymentTransactionService;
        }

        [HttpGet("get")]
        public async Task<ApiResponse<PaymentPagedResult<PaymentListDto>>> GetPayments([FromQuery] PaymentListFilterDto filter)
        {
            return await _paymentTransactionService.GetPaymentsAsync(filter);
        }

        [HttpGet("{id}")]
        public async Task<ApiResponse<UserPaymentDetailDto>> GetPaymentDetail(int id)
        {
            return await _paymentTransactionService.GetUserPaymentDetailAsync(id);
        }

        [HttpGet("user-payments")]
        public async Task<ApiResponse<PaymentPagedResult<UserPaymentListDto>>> GetUserTransactions([FromQuery] UserPaymentFilterDto filter)
        {
            return await _paymentTransactionService.GetUserPaymentsAsync(filter);
        }
    }
}
