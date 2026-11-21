using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.Common.Interfaces;
using PharmacyManagement.Application.DTOs.Audit;
using PharmacyManagement.Application.DTOs.Organization;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Application.Mappings;
using PharmacyManagement.Domain.Entities;
using PharmacyManagement.Domain.Exceptions;

namespace PharmacyManagement.Application.Services;

public sealed class BranchService : IBranchService
{
    private readonly IApplicationDbContext _db;
    private readonly IAuditService _audit;

    public BranchService(IApplicationDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<IReadOnlyList<BranchDto>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _db.Branches.AsNoTracking();
        if (!includeInactive)
        {
            query = query.Where(b => b.IsActive);
        }

        return await query.OrderBy(b => b.Name).Select(OrganizationMappings.BranchProjection).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LookupDto>> GetLookupAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Branches.AsNoTracking()
            .Where(b => b.IsActive)
            .OrderBy(b => b.Name)
            .Select(b => new LookupDto { Id = b.Id, Name = b.Name })
            .ToListAsync(cancellationToken);
    }

    public async Task<BranchDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var branch = await _db.Branches.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id, cancellationToken)
            ?? throw new NotFoundException("Branch", id);
        return branch.ToDto();
    }

    public async Task<BranchDto> CreateAsync(CreateBranchRequest request, CancellationToken cancellationToken = default)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        if (await _db.Branches.AnyAsync(b => b.Code == code, cancellationToken))
        {
            throw new ConflictException($"A branch with code '{code}' already exists.");
        }

        var branch = new Branch
        {
            Name = request.Name.Trim(),
            Code = code,
            Address = request.Address?.Trim(),
            Phone = request.Phone?.Trim(),
            Email = request.Email?.Trim(),
            IsActive = true
        };

        _db.Branches.Add(branch);
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("BranchCreated", nameof(Branch), branch.Id.ToString(), null, branch.ToDto(), cancellationToken);
        return branch.ToDto();
    }

    public async Task<BranchDto> UpdateAsync(int id, UpdateBranchRequest request, CancellationToken cancellationToken = default)
    {
        var branch = await _db.Branches.FirstOrDefaultAsync(b => b.Id == id, cancellationToken)
            ?? throw new NotFoundException("Branch", id);

        var code = request.Code.Trim().ToUpperInvariant();
        if (await _db.Branches.AnyAsync(b => b.Id != id && b.Code == code, cancellationToken))
        {
            throw new ConflictException($"A branch with code '{code}' already exists.");
        }

        var before = branch.ToDto();
        branch.Name = request.Name.Trim();
        branch.Code = code;
        branch.Address = request.Address?.Trim();
        branch.Phone = request.Phone?.Trim();
        branch.Email = request.Email?.Trim();
        branch.IsActive = request.IsActive;

        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("BranchUpdated", nameof(Branch), branch.Id.ToString(), before, branch.ToDto(), cancellationToken);
        return branch.ToDto();
    }

    public async Task DeactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        var branch = await _db.Branches.FirstOrDefaultAsync(b => b.Id == id, cancellationToken)
            ?? throw new NotFoundException("Branch", id);

        branch.IsActive = false;
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("BranchDeactivated", nameof(Branch), branch.Id.ToString(), new { IsActive = true }, new { IsActive = false }, cancellationToken);
    }
}

public sealed class EmployeeService : IEmployeeService
{
    private readonly IApplicationDbContext _db;
    private readonly IAuditService _audit;
    private readonly IAuditQueryService _auditQuery;

    public EmployeeService(IApplicationDbContext db, IAuditService audit, IAuditQueryService auditQuery)
    {
        _db = db;
        _audit = audit;
        _auditQuery = auditQuery;
    }

