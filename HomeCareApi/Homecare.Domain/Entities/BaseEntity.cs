namespace Homecare.Domain.Entities;

public class BaseEntity
{
    public DateTime CreatedOn { get; private set; }
    public int CreatedBy { get; private set; }
    public DateTime? ModifiedOn { get; private set; }
    public int ModifiedBy { get; private set; }
    public bool IsDeleted { get; private set; } = false;
    public bool IsActive { get; private set; } = true;

    public void SetCreated(int userId)
    {
        CreatedOn = DateTime.UtcNow;
        CreatedBy = userId;
    }

    public void SetModified(int userId)
    {
        ModifiedOn = DateTime.UtcNow;
        ModifiedBy = userId;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void SoftDelete(bool v)
    {
        IsDeleted = true;
        IsActive = false;
    }
}
