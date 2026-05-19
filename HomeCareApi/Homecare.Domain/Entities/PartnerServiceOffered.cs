namespace Homecare.Domain.Entities;

public class PartnerServiceOffered : BaseEntity
{
    public int Id { get; set; }

    public int PartnerId { get; set; }

    public int SubCategoryId { get; set; }

    public ServicePartner Partner { get; set; } = null!;

    public SubCategory SubCategory { get; set; } = null!;
}