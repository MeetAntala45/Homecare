using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Homecare.Application.Services.ChatBot;

public class ChatBotPromptProvider
{
    private readonly IHostEnvironment _env;
    private readonly ILogger<ChatBotPromptProvider> _logger;

    private string? _customerPrompt;
    private string? _adminPrompt;
    private string? _servicePartnerPrompt;

    private readonly object _lock = new();

    private const string CustomerPromptFile = "homecare-chatbot-prompt.txt";
    private const string AdminPromptFile = "homecare-admin-chatbot-prompt.txt";
    private const string ServicePartnerPromptFile = "homecare-servicepartner-chatbot-prompt.txt";


    public ChatBotPromptProvider(IHostEnvironment env, ILogger<ChatBotPromptProvider> logger)
    {
        _env = env;
        _logger = logger;
    }

    public string GetPrompt(string userContext)
    {
        lock (_lock)
        {
            return userContext?.ToLower() switch
            {
                "admin" => _adminPrompt
                    ??= ReadFile(AdminPromptFile),

                "servicepartner" => _servicePartnerPrompt
                    ??= ReadFile(ServicePartnerPromptFile),

                _ => _customerPrompt
                    ??= ReadFile(CustomerPromptFile)
            };
        }
    }

    public void ReloadPrompts()
    {
        lock (_lock)
        {
            _customerPrompt = null;
            _adminPrompt = null;
            _servicePartnerPrompt = null;

        }
    }

    private string ReadFile(string fileName)
    {
        var path = Path.Combine(_env.ContentRootPath, "Templates", fileName);

        if (!File.Exists(path))
        {
            _logger.LogError("ChatBot prompt file not found at: {Path}", path);
            return "You are a helpful assistant for HomeCare.";
        }

        _logger.LogInformation("ChatBot prompt loaded: {File}", fileName);
        return File.ReadAllText(path);
    }
}