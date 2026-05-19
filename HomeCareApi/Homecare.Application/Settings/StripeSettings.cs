namespace Homecare.Application.Settings;

public class StripeSettings
{
    public string SecretKey { get; set; } = null!;
    public string WebhookSecret { get; set; } = null!;
}