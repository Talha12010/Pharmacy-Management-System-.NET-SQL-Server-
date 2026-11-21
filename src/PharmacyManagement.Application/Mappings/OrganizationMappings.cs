using System.Linq.Expressions;
using PharmacyManagement.Application.DTOs.Audit;
using PharmacyManagement.Application.DTOs.Organization;
using PharmacyManagement.Application.DTOs.Settings;
using PharmacyManagement.Application.DTOs.Users;
using PharmacyManagement.Domain.Entities;

namespace PharmacyManagement.Application.Mappings;

public static class OrganizationMappings
{
    public static readonly Expression<Func<Branch, BranchDto>> BranchProjection = b => new BranchDto
    {
        Id = b.Id,
        Name = b.Name,
        Code = b.Code,
        Address = b.Address,
        Phone = b.Phone,
        Email = b.Email,
        IsActive = b.IsActive
    };

    public static BranchDto ToDto(this Branch branch) => BranchProjection.Compile()(branch);

    public static LookupDto ToLookup(this Branch branch) => new() { Id = branch.Id, Name = branch.Name };

    public static EmployeeDto ToDto(this Employee employee) => new()
    {
        Id = employee.Id,
        EmployeeCode = employee.EmployeeCode,
        FirstName = employee.FirstName,
        LastName = employee.LastName,
        Phone = employee.Phone,
        Email = employee.Email,
        Address = employee.Address,
        Position = employee.Position,
        HireDate = employee.HireDate,
        Salary = employee.Salary,
        Status = employee.Status,
        BranchId = employee.BranchId,
        BranchName = employee.Branch?.Name ?? string.Empty,
        UserId = employee.UserId,
        UserEmail = employee.User?.Email,
        Notes = employee.Notes
    };

    public static UserDto ToDto(this ApplicationUser user, IReadOnlyList<string>? roles = null) => new()
    {
        Id = user.Id,
        Email = user.Email ?? string.Empty,
        FirstName = user.FirstName,
        LastName = user.LastName,
        PhoneNumber = user.PhoneNumber,
        BranchId = user.BranchId,
        BranchName = user.Branch?.Name,
        IsActive = user.IsActive,
        CreatedAtUtc = user.CreatedAtUtc,
        LastLoginAtUtc = user.LastLoginAtUtc,
        Roles = roles ?? Array.Empty<string>()
    };

    public static AuditLogDto ToDto(this AuditLog log) => new()
    {
        Id = log.Id,
        UserId = log.UserId,
        UserName = log.UserName,
        Action = log.Action,
        EntityName = log.EntityName,
        EntityId = log.EntityId,
        OldValues = log.OldValues,
        NewValues = log.NewValues,
        IpAddress = log.IpAddress,
        UserAgent = log.UserAgent,
        CreatedAtUtc = log.CreatedAtUtc
    };

    public static SystemSettingDto ToDto(this SystemSetting setting) => new()
    {
        Id = setting.Id,
        Key = setting.Key,
        Value = setting.Value,
        Description = setting.Description,
        Group = setting.Group,
        IsEditable = setting.IsEditable,
        UpdatedAtUtc = setting.UpdatedAtUtc
    };
}
