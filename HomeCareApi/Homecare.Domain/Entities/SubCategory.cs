namespace Homecare.Domain.Entities;

public class SubCategory : BaseEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
    }
