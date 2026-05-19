namespace Homecare.Application.Interfaces.Auth;

public interface IEmailService
{
    Task SendAsync(string toEmail, string subject, string htmlContent);
}
