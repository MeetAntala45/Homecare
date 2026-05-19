using System.Text;
using System.Text.Json;
using Homecare.Application.Common.Models;
using Homecare.Application.Constants;
using Homecare.Application.Constants.ChatBot;
using Homecare.Application.DTOs.ChatBot;
using Homecare.Application.Interfaces.ChatBot;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Homecare.Application.Services.ChatBot;

public class ChatBotService : IChatBotService
{
    private readonly HttpClient _httpClient;
    private readonly GeminiSettings _settings;
    private readonly ChatBotPromptProvider _promptProvider;
    private readonly ILogger<ChatBotService> _logger;

    public ChatBotService(
        HttpClient httpClient,
        IOptions<GeminiSettings> settings,
        ChatBotPromptProvider promptProvider,
        ILogger<ChatBotService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _promptProvider = promptProvider;
        _logger = logger;
    }

    public async Task<ApiResponse<ChatResponseDto>> GetReplyAsync(ChatRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return ApiResponse<ChatResponseDto>.Fail(ChatBotMessages.EmptyMessage);

        try
        {
            var contents = new List<object>();

            foreach (var msg in request.History)
            {
                var role = msg.Role?.ToLower() == "user" ? "user" : "model";
                contents.Add(new
                {
                    role,
                    parts = new[] { new { text = msg.Text } }
                });
            }

            contents.Add(new
            {
                role = "user",
                parts = new[] { new { text = request.Message } }
            });

            var prompt = _promptProvider.GetPrompt(request.UserContext);

            var payload = new
            {
                system_instruction = new
                {
                    parts = new[] { new { text = prompt } }
                },
                contents,
                generationConfig = new
                {
                    temperature = _settings.Temperature,
                    maxOutputTokens = _settings.MaxOutputTokens
                }
            };

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_settings.Model}:generateContent?key={_settings.ApiKey}";

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var httpResponse = await _httpClient.PostAsync(url, jsonContent);
            var responseBody = await httpResponse.Content.ReadAsStringAsync();

            if (!httpResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Gemini API error. Status: {Status}", httpResponse.StatusCode);
                return ApiResponse<ChatResponseDto>.Fail(ChatBotMessages.GeminiError);
            }

            using var doc = JsonDocument.Parse(responseBody);
            var replyText = doc
                .RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrWhiteSpace(replyText))
                return ApiResponse<ChatResponseDto>.Fail(ChatBotMessages.GeminiError);

            return ApiResponse<ChatResponseDto>.SuccessResponse(
                ChatBotMessages.ReplySent,
                new ChatResponseDto { Reply = replyText.Trim() }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in ChatBotService");
            return ApiResponse<ChatResponseDto>.Fail(ChatBotMessages.GeminiError);
        }
    }
}