namespace Homecare.Application.Constants.Pagination;

public class PaymentPagedResult<T> : PagedResult<T> {
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
}