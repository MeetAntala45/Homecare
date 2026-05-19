namespace Homecare.Domain.Entities
{
    public class ServiceImage
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }  
        public bool IsDeleted { get; set; } = false;
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }

        public virtual Service Service { get; set; } = null!;
    }
}