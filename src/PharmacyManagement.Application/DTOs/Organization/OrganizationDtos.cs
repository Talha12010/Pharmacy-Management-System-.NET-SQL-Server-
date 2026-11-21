using PharmacyManagement.Domain.Enums;

namespace PharmacyManagement.Application.DTOs.Organization;

public sealed class BranchDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; }
}

public sealed class CreateBranchRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}

public sealed class UpdateBranchRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class EmployeeDto
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string Position { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public decimal Salary { get; set; }
    public EmployeeStatus Status { get; set; }
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? Notes { get; set; }
}

public class CreateEmployeeRequest
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
    public Guid? UserId { get; set; }
    public string? Notes { get; set; }
}

public sealed class UpdateEmployeeRequest : CreateEmployeeRequest
{
}

public sealed class LookupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
