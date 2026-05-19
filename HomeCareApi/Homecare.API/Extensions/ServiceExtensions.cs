using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Homecare.API.BackgroundJobs;
using Homecare.API.Filters;
using Homecare.Application.Common.Models;
using Homecare.Application.Interfaces.Auth;
using Homecare.Application.Interfaces.MyService;
using Homecare.Application.Interfaces.ServicePartnerLogin;
using Homecare.Application.Interfaces.ChatBot;
using Homecare.Application.Interfaces.Shared;
using Homecare.Application.Services;
using Homecare.Application.Services.Auth;
using Homecare.Application.Services.ChatBot;
using Homecare.Application.Services.MasterData;
using Homecare.Application.Services.MyService;
using Homecare.Application.Services.Offers;
using Homecare.Application.Services.ServicePartnerLogin;
using Homecare.Application.Settings;
using Homecare.Application.Validators;
using Homecare.Data;
using Homecare.Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Stripe;
using Homecare.Application.Interfaces.MyJobs;
using Homecare.Application.Services.MyJobs;
using Homecare.Application.Services.Shared;
using Homecare.Application.Interfaces.CouponAdvertisement;

using Homecare.API.BackgroundServices;
using Homecare.Application.Services.Leave;
using Homecare.Application.Interfaces.PartnerLeave;
using Homecare.Application.Interfaces.Notification;
using Homecare.Application.Services.Notification;

namespace Homecare.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContextFactory<AppDbContext>(options =>
        options.UseNpgsql(config.GetConnectionString("DefaultConnection")));
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        return services;
    }

    public static IServiceCollection AddAppConroller(this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add<ValidationFilter>();
            options.Filters.Add<ExceptionFilter>();
        })
        .AddJsonOptions(opts =>
        {
            opts.JsonSerializerOptions.PropertyNamingPolicy =
                System.Text.Json.JsonNamingPolicy.CamelCase;
            opts.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        });

        services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(o =>
        {
            o.MultipartBodyLengthLimit = 100 * 1024 * 1024;
        });

        return services;
    }

    public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddSignalR();
        services.AddHttpContextAccessor();
        services.AddFluentValidationAutoValidation();
        services.AddEndpointsApiExplorer();
        services.AddValidatorsFromAssemblyContaining<AdminValidator>();
        services.AddOpenApi();

        services.Scan(scan => scan
            .FromAssemblyOf<AuthService>()
            .AddClasses(c => c
                .Where(t => t.Name.EndsWith("Service")))
            .AsMatchingInterface()
            .WithScopedLifetime());

        services.AddScoped<IServicePartnerAuthService, ServicePartnerAuthService>();
        services.AddScoped<IBookingServicesService, BookingServicesService>();
        services.AddScoped<IEmailService, SendGridEmailService>();
        services.AddScoped<IServiceType, ServiceTypeService>();
        services.AddScoped<ICategory, CategoryService>();
        services.AddScoped<ISubCategory, SubCategoryService>();
        services.AddScoped<IMyService, MyServices>();
        services.AddScoped<EmailTemplateService>();
        services.AddScoped<RuleEngine>();
        services.AddScoped<IPasswordHasher<Admin>, PasswordHasher<Admin>>();
        services.AddScoped<IDataExporter, PaymentExporter>();
        services.AddScoped<IDataExporter, SupportExporter>();
        services.AddScoped<IDataExporter, UserPaymentExporter>();
        services.AddScoped<ICouponAdvertisementService, CouponAdvertisementService>();
        services.AddScoped<ILeaveService, LeaveService>();
        services.AddScoped<IPartnerSystemNotifService, PartnerSystemNotifService>();
        services.AddScoped<IAdminSystemNotifService, AdminSystemNotifService>();

        services.AddHostedService<BookingCompletionJob>();
        services.AddHostedService<BookingExpiryJob>();
        services.AddHostedService<BookingReminderBackgroundService>();
        services.AddHostedService<LeaveStatusJob>();

        services.Configure<SendGridSettings>(config.GetSection("SendGrid"));

        services.Configure<GeminiSettings>(config.GetSection("GeminiSettings"));
        services.AddSingleton<ChatBotPromptProvider>();
        services.AddHttpClient<IChatBotService, ChatBotService>(client =>
            client.Timeout = TimeSpan.FromSeconds(60));

        return services;
    }

    public static IServiceCollection AddAppAuthentication(this IServiceCollection services, IConfiguration config)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = config["Jwt:Issuer"],
                    ValidAudience = config["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(config["Jwt:Key"]!))
                };
            });

        services.AddAuthorization();
        return services;
    }

    public static IServiceCollection AddAppCors(this IServiceCollection services, IConfiguration config)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAngular", policy =>
            {
                var allowedOrigins = config.GetSection("Cors:AllowedOrigins").Get<string[]>()
                    ?? Array.Empty<string>();

                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
        return services;
    }

    public static IServiceCollection AddStripe(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<StripeSettings>(config.GetSection("Stripe"));
        StripeConfiguration.ApiKey = config["Stripe:SecretKey"];
        return services;
    }


}
