using Homecare.Application.DTOs.Offers;
using Homecare.Application.Constants.Pagination;
using Homecare.Application.Constants;

namespace Homecare.Application.Interfaces.Offers;

public interface IOfferService
{
    Task<ApiResponse<string>> AddOffer(CreateOfferDto dto, int adminId);
    Task<ApiResponse<FilterPagedResult<OfferResponseDto>>> GetAll(GetOfferListFilterDto filter);
    Task<ApiResponse<string>> UpdateOffer(UpdateOfferDto dto, int adminId);
    Task<ApiResponse<string>> DeleteOffer(int id, int adminId);
}