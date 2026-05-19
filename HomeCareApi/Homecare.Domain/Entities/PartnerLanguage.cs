namespace Homecare.Domain.Entities;

public class PartnerLanguage : BaseEntity
{
    public int Id { get; set; }

    public int PartnerId { get; set; }

    public Language Language { get; set; }

    public Proficiency Proficiency { get; set; }

    public ServicePartner Partner { get; set; }
}