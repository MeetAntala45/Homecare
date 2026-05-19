namespace Homecare.Application.DTOs.Tracking;

public class UpdateLocationDto
{
    public int BookingId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
