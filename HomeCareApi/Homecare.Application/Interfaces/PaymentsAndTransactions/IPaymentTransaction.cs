using Homecare.Application.Constants;
using Homecare.Application.Constants.Pagination;
using Homecare.Application.DTOs.PaymentsAndTransactions;

namespace Homecare.Application.Interfaces.PaymentsAndTransactions;

public interface IPaymentTransactionService
{
    Task<ApiResponse<PaymentPagedResult<PaymentListDto>>> GetPaymentsAsync(PaymentListFilterDto filter);
    Task<ApiResponse<UserPaymentDetailDto>> GetUserPaymentDetailAsync(int id);
    Task<ApiResponse<PaymentPagedResult<UserPaymentListDto>>> GetUserPaymentsAsync(UserPaymentFilterDto filter);
    Task<List<PaymentListDto>> GetAllForExportAsync(PaymentListFilterDto filter, bool paginate = false);
    Task<List<UserPaymentExportDto>> GetAllUserPaymentForExportAsync(UserPaymentFilterDto filter, bool paginate = false);
}
