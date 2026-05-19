using System.Security.Claims;
using Homecare.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Homecare.Application.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int UserId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?
                .User?.FindFirst(ClaimTypes.NameIdentifier);

            return claim != null ? int.Parse(claim.Value) : 0;
        }
    }
}
