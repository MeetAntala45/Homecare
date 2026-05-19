namespace Homecare.Domain.Entities;

public class ServicePartner : BaseEntity
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string MobileNumber { get; set; } = null!;

    public string? ProfileImage { get; set; }

    public DateTime DateOfBirth { get; set; }

    public Gender Gender { get; set; }

    public int ServiceTypeId { get; set; }

    public string PermanentAddress { get; set; } = null!;

    public string ResidentialAddress { get; set; } = null!;

    public PartnerStatus Status { get; set; }
    public int JobsCompletedCount { get; set; } = 0;
    public int? ApprovedBy { get; set; }

    public DateTime? ApprovedOn { get; set; }

    public ICollection<PartnerEducation> Educations { get; set; } = new List<PartnerEducation>();
    public ICollection<PartnerExperience> Experiences { get; set; } = new List<PartnerExperience>();
    public ICollection<PartnerSkill> Skills { get; set; } = new List<PartnerSkill>();
    public ICollection<PartnerServiceOffered> ServicesOffered { get; set; } = new List<PartnerServiceOffered>();
    public ICollection<PartnerLanguage> Languages { get; set; } = new List<PartnerLanguage>();
    public ICollection<PartnerDocument> Documents { get; set; } = new List<PartnerDocument>();
    public ICollection<PartnerOtpVerification> PartnerOtpVerifications { get; set; } = new List<PartnerOtpVerification>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<PartnerLeave> Leaves { get; set; } = new List<PartnerLeave>();
}