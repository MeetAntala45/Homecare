using System;

namespace Homecare.Application.DTOs.CustomerProfile;

public class OpenCageResponseDto
{
    public List<OpenCageResult>? results { get; set; }
}
public class OpenCageResult
{
    public OpenCageComponents? components { get; set; }
}

public class OpenCageComponents
{
    public string? city { get; set; }
    public string? town { get; set; }
    public string? village { get; set; }
    public string? hamlet { get; set; }
    public string? municipality { get; set; }
    public string? suburb { get; set; }
    public string? neighbourhood { get; set; }
    public string? city_district { get; set; }

    public string? district { get; set; }
    public string? county { get; set; }
    public string? state_district { get; set; }
    public string? state { get; set; }
    public string? region { get; set; }
    public string? province { get; set; }
}
