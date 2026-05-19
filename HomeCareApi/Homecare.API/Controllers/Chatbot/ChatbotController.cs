using Homecare.Application.Constants;
using Homecare.Application.DTOs.ChatBot;
using Homecare.Application.Interfaces.ChatBot;
using Microsoft.AspNetCore.Mvc;

namespace Homecare.API.Controllers.ChatBot;

[ApiController]
[Route("api/chatbot")]
public class ChatBotController : ControllerBase
{
    private readonly IChatBotService _chatBotService;

    public ChatBotController(IChatBotService chatBotService)
    {
        _chatBotService = chatBotService;
    }

    [HttpPost("reply")]
    public async Task<ApiResponse<ChatResponseDto>> GetReply([FromBody] ChatRequestDto request)
    {
        return await _chatBotService.GetReplyAsync(request);
    }
}