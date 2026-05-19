namespace Homecare.Application.DTOs.Dashboard;

public class TopServicePartnersResponseDto
{
    public string Name { get; set; } = null!;
    public string? ProfileImage { get; set; }
    public string ServiceTypeName { get; set; } = null!;
    public int JobsCompleted { get; set; }
}
