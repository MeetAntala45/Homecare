namespace Homecare.Application.DTOs.ChatBot;

public class ChatMessageDto
{
    public string Role { get; set; } = null!;
    public string Text { get; set; } = null!;
}

public class ChatRequestDto
{
    public string Message { get; set; } = null!;
    public List<ChatMessageDto> History { get; set; } = new();
    public string UserContext { get; set; } = "customer";
}

public class ChatResponseDto
{
    public string Reply { get; set; } = null!;
}