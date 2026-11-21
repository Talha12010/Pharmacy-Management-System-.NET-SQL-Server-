using PharmacyManagement.Domain.Common;
using PharmacyManagement.Domain.Enums;

namespace PharmacyManagement.Domain.Entities;

public class Branch : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public ICollection<MedicineBatch> Batches { get; set; } = new List<MedicineBatch>();
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}

public class Employee : SoftDeletableEntity
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string Position { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public decimal Salary { get; set; }
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;
    public int BranchId { get; set; }
    public Branch Branch { get; set; } = null!;
    public Guid? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    public string? Notes { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}

public class SystemSetting
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Group { get; set; } = "General";
    public bool IsEditable { get; set; } = true;
    public DateTime UpdatedAtUtc { get; set; }
    public Guid? UpdatedByUserId { get; set; }
}

public class DocumentSequence
{
    public int Id { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string PeriodKey { get; set; } = string.Empty;
    public int LastNumber { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}

public class AuditLog
{
    public long Id { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
