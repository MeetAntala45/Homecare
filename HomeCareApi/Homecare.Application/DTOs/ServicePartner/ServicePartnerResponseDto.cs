namespace Homecare.Application.DTOs.ServicePartner;
 
public class ServicePartnerResponseDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string MobileNumber { get; set; } = null!;
    public string? ProfileImage { get; set; }
    public DateTime DateOfBirth { get; set; }
    public int GenderId { get; set; }
    public string Gender { get; set; } = null!;
    public int ServiceTypeId { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public string? PermanentAddress { get; set; }
    public string? ResidentialAddress { get; set; }
    public int StatusId { get; set; }
    public string Status { get; set; } = null!;
    public string CreatedAt { get; set; } = null!;
    public int JobsDone { get; set; }
    public List<PartnerEducationResponseDto> Educations { get; set; } = new();
    public List<PartnerExperienceResponseDto> Experiences { get; set; } = new();
    public List<int> SkillCategoryIds { get; set; } = new();
    public List<int> ServiceOfferedIds { get; set; } = new();
    public List<PartnerLanguageResponseDto> Languages { get; set; } = new();
    public List<PartnerDocumentResponseDto> Documents { get; set; } = new();
    public string ServicePartnerId { get; internal set; }
}
 
public class PartnerEducationResponseDto
{
    public int Id { get; set; }
    public string InstituteName { get; set; } = null!;
    public short PassingYear { get; set; }
    public decimal? MarksPercentage { get; set; }
}
 
public class PartnerExperienceResponseDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = null!;
    public string Role { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    public int YearsOfExperience{get;set;}
}
 
public class PartnerLanguageResponseDto
{
    public int Id { get; set; }
    public int LanguageId { get; set; }
    public string Language { get; set; } = null!;
    public int ProficiencyId { get; set; }
    public string Proficiency { get; set; } = null!;
}
 
public class PartnerDocumentResponseDto
{
    public int Id { get; set; }
    public string DocumentName { get; set; } = null!;
    public string FilePath { get; set; } = null!;
    public int FileSizeKb { get; set; }
    public string FileType { get; set; } = null!;
}