    public async Task<PagedResult<EmployeeDto>> GetPagedAsync(QueryParameters query, int? branchId, CancellationToken cancellationToken = default)
    {
        var employees = _db.Employees.AsNoTracking()
            .Include(e => e.Branch)
            .Include(e => e.User)
            .AsQueryable();

        if (branchId.HasValue)
        {
            employees = employees.Where(e => e.BranchId == branchId);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            employees = employees.Where(e =>
                e.EmployeeCode.Contains(term) ||
                e.FirstName.Contains(term) ||
                e.LastName.Contains(term) ||
                e.Position.Contains(term) ||
                (e.Phone != null && e.Phone.Contains(term)) ||
                (e.Email != null && e.Email.Contains(term)));
        }

        employees = query.SortDesc
            ? employees.OrderByDescending(e => e.FirstName)
            : employees.OrderBy(e => e.FirstName);

        var total = await employees.CountAsync(cancellationToken);
        var items = await employees
            .Skip((Math.Max(1, query.Page) - 1) * Math.Clamp(query.PageSize, 1, 200))
            .Take(Math.Clamp(query.PageSize, 1, 200))
            .ToListAsync(cancellationToken);

        return PagedResult<EmployeeDto>.Create(items.Select(e => e.ToDto()).ToList(), Math.Max(1, query.Page), Math.Clamp(query.PageSize, 1, 200), total);
    }

    public async Task<EmployeeDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var employee = await _db.Employees.AsNoTracking()
            .Include(e => e.Branch)
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new NotFoundException("Employee", id);
        return employee.ToDto();
    }

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        var code = request.EmployeeCode.Trim().ToUpperInvariant();
        if (await _db.Employees.AnyAsync(e => e.EmployeeCode == code, cancellationToken))
        {
            throw new ConflictException($"An employee with code '{code}' already exists.");
        }

        if (!await _db.Branches.AnyAsync(b => b.Id == request.BranchId, cancellationToken))
        {
            throw new NotFoundException("Branch", request.BranchId);
        }

        if (request.UserId.HasValue && !await _db.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken))
        {
            throw new NotFoundException("User", request.UserId);
        }

        var employee = new Employee
        {
            EmployeeCode = code,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Phone = request.Phone?.Trim(),
            Email = request.Email?.Trim(),
            Address = request.Address?.Trim(),
            Position = request.Position.Trim(),
            HireDate = request.HireDate,
            Salary = request.Salary,
            Status = request.Status,
            BranchId = request.BranchId,
            UserId = request.UserId,
            Notes = request.Notes?.Trim()
        };

        _db.Employees.Add(employee);
        await _db.SaveChangesAsync(cancellationToken);
        var dto = await GetByIdAsync(employee.Id, cancellationToken);
        await _audit.LogAsync("EmployeeCreated", nameof(Employee), employee.Id.ToString(), null, dto, cancellationToken);
        return dto;
    }

    public async Task<EmployeeDto> UpdateAsync(int id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new NotFoundException("Employee", id);

        var code = request.EmployeeCode.Trim().ToUpperInvariant();
        if (await _db.Employees.AnyAsync(e => e.Id != id && e.EmployeeCode == code, cancellationToken))
        {
            throw new ConflictException($"An employee with code '{code}' already exists.");
        }

        if (!await _db.Branches.AnyAsync(b => b.Id == request.BranchId, cancellationToken))
        {
            throw new NotFoundException("Branch", request.BranchId);
        }

        if (request.UserId.HasValue && !await _db.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken))
        {
            throw new NotFoundException("User", request.UserId);
        }

        var before = (await GetByIdAsync(id, cancellationToken));
        employee.EmployeeCode = code;
        employee.FirstName = request.FirstName.Trim();
        employee.LastName = request.LastName.Trim();
        employee.Phone = request.Phone?.Trim();
        employee.Email = request.Email?.Trim();
        employee.Address = request.Address?.Trim();
        employee.Position = request.Position.Trim();
        employee.HireDate = request.HireDate;
        employee.Salary = request.Salary;
        employee.Status = request.Status;
        employee.BranchId = request.BranchId;
        employee.UserId = request.UserId;
        employee.Notes = request.Notes?.Trim();

        await _db.SaveChangesAsync(cancellationToken);
        var after = await GetByIdAsync(id, cancellationToken);
        await _audit.LogAsync("EmployeeUpdated", nameof(Employee), id.ToString(), before, after, cancellationToken);
        return after;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new NotFoundException("Employee", id);

        employee.IsDeleted = true;
        employee.DeletedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("EmployeeDeleted", nameof(Employee), id.ToString(), null, new { employee.EmployeeCode }, cancellationToken);
    }

    public Task<PagedResult<AuditLogDto>> GetActivityAsync(Guid userId, QueryParameters query, CancellationToken cancellationToken = default)
    {
        return _auditQuery.GetPagedAsync(query, userId, null, null, null, null, cancellationToken);
    }
}
