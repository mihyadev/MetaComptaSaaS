using MetaCompta.Application.DTOs;
using MetaCompta.Application.Interfaces;
using MetaCompta.Domain.Entities;
using MetaCompta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MetaCompta.Application.Services;

/// <summary>
/// Department management service implementation.
/// </summary>
public class DepartmentService : IDepartmentService
{
    private readonly MetaComptaDbContext _db;
    private readonly ILogger<DepartmentService> _logger;

    public DepartmentService(MetaComptaDbContext db, ILogger<DepartmentService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<ApiResponse<List<DepartmentDto>>> GetAllAsync(int cabinetId)
    {
        var departments = await _db.Departments
            .Where(d => d.CabinetId == cabinetId)
            .Include(d => d.Members)
            .Include(d => d.HeadOfDepartment)
            .OrderBy(d => d.Name)
            .Select(d => new DepartmentDto
            {
                Id = d.Id,
                Name = d.Name,
                Code = d.Code,
                Description = d.Description,
                IsActive = d.IsActive,
                HeadOfDepartmentUserId = d.HeadOfDepartmentUserId,
                HeadOfDepartmentName = d.HeadOfDepartment != null ? d.HeadOfDepartment.FirstName + " " + d.HeadOfDepartment.LastName : null,
                MemberCount = d.Members.Count(m => !m.IsDeleted)
            })
            .ToListAsync();

        return ApiResponse<List<DepartmentDto>>.Ok(departments);
    }

    public async Task<ApiResponse<DepartmentDto>> GetByIdAsync(int id, int cabinetId)
    {
        var d = await _db.Departments
            .Where(d => d.Id == id && d.CabinetId == cabinetId)
            .Include(d => d.Members).ThenInclude(m => m.User)
            .Include(d => d.HeadOfDepartment)
            .FirstOrDefaultAsync();

        if (d == null)
            return ApiResponse<DepartmentDto>.Fail("Département non trouvé.");

        var dto = new DepartmentDto
        {
            Id = d.Id,
            Name = d.Name,
            Code = d.Code,
            Description = d.Description,
            IsActive = d.IsActive,
            HeadOfDepartmentUserId = d.HeadOfDepartmentUserId,
            HeadOfDepartmentName = d.HeadOfDepartment != null ? d.HeadOfDepartment.FirstName + " " + d.HeadOfDepartment.LastName : null,
            MemberCount = d.Members.Count(m => !m.IsDeleted),
            Members = d.Members.Where(m => !m.IsDeleted).Select(m => new DepartmentMemberDto
            {
                Id = m.Id,
                DepartmentId = m.DepartmentId,
                DepartmentName = d.Name,
                UserId = m.UserId,
                UserName = m.User != null ? m.User.FirstName + " " + m.User.LastName : null,
                UserEmail = m.User?.Email,
                IsLead = m.IsLead,
                JoinedAt = m.JoinedAt
            }).ToList()
        };

        return ApiResponse<DepartmentDto>.Ok(dto);
    }

    public async Task<ApiResponse<DepartmentDto>> CreateAsync(DepartmentCreateDto dto, int cabinetId, string userId)
    {
        // Check code uniqueness
        var exists = await _db.Departments.AnyAsync(d => d.CabinetId == cabinetId && d.Code == dto.Code);
        if (exists)
            return ApiResponse<DepartmentDto>.Fail($"Un département avec le code '{dto.Code}' existe déjà.");

        var department = new Department
        {
            Name = dto.Name,
            Code = dto.Code.ToUpperInvariant(),
            Description = dto.Description,
            HeadOfDepartmentUserId = dto.HeadOfDepartmentUserId,
            CabinetId = cabinetId,
            CreatedBy = userId,
            IsActive = true
        };

        _db.Departments.Add(department);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Department {Code} created by {UserId} in cabinet {CabinetId}", dto.Code, userId, cabinetId);
        return await GetByIdAsync(department.Id, cabinetId);
    }

    public async Task<ApiResponse<DepartmentDto>> UpdateAsync(int id, DepartmentCreateDto dto, int cabinetId, string userId)
    {
        var department = await _db.Departments.FirstOrDefaultAsync(d => d.Id == id && d.CabinetId == cabinetId);
        if (department == null)
            return ApiResponse<DepartmentDto>.Fail("Département non trouvé.");

        var codeExists = await _db.Departments.AnyAsync(d => d.CabinetId == cabinetId && d.Code == dto.Code && d.Id != id);
        if (codeExists)
            return ApiResponse<DepartmentDto>.Fail($"Un autre département avec le code '{dto.Code}' existe déjà.");

        department.Name = dto.Name;
        department.Code = dto.Code.ToUpperInvariant();
        department.Description = dto.Description;
        department.HeadOfDepartmentUserId = dto.HeadOfDepartmentUserId;
        department.UpdatedBy = userId;

        await _db.SaveChangesAsync();
        return await GetByIdAsync(id, cabinetId);
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id, int cabinetId, string userId)
    {
        var department = await _db.Departments.FirstOrDefaultAsync(d => d.Id == id && d.CabinetId == cabinetId);
        if (department == null)
            return ApiResponse<bool>.Fail("Département non trouvé.");

        department.IsDeleted = true;
        department.DeletedAt = DateTime.UtcNow;
        department.DeletedBy = userId;
        await _db.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Département supprimé.");
    }

    public async Task<ApiResponse<bool>> AddMemberAsync(int departmentId, string userId, bool isLead, int cabinetId, string actionUserId)
    {
        var department = await _db.Departments.FirstOrDefaultAsync(d => d.Id == departmentId && d.CabinetId == cabinetId);
        if (department == null)
            return ApiResponse<bool>.Fail("Département non trouvé.");

        var exists = await _db.DepartmentMembers.AnyAsync(dm => dm.DepartmentId == departmentId && dm.UserId == userId && !dm.IsDeleted);
        if (exists)
            return ApiResponse<bool>.Fail("Cet utilisateur est déjà membre de ce département.");

        _db.DepartmentMembers.Add(new DepartmentMember
        {
            DepartmentId = departmentId,
            UserId = userId,
            IsLead = isLead,
            CabinetId = cabinetId,
            CreatedBy = actionUserId
        });
        await _db.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Membre ajouté.");
    }

    public async Task<ApiResponse<bool>> RemoveMemberAsync(int departmentId, string userId, int cabinetId, string actionUserId)
    {
        var member = await _db.DepartmentMembers
            .FirstOrDefaultAsync(dm => dm.DepartmentId == departmentId && dm.UserId == userId && dm.CabinetId == cabinetId && !dm.IsDeleted);

        if (member == null)
            return ApiResponse<bool>.Fail("Membre non trouvé dans ce département.");

        member.IsDeleted = true;
        member.DeletedAt = DateTime.UtcNow;
        member.DeletedBy = actionUserId;
        await _db.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Membre retiré.");
    }

    public async Task<ApiResponse<List<DepartmentMemberDto>>> GetMembersAsync(int departmentId, int cabinetId)
    {
        var members = await _db.DepartmentMembers
            .Where(dm => dm.DepartmentId == departmentId && dm.CabinetId == cabinetId)
            .Include(dm => dm.User)
            .Include(dm => dm.Department)
            .Select(dm => new DepartmentMemberDto
            {
                Id = dm.Id,
                DepartmentId = dm.DepartmentId,
                DepartmentName = dm.Department != null ? dm.Department.Name : null,
                UserId = dm.UserId,
                UserName = dm.User != null ? dm.User.FirstName + " " + dm.User.LastName : null,
                UserEmail = dm.User != null ? dm.User.Email : null,
                IsLead = dm.IsLead,
                JoinedAt = dm.JoinedAt
            })
            .ToListAsync();

        return ApiResponse<List<DepartmentMemberDto>>.Ok(members);
    }
}
