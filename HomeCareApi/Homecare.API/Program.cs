using Homecare.API.Extensions;
using QuestPDF.Infrastructure;
using Serilog;
using Serilog.Events;
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Error)
    .MinimumLevel.Override("Microsoft.Extensions.Http", LogEventLevel.Warning)

    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(e =>
            e.Properties.ContainsKey("SourceContext") &&
            e.Properties["SourceContext"].ToString().Contains("Microsoft"))
        .WriteTo.Console()
    )

    .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Warning)
    .WriteTo.Logger(lc => lc
        .Filter.ByExcluding(e =>
            e.Properties.ContainsKey("SourceContext") &&
            e.Properties["SourceContext"].ToString().Contains("Microsoft"))
    .WriteTo.File(
        path: "Logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 5,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    ))
    .CreateLogger();
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();
QuestPDF.Settings.License = LicenseType.Community;

builder.Services
    .AddDatabase(builder.Configuration)
    .AddAppConroller()
    .AddAppServices(builder.Configuration)
    .AddMemoryCache()
    .AddAppAuthentication(builder.Configuration)
    .AddAppCors(builder.Configuration)
    .AddStripe(builder.Configuration)
    .AddEndpointsApiExplorer()
    .AddOpenApi()
    .AddHttpClient();
var app = builder.Build();

app.UseAppMiddleware(app.Configuration);
app.Run();