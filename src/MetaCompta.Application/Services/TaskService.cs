using MetaCompta.Application.DTOs;
using MetaCompta.Application.Interfaces;
using MetaCompta.Domain.Common;
using MetaCompta.Domain.Entities;
using MetaCompta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MetaCompta.Application.Services;

public class TaskService : ITaskService
{
    private readonly MetaComptaDbContext _db;
    private readonly IUnitTakeService _unitTakes;
    private readonly ILogger<TaskService> _logger;

    public TaskService(MetaComptaDbContext db, IUnitTakeService unitTakes, ILogger<TaskService> logger)
    {
        _db = db;
        _unitTakes = unitTakes;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResult<TaskItemDto>>> GetAllAsync(
        int cabinetId, string? assignedToUserId = null, string? statut = null, int? clientId = null, string? assignedToKind = null, int page = 1, int pageSize = 20)
    {
        var query = _db.TaskItems
            .Include(t => t.Client)
            .Include(t => t.Declaration)
            .Include(t => t.AssignedToUser)
            .Include(t => t.AssignedByUser)
            .Include(t => t.Comments)
            .Where(t => t.CabinetId == cabinetId)
            .AsQueryable();

        if (!string.IsNullOrEmpty(assignedToUserId))
            query = query.Where(t => t.AssignedToUserId == assignedToUserId);

        if (!string.IsNullOrEmpty(statut))
            query = query.Where(t => t.Statut == statut);

        if (clientId.HasValue)
            query = query.Where(t => t.ClientId == clientId.Value);

        // Client = assignee is an extranet user (ApplicationUser.ClientId set)
        // Employee = assignee is cabinet staff (no ClientId) or unassigned
        if (string.Equals(assignedToKind, "Client", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(t => t.AssignedToUser != null && t.AssignedToUser.ClientId != null
                && (!clientId.HasValue || t.AssignedToUser.ClientId == clientId.Value));
        }
        else if (string.Equals(assignedToKind, "Employee", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(t => t.AssignedToUserId == null
                || t.AssignedToUser == null
                || t.AssignedToUser.ClientId == null);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(t => t.DateEcheance)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(MapToDto).ToList();
        await EnrichTasksWithWorkflowAsync(dtos);

        return ApiResponse<PagedResult<TaskItemDto>>.Ok(new PagedResult<TaskItemDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<TaskItemDto>> GetByIdAsync(int id, int cabinetId)
    {
        var task = await _db.TaskItems
            .Include(t => t.Client)
            .Include(t => t.Declaration)
            .Include(t => t.AssignedToUser)
            .Include(t => t.AssignedByUser)
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == id && t.CabinetId == cabinetId);

        if (task == null)
            return ApiResponse<TaskItemDto>.Fail("Tâche introuvable.");

        var dto = MapToDto(task);

        // Load workflow step info if this task is linked to a workflow step
        // Avoid Include(WorkflowInstance -> StepInstances): cyclic path breaks AsNoTracking queries.
        var stepInstance = await _db.WorkflowStepInstances
            .Include(s => s.WorkflowStepTemplate)
            .Include(s => s.WorkflowInstance)
                .ThenInclude(wi => wi!.Declaration)
            .FirstOrDefaultAsync(s => s.TaskItemId == task.Id && !s.IsDeleted);

        if (stepInstance != null)
        {
            var siblingSteps = await _db.WorkflowStepInstances
                .AsNoTracking()
                .Where(s => s.WorkflowInstanceId == stepInstance.WorkflowInstanceId && !s.IsDeleted)
                .Select(s => s.Status)
                .ToListAsync();

            dto.IsWorkflowTask = true;
            dto.WorkflowInstanceId = stepInstance.WorkflowInstanceId;
            dto.WorkflowStepInstanceId = stepInstance.Id;
            dto.WorkflowStepName = stepInstance.StepName;
            dto.WorkflowStepOrder = stepInstance.StepOrder;
            dto.WorkflowTotalSteps = siblingSteps.Count;
            dto.WorkflowCompletedSteps = siblingSteps
                .Count(st => st == WorkflowStepStatus.Completed || st == WorkflowStepStatus.Skipped);
            dto.WorkflowStepStatus = stepInstance.Status.ToString();
            dto.WorkflowStepInstructions = stepInstance.WorkflowStepTemplate?.Description;
            if (string.IsNullOrEmpty(dto.DeclarationStatut) && stepInstance.WorkflowInstance?.Declaration != null)
                dto.DeclarationStatut = stepInstance.WorkflowInstance.Declaration.Statut;
        }

        return ApiResponse<TaskItemDto>.Ok(dto);
    }

    public async Task<ApiResponse<TaskItemDto>> CreateAsync(TaskItemDto dto, int cabinetId, string userId)
    {
        var task = new TaskItem
        {
            CabinetId = cabinetId,
            Titre = dto.Titre,
            Description = dto.Description,
            ClientId = dto.ClientId,
            AssignedToUserId = dto.AssigneA,
            AssignedByUserId = userId,
            DateDebut = dto.DateDebut,
            DateEcheance = dto.DateEcheance ?? DateTime.UtcNow.AddDays(7),
            Priorite = dto.Priorite ?? "Moyenne",
            Statut = "AFaire",
            CreatedAt = DateTime.UtcNow
        };

        _db.TaskItems.Add(task);
        await _db.SaveChangesAsync();

        // Reload with includes
        var created = await _db.TaskItems
            .Include(t => t.Client)
            .Include(t => t.AssignedToUser)
            .Include(t => t.AssignedByUser)
            .FirstAsync(t => t.Id == task.Id);

        _logger.LogInformation("Task {TaskId} created by {UserId} in cabinet {CabinetId}", task.Id, userId, cabinetId);
        return ApiResponse<TaskItemDto>.Ok(MapToDto(created), "Tâche créée avec succès.");
    }

    public async Task<ApiResponse<TaskItemDto>> UpdateAsync(int id, TaskItemDto dto, int cabinetId, string userId)
    {
        var task = await _db.TaskItems
            .FirstOrDefaultAsync(t => t.Id == id && t.CabinetId == cabinetId);

        if (task == null)
            return ApiResponse<TaskItemDto>.Fail("Tâche introuvable.");

        task.Titre = dto.Titre;
        task.Description = dto.Description;
        task.ClientId = dto.ClientId;
        task.AssignedToUserId = dto.AssigneA;
        task.DateDebut = dto.DateDebut;
        task.DateEcheance = dto.DateEcheance ?? task.DateEcheance;
        task.Priorite = dto.Priorite ?? task.Priorite;
        task.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        var updated = await _db.TaskItems
            .Include(t => t.Client)
            .Include(t => t.AssignedToUser)
            .Include(t => t.AssignedByUser)
            .Include(t => t.Comments)
            .FirstAsync(t => t.Id == task.Id);

        _logger.LogInformation("Task {TaskId} updated by {UserId}", id, userId);
        return ApiResponse<TaskItemDto>.Ok(MapToDto(updated), "Tâche mise à jour.");
    }

    public async Task<ApiResponse<bool>> ChangeStatusAsync(int id, string newStatut, int cabinetId, string userId)
    {
        var task = await _db.TaskItems
            .FirstOrDefaultAsync(t => t.Id == id && t.CabinetId == cabinetId);

        if (task == null)
            return ApiResponse<bool>.Fail("Tâche introuvable.");

        var validStatuts = new[] { "AFaire", "EnCours", "EnAttenteRevision", "Termine", "Annule" };
        if (!validStatuts.Contains(newStatut))
            return ApiResponse<bool>.Fail($"Statut invalide: {newStatut}");

        task.Statut = newStatut;
        task.UpdatedAt = DateTime.UtcNow;

        if (newStatut == "EnCours" && task.DateDebut == null)
            task.DateDebut = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        if (newStatut == "EnCours")
        {
            var open = await _unitTakes.OpenTakeAsync(new OpenUnitTakeDto
            {
                UnitTypeCode = UnitTypeCodes.Task,
                UnitEntityId = task.Id,
                Comment = "Démarrage tâche",
                TaskItemId = task.Id,
                DeclarationId = task.DeclarationId
            }, cabinetId, userId);
            if (!open.Success)
                return ApiResponse<bool>.Fail(open.Message ?? "Impossible de prendre l'unité tâche.");
        }
        else if (newStatut is "Termine" or "Annule")
        {
            await _unitTakes.CloseTakeAsync(new CloseUnitTakeDto
            {
                UnitTypeCode = UnitTypeCodes.Task,
                UnitEntityId = task.Id,
                Comment = $"Statut → {newStatut}",
                ForceClose = true
            }, cabinetId, userId);
        }

        _logger.LogInformation("Task {TaskId} status changed to {Status} by {UserId}", id, newStatut, userId);
        return ApiResponse<bool>.Ok(true, $"Statut changé en {newStatut}.");
    }

    public async Task<ApiResponse<TaskCommentDto>> AddCommentAsync(int taskId, string contenu, int cabinetId, string userId, string userName)
    {
        if (string.IsNullOrWhiteSpace(contenu))
            return ApiResponse<TaskCommentDto>.Fail("Le commentaire ne peut pas être vide.");

        var task = await _db.TaskItems
            .FirstOrDefaultAsync(t => t.Id == taskId && t.CabinetId == cabinetId);

        if (task == null)
            return ApiResponse<TaskCommentDto>.Fail("Tâche introuvable.");

        var displayName = userName;
        if (string.IsNullOrWhiteSpace(displayName))
        {
            var user = await _db.Users.FindAsync(userId);
            displayName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : userId;
        }

        var comment = new TaskComment
        {
            TaskItemId = taskId,
            UserId = userId,
            UserName = displayName,
            Contenu = contenu.Trim(),
            CabinetId = cabinetId,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        _db.TaskComments.Add(comment);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Comment {CommentId} added to task {TaskId} by {UserId}", comment.Id, taskId, userId);
        return ApiResponse<TaskCommentDto>.Ok(new TaskCommentDto
        {
            Id = comment.Id,
            TaskItemId = comment.TaskItemId,
            UserId = comment.UserId,
            UserName = comment.UserName,
            Contenu = comment.Contenu,
            CreatedAt = comment.CreatedAt
        }, "Commentaire ajouté.");
    }

    private static TaskItemDto MapToDto(TaskItem t) => new()
    {
        Id = t.Id,
        Titre = t.Titre,
        Description = t.Description,
        ClientId = t.ClientId,
        ClientNom = t.Client?.RaisonSociale,
        AssigneA = t.AssignedToUserId,
        AssignedToUserName = t.AssignedToUser != null ? $"{t.AssignedToUser.FirstName} {t.AssignedToUser.LastName}" : null,
        AssignedByUserName = t.AssignedByUser != null ? $"{t.AssignedByUser.FirstName} {t.AssignedByUser.LastName}" : null,
        DateDebut = t.DateDebut,
        DateEcheance = t.DateEcheance,
        Priorite = t.Priorite,
        Statut = t.Statut,
        DateCreation = t.CreatedAt,
        CommentCount = t.Comments?.Count ?? 0,
        Comments = (t.Comments ?? Enumerable.Empty<TaskComment>())
            .OrderBy(c => c.CreatedAt)
            .Select(c => new TaskCommentDto
            {
                Id = c.Id,
                TaskItemId = c.TaskItemId,
                UserId = c.UserId,
                UserName = c.UserName,
                Contenu = c.Contenu,
                CreatedAt = c.CreatedAt
            }).ToList(),
        DeclarationId = t.DeclarationId,
        DeclarationType = t.Declaration?.Type,
        DeclarationPeriode = t.Declaration?.Periode,
        DeclarationStatut = t.Declaration?.Statut
    };

    private async Task EnrichTasksWithWorkflowAsync(List<TaskItemDto> dtos)
    {
        if (dtos.Count == 0) return;
        var taskIds = dtos.Select(d => d.Id).ToList();
        // Do not ThenInclude StepInstances: WorkflowInstance ↔ StepInstances is a cycle and
        // EF Core rejects it on AsNoTracking queries (breaks Mes tâches / task list).
        var steps = await _db.WorkflowStepInstances
            .AsNoTracking()
            .Include(s => s.WorkflowInstance).ThenInclude(wi => wi!.Declaration)
            .Where(s => s.TaskItemId != null && taskIds.Contains(s.TaskItemId.Value) && !s.IsDeleted)
            .ToListAsync();

        var instanceIds = steps.Select(s => s.WorkflowInstanceId).Distinct().ToList();
        var progressByInstance = instanceIds.Count == 0
            ? new Dictionary<int, (int Total, int Completed)>()
            : await _db.WorkflowStepInstances
                .AsNoTracking()
                .Where(s => instanceIds.Contains(s.WorkflowInstanceId) && !s.IsDeleted)
                .GroupBy(s => s.WorkflowInstanceId)
                .Select(g => new
                {
                    InstanceId = g.Key,
                    Total = g.Count(),
                    Completed = g.Count(x =>
                        x.Status == WorkflowStepStatus.Completed || x.Status == WorkflowStepStatus.Skipped)
                })
                .ToDictionaryAsync(x => x.InstanceId, x => (x.Total, x.Completed));

        foreach (var dto in dtos)
        {
            var step = steps.FirstOrDefault(s => s.TaskItemId == dto.Id);
            if (step == null) continue;
            dto.IsWorkflowTask = true;
            dto.WorkflowInstanceId = step.WorkflowInstanceId;
            dto.WorkflowStepInstanceId = step.Id;
            dto.WorkflowStepName = step.StepName;
            dto.WorkflowStepOrder = step.StepOrder;
            if (progressByInstance.TryGetValue(step.WorkflowInstanceId, out var prog))
            {
                dto.WorkflowTotalSteps = prog.Total;
                dto.WorkflowCompletedSteps = prog.Completed;
            }
            dto.WorkflowStepStatus = step.Status.ToString();
            if (string.IsNullOrEmpty(dto.DeclarationStatut))
                dto.DeclarationStatut = step.WorkflowInstance?.Declaration?.Statut ?? dto.DeclarationStatut;
        }
    }
}
