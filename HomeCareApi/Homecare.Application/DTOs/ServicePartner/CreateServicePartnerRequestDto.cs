using Microsoft.AspNetCore.Http;

namespace Homecare.Application.DTOs.ServicePartner;

public class CreateServicePartnerRequestDto
{
    public string PersonalDetail { get; set; } = string.Empty;
    public string EducationInfoList { get; set; } = string.Empty;
    public string ProfessionalInfoList { get; set; } = string.Empty;
    public string SkillExpertiseList { get; set; } = string.Empty;
    public string LanguageList { get; set; } = string.Empty;
    public IFormFile? ProfileImage { get; set; }
    public IFormFile[] AttachmentFiles { get; set; } = Array.Empty<IFormFile>();
}

public class PersonalDetailDto
{
    public string FullName { get; set; } = string.Empty;
    public string DateOfBirth { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ApplyingFor { get; set; } = string.Empty;
    public string? PermanentAddress { get; set; }
    public string? ResidentialAddress { get; set; }
}

public class EducationInfoDto
{
    public string SchoolCollege { get; set; } = string.Empty;
    public string PassingYear { get; set; } = string.Empty;
    public decimal? Marks { get; set; }
}

public class ProfessionalInfoDto
{
    public string? CompanyName { get; set; }
    public string? Role { get; set; }
    public string? FromDate { get; set; }
    public string? ToDate { get; set; }
}

public class SkillExpertiseDto
{
    public string CategoryId { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public List<string> SubCategories { get; set; } = new();
}

public class LanguageDto
{
    public string Language { get; set; } = string.Empty;
    public string Proficiency { get; set; } = string.Empty;
}