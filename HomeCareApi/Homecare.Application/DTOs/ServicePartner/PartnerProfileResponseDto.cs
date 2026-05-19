namespace Homecare.Application.DTOs.ServicePartner;

public class PartnerProfileResponseDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? PermanentAddress { get; set; }
    public string? ResidentialAddress { get; set; }
    public string? ProfileImage { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public int ServiceTypeId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int StatusId { get; set; }
    public List<EducationInfoDto> Educations { get; set; } = new();
    public List<ProfessionalInfoDto> ProfessionalExperiences { get; set; } = new();
    public List<LanguageDto> Languages { get; set; } = new();
    public List<PartnerDocumentDto> Documents { get; set; } = new();
    public List<string> Skills { get; set; } = new();
    public List<string> ServicesOffered { get; set; } = new();
}

public class PartnerDocumentDto
{
    public int Id { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int FileSizeKb { get; set; }
    public string FileType { get; set; } = string.Empty;
}