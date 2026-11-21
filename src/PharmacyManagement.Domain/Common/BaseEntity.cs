namespace PharmacyManagement.Domain.Common;

public abstract class BaseEntity
{
    public int Id { get; set; }
}

public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAtUtc { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public Guid? UpdatedByUserId { get; set; }
}

public abstract class SoftDeletableEntity : AuditableEntity, Interfaces.ISoftDeletable
{
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
}
