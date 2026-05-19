namespace Homecare.Domain.Entities
{
    public class Service
    {
        public int Id { get; set; }
        public int SubCategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal CommissionPct { get; set; }
        public int DurationMin { get; set; }
        public string Description { get; set; }
        public bool IsAvailable { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool IsDeleted { get; set; } = false;
       

        public virtual SubCategory SubCategory { get; set; } = null!;
        public virtual List<ServiceImage> ServiceImages { get; set; } = new List<ServiceImage>();
        public virtual List<ServiceChecklist> ServiceChecklists { get; set; } = new List<ServiceChecklist>();
    }
}