using Microsoft.AspNetCore.Http;

namespace Homecare.Application.DTOs.ServicePartner;

public class UpdateServicePartnerProfileRequestDto
{
    public string? PersonalDetail { get; set; }
    public string? EducationInfoList { get; set; }
    public string? ProfessionalInfoList { get; set; }
    public string? LanguageList { get; set; }
    public string? RemovedDocumentIds { get; set; }
    public IFormFile? ProfileImage { get; set; }
    public List<IFormFile> AttachmentFiles { get; set; } = new();
}