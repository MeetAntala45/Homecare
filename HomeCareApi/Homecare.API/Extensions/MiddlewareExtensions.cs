using Homecare.API.Middlewares;
using Homecare.Application.Hubs;

namespace Homecare.API.Extensions;

public static class MiddlewareExtensions
{
    public static WebApplication UseAppMiddleware(this WebApplication app, IConfiguration config)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "Homecare API v1");
            });
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowAngular");

        var allowedOrigins = config.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? Array.Empty<string>();


        app.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponse = ctx =>
            {
                var origin = ctx.Context.Request.Headers["Origin"].ToString();
                if (allowedOrigins.Contains(origin))
                {
                    ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", origin);
                    ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers", "*");
                }
            }
        });

        app.Use(async (context, next) =>
        {
            if (context.Request.Path.StartsWithSegments("/api/payment/webhook"))
                context.Request.EnableBuffering();
            await next();
        });

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<CustomerStatusMiddleware>();
        app.MapControllers();

        app.MapHub<BookingHub>("/hubs/booking")
           .RequireCors("AllowAngular");

        app.MapControllers();

        return app;
    }
}