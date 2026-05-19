using Homecare.Application.Constants;
using Homecare.Application.DTOs.ChatBot;

namespace Homecare.Application.Interfaces.ChatBot;

public interface IChatBotService
{
    Task<ApiResponse<ChatResponseDto>> GetReplyAsync(ChatRequestDto request);
}