using System.Security.Claims;
using Homecare.Data;
using Homecare.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Homecare.API.Middlewares; 

public class CustomerStatusMiddleware
{
    private readonly RequestDelegate _next;

    public CustomerStatusMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var role = context.User.FindFirst(ClaimTypes.Role)?.Value;
            var idClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (role == "Customer" && idClaim != null && int.TryParse(idClaim, out var customerId))
            {
                var customer = await db.Customers
                    .FirstOrDefaultAsync(c => c.Id == customerId);

                if (customer == null || customer.Status == UserStatus.Blocked)
                {
                    var tokens = await db.RefreshTokens
                        .Where(r => r.CustomerId == customerId && !r.IsRevoked)
                        .ToListAsync();

                    foreach (var token in tokens)
                        token.IsRevoked = true;

                    await db.SaveChangesAsync();

                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        success = false,
                        message = "Your account has been blocked."
                    });
                    return;
                }
            }
        }

        await _next(context);
    }
}