namespace Homecare.Domain.Entities;

public class PartnerExperience : BaseEntity
{
    public int Id { get; set; }

    public int PartnerId { get; set; }

    public string CompanyName { get; set; }

    public string Role { get; set; }

    public DateTime FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public ServicePartner Partner { get; set; }
}