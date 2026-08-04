using AutoMapper;
using MetaCompta.Application.DTOs;
using MetaCompta.Application.Interfaces;
using MetaCompta.Domain.Common;
using MetaCompta.Domain.Entities;
using MetaCompta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MetaCompta.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly MetaComptaDbContext _db;
    private readonly IMapper _mapper;

    public EmployeeService(MetaComptaDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PagedResult<EmployeeDto>>> GetAllAsync(int cabinetId, int? clientId = null, int page = 1, int pageSize = 20, string? search = null)
    {
        var query = _db.Employees
            .Include(e => e.Client)
            .Where(e => e.CabinetId == cabinetId)
            .AsQueryable();

        if (clientId.HasValue)
            query = query.Where(e => e.ClientId == clientId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(e =>
                e.Nom.ToLower().Contains(s) ||
                e.Prenom.ToLower().Contains(s) ||
                e.CIN.ToLower().Contains(s) ||
                (e.NumeroCNSS != null && e.NumeroCNSS.ToLower().Contains(s)) ||
                (e.Poste != null && e.Poste.ToLower().Contains(s)));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(e => e.Nom).ThenBy(e => e.Prenom)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return ApiResponse<PagedResult<EmployeeDto>>.Ok(new PagedResult<EmployeeDto>
        {
            Items = _mapper.Map<List<EmployeeDto>>(items),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<EmployeeDto>> GetByIdAsync(int id, int cabinetId)
    {
        var employee = await _db.Employees
            .Include(e => e.Client)
            .FirstOrDefaultAsync(e => e.Id == id && e.CabinetId == cabinetId);

        if (employee == null)
            return ApiResponse<EmployeeDto>.Fail("Employé non trouvé.");

        return ApiResponse<EmployeeDto>.Ok(_mapper.Map<EmployeeDto>(employee));
    }

    public async Task<ApiResponse<EmployeeDto>> CreateAsync(EmployeeCreateDto dto, int cabinetId, string userId)
    {
        // Validate client belongs to cabinet
        var client = await _db.Clients.FirstOrDefaultAsync(c => c.Id == dto.ClientId && c.CabinetId == cabinetId);
        if (client == null)
            return ApiResponse<EmployeeDto>.Fail("Client non trouvé dans ce cabinet.");

        // Validate CIN uniqueness within cabinet
        if (await _db.Employees.AnyAsync(e => e.CabinetId == cabinetId && e.CIN == dto.CIN))
            return ApiResponse<EmployeeDto>.Fail("Un employé avec ce CIN existe déjà.");

        // Validate CNSS uniqueness within client
        if (!string.IsNullOrWhiteSpace(dto.NumeroCNSS) && await _db.Employees.AnyAsync(e => e.ClientId == dto.ClientId && e.NumeroCNSS == dto.NumeroCNSS))
            return ApiResponse<EmployeeDto>.Fail("Un employé avec ce numéro CNSS existe déjà pour ce client.");

        var employee = _mapper.Map<Employee>(dto);
        employee.CabinetId = cabinetId;
        employee.CreatedBy = userId;
        employee.Statut = dto.DateSortie.HasValue
            ? RefCodes.StatutEmploye.Inactif
            : RefCodes.StatutEmploye.Actif;

        _db.Employees.Add(employee);
        await _db.SaveChangesAsync();

        // Audit log
        _db.AuditLogs.Add(new Domain.Common.AuditLog
        {
            UserId = userId,
            UserName = userId,
            Action = "Create",
            EntityType = "Employee",
            EntityId = employee.Id.ToString(),
            CabinetId = cabinetId,
            NewValues = System.Text.Json.JsonSerializer.Serialize(dto)
        });
        await _db.SaveChangesAsync();

        // Reload with includes
        var created = await _db.Employees.Include(e => e.Client).FirstAsync(e => e.Id == employee.Id);
        return ApiResponse<EmployeeDto>.Ok(_mapper.Map<EmployeeDto>(created), "Employé créé avec succès.");
    }

    public async Task<ApiResponse<EmployeeDto>> UpdateAsync(int id, EmployeeCreateDto dto, int cabinetId, string userId)
    {
        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Id == id && e.CabinetId == cabinetId);
        if (employee == null)
            return ApiResponse<EmployeeDto>.Fail("Employé non trouvé.");

        _mapper.Map(dto, employee);
        employee.UpdatedBy = userId;
        employee.UpdatedAt = DateTime.UtcNow;
        if (dto.DateSortie.HasValue)
            employee.Statut = RefCodes.StatutEmploye.Inactif;
        else if (employee.Statut == RefCodes.StatutEmploye.Inactif)
            employee.Statut = RefCodes.StatutEmploye.Actif;

        await _db.SaveChangesAsync();

        // Audit log
        _db.AuditLogs.Add(new Domain.Common.AuditLog
        {
            UserId = userId,
            UserName = userId,
            Action = "Update",
            EntityType = "Employee",
            EntityId = id.ToString(),
            CabinetId = cabinetId,
            NewValues = System.Text.Json.JsonSerializer.Serialize(dto)
        });
        await _db.SaveChangesAsync();

        var updated = await _db.Employees.Include(e => e.Client).FirstAsync(e => e.Id == id);
        return ApiResponse<EmployeeDto>.Ok(_mapper.Map<EmployeeDto>(updated), "Employé modifié avec succès.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id, int cabinetId, string userId)
    {
        var employee = await _db.Employees
            .Include(e => e.PaySlips)
            .FirstOrDefaultAsync(e => e.Id == id && e.CabinetId == cabinetId);

        if (employee == null)
            return ApiResponse<bool>.Fail("Employé non trouvé.");

        if (employee.PaySlips.Any())
            return ApiResponse<bool>.Fail("Impossible de supprimer un employé avec des bulletins de paie associés.");

        employee.IsDeleted = true;
        employee.DeletedAt = DateTime.UtcNow;
        employee.DeletedBy = userId;
        employee.Statut = RefCodes.StatutEmploye.Inactif;

        await _db.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Employé supprimé avec succès.");
    }
}
