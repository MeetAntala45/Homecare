namespace Homecare.Domain.Entities;

public class CouponConditionType
{
    public int Id { get; set; }

    // visible name in admin UI
    public string Label { get; set; } = null!;

    // key used in evaluation
    public string ContextKey { get; set; } = null!;

    // determines input UI
    public string InputType { get; set; } = null!;

    // eq, gte, in etc
    public string DefaultOperator { get; set; } = null!;

    public string DefaultFailBehaviour { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

}
