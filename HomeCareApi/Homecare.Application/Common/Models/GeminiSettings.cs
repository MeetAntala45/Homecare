namespace Homecare.Application.Common.Models;

public class GeminiSettings
{
    public string ApiKey { get; set; } = null!;
    public string Model { get; set; } = "gemini-3.1-flash-lite-preview";
    public int MaxOutputTokens { get; set; } = 400;
    public double Temperature { get; set; } = 0.65;
}