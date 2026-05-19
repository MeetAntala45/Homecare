namespace Homecare.Domain.Entities;

public class ServiceType : BaseEntity
{
    public int Id {get; set;}
    public string Name {get; set;} = string.Empty;
    public string? ImagePath {get; set;}

}
