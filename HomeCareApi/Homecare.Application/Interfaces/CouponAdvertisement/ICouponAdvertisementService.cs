using Homecare.Application.Constants;
using Homecare.Application.DTOs.CouponAddvertisement;

namespace Homecare.Application.Interfaces.CouponAdvertisement;

public interface ICouponAdvertisementService
{
    Task<ApiResponse<CouponAdvertisementDto?>> GetBestAdvertisementAsync();
}