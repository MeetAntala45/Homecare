using System.ComponentModel.DataAnnotations;

namespace Homecare.Application.DTOs.CustomerProfile;

public class AddressRequestDto
{
    [Required(ErrorMessage = "House/Flat number is required.")]
    [MaxLength(100, ErrorMessage = "Max 100 Characters allowed.")]
    public string HouseFlatNo { get; set; } = null!;

    [Required(ErrorMessage = "Landmark is required.")]
    [MaxLength(150, ErrorMessage = "Max 150 Characters allowed.")]
    public string Landmark { get; set; } = null!;

    [Required(ErrorMessage = "Label is required.")]
    [MaxLength(100, ErrorMessage = "Max 100 Characters allowed.")]
    public string Label { get; set; } = null!;
    public string? DisplayName { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}