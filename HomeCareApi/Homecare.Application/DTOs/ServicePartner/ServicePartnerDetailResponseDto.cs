namespace Homecare.Application.DTOs.ServicePartner;

public class ServicePartnerDetailResponseDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string MobileNumber { get; set; } = null!;
    public string ServiceType { get; set; } = string.Empty;
    public string? ResidentialAddress { get; set; }
    public int StatusId { get; set; }
    public double TotalExperienceYears { get; set; }

    public List<PartnerExperiencesResponseDto> Experiences { get; set; } = new();
    public List<string> Skills { get; set; } = new();
    public List<string> ServicesOffered { get; set; } = new();
    public List<PartnerLanguagesResponseDto> Languages { get; set; } = new();
    public List<PartnerDocumentsResponseDto> Documents { get; set; } = new();
    public decimal AverageRating { get; set; } = 0;
    public int TotalReviews { get; set; } = 0;
}
public class PartnerExperiencesResponseDto
{
    public string CompanyName { get; set; } = null!;
    public string Role { get; set; } = null!;
    public double YearsOfExperience { get; set; }
}

public class PartnerLanguagesResponseDto
{

    public string Language { get; set; } = null!;

}

public class PartnerDocumentsResponseDto
{
    public string DocumentName { get; set; } = null!;
    public string FilePath { get; set; } = null!;
    public int FileSizeKb { get; set; }
    public string FileType { get; set; } = null!;


}