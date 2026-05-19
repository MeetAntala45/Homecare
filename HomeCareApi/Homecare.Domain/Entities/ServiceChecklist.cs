using Homecare.Domain.Enums;

namespace Homecare.Domain.Entities
{
    public class ServiceChecklist
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public ChecklistType Type { get; set; }
        public string ItemText { get; set; } = string.Empty;  
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
         public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool IsDeleted { get; set; } = false;

        public virtual Service Service { get; set; } = null!;
    }
}