using MetaCompta.Application.DTOs;
using MetaCompta.Application.Interfaces;
using MetaCompta.Domain.Common;
using MetaCompta.Domain.Entities;
using MetaCompta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MetaCompta.Application.Services;

public class UnitTakeService : IUnitTakeService
{
    private readonly MetaComptaDbContext _db;
    private readonly ILogger<UnitTakeService> _logger;

    public UnitTakeService(MetaComptaDbContext db, ILogger<UnitTakeService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<ApiResponse<List<UnitTypeDto>>> GetUnitTypesAsync()
    {
        var types = await _db.UnitTypes.AsNoTracking()
            .Where(t => t.IsActive && !t.IsDeleted)
            .OrderBy(t => t.Name)
            .Select(t => new UnitTypeDto
            {
                Id = t.Id,
                Code = t.Code,
                Name = t.Name,
                Description = t.Description,
                EntityTable = t.EntityTable
            })
            .ToListAsync();
        return ApiResponse<List<UnitTypeDto>>.Ok(types);
    }

    public async Task<ApiResponse<UnitTakeDto?>> GetOpenTakeAsync(string unitTypeCode, int unitEntityId, int cabinetId)
    {
        var type = await ResolveTypeAsync(unitTypeCode);
        if (type == null)
            return ApiResponse<UnitTakeDto?>.Fail($"Type d'unité inconnu: {unitTypeCode}");

        var take = await _db.UnitTakes
            .AsNoTracking()
            .Include(t => t.TakenByUser)
            .Include(t => t.UnitType)
            .Where(t => t.CabinetId == cabinetId
                        && t.UnitTypeId == type.Id
                        && t.UnitEntityId == unitEntityId
                        && t.ClosedAt == null
                        && t.Status == UnitTakeStatuses.Open
                        && !t.IsDeleted)
            .OrderByDescending(t => t.TakenAt)
            .FirstOrDefaultAsync();

        return ApiResponse<UnitTakeDto?>.Ok(take == null ? null : Map(take));
    }

    public async Task<ApiResponse<UnitTakeDto>> OpenTakeAsync(OpenUnitTakeDto dto, int cabinetId, string userId)
    {
        if (string.IsNullOrWhiteSpace(dto.UnitTypeCode) || dto.UnitEntityId <= 0)
            return ApiResponse<UnitTakeDto>.Fail("Type d'unité et identifiant requis.");

        var type = await ResolveTypeAsync(dto.UnitTypeCode);
        if (type == null)
            return ApiResponse<UnitTakeDto>.Fail($"Type d'unité inconnu: {dto.UnitTypeCode}");

        var open = await _db.UnitTakes
            .Include(t => t.TakenByUser)
            .Where(t => t.CabinetId == cabinetId
                        && t.UnitTypeId == type.Id
                        && t.UnitEntityId == dto.UnitEntityId
                        && t.ClosedAt == null
                        && t.Status == UnitTakeStatuses.Open
                        && !t.IsDeleted)
            .OrderByDescending(t => t.TakenAt)
            .FirstOrDefaultAsync();

        if (open != null)
        {
            if (string.Equals(open.TakenByUserId, userId, StringComparison.OrdinalIgnoreCase))
            {
                open.UnitType ??= type;
                return ApiResponse<UnitTakeDto>.Ok(Map(open), "Unité déjà à votre charge.");
            }

            if (!dto.ForceRelease)
            {
                var who = open.TakenByUser != null
                    ? $"{open.TakenByUser.FirstName} {open.TakenByUser.LastName}".Trim()
                    : "un autre collaborateur";
                return ApiResponse<UnitTakeDto>.Fail(
                    $"Unité déjà prise par {who} depuis {open.TakenAt:dd/MM/yyyy HH:mm}. Fermez ou forcez la reprise.");
            }

            if (string.IsNullOrWhiteSpace(dto.ForceReason))
                return ApiResponse<UnitTakeDto>.Fail("Un motif est requis pour forcer la reprise.");

            open.ClosedAt = DateTime.UtcNow;
            open.Status = UnitTakeStatuses.Released;
            open.Comment = string.IsNullOrWhiteSpace(open.Comment)
                ? $"Reprise forcée: {dto.ForceReason}"
                : $"{open.Comment} | Reprise forcée: {dto.ForceReason}";
            open.UpdatedAt = DateTime.UtcNow;
            open.UpdatedBy = userId;
        }

        var take = new UnitTake
        {
            CabinetId = cabinetId,
            UnitTypeId = type.Id,
            UnitEntityId = dto.UnitEntityId,
            TakenByUserId = userId,
            TakenAt = DateTime.UtcNow,
            ClosedAt = null,
            Status = UnitTakeStatuses.Open,
            Comment = dto.Comment,
            TaskItemId = dto.TaskItemId,
            WorkflowStepInstanceId = dto.WorkflowStepInstanceId,
            DeclarationId = dto.DeclarationId,
            CreatedBy = userId
        };
        _db.UnitTakes.Add(take);
        await _db.SaveChangesAsync();

        await _db.Entry(take).Reference(t => t.TakenByUser).LoadAsync();
        take.UnitType = type;

        _logger.LogInformation(
            "UnitTake opened: type={Type} entity={EntityId} by={UserId}",
            type.Code, dto.UnitEntityId, userId);

        return ApiResponse<UnitTakeDto>.Ok(Map(take), "Unité prise en charge.");
    }

    public async Task<ApiResponse<UnitTakeDto>> CloseTakeAsync(CloseUnitTakeDto dto, int cabinetId, string userId)
    {
        var type = await ResolveTypeAsync(dto.UnitTypeCode);
        if (type == null)
            return ApiResponse<UnitTakeDto>.Fail($"Type d'unité inconnu: {dto.UnitTypeCode}");

        var open = await _db.UnitTakes
            .Include(t => t.TakenByUser)
            .Where(t => t.CabinetId == cabinetId
                        && t.UnitTypeId == type.Id
                        && t.UnitEntityId == dto.UnitEntityId
                        && t.ClosedAt == null
                        && t.Status == UnitTakeStatuses.Open
                        && !t.IsDeleted)
            .OrderByDescending(t => t.TakenAt)
            .FirstOrDefaultAsync();

        if (open == null)
            return ApiResponse<UnitTakeDto>.Fail("Aucune prise ouverte pour cette unité.");

        if (!dto.ForceClose
            && !string.Equals(open.TakenByUserId, userId, StringComparison.OrdinalIgnoreCase)
            && !await IsManagerAsync(userId, cabinetId))
        {
            return ApiResponse<UnitTakeDto>.Fail("Seul le titulaire (ou un Manager) peut fermer cette prise.");
        }

        open.ClosedAt = DateTime.UtcNow;
        open.Status = UnitTakeStatuses.Closed;
        if (!string.IsNullOrWhiteSpace(dto.Comment))
            open.Comment = string.IsNullOrWhiteSpace(open.Comment) ? dto.Comment : $"{open.Comment} | {dto.Comment}";
        open.UpdatedAt = DateTime.UtcNow;
        open.UpdatedBy = userId;
        await _db.SaveChangesAsync();

        open.UnitType = type;
        _logger.LogInformation(
            "UnitTake closed: type={Type} entity={EntityId} by={UserId}",
            type.Code, dto.UnitEntityId, userId);

        return ApiResponse<UnitTakeDto>.Ok(Map(open), "Prise d'unité fermée.");
    }

    private async Task<UnitType?> ResolveTypeAsync(string code)
        => await _db.UnitTypes.FirstOrDefaultAsync(t =>
            t.Code == code && t.IsActive && !t.IsDeleted);

    private async Task<bool> IsManagerAsync(string userId, int cabinetId)
        => await _db.Users
            .Where(u => u.Id == userId && u.CabinetId == cabinetId && u.IsActive)
            .Join(_db.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => ur)
            .Join(_db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
            .AnyAsync(name => name == "Manager" || name == "Gerant" || name == "Admin" || name == "SuperAdmin");

    private static UnitTakeDto Map(UnitTake t) => new()
    {
        Id = t.Id,
        UnitTypeId = t.UnitTypeId,
        UnitTypeCode = t.UnitType?.Code ?? "",
        UnitTypeName = t.UnitType?.Name ?? "",
        UnitEntityId = t.UnitEntityId,
        TakenByUserId = t.TakenByUserId,
        TakenByUserName = t.TakenByUser != null
            ? $"{t.TakenByUser.FirstName} {t.TakenByUser.LastName}".Trim()
            : null,
        TakenAt = t.TakenAt,
        ClosedAt = t.ClosedAt,
        Status = t.Status,
        Comment = t.Comment,
        TaskItemId = t.TaskItemId,
        WorkflowStepInstanceId = t.WorkflowStepInstanceId,
        DeclarationId = t.DeclarationId
    };
}
