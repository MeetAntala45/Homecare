namespace Homecare.Domain.Entities;

public class RecentSearch
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string DisplayName { get; set; } = string.Empty; 
    public decimal Latitude { get; set; }                       
    public decimal Longitude { get; set; }                         
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
}