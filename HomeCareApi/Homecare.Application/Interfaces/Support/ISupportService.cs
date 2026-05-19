using Homecare.Application.Constants.Pagination;
using Homecare.Application.DTOs.Support.cs;

namespace Homecare.Application.Interfaces.Support;

public interface ISupportService
{
    Task<PagedResult<SupportResponseDto>> GetAllContactsAsync(SupportFilterRequest req);
    Task AddContactAsync(SupportCreateDto dto);
    Task<List<SupportResponseDto>> GetAllForExportAsync(SupportFilterRequest filter, bool paginate = false);
}