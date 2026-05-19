namespace Homecare.Domain.Entities;

public class Address
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string? DisplayName { get; set; }
    public string HouseFlatNo { get; set; } = null!;
    public string Landmark { get; set; } = null!;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string Label { get; set; } = "Home";
     public string City { get; set; } = string.Empty; 
     public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public Customer Customer {get; set;} = null!;
}