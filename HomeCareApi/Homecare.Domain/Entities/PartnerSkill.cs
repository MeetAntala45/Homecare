namespace Homecare.Domain.Entities;

public class PartnerSkill : BaseEntity
{
    public int Id { get; set; }

    public int PartnerId { get; set; }

    public int CategoryId { get; set; }

    public ServicePartner Partner { get; set; } = null!;

    public Category Category { get; set; } = null!;
}