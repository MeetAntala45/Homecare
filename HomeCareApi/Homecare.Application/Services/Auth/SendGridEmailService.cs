using Homecare.Application.Settings;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using Homecare.Application.Interfaces.Auth;
using Microsoft.Extensions.Logging;
namespace Homecare.Application.Services.Auth;

public class SendGridEmailService : IEmailService
{
    private readonly SendGridSettings _settings;
    private readonly ILogger<SendGridEmailService> _logger;

    public SendGridEmailService(IOptions<SendGridSettings> options, ILogger<SendGridEmailService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }
    public async Task SendAsync(string toEmail, string subject, string htmlContent)
    {
        var client = new SendGridClient(_settings.ApiKey);

        var from = new EmailAddress(_settings.FromEmail, _settings.FromName);
        var to = new EmailAddress(toEmail);

        var message = MailHelper.CreateSingleEmail(
            from,
            to,
            subject,
            plainTextContent: "",
            htmlContent: htmlContent
        );

        var response = await client.SendEmailAsync(message);

        var body = await response.Body.ReadAsStringAsync();
       _logger.LogInformation("SendGrid status: {StatusCode}", response.StatusCode);
       _logger.LogInformation("SendGrid response body: {Body}", body);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"SendGrid error: {body}");
        }
    }
}
