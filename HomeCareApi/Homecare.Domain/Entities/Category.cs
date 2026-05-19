
namespace Homecare.Domain.Entities;

public class Category : BaseEntity
{
    public int Id {get; set;}
    public string Name {get; set;} = string.Empty;
    public int ServiceTypeId {get; set;}
}
