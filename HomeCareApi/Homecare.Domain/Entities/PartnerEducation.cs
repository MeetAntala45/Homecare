namespace Homecare.Domain.Entities;

public class PartnerEducation : BaseEntity
{
    public int Id { get; set; }

    public int PartnerId { get; set; }

    public string InstituteName { get; set; }

    public short PassingYear { get; set; }

    public decimal? MarksPercentage { get; set; }

    public ServicePartner Partner { get; set; }
}