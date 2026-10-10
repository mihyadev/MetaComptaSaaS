using MetaCompta.Application.DTOs;
using MetaCompta.Application.Interfaces;
using MetaCompta.Domain.Common;
using MetaCompta.Domain.Entities;
using MetaCompta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MetaCompta.Application.Services;

/// <summary>
/// Core Workflow Engine — manages templates, instances, step execution,
/// task auto-creation, notifications, and the full workflow lifecycle.
/// </summary>
public class WorkflowService : IWorkflowService
{
    private readonly MetaComptaDbContext _db;
    private readonly INotificationService _notifications;
    private readonly IDeclarationService _declarations;
    private readonly IUnitTakeService _unitTakes;
    private readonly ILogger<WorkflowService> _logger;

    public WorkflowService(
        MetaComptaDbContext db,
        INotificationService notifications,
        IDeclarationService declarations,
        IUnitTakeService unitTakes,
        ILogger<WorkflowService> logger)
    {
        _db = db;
        _notifications = notifications;
        _declarations = declarations;
        _unitTakes = unitTakes;
        _logger = logger;
    }

    // ╔══════════════════════════════════════════════════════════════════════╗
    // ║                         TEMPLATES                                   ║
    // ╚══════════════════════════════════════════════════════════════════════╝

    public async Task<ApiResponse<PagedResult<WorkflowTemplateDto>>> GetTemplatesAsync(
        int cabinetId, string? declarationType = null, int? clientId = null, int page = 1, int pageSize = 20,
        bool cabinetOnly = false, bool activeOnly = false, bool excludeAncien = false)
    {
        var query = _db.WorkflowTemplates
            .Where(wt => wt.CabinetId == cabinetId)
            .Include(wt => wt.Steps)
            .Include(wt => wt.Client)
            .AsQueryable();

        if (!string.IsNullOrEmpty(declarationType) && Enum.TryParse<WorkflowDeclarationType>(declarationType, true, out var dt))
            query = query.Where(wt => wt.DeclarationType == dt);

        if (cabinetOnly)
            query = query.Where(wt => wt.ClientId == null);
        else if (clientId.HasValue)
            query = query.Where(wt => wt.ClientId == clientId.Value || wt.ClientId == null);

        if (activeOnly)
            query = query.Where(wt => wt.IsActive);

        if (excludeAncien)
            query = query.Where(wt => !wt.Name.Contains("(ancien)"));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(wt => wt.IsDefault).ThenBy(wt => wt.DeclarationType).ThenBy(wt => wt.Name)
            .Skip((page - 1) * Math.Max(1, pageSize)).Take(Math.Clamp(pageSize, 1, 500))
            .Select(wt => MapTemplateDto(wt))
            .ToListAsync();

        return ApiResponse<PagedResult<WorkflowTemplateDto>>.Ok(new PagedResult<WorkflowTemplateDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<WorkflowTemplateDto>> GetTemplateByIdAsync(int id, int cabinetId)
    {
        var wt = await _db.WorkflowTemplates
            .Where(wt => wt.Id == id && wt.CabinetId == cabinetId)
            .Include(wt => wt.Steps.OrderBy(s => s.StepOrder))
                .ThenInclude(s => s.AssignedToUser)
            .Include(wt => wt.Steps)
                .ThenInclude(s => s.AssignedToDepartment)
            .Include(wt => wt.Client)
            .FirstOrDefaultAsync();

        if (wt == null)
            return ApiResponse<WorkflowTemplateDto>.Fail("Modèle de workflow non trouvé.");

        return ApiResponse<WorkflowTemplateDto>.Ok(MapTemplateDto(wt));
    }

    public async Task<ApiResponse<WorkflowTemplateDto>> CreateTemplateAsync(WorkflowTemplateCreateDto dto, int cabinetId, string userId)
    {
        if (!Enum.TryParse<WorkflowDeclarationType>(dto.DeclarationType, true, out var declType))
            declType = WorkflowDeclarationType.All;

        // Cabinet base: one active template per declaration type — except TVA (Mensuelle + Trimestrielle)
        if (!dto.ClientId.HasValue)
        {
            if (declType == WorkflowDeclarationType.TVA)
            {
                var existingSameName = await _db.WorkflowTemplates
                    .AnyAsync(wt => wt.CabinetId == cabinetId && !wt.IsDeleted
                        && wt.ClientId == null
                        && wt.DeclarationType == declType
                        && wt.IsActive
                        && !wt.Name.Contains("(ancien)")
                        && wt.Name == dto.Name);
                if (existingSameName)
                    return ApiResponse<WorkflowTemplateDto>.Fail(
                        "Un modèle cabinet TVA avec ce nom existe déjà. Ouvrez-le dans le Builder pour le modifier.");
                // Keep dto.IsDefault as provided (Mensuelle=true, Trimestrielle=false)
            }
            else
            {
                dto.IsDefault = true;
                var existingCabinet = await _db.WorkflowTemplates
                    .AnyAsync(wt => wt.CabinetId == cabinetId && !wt.IsDeleted
                        && wt.ClientId == null
                        && wt.DeclarationType == declType
                        && wt.IsActive
                        && !wt.Name.Contains("(ancien)"));
                if (existingCabinet)
                    return ApiResponse<WorkflowTemplateDto>.Fail(
                        "Un modèle cabinet existe déjà pour ce type de déclaration. Ouvrez-le dans le Builder pour le modifier (personnalisation client = fiche client → Workflow).");
            }
        }

        var template = new WorkflowTemplate
        {
            Name = dto.Name,
            Description = dto.Description,
            DeclarationType = declType,
            ClientId = dto.ClientId,
            IsDefault = dto.IsDefault,
            IsSystem = false, // client / cabinet custom copies are never Plan Marocain system templates
            IsActive = true,
            Version = 1,
            CabinetId = cabinetId,
            CreatedBy = userId
        };

        // If setting as default, unset other defaults for same scope
        if (template.IsDefault)
        {
            var existingDefaults = await _db.WorkflowTemplates
                .Where(wt => wt.CabinetId == cabinetId && wt.DeclarationType == declType && wt.ClientId == dto.ClientId && wt.IsDefault)
                .ToListAsync();
            foreach (var ed in existingDefaults)
                ed.IsDefault = false;
        }

        _db.WorkflowTemplates.Add(template);
        await _db.SaveChangesAsync();

        // Add steps
        int order = 0;
        foreach (var stepDto in dto.Steps.OrderBy(s => s.StepOrder))
        {
            if (!Enum.TryParse<WorkflowStepType>(stepDto.StepType, true, out var stepType))
                stepType = (WorkflowStepType)order;
            if (!Enum.TryParse<AssignmentType>(stepDto.AssignmentType, true, out var assignType))
                assignType = AssignmentType.Employee;
            if (!Enum.TryParse<WorkflowTaskType>(stepDto.TaskType, true, out var taskType))
                taskType = WorkflowTaskType.Standard;

            var (primaryUserId, userIdsCsv) = NormalizeAssigneeIds(stepDto.AssignedToUserIds, stepDto.AssignedToUserId);

            _db.WorkflowStepTemplates.Add(new WorkflowStepTemplate
            {
                WorkflowTemplateId = template.Id,
                Name = stepDto.Name,
                Description = stepDto.Description,
                StepOrder = order,
                StepType = stepType,
                AssignmentType = assignType,
                AssignedToUserId = primaryUserId,
                AssignedToUserIds = userIdsCsv,
                AssignedToRoleName = stepDto.AssignedToRoleName,
                AssignedToDepartmentId = stepDto.AssignedToDepartmentId,
                IsClientStep = stepDto.IsClientStep || assignType == AssignmentType.Client,
                RequiredPermission = stepDto.RequiredPermission,
                SLAHours = stepDto.SLAHours,
                DeadlineAnchor = stepType == WorkflowStepType.ClotureArchivage ? "None"
                    : (string.IsNullOrWhiteSpace(stepDto.DeadlineAnchor) ? "CalendrierMaroc" : stepDto.DeadlineAnchor.Trim()),
                DeadlineOffsetDays = stepType == WorkflowStepType.ClotureArchivage ? null : stepDto.DeadlineOffsetDays,
                DeadlineDay = stepType == WorkflowStepType.ClotureArchivage ? null : stepDto.DeadlineDay,
                DeadlineMonth = stepType == WorkflowStepType.ClotureArchivage ? null : stepDto.DeadlineMonth,
                DeadlineMonthOffset = stepType == WorkflowStepType.ClotureArchivage ? null : stepDto.DeadlineMonthOffset,
                DeadlineYearOffset = stepType == WorkflowStepType.ClotureArchivage ? null : stepDto.DeadlineYearOffset,
                ApplyDeadlineToDeclaration = stepType == WorkflowStepType.Initialisation
                    || (stepType != WorkflowStepType.ClotureArchivage && stepDto.ApplyDeadlineToDeclaration),
                LifecycleEndMode = stepType == WorkflowStepType.Initialisation
                    ? "PeriodEnd"
                    : null,
                LifecycleEndDays = stepType == WorkflowStepType.Initialisation ? stepDto.LifecycleEndDays : null,
                ActionOnLifecycleEnd = stepType == WorkflowStepType.Initialisation
                    ? (string.IsNullOrWhiteSpace(stepDto.ActionOnLifecycleEnd) ? "ClotureDeclaration" : stepDto.ActionOnLifecycleEnd.Trim())
                    : null,
                IsRequired = stepDto.IsRequired,
                IsBlocking = stepDto.IsBlocking,
                AutoCreateTask = stepDto.AutoCreateTask,
                TaskType = taskType,
                TaskPriority = stepDto.TaskPriority,
                AutoNotify = stepDto.AutoNotify,
                ValidationRulesJson = stepDto.ValidationRulesJson,
                DeclarationStatusOnComplete = stepDto.DeclarationStatusOnComplete,
                DeclarationStatusOnEntry = stepDto.DeclarationStatusOnEntry,
                ActionOnEntry = Enum.TryParse<WorkflowActionType>(stepDto.ActionOnEntry, true, out var aoe) ? aoe : WorkflowActionType.None,
                ActionOnComplete = Enum.TryParse<WorkflowActionType>(stepDto.ActionOnComplete, true, out var aoc) ? aoc : WorkflowActionType.None,
                ActionConfigJson = stepDto.ActionConfigJson,
                CronExpression = stepDto.CronExpression,
                CronDescription = stepDto.CronDescription,
                IsScheduled = stepDto.IsScheduled,
                MaxRetries = stepDto.MaxRetries,
                RetryIntervalHours = stepDto.RetryIntervalHours,
                OnMaxRetriesAction = stepDto.OnMaxRetriesAction,
                RoutingMode = Enum.TryParse<WorkflowRoutingMode>(stepDto.RoutingMode, true, out var rm) ? rm : WorkflowRoutingMode.Sequential,
                RoutingConditionsJson = stepDto.RoutingConditionsJson,
                GotoStepOrder = stepDto.GotoStepOrder,
                TriggerSource = Enum.TryParse<WorkflowTriggerSource>(stepDto.TriggerSource, true, out var ts) ? ts : WorkflowTriggerSource.Manual,
                UseServiceContratAffectation = stepDto.UseServiceContratAffectation,
                ServiceContratTypeCode = stepDto.ServiceContratTypeCode,
                CabinetId = cabinetId,
                CreatedBy = userId
            });
            order++;
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Workflow template '{Name}' created by {UserId}", dto.Name, userId);
        return await GetTemplateByIdAsync(template.Id, cabinetId);
    }

    public async Task<ApiResponse<WorkflowTemplateDto>> UpdateTemplateAsync(int id, WorkflowTemplateCreateDto dto, int cabinetId, string userId)
    {
        var template = await _db.WorkflowTemplates
            .Include(wt => wt.Steps)
            .FirstOrDefaultAsync(wt => wt.Id == id && wt.CabinetId == cabinetId);

        if (template == null)
            return ApiResponse<WorkflowTemplateDto>.Fail("Modèle non trouvé.");

        // Global Plan Marocain only — client-owned copies are always editable
        if (template.IsSystem && !template.ClientId.HasValue)
            return ApiResponse<WorkflowTemplateDto>.Fail("Ce modèle système (Plan Marocain) ne peut pas être modifié. Assignez-le au client pour créer une copie modifiable.");

        // Heal bad data: client copy wrongly flagged as system
        if (template.ClientId.HasValue && template.IsSystem)
            template.IsSystem = false;

        // Client-owned copies can be customized even with running instances (in-flight keep their step instances).
        // Cabinet-wide templates remain locked while active instances exist.
        if (!template.ClientId.HasValue)
        {
            var hasActiveInstances = await _db.WorkflowInstances
                .AnyAsync(wi => wi.WorkflowTemplateId == id && wi.Status == WorkflowInstanceStatus.Active);
            if (hasActiveInstances)
                return ApiResponse<WorkflowTemplateDto>.Fail("Impossible de modifier un modèle avec des instances actives. Créez une nouvelle version.");
        }

        if (!Enum.TryParse<WorkflowDeclarationType>(dto.DeclarationType, true, out var declType))
            declType = WorkflowDeclarationType.All;

        // Cabinet templates stay cabinet-scoped (client copies are edited via client customize)
        if (!template.ClientId.HasValue)
        {
            dto.ClientId = null;
            if (declType != WorkflowDeclarationType.TVA)
                dto.IsDefault = true;

            if (declType != template.DeclarationType
                || (declType == WorkflowDeclarationType.TVA && !string.Equals(dto.Name, template.Name, StringComparison.OrdinalIgnoreCase)))
            {
                var clash = declType == WorkflowDeclarationType.TVA
                    ? await _db.WorkflowTemplates.AnyAsync(wt => wt.CabinetId == cabinetId && !wt.IsDeleted
                        && wt.ClientId == null && wt.DeclarationType == declType && wt.IsActive
                        && !wt.Name.Contains("(ancien)") && wt.Name == dto.Name && wt.Id != id)
                    : await _db.WorkflowTemplates.AnyAsync(wt => wt.CabinetId == cabinetId && !wt.IsDeleted
                        && wt.ClientId == null && wt.DeclarationType == declType && wt.IsActive
                        && !wt.Name.Contains("(ancien)") && wt.Id != id);
                if (clash)
                    return ApiResponse<WorkflowTemplateDto>.Fail(
                        declType == WorkflowDeclarationType.TVA
                            ? "Un modèle cabinet TVA avec ce nom existe déjà."
                            : "Un modèle cabinet existe déjà pour ce type de déclaration. Un seul workflow de base par type.");
            }
        }

        template.Name = dto.Name;
        template.Description = dto.Description;
        template.DeclarationType = declType;
        if (!template.ClientId.HasValue)
        {
            template.ClientId = null;
            template.IsDefault = declType == WorkflowDeclarationType.TVA ? dto.IsDefault : true;
        }
        else
        {
            if (dto.ClientId.HasValue)
                template.ClientId = dto.ClientId;
            template.IsDefault = dto.IsDefault;
        }
        template.Version++;
        template.UpdatedBy = userId;

        // Soft-delete ALL prior steps (including already-deleted with leftover StepOrder)
        // and free unique (TemplateId, StepOrder) before inserting replacements.
        var allSteps = await _db.WorkflowStepTemplates
            .IgnoreQueryFilters()
            .Where(s => s.WorkflowTemplateId == template.Id)
            .ToListAsync();

        foreach (var oldStep in allSteps)
        {
            if (!oldStep.IsDeleted)
            {
                oldStep.IsDeleted = true;
                oldStep.DeletedAt = DateTime.UtcNow;
                oldStep.DeletedBy = userId;
            }
            // Unique index historically includes soft-deleted rows — park orders as -Id
            if (oldStep.StepOrder >= 0)
                oldStep.StepOrder = -Math.Abs(oldStep.Id);
        }

        await _db.SaveChangesAsync();

        // Add new steps
        int order = 0;
        foreach (var stepDto in dto.Steps.OrderBy(s => s.StepOrder))
        {
            if (!Enum.TryParse<WorkflowStepType>(stepDto.StepType, true, out var stepType))
                stepType = (WorkflowStepType)order;
            if (!Enum.TryParse<AssignmentType>(stepDto.AssignmentType, true, out var assignType))
                assignType = AssignmentType.Employee;
            if (!Enum.TryParse<WorkflowTaskType>(stepDto.TaskType, true, out var taskType))
                taskType = WorkflowTaskType.Standard;

            var (primaryUserId, userIdsCsv) = NormalizeAssigneeIds(stepDto.AssignedToUserIds, stepDto.AssignedToUserId);

            _db.WorkflowStepTemplates.Add(new WorkflowStepTemplate
            {
                WorkflowTemplateId = template.Id,
                Name = stepDto.Name,
                Description = stepDto.Description,
                StepOrder = order,
                StepType = stepType,
                AssignmentType = assignType,
                AssignedToUserId = primaryUserId,
                AssignedToUserIds = userIdsCsv,
                AssignedToRoleName = stepDto.AssignedToRoleName,
                AssignedToDepartmentId = stepDto.AssignedToDepartmentId,
                IsClientStep = stepDto.IsClientStep || assignType == AssignmentType.Client,
                RequiredPermission = stepDto.RequiredPermission,
                SLAHours = stepDto.SLAHours,
                DeadlineAnchor = stepType == WorkflowStepType.ClotureArchivage ? "None"
                    : (string.IsNullOrWhiteSpace(stepDto.DeadlineAnchor) ? "CalendrierMaroc" : stepDto.DeadlineAnchor.Trim()),
                DeadlineOffsetDays = stepType == WorkflowStepType.ClotureArchivage ? null : stepDto.DeadlineOffsetDays,
                DeadlineDay = stepType == WorkflowStepType.ClotureArchivage ? null : stepDto.DeadlineDay,
                DeadlineMonth = stepType == WorkflowStepType.ClotureArchivage ? null : stepDto.DeadlineMonth,
                DeadlineMonthOffset = stepType == WorkflowStepType.ClotureArchivage ? null : stepDto.DeadlineMonthOffset,
                DeadlineYearOffset = stepType == WorkflowStepType.ClotureArchivage ? null : stepDto.DeadlineYearOffset,
                ApplyDeadlineToDeclaration = stepType == WorkflowStepType.Initialisation
                    || (stepType != WorkflowStepType.ClotureArchivage && stepDto.ApplyDeadlineToDeclaration),
                LifecycleEndMode = stepType == WorkflowStepType.Initialisation
                    ? "PeriodEnd"
                    : null,
                LifecycleEndDays = stepType == WorkflowStepType.Initialisation ? stepDto.LifecycleEndDays : null,
                ActionOnLifecycleEnd = stepType == WorkflowStepType.Initialisation
                    ? (string.IsNullOrWhiteSpace(stepDto.ActionOnLifecycleEnd) ? "ClotureDeclaration" : stepDto.ActionOnLifecycleEnd.Trim())
                    : null,
                IsRequired = stepDto.IsRequired,
                IsBlocking = stepDto.IsBlocking,
                AutoCreateTask = stepDto.AutoCreateTask,
                TaskType = taskType,
                TaskPriority = stepDto.TaskPriority,
                AutoNotify = stepDto.AutoNotify,
                ValidationRulesJson = stepDto.ValidationRulesJson,
                DeclarationStatusOnComplete = stepDto.DeclarationStatusOnComplete,
                DeclarationStatusOnEntry = stepDto.DeclarationStatusOnEntry,
                ActionOnEntry = Enum.TryParse<WorkflowActionType>(stepDto.ActionOnEntry, true, out var uaoe) ? uaoe : WorkflowActionType.None,
                ActionOnComplete = Enum.TryParse<WorkflowActionType>(stepDto.ActionOnComplete, true, out var uaoc) ? uaoc : WorkflowActionType.None,
                ActionConfigJson = stepDto.ActionConfigJson,
                CronExpression = stepDto.CronExpression,
                CronDescription = stepDto.CronDescription,
                IsScheduled = stepDto.IsScheduled,
                MaxRetries = stepDto.MaxRetries,
                RetryIntervalHours = stepDto.RetryIntervalHours,
                OnMaxRetriesAction = stepDto.OnMaxRetriesAction,
                RoutingMode = Enum.TryParse<WorkflowRoutingMode>(stepDto.RoutingMode, true, out var urm) ? urm : WorkflowRoutingMode.Sequential,
                RoutingConditionsJson = stepDto.RoutingConditionsJson,
                GotoStepOrder = stepDto.GotoStepOrder,
                TriggerSource = Enum.TryParse<WorkflowTriggerSource>(stepDto.TriggerSource, true, out var uts) ? uts : WorkflowTriggerSource.Manual,
                UseServiceContratAffectation = stepDto.UseServiceContratAffectation,
                ServiceContratTypeCode = stepDto.ServiceContratTypeCode,
                CabinetId = cabinetId,
                CreatedBy = userId
            });
            order++;
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Workflow template {Id} updated to v{Version} by {UserId}", id, template.Version, userId);
        return await GetTemplateByIdAsync(id, cabinetId);
    }

    public async Task<ApiResponse<bool>> DeleteTemplateAsync(int id, int cabinetId, string userId)
    {
        var template = await _db.WorkflowTemplates.FirstOrDefaultAsync(wt => wt.Id == id && wt.CabinetId == cabinetId);
        if (template == null)
            return ApiResponse<bool>.Fail("Modèle non trouvé.");

        // Global Plan Marocain only — client copies can be removed
        if (template.IsSystem && !template.ClientId.HasValue)
            return ApiResponse<bool>.Fail("Ce modèle système (Plan Marocain) ne peut pas être supprimé.");
        if (template.ClientId.HasValue && template.IsSystem)
            template.IsSystem = false;

        var hasInstances = await _db.WorkflowInstances.AnyAsync(wi => wi.WorkflowTemplateId == id && wi.Status == WorkflowInstanceStatus.Active);
        if (hasInstances)
            return ApiResponse<bool>.Fail("Impossible de supprimer un modèle avec des instances actives.");

        template.IsDeleted = true;
        template.DeletedAt = DateTime.UtcNow;
        template.DeletedBy = userId;
        template.IsActive = false;
        await _db.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Modèle supprimé.");
    }

    /// <summary>
    /// Creates the default 9-step workflow template for a cabinet.
    /// </summary>
    public async Task<ApiResponse<WorkflowTemplateDto>> CreateDefaultTemplateAsync(int cabinetId, string userId)
    {
        var dto = new WorkflowTemplateCreateDto
        {
            Name = "Workflow Standard — 9 Étapes",
            Description = "Cycle de vie complet d'une déclaration : Initialisation → Demande de pièces → Collecte → Saisie → Contrôle → Validation interne → Validation client → Dépôt/Soumission → Clôture/Archivage",
            DeclarationType = "All",
            IsDefault = true,
            Steps = new List<WorkflowStepTemplateCreateDto>
            {
                new() { Name = "Initialisation", StepOrder = 0, StepType = "Initialisation", AssignmentType = "AutoSystem", IsRequired = true, IsBlocking = true, AutoCreateTask = false, AutoNotify = true, SLAHours = 1, DeclarationStatusOnEntry = RefCodes.StatutDeclaration.Brouillon, TaskType = "Standard" },
                new() { Name = "Demande de pièces", StepOrder = 1, StepType = "DemandePieces", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 48, DeclarationStatusOnEntry = RefCodes.StatutDeclaration.ACompleter, TaskType = "DocumentRequest", TaskPriority = RefCodes.PrioriteTache.Haute },
                new() { Name = "Collecte documents", StepOrder = 2, StepType = "Collecte", AssignmentType = "Client", IsClientStep = true, IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 120, TaskType = "ClientAction", TaskPriority = RefCodes.PrioriteTache.Haute },
                new() { Name = "Saisie comptable", StepOrder = 3, StepType = "Saisie", AssignmentType = "Department", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 48, DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Completee, TaskType = "DataEntry", TaskPriority = RefCodes.PrioriteTache.Haute },
                new() { Name = "Contrôle qualité", StepOrder = 4, StepType = "Controle", AssignmentType = "Role", AssignedToRoleName = "Manager", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 24, DeclarationStatusOnEntry = RefCodes.StatutDeclaration.EnRevision, TaskType = "Review", TaskPriority = RefCodes.PrioriteTache.Haute, RequiredPermission = "workflow.validate" },
                new() { Name = "Validation interne", StepOrder = 5, StepType = "ValidationInterne", AssignmentType = "Role", AssignedToRoleName = "Manager", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 24, DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Validee, TaskType = "Validation", TaskPriority = RefCodes.PrioriteTache.Urgente, RequiredPermission = "workflow.validate" },
                new() { Name = "Validation client", StepOrder = 6, StepType = "ValidationClient", AssignmentType = "Client", IsClientStep = true, IsRequired = false, IsBlocking = false, AutoCreateTask = true, AutoNotify = true, SLAHours = 72, TaskType = "ClientAction", TaskPriority = RefCodes.PrioriteTache.Haute },
                new() { Name = "Dépôt / Soumission", StepOrder = 7, StepType = "DepotSoumission", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 24, DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Soumise, TaskType = "Submission", TaskPriority = RefCodes.PrioriteTache.Urgente },
                new() { Name = "Clôture & Archivage", StepOrder = 8, StepType = "ClotureArchivage", AssignmentType = "AutoSystem", IsRequired = true, IsBlocking = false, AutoCreateTask = false, AutoNotify = true, SLAHours = 48, TaskType = "Archiving" }
            }
        };

        return await CreateTemplateAsync(dto, cabinetId, userId);
    }

    /// <summary>
    /// Seeds all default workflow templates for each declaration type:
    /// TVA, IS, IR, CNSS, DTS, Paie, Juridique, Comptabilite, IR_SALAIRES
    /// Each template has optimized steps, SLAs, and assignments for its domain.
    /// </summary>
    /// <summary>
    /// Ensures one active cabinet base template per declaration type.
    /// When <paramref name="forceReset"/> is true: retire ALL cabinet bases for seeded types, then recreate fresh defaults.
    /// </summary>
    public async Task<ApiResponse<List<WorkflowTemplateDto>>> SeedAllDefaultTemplatesAsync(int cabinetId, string userId, bool forceReset = false)
    {
        var createdTemplates = new List<WorkflowTemplateDto>();
        var templateDefinitions = GetAllDefaultTemplateDefinitions();
        var seededNamesByType = new Dictionary<WorkflowDeclarationType, HashSet<string>>();

        foreach (var def in templateDefinitions)
        {
            if (!Enum.TryParse<WorkflowDeclarationType>(def.DeclarationType, true, out var declType))
                continue;

            if (!seededNamesByType.TryGetValue(declType, out var nameSet))
            {
                nameSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                seededNamesByType[declType] = nameSet;
            }
            nameSet.Add(def.Name);

            var cabinetForType = await _db.WorkflowTemplates
                .Include(t => t.Steps)
                .Where(t => t.CabinetId == cabinetId && !t.IsDeleted
                    && t.ClientId == null
                    && t.DeclarationType == declType)
                .ToListAsync();

            foreach (var old in cabinetForType.Where(t => t.Name.Contains("(ancien)")))
            {
                old.IsDefault = false;
                old.IsActive = false;
                old.UpdatedBy = userId;
            }

            // Match by name (TVA Mensuelle / TVA Trimestrielle coexist)
            var current = cabinetForType
                .Where(t => !t.Name.Contains("(ancien)") && t.IsActive
                    && string.Equals(t.Name, def.Name, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(t => t.IsDefault)
                .ThenBy(t => t.Id)
                .FirstOrDefault();

            // Legacy single TVA template → treat as Mensuelle when seeding Mensuelle
            if (current == null && declType == WorkflowDeclarationType.TVA
                && def.Name.Equals("TVA Mensuelle", StringComparison.OrdinalIgnoreCase))
            {
                current = cabinetForType
                    .Where(t => !t.Name.Contains("(ancien)") && t.IsActive
                        && !t.Name.Equals("TVA Trimestrielle", StringComparison.OrdinalIgnoreCase)
                        && !t.Name.Equals("TVA Mensuelle", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(t => t.IsDefault)
                    .ThenBy(t => t.Id)
                    .FirstOrDefault();
            }

            if (!forceReset && current != null && !TemplateNeedsRefresh(current, def))
            {
                // Retire same-name duplicates only
                foreach (var dup in cabinetForType.Where(t => t.IsActive && !t.Name.Contains("(ancien)")
                    && t.Id != current.Id
                    && string.Equals(t.Name, current.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    dup.IsDefault = false;
                    dup.IsActive = false;
                    dup.Name = $"{dup.Name} (ancien)";
                    dup.UpdatedBy = userId;
                }
                if (current.IsDefault != def.IsDefault)
                {
                    current.IsDefault = def.IsDefault;
                    current.UpdatedBy = userId;
                }
                if (!string.Equals(current.Name, def.Name, StringComparison.OrdinalIgnoreCase))
                {
                    current.Name = def.Name;
                    current.UpdatedBy = userId;
                }
                continue;
            }

            // Retire only the matching template (not the sibling TVA frequency)
            var toRetire = cabinetForType.Where(t => !t.Name.Contains("(ancien)") && (
                string.Equals(t.Name, def.Name, StringComparison.OrdinalIgnoreCase)
                || (current != null && t.Id == current.Id)
                || (declType != WorkflowDeclarationType.TVA && t.IsActive)
            ));
            foreach (var old in toRetire)
            {
                old.IsDefault = false;
                old.IsActive = false;
                if (!old.Name.Contains("(ancien)"))
                    old.Name = $"{old.Name} (ancien)";
                old.UpdatedBy = userId;
            }

            await _db.SaveChangesAsync();

            var result = await CreateTemplateAsync(def, cabinetId, userId);
            if (result.Success && result.Data != null)
                createdTemplates.Add(result.Data);
        }

        await _db.SaveChangesAsync();

        // Retire orphans: wrong DeclarationType, or TVA names not in seed list
        var orphans = await _db.WorkflowTemplates
            .Where(t => t.CabinetId == cabinetId && !t.IsDeleted && t.ClientId == null && t.IsActive)
            .ToListAsync();
        foreach (var old in orphans)
        {
            var keep = seededNamesByType.TryGetValue(old.DeclarationType, out var names)
                && names.Contains(old.Name);
            if (keep) continue;
            old.IsDefault = false;
            old.IsActive = false;
            if (!old.Name.Contains("(ancien)"))
                old.Name = $"{old.Name} (ancien)";
            old.UpdatedBy = userId;
        }

        await _db.SaveChangesAsync();

        if (forceReset)
            return ApiResponse<List<WorkflowTemplateDto>>.Ok(createdTemplates,
                $"Réinitialisation terminée : {createdTemplates.Count} workflow(s) cabinet recréé(s) (TVA mensuelle + trimestrielle inclus).");

        if (!createdTemplates.Any())
            return ApiResponse<List<WorkflowTemplateDto>>.Ok(createdTemplates, "Tous les modèles par défaut sont à jour (TVA Mensuelle, TVA Trimestrielle, IS, IR, CNSS…).");

        return ApiResponse<List<WorkflowTemplateDto>>.Ok(createdTemplates, $"{createdTemplates.Count} modèle(s) cabinet créé(s)/rafraîchi(s).");
    }

    private static bool TemplateNeedsRefresh(WorkflowTemplate template, WorkflowTemplateCreateDto def)
    {
        var steps = template.Steps?.Where(s => !s.IsDeleted).ToList() ?? new List<WorkflowStepTemplate>();
        if (steps.Count < def.Steps.Count) return true;
        if (steps.Any(s => s.StepType == WorkflowStepType.Relance)) return true;
        if (def.DeclarationType is "TVA" or "IS")
        {
            var collecte = steps.FirstOrDefault(s => s.StepType == WorkflowStepType.Collecte);
            if (collecte == null || collecte.MaxRetries <= 0) return true;
        }
        if (def.DeclarationType == "TVA")
        {
            if (!string.Equals(template.Name, def.Name, StringComparison.OrdinalIgnoreCase)
                && !def.Name.Equals("TVA Mensuelle", StringComparison.OrdinalIgnoreCase))
                return true;

            var init = steps.FirstOrDefault(s => s.StepType == WorkflowStepType.Initialisation);
            if (init == null || string.IsNullOrWhiteSpace(init.ActionConfigJson)) return true;
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(init.ActionConfigJson);
                var root = doc.RootElement;
                var wantTrim = def.Name.Contains("Trimestrielle", StringComparison.OrdinalIgnoreCase);
                if (root.TryGetProperty("periodFrequency", out var pf) && pf.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    var freq = pf.GetString() ?? "";
                    if (wantTrim && !freq.StartsWith("Trim", StringComparison.OrdinalIgnoreCase)) return true;
                    if (!wantTrim && freq.StartsWith("Trim", StringComparison.OrdinalIgnoreCase)) return true;
                }
                if (root.TryGetProperty("periods", out var periods) && periods.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    var n = periods.GetArrayLength();
                    if (wantTrim && n != 4) return true;
                    if (!wantTrim && n != 12) return true;
                    // Each period must carry its own due (Début/Fin/Échéance)
                    foreach (var el in periods.EnumerateArray())
                    {
                        var hasDue = (el.TryGetProperty("dueMonth", out var dm) && dm.TryGetInt32(out _))
                            || (el.TryGetProperty("dueDay", out var dd) && dd.TryGetInt32(out _))
                            || (el.TryGetProperty("due", out var dueEl) && dueEl.ValueKind == System.Text.Json.JsonValueKind.String
                                && !string.IsNullOrWhiteSpace(dueEl.GetString()));
                        if (!hasDue) return true;
                    }
                    // Morocco N+1: first monthly row must be December activity (decl. January)
                    var firstPeriod = periods.EnumerateArray().FirstOrDefault();
                    if (firstPeriod.ValueKind == System.Text.Json.JsonValueKind.Object)
                    {
                        if (!wantTrim
                            && firstPeriod.TryGetProperty("startMonth", out var fsm)
                            && fsm.TryGetInt32(out var firstStart)
                            && firstStart != 12)
                            return true;
                        if (wantTrim
                            && firstPeriod.TryGetProperty("startMonth", out var qsm)
                            && qsm.TryGetInt32(out var qStart)
                            && qStart != 10)
                            return true;
                    }
                }
                else return true;
            }
            catch
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Refresh TVA templates: ensure both Mensuelle and Trimestrielle are aligned.
    /// </summary>
    public async Task<ApiResponse<WorkflowTemplateDto>> RefreshTvaDefaultTemplateAsync(int cabinetId, string userId)
    {
        var seeded = await SeedAllDefaultTemplatesAsync(cabinetId, userId, forceReset: false);
        var mensuelle = await _db.WorkflowTemplates
            .Include(t => t.Steps)
            .Where(t => t.CabinetId == cabinetId && !t.IsDeleted && t.ClientId == null
                && t.DeclarationType == WorkflowDeclarationType.TVA && t.IsActive
                && t.Name == "TVA Mensuelle")
            .OrderByDescending(t => t.IsDefault)
            .FirstOrDefaultAsync();

        if (mensuelle == null)
            return ApiResponse<WorkflowTemplateDto>.Fail(seeded.Message ?? "Échec refresh modèles TVA.");

        _logger.LogInformation("TVA templates refreshed for cabinet {CabinetId}: Mensuelle #{Id}", cabinetId, mensuelle.Id);
        return ApiResponse<WorkflowTemplateDto>.Ok(MapTemplateDto(mensuelle),
            seeded.Message ?? "Modèles TVA Mensuelle et Trimestrielle à jour.");
    }

    /// <summary>
    /// Refresh IS (liasse fiscale) default template: TVA-pattern + métier steps. Keeps in-flight instances on old templates.
    /// </summary>
    public async Task<ApiResponse<WorkflowTemplateDto>> RefreshIsDefaultTemplateAsync(int cabinetId, string userId)
    {
        var isDef = GetAllDefaultTemplateDefinitions()
            .First(d => d.DeclarationType == "IS");

        var oldDefaults = await _db.WorkflowTemplates
            .Include(t => t.Steps)
            .Where(t => t.CabinetId == cabinetId && !t.IsDeleted
                && t.DeclarationType == WorkflowDeclarationType.IS && t.IsDefault)
            .ToListAsync();

        var expectedSteps = isDef.Steps.Count;
        var alreadyAligned = oldDefaults.Any(t => t.Steps.Count(s => !s.IsDeleted) >= expectedSteps
            && t.Steps.Any(s => !s.IsDeleted && s.StepType == WorkflowStepType.Collecte && s.MaxRetries > 0)
            && !t.Steps.Any(s => !s.IsDeleted && s.StepType == WorkflowStepType.Relance)
            && t.Steps.Any(s => !s.IsDeleted && s.ActionOnEntry == WorkflowActionType.CreateDeclaration));

        if (alreadyAligned)
            return ApiResponse<WorkflowTemplateDto>.Ok(MapTemplateDto(oldDefaults.First(t => t.IsDefault)), "Modèle IS (liasse) déjà aligné.");

        foreach (var old in oldDefaults)
        {
            old.IsDefault = false;
            var hasActive = await _db.WorkflowInstances.AnyAsync(wi =>
                wi.WorkflowTemplateId == old.Id && wi.Status == WorkflowInstanceStatus.Active);
            if (!hasActive)
                old.IsActive = false;
            old.Name = old.Name.Contains("(ancien)") ? old.Name : $"{old.Name} (ancien)";
            old.UpdatedBy = userId;
        }

        await _db.SaveChangesAsync();

        isDef.Name = "IS — Déclaration fiscale (Liasse annuelle)";
        var created = await CreateTemplateAsync(isDef, cabinetId, userId);
        if (!created.Success || created.Data == null)
            return ApiResponse<WorkflowTemplateDto>.Fail(created.Message ?? "Échec refresh modèle IS.");

        _logger.LogInformation("IS template refreshed for cabinet {CabinetId}: new template {Id}", cabinetId, created.Data.Id);
        return ApiResponse<WorkflowTemplateDto>.Ok(created.Data, $"Modèle IS rafraîchi (cabinet {cabinetId}). Anciens modèles désactivés comme défaut.");
    }

    /// <summary>
    /// Morocco TVA Init (CGI art. 108/111): declare previous period in month N+1.
    /// Monthly order = declaration cycle: Dec\to Jan, Jan\to Feb, \u2026 Nov\to Dec.
    /// Quarterly: T4\to Jan, T1\to Apr, T2\to Jul, T3\to Oct.
    /// </summary>
    private static string BuildTvaInitActionConfigJson(bool mensuelle)
    {
        // Non-leap pattern (Feb end/due use 28); leap days clamped at materialization.
        var dim = new[] { 0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
        var monthFr = new[]
        {
            "", "janvier", "f\u00e9vrier", "mars", "avril", "mai", "juin",
            "juillet", "ao\u00fbt", "septembre", "octobre", "novembre", "d\u00e9cembre"
        };
        var monthFrCap = new[]
        {
            "", "Janvier", "F\u00e9vrier", "Mars", "Avril", "Mai", "Juin",
            "Juillet", "Ao\u00fbt", "Septembre", "Octobre", "Novembre", "D\u00e9cembre"
        };

        var periods = new List<object>();
        if (mensuelle)
        {
            // Declaration month 1..12: activity = previous month (Jan declares Dec)
            for (var declMonth = 1; declMonth <= 12; declMonth++)
            {
                var activityMonth = declMonth == 1 ? 12 : declMonth - 1;
                var dueDay = dim[declMonth];
                periods.Add(new
                {
                    label = $"{monthFrCap[activityMonth]} (d\u00e9cl. {monthFr[declMonth]})",
                    start = $"{activityMonth:D2}-01",
                    end = $"{activityMonth:D2}-{dim[activityMonth]:D2}",
                    due = $"{declMonth:D2}-{dueDay:D2}",
                    startMonth = activityMonth,
                    startDay = 1,
                    endMonth = activityMonth,
                    endDay = dim[activityMonth],
                    dueMonth = declMonth,
                    dueDay
                });
            }
        }
        else
        {
            // Declaration order: T4\to janvier, T1\to avril, T2\to juillet, T3\to octobre
            var q = new (string Label, int Sm, int Em, int Ed, int Dm, int Dd)[]
            {
                ("Octobre\u2013d\u00e9cembre (d\u00e9cl. janvier)", 10, 12, 31, 1, 31),
                ("Janvier\u2013mars (d\u00e9cl. avril)", 1, 3, 31, 4, 30),
                ("Avril\u2013juin (d\u00e9cl. juillet)", 4, 6, 30, 7, 31),
                ("Juillet\u2013septembre (d\u00e9cl. octobre)", 7, 9, 30, 10, 31)
            };
            foreach (var (label, sm, em, ed, dm, dd) in q)
            {
                periods.Add(new
                {
                    label,
                    start = $"{sm:D2}-01",
                    end = $"{em:D2}-{ed:D2}",
                    due = $"{dm:D2}-{dd:D2}",
                    startMonth = sm,
                    startDay = 1,
                    endMonth = em,
                    endDay = ed,
                    dueMonth = dm,
                    dueDay = dd
                });
            }
        }

        var payload = new
        {
            initTrigger = "both",
            allowManualInit = true,
            periodFrequency = mensuelle ? "Mensuelle" : "Trimestrielle",
            periodTemplate = mensuelle ? "Mensuelle" : "Trimestrielle",
            periods
        };
        return System.Text.Json.JsonSerializer.Serialize(payload);
    }

    private static WorkflowTemplateCreateDto BuildTvaTemplateDefinition(bool mensuelle)
    {
        var freq = mensuelle ? "Mensuelle" : "Trimestrielle";
        var initJson = BuildTvaInitActionConfigJson(mensuelle);
        var cron = mensuelle ? "0 8 1 * *" : "0 8 1 1,4,7,10 *";
        var cronDesc = mensuelle
            ? "Le 1er de chaque mois \u00e0 08:00 \u2014 ouvre la TVA du mois N-1 (d\u00e9cl. mois N, \u00e9ch\u00e9ance fin de mois N)"
            : "Le 1er jan/avr/juil/oct \u00e0 08:00 \u2014 ouvre le trimestre N-1 (ex. janvier = oct\u2013d\u00e9c.)";

        return new WorkflowTemplateCreateDto
        {
            Name = mensuelle ? "TVA Mensuelle" : "TVA Trimestrielle",
            Description = mensuelle
                ? "Workflow TVA mensuelle (Maroc CGI 108/111) : d\u00e9clare le mois N-1 (ex. janvier = d\u00e9cembre, \u00e9ch\u00e9ance 31/01)."
                : "Workflow TVA trimestrielle (Maroc CGI 111) : d\u00e9clare le trimestre N-1 (ex. janvier = oct\u2013d\u00e9c., \u00e9ch\u00e9ance 31/01).",
            DeclarationType = "TVA",
            IsDefault = mensuelle, // Mensuelle = default; Trimestrielle also active for clients TypeTVA=Trimestrielle
            Steps = new()
            {
                new() { Name = "Ouverture / cr\u00e9ation dossier", StepOrder = 0, StepType = "Initialisation", AssignmentType = "AutoSystem", IsRequired = true, IsBlocking = true, AutoCreateTask = false, AutoNotify = true, SLAHours = 1, Description = $"Cr\u00e9ation automatique du dossier TVA {freq.ToLowerInvariant()} pour la p\u00e9riode", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.CreeeParSysteme, ActionOnEntry = "CreateDeclaration", LifecycleEndMode = "PeriodEnd", ActionOnLifecycleEnd = "ClotureDeclaration", TaskType = "Standard", IsScheduled = true, CronExpression = cron, CronDescription = cronDesc, ActionConfigJson = initJson },
                new() { Name = "Demande documents", StepOrder = 1, StepType = "DemandePieces", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 24, Description = "Envoi de la demande de factures d'achat/vente au client", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.DocumentsDemandes, ActionOnEntry = "RequestDocuments", TaskType = "DocumentRequest", TaskPriority = RefCodes.PrioriteTache.Haute, ValidationRulesJson = "{\"requireDocuments\":false,\"minDocuments\":0}" },
                new() { Name = "R\u00e9ception documents (client)", StepOrder = 2, StepType = "Collecte", AssignmentType = "Client", IsClientStep = true, IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 96, Description = "Le client d\u00e9pose les factures \u2014 relances auto si retard", DeclarationStatusOnComplete = RefCodes.StatutDeclaration.DocumentsRecus, TaskType = "ClientAction", TaskPriority = RefCodes.PrioriteTache.Haute, ValidationRulesJson = "{\"requireDocuments\":true,\"minDocuments\":1}", MaxRetries = 3, RetryIntervalHours = 360, OnMaxRetriesAction = "Suspend" },
                new() { Name = "Contr\u00f4le exhaustivit\u00e9", StepOrder = 3, StepType = "Controle", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 24, Description = "V\u00e9rification exhaustivit\u00e9 pi\u00e8ces", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.ControleEnCours, TaskType = "Review", TaskPriority = RefCodes.PrioriteTache.Haute, ValidationRulesJson = "{\"requireDocuments\":true,\"minDocuments\":1}" },
                new() { Name = "Import & calcul TVA", StepOrder = 4, StepType = "Saisie", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 36, Description = "Import factures, ventilation et calcul TVA", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.CalculTVA, DeclarationStatusOnComplete = RefCodes.StatutDeclaration.CalculTVA, TaskType = "DataEntry", TaskPriority = RefCodes.PrioriteTache.Haute },
                new() { Name = "Traitement anomalies", StepOrder = 5, StepType = "Controle", AssignmentType = "Employee", IsRequired = false, IsBlocking = false, AutoCreateTask = true, AutoNotify = true, SLAHours = 24, Description = "Correction des anomalies", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.EnAttenteCorrection, TaskType = "Review", TaskPriority = RefCodes.PrioriteTache.Moyenne },
                new() { Name = "Revue superviseur", StepOrder = 6, StepType = "ValidationInterne", AssignmentType = "Role", AssignedToRoleName = "Manager", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 12, Description = "Approuver ou renvoyer \u00e0 corriger", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.EnRevueSuperviseur, DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Validee, TaskType = "Validation", TaskPriority = RefCodes.PrioriteTache.Urgente, RequiredPermission = "workflow.validate" },
                new() { Name = "Soumission SIMPL-TVA", StepOrder = 7, StepType = "DepotSoumission", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 12, Description = "Soumission SIMPL-TVA", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.EnvoyeeSimplTva, DeclarationStatusOnComplete = RefCodes.StatutDeclaration.EnvoyeeSimplTva, ActionOnComplete = "ExportXML", TaskType = "Submission", TaskPriority = RefCodes.PrioriteTache.Urgente },
                new() { Name = "D\u00e9p\u00f4t confirm\u00e9", StepOrder = 8, StepType = "DepotSoumission", AssignmentType = "AutoSystem", IsRequired = true, IsBlocking = true, AutoCreateTask = false, AutoNotify = true, SLAHours = 1, Description = "Confirmation d\u00e9p\u00f4t", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.Deposee, DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Deposee, TaskType = "Standard" },
                new() { Name = "Paiement (si TVA due)", StepOrder = 9, StepType = "Paiement", AssignmentType = "Employee", IsRequired = false, IsBlocking = false, AutoCreateTask = true, AutoNotify = true, SLAHours = 48, Description = "Paiement si TVA due", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.APayer, DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Payee, ActionOnEntry = "PaymentReminder", TaskType = "Standard", TaskPriority = RefCodes.PrioriteTache.Haute },
                new() { Name = "Rapprochement", StepOrder = 10, StepType = "Rapprochement", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 24, Description = "Rapprochement comptable", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.Rapprochee, DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Rapprochee, TaskType = "Review", TaskPriority = RefCodes.PrioriteTache.Haute },
                new() { Name = "Archivage & cl\u00f4ture", StepOrder = 11, StepType = "ClotureArchivage", AssignmentType = "AutoSystem", IsRequired = true, IsBlocking = false, AutoCreateTask = false, AutoNotify = true, SLAHours = 24, Description = "Archivage puis cl\u00f4ture", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.Archivee, DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Cloturee, ActionOnComplete = "ArchiveDeclaration", TaskType = "Archiving" }
            }
        };
    }

    /// <summary>
    /// Returns all per-type default template definitions.
    /// </summary>
    private static List<WorkflowTemplateCreateDto> GetAllDefaultTemplateDefinitions()
    {
        return new List<WorkflowTemplateCreateDto>
        {
            // ── TVA Mensuelle — échéance de travail = 26 du mois suivant ──
            BuildTvaTemplateDefinition(mensuelle: true),
            // ── TVA Trimestrielle — échéance de travail = 26 du 1er mois du trimestre suivant ──
            BuildTvaTemplateDefinition(mensuelle: false),

            // ── IS (Déclaration fiscale — Liasse annuelle, pattern TVA + métier) ──
            new()
            {
                Name = "IS — Déclaration fiscale (Liasse annuelle)",
                Description = "Workflow liasse fiscale annuelle bout-en-bout : collecte Extrane, saisie, lettrage, révision, retraitements, IS/CM, validation client, SIMPL-IS (stub), paiement et clôture.",
                DeclarationType = "IS", IsDefault = true,
                Steps = new()
                {
                    new() { Name = "Ouverture dossier liasse", StepOrder = 0, StepType = "Initialisation", AssignmentType = "AutoSystem", IsRequired = true, IsBlocking = true, AutoCreateTask = false, AutoNotify = true, SLAHours = 1, Description = "Création automatique du dossier liasse (exercice, clôture, deadline 31/03 N+1)", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.CreeeParSysteme, ActionOnEntry = "CreateDeclaration", LifecycleEndMode = "PeriodEnd", ActionOnLifecycleEnd = "ClotureDeclaration", TaskType = "Standard" },
                    new() { Name = "Demande pièces annuelles", StepOrder = 1, StepType = "DemandePieces", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 72, Description = "Checklist annuelle : inventaire, banques, paie, immobilisations, PV AG…", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.DocumentsDemandes, ActionOnEntry = "RequestDocuments", TaskType = "DocumentRequest", TaskPriority = RefCodes.PrioriteTache.Haute, ValidationRulesJson = "{\"requireDocuments\":false,\"minDocuments\":0,\"checklist\":\"annuelle\"}" },
                    new() { Name = "Réception pièces (client)", StepOrder = 2, StepType = "Collecte", AssignmentType = "Client", IsClientStep = true, IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 168, Description = "Le client dépose les pièces — relances auto si retard", DeclarationStatusOnComplete = RefCodes.StatutDeclaration.DocumentsRecus, TaskType = "ClientAction", TaskPriority = RefCodes.PrioriteTache.Haute, ValidationRulesJson = "{\"requireDocuments\":true,\"minDocuments\":1,\"requireChecklistComplete\":true}", MaxRetries = 3, RetryIntervalHours = 360, OnMaxRetriesAction = "Suspend" },
                    new() { Name = "Contrôle exhaustivité", StepOrder = 3, StepType = "Controle", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 48, Description = "Vérification exhaustivité pièces; Refuser → retour collecte", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.ControleEnCours, TaskType = "Review", TaskPriority = RefCodes.PrioriteTache.Haute, ValidationRulesJson = "{\"requireDocuments\":true,\"minDocuments\":1,\"requireChecklistComplete\":true}" },
                    new() { Name = "Saisie & tenue comptable", StepOrder = 4, StepType = "Saisie", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 96, Description = "Saisie / tenue comptable de l'exercice (lien Comptabilité)", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.EnCours, TaskType = "DataEntry", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Lettrage & rapprochements", StepOrder = 5, StepType = "Controle", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 72, Description = "Lettrage clients/fournisseurs et rapprochements bancaires", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.EnCours, TaskType = "Review", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Révision / inventaire", StepOrder = 6, StepType = "Controle", AssignmentType = "Role", AssignedToRoleName = "Manager", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 72, Description = "Amortissements, stocks, provisions — révision chef de mission", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.EnRevision, TaskType = "Review", TaskPriority = RefCodes.PrioriteTache.Haute, RequiredPermission = "workflow.validate" },
                    new() { Name = "Retraitements fiscaux", StepOrder = 7, StepType = "Saisie", AssignmentType = "Role", AssignedToRoleName = "Manager", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 72, Description = "Réintégrations, déductions, déficits reportables", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.CalculIS, TaskType = "DataEntry", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Calcul IS & cotisation minimale", StepOrder = 8, StepType = "Saisie", AssignmentType = "Role", AssignedToRoleName = "Manager", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 48, Description = "Barème IS + CM + acomptes (EditIS)", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.CalculIS, DeclarationStatusOnComplete = RefCodes.StatutDeclaration.CalculIS, TaskType = "DataEntry", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Revue associé", StepOrder = 9, StepType = "ValidationInterne", AssignmentType = "Role", AssignedToRoleName = "Manager", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 48, Description = "Contrôle qualité associé ; Refuser → étape précédente", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.EnRevueSuperviseur, DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Validee, TaskType = "Validation", TaskPriority = RefCodes.PrioriteTache.Urgente, RequiredPermission = "workflow.validate" },
                    new() { Name = "Validation client (bilan)", StepOrder = 10, StepType = "ValidationClient", AssignmentType = "Client", IsClientStep = true, IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 120, Description = "Le client valide Bilan / CPC / résultat fiscal avant dépôt DGI", TaskType = "ClientAction", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Génération & SIMPL-IS", StepOrder = 11, StepType = "DepotSoumission", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 24, Description = "Pack liasse + stub SIMPL-IS (miroir TVA)", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.EnvoyeeSimplIs, DeclarationStatusOnComplete = RefCodes.StatutDeclaration.EnvoyeeSimplIs, ActionOnComplete = "ExportXML", TaskType = "Submission", TaskPriority = RefCodes.PrioriteTache.Urgente },
                    new() { Name = "Dépôt confirmé", StepOrder = 12, StepType = "DepotSoumission", AssignmentType = "AutoSystem", IsRequired = true, IsBlocking = true, AutoCreateTask = false, AutoNotify = true, SLAHours = 1, Description = "Confirmation dépôt après ACK stub SIMPL-IS", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.Deposee, DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Deposee, TaskType = "Standard" },
                    new() { Name = "Paiement solde IS/CM", StepOrder = 13, StepType = "Paiement", AssignmentType = "Employee", IsRequired = false, IsBlocking = false, AutoCreateTask = true, AutoNotify = true, SLAHours = 72, Description = "Paiement solde IS ou CM (stub comme TVA)", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.APayer, DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Payee, ActionOnEntry = "PaymentReminder", TaskType = "Standard", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Rapprochement", StepOrder = 14, StepType = "Rapprochement", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 48, Description = "Rapprochement comptable (stub lien compta)", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.Rapprochee, DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Rapprochee, TaskType = "Review", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Archivage & clôture", StepOrder = 15, StepType = "ClotureArchivage", AssignmentType = "AutoSystem", IsRequired = true, IsBlocking = false, AutoCreateTask = false, AutoNotify = true, SLAHours = 72, Description = "Archivage liasse, clôture dossier (+ option N+1)", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.Archivee, DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Cloturee, ActionOnComplete = "ArchiveDeclaration", TaskType = "Archiving" }
                }
            },

            // ── IR (Impôt sur le Revenu) ─────────────────────────────────
            new()
            {
                Name = "IR — Impôt sur le Revenu",
                Description = "Workflow pour la déclaration annuelle d'IR professionnel : revenus, charges déductibles, calcul barème et dépôt.",
                DeclarationType = "IR", IsDefault = true,
                Steps = new()
                {
                    new() { Name = "Ouverture dossier IR", StepOrder = 0, StepType = "Initialisation", AssignmentType = "AutoSystem", IsRequired = true, IsBlocking = true, AutoCreateTask = false, AutoNotify = true, SLAHours = 1, Description = "Création du dossier IR pour l'année fiscale", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.Brouillon, TaskType = "Standard" },
                    new() { Name = "Demande justificatifs revenus", StepOrder = 1, StepType = "DemandePieces", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 48, Description = "Demande des bulletins de paie, attestations, revenus fonciers, dividendes", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.ACompleter, TaskType = "DocumentRequest", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Collecte documents", StepOrder = 2, StepType = "Collecte", AssignmentType = "Client", IsClientStep = true, IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 120, Description = "Le client fournit les justificatifs de revenus et charges déductibles", TaskType = "ClientAction", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Calcul IR & déductions", StepOrder = 3, StepType = "Saisie", AssignmentType = "Department", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 48, Description = "Application du barème progressif, déductions (intérêts prêt, assurance, etc.)", DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Completee, TaskType = "DataEntry", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Contrôle & vérification", StepOrder = 4, StepType = "Controle", AssignmentType = "Role", AssignedToRoleName = "Manager", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 24, Description = "Vérification des revenus déclarés, cohérence et conformité", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.EnRevision, TaskType = "Review", TaskPriority = RefCodes.PrioriteTache.Haute, RequiredPermission = "workflow.validate" },
                    new() { Name = "Validation responsable", StepOrder = 5, StepType = "ValidationInterne", AssignmentType = "Role", AssignedToRoleName = "Manager", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 24, Description = "Validation finale par le responsable fiscal du cabinet", DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Validee, TaskType = "Validation", TaskPriority = RefCodes.PrioriteTache.Urgente, RequiredPermission = "workflow.validate" },
                    new() { Name = "Télédéclaration IR", StepOrder = 6, StepType = "DepotSoumission", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 24, Description = "Soumission sur le portail Simpl-IR de la DGI", DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Soumise, TaskType = "Submission", TaskPriority = RefCodes.PrioriteTache.Urgente },
                    new() { Name = "Clôture & archivage", StepOrder = 7, StepType = "ClotureArchivage", AssignmentType = "AutoSystem", IsRequired = true, IsBlocking = false, AutoCreateTask = false, AutoNotify = true, SLAHours = 48, Description = "Archivage des pièces et accusé de réception", TaskType = "Archiving" }
                }
            },

            // ── CNSS (Cotisations sociales) ──────────────────────────────
            new()
            {
                Name = "CNSS — Cotisations Sociales",
                Description = "Workflow mensuel de déclaration CNSS : bordereaux de cotisation, état des salariés, télédéclaration Damancom.",
                DeclarationType = "CNSS", IsDefault = true,
                Steps = new()
                {
                    new() { Name = "Ouverture période CNSS", StepOrder = 0, StepType = "Initialisation", AssignmentType = "AutoSystem", IsRequired = true, IsBlocking = true, AutoCreateTask = false, AutoNotify = true, SLAHours = 1, Description = "Le 1er du mois : ouvre le CNSS du mois précédent (ex. 1 août → CNSS juillet, dépôt jusqu'au 10)", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.Brouillon, ActionOnEntry = "CreateDeclaration", LifecycleEndMode = "PeriodEnd", ActionOnLifecycleEnd = "ClotureDeclaration", TaskType = "Standard", IsScheduled = true, CronExpression = "0 8 1 * *", CronDescription = "Le 1er de chaque mois à 08:00 — CNSS mois précédent (fenêtre dépôt 1–10)", ActionConfigJson = "{\"initTrigger\":\"both\",\"allowManualInit\":true,\"periodFrequency\":\"Mensuelle\",\"windowStartDay\":1,\"windowEndDay\":10,\"windowMonthOffset\":1}" },
                    new() { Name = "Demande état du personnel", StepOrder = 1, StepType = "DemandePieces", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 24, Description = "Demande des entrées/sorties, absences, heures supplémentaires", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.ACompleter, TaskType = "DocumentRequest", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Collecte données paie", StepOrder = 2, StepType = "Collecte", AssignmentType = "Client", IsClientStep = true, IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 72, Description = "Le client transmet les éléments variables de paie du mois", TaskType = "ClientAction", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Calcul cotisations CNSS", StepOrder = 3, StepType = "Saisie", AssignmentType = "Department", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 24, Description = "Calcul des parts salariales et patronales (CNSS, AMO, TFP)", DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Completee, TaskType = "DataEntry", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Contrôle bordereau", StepOrder = 4, StepType = "Controle", AssignmentType = "Role", AssignedToRoleName = "Manager", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 8, Description = "Vérification des montants, effectif déclaré, plafonds", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.EnRevision, TaskType = "Review", TaskPriority = RefCodes.PrioriteTache.Haute, RequiredPermission = "workflow.validate" },
                    new() { Name = "Validation & signature", StepOrder = 5, StepType = "ValidationInterne", AssignmentType = "Role", AssignedToRoleName = "Manager", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 8, Description = "Approbation finale du bordereau de cotisations", DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Validee, TaskType = "Validation", TaskPriority = RefCodes.PrioriteTache.Urgente, RequiredPermission = "workflow.validate" },
                    new() { Name = "Télédéclaration Damancom", StepOrder = 6, StepType = "DepotSoumission", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 12, Description = "Soumission sur le portail Damancom de la CNSS", DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Soumise, TaskType = "Submission", TaskPriority = RefCodes.PrioriteTache.Urgente },
                    new() { Name = "Clôture & archivage", StepOrder = 7, StepType = "ClotureArchivage", AssignmentType = "AutoSystem", IsRequired = true, IsBlocking = false, AutoCreateTask = false, AutoNotify = true, SLAHours = 24, Description = "Archivage du bordereau et de l'accusé de paiement", TaskType = "Archiving" }
                }
            },

            // ── Paie (Traitement des salaires) ───────────────────────────
            new()
            {
                Name = "Paie — Traitement des Salaires",
                Description = "Workflow mensuel de paie : collecte variables, calcul bulletins, validation, virement et déclarations sociales associées.",
                DeclarationType = "Paie", IsDefault = true,
                Steps = new()
                {
                    new() { Name = "Ouverture période paie", StepOrder = 0, StepType = "Initialisation", AssignmentType = "AutoSystem", IsRequired = true, IsBlocking = true, AutoCreateTask = false, AutoNotify = true, SLAHours = 1, Description = "Initialisation du cycle de paie mensuel", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.Brouillon, TaskType = "Standard" },
                    new() { Name = "Demande éléments variables", StepOrder = 1, StepType = "DemandePieces", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 24, Description = "Demande des heures sup, primes, absences, congés, entrées/sorties", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.ACompleter, TaskType = "DocumentRequest", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Collecte éléments variables", StepOrder = 2, StepType = "Collecte", AssignmentType = "Client", IsClientStep = true, IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 72, Description = "Le client transmet les éléments variables de paie", TaskType = "ClientAction", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Calcul bulletins de paie", StepOrder = 3, StepType = "Saisie", AssignmentType = "Department", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 36, Description = "Calcul brut→net, IR salarial, CNSS, AMO, primes, retenues", DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Completee, TaskType = "DataEntry", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Contrôle paie", StepOrder = 4, StepType = "Controle", AssignmentType = "Role", AssignedToRoleName = "Manager", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 12, Description = "Vérification masse salariale, écarts vs mois précédent, conformité", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.EnRevision, TaskType = "Review", TaskPriority = RefCodes.PrioriteTache.Haute, RequiredPermission = "workflow.validate" },
                    new() { Name = "Validation responsable paie", StepOrder = 5, StepType = "ValidationInterne", AssignmentType = "Role", AssignedToRoleName = "Manager", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 8, Description = "Approbation finale des bulletins avant envoi au client", DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Validee, TaskType = "Validation", TaskPriority = RefCodes.PrioriteTache.Urgente, RequiredPermission = "workflow.validate" },
                    new() { Name = "Approbation client", StepOrder = 6, StepType = "ValidationClient", AssignmentType = "Client", IsClientStep = true, IsRequired = false, IsBlocking = false, AutoCreateTask = true, AutoNotify = true, SLAHours = 48, Description = "Le client vérifie et approuve les bulletins de paie", TaskType = "ClientAction", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Envoi bulletins & OV", StepOrder = 7, StepType = "DepotSoumission", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 12, Description = "Envoi des bulletins au client et préparation de l'ordre de virement", DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Soumise, TaskType = "Submission", TaskPriority = RefCodes.PrioriteTache.Urgente },
                    new() { Name = "Clôture & archivage paie", StepOrder = 8, StepType = "ClotureArchivage", AssignmentType = "AutoSystem", IsRequired = true, IsBlocking = false, AutoCreateTask = false, AutoNotify = true, SLAHours = 24, Description = "Archivage bulletins, journaux de paie, écritures comptables", TaskType = "Archiving" }
                }
            },

            // ── Juridique (Actes et formalités juridiques) ───────────────
            new()
            {
                Name = "Juridique — Formalités Légales",
                Description = "Workflow pour les missions juridiques : constitution, modifications statutaires, PV d'AG, dépôt tribunal de commerce.",
                DeclarationType = "Juridique", IsDefault = true,
                Steps = new()
                {
                    new() { Name = "Ouverture dossier juridique", StepOrder = 0, StepType = "Initialisation", AssignmentType = "AutoSystem", IsRequired = true, IsBlocking = true, AutoCreateTask = false, AutoNotify = true, SLAHours = 1, Description = "Création du dossier juridique et identification de la mission", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.Brouillon, TaskType = "Standard" },
                    new() { Name = "Demande pièces légales", StepOrder = 1, StepType = "DemandePieces", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 48, Description = "Demande des statuts, CIN associés, PV précédents, registre de commerce", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.ACompleter, TaskType = "DocumentRequest", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Collecte documents juridiques", StepOrder = 2, StepType = "Collecte", AssignmentType = "Client", IsClientStep = true, IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 120, Description = "Le client transmet les pièces nécessaires (CIN, statuts, mandats)", TaskType = "ClientAction", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Rédaction actes juridiques", StepOrder = 3, StepType = "Saisie", AssignmentType = "Department", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 72, Description = "Rédaction PV AG, modifications statutaires, actes de cession, etc.", DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Completee, TaskType = "DataEntry", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Contrôle juridique", StepOrder = 4, StepType = "Controle", AssignmentType = "Role", AssignedToRoleName = "Manager", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 48, Description = "Relecture par le responsable juridique, conformité légale", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.EnRevision, TaskType = "Review", TaskPriority = RefCodes.PrioriteTache.Haute, RequiredPermission = "workflow.validate" },
                    new() { Name = "Validation & signature client", StepOrder = 5, StepType = "ValidationClient", AssignmentType = "Client", IsClientStep = true, IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 96, Description = "Le client signe les actes et PV via l'extranet ou en personne", TaskType = "ClientAction", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Dépôt tribunal / RC", StepOrder = 6, StepType = "DepotSoumission", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 48, Description = "Dépôt au tribunal de commerce, publication au bulletin officiel", DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Soumise, TaskType = "Submission", TaskPriority = RefCodes.PrioriteTache.Urgente },
                    new() { Name = "Clôture & archivage", StepOrder = 7, StepType = "ClotureArchivage", AssignmentType = "AutoSystem", IsRequired = true, IsBlocking = false, AutoCreateTask = false, AutoNotify = true, SLAHours = 48, Description = "Archivage des actes déposés, accusés et nouveau RC", TaskType = "Archiving" }
                }
            },

            // ── Comptabilite (Tenue comptable générale) ──────────────────
            new()
            {
                Name = "Comptabilité — Tenue Comptable",
                Description = "Workflow pour la tenue comptable courante : collecte pièces, saisie journal, lettrage, rapprochement bancaire, situations intermédiaires.",
                DeclarationType = "Comptabilite", IsDefault = true,
                Steps = new()
                {
                    new() { Name = "Ouverture période comptable", StepOrder = 0, StepType = "Initialisation", AssignmentType = "AutoSystem", IsRequired = true, IsBlocking = true, AutoCreateTask = false, AutoNotify = true, SLAHours = 1, Description = "Initialisation de la période de saisie comptable", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.Brouillon, TaskType = "Standard" },
                    new() { Name = "Demande pièces comptables", StepOrder = 1, StepType = "DemandePieces", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 24, Description = "Demande des factures, relevés bancaires, tickets de caisse, notes de frais", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.ACompleter, TaskType = "DocumentRequest", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Collecte pièces", StepOrder = 2, StepType = "Collecte", AssignmentType = "Client", IsClientStep = true, IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 96, Description = "Le client dépose ses documents comptables sur l'extranet", TaskType = "ClientAction", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Saisie & imputation", StepOrder = 3, StepType = "Saisie", AssignmentType = "Department", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 48, Description = "Saisie des écritures dans les journaux, imputation comptable", DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Completee, TaskType = "DataEntry", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Rapprochement bancaire", StepOrder = 4, StepType = "Controle", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 24, Description = "Lettrage et rapprochement des comptes clients/fournisseurs/banque", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.EnRevision, TaskType = "Review", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Contrôle chef comptable", StepOrder = 5, StepType = "ValidationInterne", AssignmentType = "Role", AssignedToRoleName = "Manager", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 24, Description = "Vérification de la balance, contrôle des anomalies", DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Validee, TaskType = "Validation", TaskPriority = RefCodes.PrioriteTache.Haute, RequiredPermission = "workflow.validate" },
                    new() { Name = "Clôture période", StepOrder = 6, StepType = "ClotureArchivage", AssignmentType = "AutoSystem", IsRequired = true, IsBlocking = false, AutoCreateTask = false, AutoNotify = true, SLAHours = 24, Description = "Clôture de la période comptable et archivage des écritures", TaskType = "Archiving" }
                }
            },

            // ── IR_SALAIRES (Déclaration IR salariés) ────────────────────
            new()
            {
                Name = "IR Salaires — Retenue à la Source",
                Description = "Workflow pour la déclaration mensuelle d'IR prélevé à la source sur les salaires versés aux employés.",
                DeclarationType = "IR_SALAIRES", IsDefault = true,
                Steps = new()
                {
                    new() { Name = "Ouverture déclaration IR", StepOrder = 0, StepType = "Initialisation", AssignmentType = "AutoSystem", IsRequired = true, IsBlocking = true, AutoCreateTask = false, AutoNotify = true, SLAHours = 1, Description = "Initialisation de la déclaration IR salarié du mois", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.Brouillon, TaskType = "Standard" },
                    new() { Name = "Collecte bulletins", StepOrder = 1, StepType = "DemandePieces", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 24, Description = "Récupération des bulletins de paie validés du mois", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.ACompleter, TaskType = "DocumentRequest", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Calcul IR retenu", StepOrder = 2, StepType = "Saisie", AssignmentType = "Department", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 24, Description = "Calcul de l'IR retenu à la source par salarié selon le barème", DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Completee, TaskType = "DataEntry", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Contrôle déclaration", StepOrder = 3, StepType = "Controle", AssignmentType = "Role", AssignedToRoleName = "Manager", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 8, Description = "Vérification des montants IR vs masse salariale brute", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.EnRevision, TaskType = "Review", TaskPriority = RefCodes.PrioriteTache.Haute, RequiredPermission = "workflow.validate" },
                    new() { Name = "Validation", StepOrder = 4, StepType = "ValidationInterne", AssignmentType = "Role", AssignedToRoleName = "Manager", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 8, Description = "Approbation finale de la déclaration IR", DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Validee, TaskType = "Validation", TaskPriority = RefCodes.PrioriteTache.Urgente, RequiredPermission = "workflow.validate" },
                    new() { Name = "Télédéclaration IR", StepOrder = 5, StepType = "DepotSoumission", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 12, Description = "Soumission de la déclaration d'IR sur le portail DGI", DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Soumise, TaskType = "Submission", TaskPriority = RefCodes.PrioriteTache.Urgente },
                    new() { Name = "Clôture & archivage", StepOrder = 6, StepType = "ClotureArchivage", AssignmentType = "AutoSystem", IsRequired = true, IsBlocking = false, AutoCreateTask = false, AutoNotify = true, SLAHours = 24, Description = "Archivage de l'accusé de dépôt et classement", TaskType = "Archiving" }
                }
            },

            // ── DTS / État 9421 (Déclaration des Traitements et Salaires) ──
            new()
            {
                Name = "DTS — État 9421 Traitements et Salaires",
                Description = "Workflow annuel D.T.S / État 9421 : collecte des états de paie, établissement de la déclaration, contrôle, validation et télédéclaration DGI / SIMPL-Salaires.",
                DeclarationType = "DTS", IsDefault = true,
                Steps = new()
                {
                    new() { Name = "Ouverture période DTS", StepOrder = 0, StepType = "Initialisation", AssignmentType = "AutoSystem", IsRequired = true, IsBlocking = true, AutoCreateTask = false, AutoNotify = true, SLAHours = 1, Description = "Création automatique de la déclaration annuelle DTS / État 9421", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.Brouillon, ActionOnEntry = "CreateDeclaration", LifecycleEndMode = "PeriodEnd", ActionOnLifecycleEnd = "ClotureDeclaration", TaskType = "Standard" },
                    new() { Name = "Demande états de paie", StepOrder = 1, StepType = "DemandePieces", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 48, Description = "Demande des états du personnel, bulletins annuels et éléments de rémunération", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.ACompleter, TaskType = "DocumentRequest", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Collecte données annuelles", StepOrder = 2, StepType = "Collecte", AssignmentType = "Client", IsClientStep = true, IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 120, Description = "Le client transmet les états de paie et variables de l'exercice", TaskType = "ClientAction", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Établissement État 9421", StepOrder = 3, StepType = "Saisie", AssignmentType = "Department", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 48, Description = "Saisie des traitements, salaires et retenues pour l'État 9421", DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Completee, TaskType = "DataEntry", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Contrôle déclaration DTS", StepOrder = 4, StepType = "Controle", AssignmentType = "Role", AssignedToRoleName = "Manager", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 24, Description = "Vérification masse salariale, effectif et cohérence IR / CNSS", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.EnRevision, TaskType = "Review", TaskPriority = RefCodes.PrioriteTache.Haute, RequiredPermission = "workflow.validate" },
                    new() { Name = "Validation DTS", StepOrder = 5, StepType = "ValidationInterne", AssignmentType = "Role", AssignedToRoleName = "Manager", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 12, Description = "Approbation finale de la déclaration des traitements et salaires", DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Validee, TaskType = "Validation", TaskPriority = RefCodes.PrioriteTache.Urgente, RequiredPermission = "workflow.validate" },
                    new() { Name = "Télédéclaration DGI / SIMPL-Salaires", StepOrder = 6, StepType = "DepotSoumission", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 24, Description = "Soumission de l'État 9421 sur le portail DGI / SIMPL-Salaires", DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Soumise, TaskType = "Submission", TaskPriority = RefCodes.PrioriteTache.Urgente },
                    new() { Name = "Clôture & archivage DTS", StepOrder = 7, StepType = "ClotureArchivage", AssignmentType = "AutoSystem", IsRequired = true, IsBlocking = false, AutoCreateTask = false, AutoNotify = true, SLAHours = 24, Description = "Archivage de l'accusé de dépôt et classement de l'État 9421", TaskType = "Archiving" }
                }
            },

            // ── Domiciliation (courrier + documents — employee-initiated, not system) ──
            new()
            {
                Name = "Domiciliation — Siège social & courrier",
                Description = "Workflow opérationnel domiciliation initié par un collaborateur du cabinet (pas le système). Étapes employés + étapes client pour dépôt / actions.",
                DeclarationType = "Domiciliation", IsDefault = true,
                Steps = new()
                {
                    // Init = Employee (unlike TVA/CNSS AutoSystem CreateDeclaration)
                    new() { Name = "Ouverture dossier par le collaborateur", StepOrder = 0, StepType = "Initialisation", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 8, Description = "Un employé du cabinet crée / démarre le dossier domiciliation pour ce client", TaskType = "Standard", TaskPriority = RefCodes.PrioriteTache.Haute, ServiceContratTypeCode = "DOMICILIATION", UseServiceContratAffectation = true, TriggerSource = "Employee" },
                    new() { Name = "Demande documents KYC", StepOrder = 1, StepType = "DemandePieces", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 24, Description = "Le collaborateur envoie la demande de pièces (CIN, RC, statuts…)", ActionOnEntry = "RequestDocuments", TaskType = "DocumentRequest", TaskPriority = RefCodes.PrioriteTache.Haute, ServiceContratTypeCode = "DOMICILIATION", ValidationRulesJson = "{\"requireDocuments\":false,\"minDocuments\":0,\"domain\":\"domiciliation\"}" },
                    new() { Name = "Dépôt documents (client)", StepOrder = 2, StepType = "Collecte", AssignmentType = "Client", IsClientStep = true, IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 120, Description = "Le client dépose les pièces via l'extranet", TaskType = "ClientAction", TaskPriority = RefCodes.PrioriteTache.Haute, ValidationRulesJson = "{\"requireDocuments\":true,\"minDocuments\":1,\"domain\":\"domiciliation\"}", MaxRetries = 3, RetryIntervalHours = 360, OnMaxRetriesAction = "EscalateToManager" },
                    new() { Name = "Contrôle / validation pièces", StepOrder = 3, StepType = "Controle", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 48, Description = "Collaborateur : valider / rejeter / demander correction", TaskType = "Review", TaskPriority = RefCodes.PrioriteTache.Haute, ServiceContratTypeCode = "DOMICILIATION" },
                    new() { Name = "Traitement courrier & opérations", StepOrder = 4, StepType = "Controle", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 72, Description = "Collaborateur : réception, classement, traitement des courriers", TaskType = "Review", TaskPriority = RefCodes.PrioriteTache.Haute, ServiceContratTypeCode = "DOMICILIATION" },
                    new() { Name = "Notification client (par le cabinet)", StepOrder = 5, StepType = "ValidationInterne", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 24, Description = "Le collaborateur notifie le client (correspondance / statut)", ActionOnComplete = "NotifyClient", TaskType = "Validation", TaskPriority = RefCodes.PrioriteTache.Haute, ServiceContratTypeCode = "DOMICILIATION" },
                    new() { Name = "Actions client (retrait / réexpédition)", StepOrder = 6, StepType = "ValidationClient", AssignmentType = "Client", IsClientStep = true, IsRequired = false, IsBlocking = false, AutoCreateTask = true, AutoNotify = true, SLAHours = 72, Description = "Le client demande retrait ou réexpédition si besoin", TaskType = "ClientAction", TaskPriority = RefCodes.PrioriteTache.Moyenne },
                    new() { Name = "Clôture dossier", StepOrder = 7, StepType = "ClotureArchivage", AssignmentType = "Employee", IsRequired = true, IsBlocking = false, AutoCreateTask = true, AutoNotify = true, SLAHours = 24, Description = "Collaborateur archive et clôture le cycle", TaskType = "Archiving", TaskPriority = RefCodes.PrioriteTache.Moyenne }
                }
            }
        };
    }

    // ╔══════════════════════════════════════════════════════════════════════╗
    // ║                    INSTANCES (WORKFLOW EXECUTION)                    ║
    // ╚══════════════════════════════════════════════════════════════════════╝

    public async Task<ApiResponse<WorkflowInstanceDto>> StartWorkflowAsync(StartWorkflowDto dto, int cabinetId, string userId)
    {
        // 1. Resolve template
        WorkflowTemplate? template;
        if (dto.WorkflowTemplateId.HasValue)
        {
            template = await _db.WorkflowTemplates
                .Include(wt => wt.Steps.Where(s => !s.IsDeleted).OrderBy(s => s.StepOrder))
                .FirstOrDefaultAsync(wt => wt.Id == dto.WorkflowTemplateId.Value && wt.CabinetId == cabinetId && wt.IsActive);
        }
        else
        {
            // Prefer client affectation pointer (client-owned copy) when present
            template = null;
            if (!string.IsNullOrWhiteSpace(dto.DeclarationType)
                && Enum.TryParse<WorkflowDeclarationType>(dto.DeclarationType, true, out var startDecl))
            {
                var cfg = await _db.ClientWorkflowConfigs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c =>
                        c.ClientId == dto.ClientId
                        && c.CabinetId == cabinetId
                        && c.IsActive
                        && !c.IsDeleted
                        && (c.DeclarationType == startDecl || c.DeclarationType == WorkflowDeclarationType.All)
                        && c.WorkflowTemplateId != null);
                if (cfg?.WorkflowTemplateId != null)
                {
                    template = await _db.WorkflowTemplates
                        .Include(wt => wt.Steps.Where(s => !s.IsDeleted).OrderBy(s => s.StepOrder))
                        .FirstOrDefaultAsync(wt => wt.Id == cfg.WorkflowTemplateId.Value && wt.CabinetId == cabinetId && wt.IsActive && !wt.IsDeleted);
                }
            }

            template ??= await ResolveTemplateAsync(cabinetId, dto.ClientId, dto.DeclarationType);
        }

        if (template == null || !template.Steps.Any(s => !s.IsDeleted))
        {
            // Auto-create default template if none exists
            _logger.LogInformation("No workflow template found for cabinet {CabinetId}, creating default.", cabinetId);
            var createResult = await CreateDefaultTemplateAsync(cabinetId, userId);
            if (!createResult.Success || createResult.Data == null)
                return ApiResponse<WorkflowInstanceDto>.Fail("Impossible de créer le modèle de workflow par défaut.");

            template = await _db.WorkflowTemplates
                .Include(wt => wt.Steps.Where(s => !s.IsDeleted).OrderBy(s => s.StepOrder))
                .FirstOrDefaultAsync(wt => wt.Id == createResult.Data.Id);

            if (template == null)
                return ApiResponse<WorkflowInstanceDto>.Fail("Erreur lors de la création du modèle.");
        }

        // 2. Check for duplicate active workflow on same declaration
        if (dto.DeclarationId.HasValue)
        {
            var existingActive = await _db.WorkflowInstances
                .AnyAsync(wi => wi.DeclarationId == dto.DeclarationId.Value && wi.CabinetId == cabinetId && wi.Status == WorkflowInstanceStatus.Active);
            if (existingActive)
                return ApiResponse<WorkflowInstanceDto>.Fail("Un workflow actif existe déjà pour cette déclaration.");
        }
        else if (dto.ClientId > 0 && template.DeclarationType != WorkflowDeclarationType.All)
        {
            // Prevent two active typed workflows (CNSS, DTS, …) for the same client
            var declType = template.DeclarationType;
            var existingTyped = await (
                from wi in _db.WorkflowInstances
                join wt in _db.WorkflowTemplates on wi.WorkflowTemplateId equals wt.Id
                where wi.CabinetId == cabinetId
                    && wi.ClientId == dto.ClientId
                    && wi.Status == WorkflowInstanceStatus.Active
                    && wt.DeclarationType == declType
                select wi.Id).AnyAsync();
            if (existingTyped)
                return ApiResponse<WorkflowInstanceDto>.Fail(
                    $"Un workflow actif existe déjà pour ce client ({declType}).");
        }

        // 3. Load client config for overrides
        ClientWorkflowConfig? clientConfig = null;
        if (dto.ClientId > 0)
        {
            WorkflowDeclarationType? declTypeEnum = null;
            if (!string.IsNullOrEmpty(dto.DeclarationType) && Enum.TryParse<WorkflowDeclarationType>(dto.DeclarationType, true, out var parsed))
                declTypeEnum = parsed;

            clientConfig = await _db.ClientWorkflowConfigs
                .FirstOrDefaultAsync(c => c.ClientId == dto.ClientId && c.CabinetId == cabinetId && c.IsActive && !c.IsDeleted &&
                    (c.DeclarationType == (declTypeEnum ?? WorkflowDeclarationType.All) || c.DeclarationType == WorkflowDeclarationType.All));
        }

        // 4. Resolve exercice (from declaration or open exercice)
        int? exerciceId = null;
        if (dto.DeclarationId is int declId)
        {
            exerciceId = await _db.Declarations.AsNoTracking()
                .Where(d => d.Id == declId && d.CabinetId == cabinetId)
                .Select(d => d.ExerciceComptableId)
                .FirstOrDefaultAsync();
        }
        if (exerciceId == null && dto.ClientId > 0)
        {
            exerciceId = await _db.ExercicesComptables.AsNoTracking()
                .Where(e => e.ClientId == dto.ClientId && e.CabinetId == cabinetId && e.Statut == "Ouvert")
                .OrderByDescending(e => e.Annee)
                .Select(e => (int?)e.Id)
                .FirstOrDefaultAsync();
        }

        // 5. Create workflow instance
        var instance = new WorkflowInstance
        {
            WorkflowTemplateId = template.Id,
            DeclarationId = dto.DeclarationId,
            ClientId = dto.ClientId,
            ExerciceComptableId = exerciceId,
            CurrentStepOrder = 0,
            Status = WorkflowInstanceStatus.Active,
            StartedByUserId = userId,
            StartedAt = DateTime.UtcNow,
            CabinetId = cabinetId,
            CreatedBy = userId
        };

        _db.WorkflowInstances.Add(instance);
        await _db.SaveChangesAsync();

        // 6. Create all step instances from template
        var activeSteps = template.Steps.Where(s => !s.IsDeleted).OrderBy(s => s.StepOrder).ToList();

        // Skip client validation step if disabled in client config
        if (clientConfig != null && !clientConfig.ClientValidationEnabled)
            activeSteps = activeSteps.Where(s => s.StepType != WorkflowStepType.ValidationClient).ToList();
        if (clientConfig != null && !clientConfig.DocumentRequestEnabled)
            activeSteps = activeSteps.Where(s => s.StepType != WorkflowStepType.DemandePieces).ToList();

        foreach (var stepTemplate in activeSteps)
        {
            // DueDate is computed when the step actually opens (ActivateStepAsync) from DeadlineAnchor.
            var stepInstance = new WorkflowStepInstance
            {
                WorkflowInstanceId = instance.Id,
                WorkflowStepTemplateId = stepTemplate.Id,
                StepOrder = stepTemplate.StepOrder,
                StepName = stepTemplate.Name,
                Status = WorkflowStepStatus.Pending,
                DueDate = null,
                CabinetId = cabinetId,
                CreatedBy = userId
            };

            _db.WorkflowStepInstances.Add(stepInstance);
        }

        // 6. Record history
        _db.WorkflowHistories.Add(new WorkflowHistory
        {
            WorkflowInstanceId = instance.Id,
            Action = "WorkflowStarted",
            NewStatus = "Active",
            Comment = $"Workflow démarré à partir du modèle '{template.Name}'",
            UserId = userId,
            CabinetId = cabinetId,
            CreatedBy = userId
        });

        await _db.SaveChangesAsync();

        // 6b. Auto-create / link declaration for typed fiscal workflows (CNSS, TVA, …).
        // Domiciliation is operational (courrier/docs) — never create a Declaration shell.
        if (!instance.DeclarationId.HasValue
            && template.DeclarationType != WorkflowDeclarationType.All
            && template.DeclarationType != WorkflowDeclarationType.Domiciliation)
        {
            instance.WorkflowTemplate = template;
            await EnsureDeclarationLinkedAsync(instance, cabinetId, userId);
            await _db.SaveChangesAsync();
        }

        // 7. Activate first step
        await ActivateStepAsync(instance.Id, 0, cabinetId, userId);

        _logger.LogInformation("Workflow instance {Id} started from template {TemplateId} for client {ClientId}", instance.Id, template.Id, dto.ClientId);
        return await GetInstanceByIdAsync(instance.Id, cabinetId);
    }

    public async Task<ApiResponse<WorkflowInstanceDto>> GetInstanceByIdAsync(int id, int cabinetId)
    {
        var wi = await _db.WorkflowInstances
            .Where(wi => wi.Id == id && wi.CabinetId == cabinetId)
            .Include(wi => wi.WorkflowTemplate)
            .Include(wi => wi.Client)
            .Include(wi => wi.Declaration)
            .Include(wi => wi.StartedByUser)
            .Include(wi => wi.StepInstances.OrderBy(s => s.StepOrder))
                .ThenInclude(si => si.AssignedToUser)
            .Include(wi => wi.StepInstances)
                .ThenInclude(si => si.CompletedByUser)
            // Do NOT ThenInclude WorkflowStepTemplate here: soft-deleted templates
            // (after Builder refresh / client re-sync) would filter out the steps via query filter.
            .Include(wi => wi.History.OrderByDescending(h => h.ActionDate).Take(20))
            .AsSplitQuery()
            .FirstOrDefaultAsync();

        if (wi == null)
            return ApiResponse<WorkflowInstanceDto>.Fail("Instance de workflow non trouvée.");

        await AttachStepTemplatesIgnoreDeletedAsync(wi.StepInstances);

        // Heal open typed workflows that were started without CreateDeclaration on Init
        if (wi.Status == WorkflowInstanceStatus.Active
            && !wi.DeclarationId.HasValue
            && wi.WorkflowTemplate != null
            && wi.WorkflowTemplate.DeclarationType != WorkflowDeclarationType.All)
        {
            await EnsureDeclarationLinkedAsync(wi, cabinetId, wi.StartedByUserId ?? "system");
            await _db.SaveChangesAsync();
        }

        var dto = MapInstanceDto(wi);
        await EnrichStepsWithOpenTakesAsync(dto.StepInstances, cabinetId);
        return ApiResponse<WorkflowInstanceDto>.Ok(dto);
    }

    /// <summary>
    /// Loads step templates including soft-deleted ones so in-flight instances keep their steps
    /// after a cabinet/client template refresh that soft-deletes prior step definitions.
    /// </summary>
    private async Task AttachStepTemplatesIgnoreDeletedAsync(IEnumerable<WorkflowStepInstance>? steps)
    {
        var list = steps?.ToList() ?? new List<WorkflowStepInstance>();
        if (list.Count == 0) return;

        var ids = list.Select(s => s.WorkflowStepTemplateId).Distinct().ToList();
        var templates = await _db.WorkflowStepTemplates
            .IgnoreQueryFilters()
            .Where(t => ids.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id);

        foreach (var s in list)
        {
            if (templates.TryGetValue(s.WorkflowStepTemplateId, out var tpl))
                s.WorkflowStepTemplate = tpl;
        }
    }

    public async Task<ApiResponse<PagedResult<WorkflowInstanceDto>>> GetInstancesAsync(
        int cabinetId, int? clientId = null, string? status = null, int? declarationId = null,
        int page = 1, int pageSize = 20, int? exerciceComptableId = null)
    {
        var query = _db.WorkflowInstances
            .Where(wi => wi.CabinetId == cabinetId)
            .Include(wi => wi.WorkflowTemplate)
            .Include(wi => wi.Client)
            .Include(wi => wi.Declaration)
            .Include(wi => wi.StepInstances)
            .AsQueryable();

        if (clientId.HasValue)
            query = query.Where(wi => wi.ClientId == clientId.Value);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<WorkflowInstanceStatus>(status, true, out var st))
            query = query.Where(wi => wi.Status == st);

        if (declarationId.HasValue)
            query = query.Where(wi => wi.DeclarationId == declarationId.Value);

        if (exerciceComptableId is > 0)
        {
            var exoId = exerciceComptableId.Value;
            query = query.Where(wi =>
                wi.ExerciceComptableId == exoId
                || (wi.Declaration != null && wi.Declaration.ExerciceComptableId == exoId));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(wi => wi.StartedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

        return ApiResponse<PagedResult<WorkflowInstanceDto>>.Ok(new PagedResult<WorkflowInstanceDto>
        {
            Items = items.Select(MapInstanceDto).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<bool>> CancelWorkflowAsync(int instanceId, string reason, int cabinetId, string userId)
    {
        var instance = await _db.WorkflowInstances
            .Include(wi => wi.StepInstances)
            .FirstOrDefaultAsync(wi => wi.Id == instanceId && wi.CabinetId == cabinetId);

        if (instance == null)
            return ApiResponse<bool>.Fail("Instance non trouvée.");
        if (instance.Status != WorkflowInstanceStatus.Active && instance.Status != WorkflowInstanceStatus.Paused)
            return ApiResponse<bool>.Fail("Ce workflow ne peut pas être annulé dans son état actuel.");

        instance.Status = WorkflowInstanceStatus.Cancelled;
        instance.CancelledAt = DateTime.UtcNow;
        instance.CancellationReason = reason;

        // Cancel all pending steps + close open unit takes
        foreach (var step in instance.StepInstances.Where(s => s.Status == WorkflowStepStatus.Pending || s.Status == WorkflowStepStatus.InProgress))
        {
            step.Status = WorkflowStepStatus.Skipped;
            step.Notes = $"Annulé: {reason}";
            await CloseWorkflowStepTakesAsync(step, cabinetId, userId, $"Annulation workflow: {reason}");
        }

        _db.WorkflowHistories.Add(new WorkflowHistory
        {
            WorkflowInstanceId = instanceId,
            Action = "WorkflowCancelled",
            PreviousStatus = "Active",
            NewStatus = "Cancelled",
            Comment = reason,
            UserId = userId,
            UserName = await GetUserNameAsync(userId),
            CabinetId = cabinetId,
            CreatedBy = userId
        });

        await _db.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Workflow annulé.");
    }

    public async Task<ApiResponse<bool>> PauseWorkflowAsync(int instanceId, int cabinetId, string userId)
    {
        var instance = await _db.WorkflowInstances.FirstOrDefaultAsync(wi => wi.Id == instanceId && wi.CabinetId == cabinetId);
        if (instance == null)
            return ApiResponse<bool>.Fail("Instance non trouvée.");
        if (instance.Status != WorkflowInstanceStatus.Active)
            return ApiResponse<bool>.Fail("Seul un workflow actif peut être mis en pause.");

        instance.Status = WorkflowInstanceStatus.Paused;

        _db.WorkflowHistories.Add(new WorkflowHistory
        {
            WorkflowInstanceId = instanceId,
            Action = "WorkflowPaused",
            PreviousStatus = "Active",
            NewStatus = "Paused",
            UserId = userId,
            UserName = await GetUserNameAsync(userId),
            CabinetId = cabinetId,
            CreatedBy = userId
        });

        await _db.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Workflow mis en pause.");
    }

    public async Task<ApiResponse<bool>> ResumeWorkflowAsync(int instanceId, int cabinetId, string userId)
    {
        var instance = await _db.WorkflowInstances.FirstOrDefaultAsync(wi => wi.Id == instanceId && wi.CabinetId == cabinetId);
        if (instance == null)
            return ApiResponse<bool>.Fail("Instance non trouvée.");
        if (instance.Status != WorkflowInstanceStatus.Paused)
            return ApiResponse<bool>.Fail("Seul un workflow en pause peut être repris.");

        instance.Status = WorkflowInstanceStatus.Active;

        _db.WorkflowHistories.Add(new WorkflowHistory
        {
            WorkflowInstanceId = instanceId,
            Action = "WorkflowResumed",
            PreviousStatus = "Paused",
            NewStatus = "Active",
            UserId = userId,
            UserName = await GetUserNameAsync(userId),
            CabinetId = cabinetId,
            CreatedBy = userId
        });

        await _db.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Workflow repris.");
    }

    // ╔══════════════════════════════════════════════════════════════════════╗
    // ║                       STEP OPERATIONS                               ║
    // ╚══════════════════════════════════════════════════════════════════════╝

    public async Task<ApiResponse<WorkflowStepInstanceDto>> AdvanceStepAsync(int instanceId, AdvanceStepDto dto, int cabinetId, string userId)
    {
        var instance = await _db.WorkflowInstances
            .Include(wi => wi.StepInstances.OrderBy(s => s.StepOrder))
            .Include(wi => wi.Declaration)
            .FirstOrDefaultAsync(wi => wi.Id == instanceId && wi.CabinetId == cabinetId);

        if (instance == null)
            return ApiResponse<WorkflowStepInstanceDto>.Fail("Instance non trouvée.");
        if (instance.Status != WorkflowInstanceStatus.Active)
            return ApiResponse<WorkflowStepInstanceDto>.Fail("Le workflow n'est pas actif.");

        await AttachStepTemplatesIgnoreDeletedAsync(instance.StepInstances);

        // Find current active step
        var currentStep = instance.StepInstances
            .FirstOrDefault(s => s.Status == WorkflowStepStatus.InProgress || s.Status == WorkflowStepStatus.WaitingForClient || s.Status == WorkflowStepStatus.WaitingForValidation);

        if (currentStep == null)
        {
            // Try to find next pending step
            currentStep = instance.StepInstances.FirstOrDefault(s => s.Status == WorkflowStepStatus.Pending);
            if (currentStep == null)
                return ApiResponse<WorkflowStepInstanceDto>.Fail("Aucune étape active à avancer.");
        }

        var stepTemplate = currentStep.WorkflowStepTemplate;

        // Repair assignee when template has no explicit users and current user owns this role on the dossier
        await TryClaimStepIfEligibleAsync(currentStep, stepTemplate, instance, cabinetId, userId);

        // Strict assignee-only: SYSTEM (Hangfire), step assignee / template CSV, or any user of this client on client steps
        if (!await CanUserAdvanceStepAsync(currentStep, stepTemplate, instance, userId))
            return ApiResponse<WorkflowStepInstanceDto>.Fail(
                "Seul le collaborateur (ou le client) assigné à cette étape peut l'avancer.");

        // Document gate: Collecte / Controle / Relance may require ≥1 document
        if (!dto.ForceSkip && stepTemplate != null && instance.DeclarationId.HasValue)
        {
            var gateError = await EvaluateDocumentGateAsync(stepTemplate, instance.DeclarationId.Value, cabinetId);
            if (gateError != null)
                return ApiResponse<WorkflowStepInstanceDto>.Fail(gateError);
        }

        // Check blocking: all previous blocking steps must be completed
        if (stepTemplate != null && stepTemplate.IsBlocking)
        {
            var blockingIncomplete = instance.StepInstances
                .Where(s => s.StepOrder < currentStep.StepOrder && s.Status != WorkflowStepStatus.Completed && s.Status != WorkflowStepStatus.Skipped)
                .Any(s => s.WorkflowStepTemplate?.IsBlocking == true);

            if (blockingIncomplete)
                return ApiResponse<WorkflowStepInstanceDto>.Fail("Des étapes bloquantes précédentes ne sont pas terminées.");
        }

        // Handle skip
        if (dto.ForceSkip && stepTemplate != null && !stepTemplate.IsRequired)
        {
            currentStep.Status = WorkflowStepStatus.Skipped;
            currentStep.CompletedAt = DateTime.UtcNow;
            currentStep.CompletedByUserId = userId;
            currentStep.Notes = dto.Notes ?? "Étape ignorée";

            _db.WorkflowHistories.Add(new WorkflowHistory
            {
                WorkflowInstanceId = instanceId,
                WorkflowStepInstanceId = currentStep.Id,
                Action = "StepSkipped",
                PreviousStatus = currentStep.Status.ToString(),
                NewStatus = "Skipped",
                Comment = dto.Comment ?? dto.Notes,
                UserId = userId,
                UserName = await GetUserNameAsync(userId),
                CabinetId = cabinetId,
                CreatedBy = userId
            });
        }
        else
        {
            // Complete current step
            var previousStatus = currentStep.Status.ToString();
            currentStep.Status = WorkflowStepStatus.Completed;
            currentStep.CompletedAt = DateTime.UtcNow;
            currentStep.CompletedByUserId = userId;
            currentStep.Notes = dto.Notes;

            // Execute ActionOnComplete
            if (stepTemplate != null && stepTemplate.ActionOnComplete != WorkflowActionType.None)
                await ExecuteStepActionAsync(stepTemplate.ActionOnComplete, instance, currentStep, cabinetId, userId);

            // Update declaration status if configured
            if (stepTemplate?.DeclarationStatusOnComplete != null && instance.Declaration != null)
            {
                instance.Declaration.Statut = stepTemplate.DeclarationStatusOnComplete;
                instance.Declaration.UpdatedBy = userId;
            }

            // Stop further relances once the waiting step is completed
            currentStep.NextRetryAt = null;
            if (stepTemplate?.StepType == WorkflowStepType.Collecte
                || stepTemplate?.DeclarationStatusOnComplete == RefCodes.StatutDeclaration.DocumentsRecus)
            {
                foreach (var relance in instance.StepInstances.Where(s =>
                    s.Id != currentStep.Id && s.NextRetryAt != null
                    && (s.WorkflowStepTemplate?.MaxRetries > 0
                        || s.WorkflowStepTemplate?.ActionOnEntry == WorkflowActionType.RelanceClient
                        || s.WorkflowStepTemplate?.StepType == WorkflowStepType.Relance)))
                {
                    relance.NextRetryAt = null;
                }
            }

            _db.WorkflowHistories.Add(new WorkflowHistory
            {
                WorkflowInstanceId = instanceId,
                WorkflowStepInstanceId = currentStep.Id,
                Action = "StepCompleted",
                PreviousStatus = previousStatus,
                NewStatus = "Completed",
                Comment = dto.Comment ?? dto.Notes,
                UserId = userId,
                UserName = await GetUserNameAsync(userId),
                CabinetId = cabinetId,
                CreatedBy = userId
            });
        }

        // Close the linked task so "Mes tâches" reflects completion
        if (currentStep.TaskItemId.HasValue)
        {
            var linkedTask = await _db.TaskItems.FirstOrDefaultAsync(t => t.Id == currentStep.TaskItemId.Value);
            if (linkedTask != null && linkedTask.Statut != "Termine" && linkedTask.Statut != "Annule")
            {
                linkedTask.Statut = "Termine";
                linkedTask.UpdatedAt = DateTime.UtcNow;
                linkedTask.UpdatedBy = userId;
            }
        }

        await _db.SaveChangesAsync();

        // Close exclusive UnitTakes for this step (and linked task)
        await CloseWorkflowStepTakesAsync(currentStep, cabinetId, userId, dto.Comment ?? dto.Notes);

        // Find and activate next step
        var nextStep = instance.StepInstances
            .Where(s => s.StepOrder > currentStep.StepOrder && s.Status == WorkflowStepStatus.Pending)
            .OrderBy(s => s.StepOrder)
            .FirstOrDefault();

        if (nextStep != null)
        {
            instance.CurrentStepOrder = nextStep.StepOrder;
            await _db.SaveChangesAsync();
            // Pass completer so consecutive Comptable steps (Contrôle → Calcul → Anomalies) stay with them
            await ActivateStepAsync(instanceId, nextStep.StepOrder, cabinetId, userId, preferredAssigneeUserId: userId);
        }
        else
        {
            // All steps done → complete workflow
            instance.Status = WorkflowInstanceStatus.Completed;
            instance.CompletedAt = DateTime.UtcNow;
            instance.CurrentStepOrder = currentStep.StepOrder;

            _db.WorkflowHistories.Add(new WorkflowHistory
            {
                WorkflowInstanceId = instanceId,
                Action = "WorkflowCompleted",
                PreviousStatus = "Active",
                NewStatus = "Completed",
                Comment = "Toutes les étapes sont terminées.",
                UserId = userId,
                UserName = await GetUserNameAsync(userId),
                CabinetId = cabinetId,
                CreatedBy = userId
            });

            await _db.SaveChangesAsync();
            _logger.LogInformation("Workflow instance {Id} completed", instanceId);
        }

        // Reload to return updated state
        var reloaded = await _db.WorkflowStepInstances
            .Include(s => s.AssignedToUser)
            .Include(s => s.CompletedByUser)
            .Include(s => s.WorkflowStepTemplate)
            .FirstOrDefaultAsync(s => s.Id == currentStep.Id);

        return ApiResponse<WorkflowStepInstanceDto>.Ok(MapStepInstanceDto(reloaded!));
    }

    /// <summary>
    /// Refuse the current step and send the workflow back to the previous actionable step
    /// (e.g. Contrôle TVA → étape précédente Saisie / Collecte).
    /// </summary>
    public async Task<ApiResponse<WorkflowStepInstanceDto>> RejectStepAsync(
        int instanceId, RejectStepDto dto, int cabinetId, string userId)
    {
        var reason = (dto.Reason ?? dto.Comment ?? "").Trim();
        if (string.IsNullOrWhiteSpace(reason))
            return ApiResponse<WorkflowStepInstanceDto>.Fail("Un motif de refus est obligatoire.");

        var instance = await _db.WorkflowInstances
            .Include(wi => wi.StepInstances.OrderBy(s => s.StepOrder))
            .Include(wi => wi.Declaration)
            .FirstOrDefaultAsync(wi => wi.Id == instanceId && wi.CabinetId == cabinetId && !wi.IsDeleted);

        if (instance == null)
            return ApiResponse<WorkflowStepInstanceDto>.Fail("Instance non trouvée.");
        if (instance.Status != WorkflowInstanceStatus.Active)
            return ApiResponse<WorkflowStepInstanceDto>.Fail("Le workflow n'est pas actif.");

        await AttachStepTemplatesIgnoreDeletedAsync(instance.StepInstances);

        var currentStep = instance.StepInstances
            .FirstOrDefault(s => s.Status == WorkflowStepStatus.InProgress
                              || s.Status == WorkflowStepStatus.WaitingForClient
                              || s.Status == WorkflowStepStatus.WaitingForValidation);

        if (currentStep == null)
            return ApiResponse<WorkflowStepInstanceDto>.Fail("Aucune étape active à refuser.");

        var stepTemplate = currentStep.WorkflowStepTemplate;
        await TryClaimStepIfEligibleAsync(currentStep, stepTemplate, instance, cabinetId, userId);

        if (!await CanUserAdvanceStepAsync(currentStep, stepTemplate, instance, userId))
            return ApiResponse<WorkflowStepInstanceDto>.Fail(
                "Seul le collaborateur (ou le client) assigné à cette étape peut la refuser.");

        // Previous human/actionable step only (never reopen AutoSystem init/clôture shells)
        var previousStep = instance.StepInstances
            .Where(s => s.StepOrder < currentStep.StepOrder
                        && (s.Status == WorkflowStepStatus.Completed || s.Status == WorkflowStepStatus.Skipped)
                        && s.WorkflowStepTemplate?.AssignmentType != AssignmentType.AutoSystem)
            .OrderByDescending(s => s.StepOrder)
            .FirstOrDefault();

        var previousStatus = currentStep.Status.ToString();
        var userName = await GetUserNameAsync(userId);

        // First human step (only AutoSystem before): refuse = demand correction on current step
        if (previousStep == null)
        {
            currentStep.Notes = $"Correction demandée : {reason}";
            if (instance.Declaration != null)
            {
                var oldDeclStatut = instance.Declaration.Statut;
                var targetStatut = !string.IsNullOrWhiteSpace(dto.DeclarationStatusOnReject)
                    ? dto.DeclarationStatusOnReject!.Trim()
                    : RefCodes.StatutDeclaration.EnAttenteCorrection;

                instance.Declaration.Statut = targetStatut!;
                instance.Declaration.MotifRefus = reason;
                instance.Declaration.UpdatedBy = userId;
                instance.Declaration.UpdatedAt = DateTime.UtcNow;

                _db.DeclarationHistories.Add(new DeclarationHistory
                {
                    DeclarationId = instance.Declaration.Id,
                    CabinetId = cabinetId,
                    AncienStatut = oldDeclStatut,
                    NouveauStatut = targetStatut!,
                    UserId = userId,
                    Commentaire = $"Refus / correction sur « {currentStep.StepName} » : {reason}",
                    CreatedBy = userId
                });
            }

            _db.WorkflowHistories.Add(new WorkflowHistory
            {
                WorkflowInstanceId = instanceId,
                WorkflowStepInstanceId = currentStep.Id,
                Action = "StepRejectedForCorrection",
                PreviousStatus = previousStatus,
                NewStatus = currentStep.Status.ToString(),
                Comment = reason,
                UserId = userId,
                UserName = userName,
                CabinetId = cabinetId,
                CreatedBy = userId
            });

            await _db.SaveChangesAsync();

            var stay = await _db.WorkflowStepInstances
                .Include(s => s.AssignedToUser)
                .Include(s => s.CompletedByUser)
                .Include(s => s.WorkflowStepTemplate)
                .FirstOrDefaultAsync(s => s.Id == currentStep.Id);

            return ApiResponse<WorkflowStepInstanceDto>.Ok(
                MapStepInstanceDto(stay!),
                $"Correction demandée sur « {currentStep.StepName} » (pas d'étape précédente humaine).");
        }

        // Reset current step so it can be activated again after correction
        currentStep.Status = WorkflowStepStatus.Pending;
        currentStep.CompletedAt = null;
        currentStep.CompletedByUserId = null;
        currentStep.StartedAt = null;
        currentStep.ActivatedAt = null;
        currentStep.Notes = $"Refusé → retour à « {previousStep.StepName} » : {reason}";

        if (currentStep.TaskItemId.HasValue)
        {
            var linkedTask = await _db.TaskItems.FirstOrDefaultAsync(t => t.Id == currentStep.TaskItemId.Value);
            if (linkedTask != null && linkedTask.Statut != "Annule")
            {
                linkedTask.Statut = "Annule";
                linkedTask.UpdatedAt = DateTime.UtcNow;
                linkedTask.UpdatedBy = userId;
            }
        }

        await CloseWorkflowStepTakesAsync(currentStep, cabinetId, userId, $"Refus: {reason}");

        _db.WorkflowHistories.Add(new WorkflowHistory
        {
            WorkflowInstanceId = instanceId,
            WorkflowStepInstanceId = currentStep.Id,
            Action = "StepRejected",
            PreviousStatus = previousStatus,
            NewStatus = "Pending",
            Comment = reason,
            UserId = userId,
            UserName = userName,
            CabinetId = cabinetId,
            CreatedBy = userId
        });

        // Reopen previous step
        var prevTemplate = previousStep.WorkflowStepTemplate;
        previousStep.CompletedAt = null;
        previousStep.CompletedByUserId = null;
        previousStep.Notes = $"Réouverte après refus de « {currentStep.StepName} » : {reason}";
        previousStep.ActivatedAt = DateTime.UtcNow;
        previousStep.StartedAt = DateTime.UtcNow;

        if (prevTemplate != null && (prevTemplate.IsClientStep || prevTemplate.AssignmentType == AssignmentType.Client))
            previousStep.Status = WorkflowStepStatus.WaitingForClient;
        else
            previousStep.Status = WorkflowStepStatus.InProgress;

        // Prefer keep previous assignee; else resolve again
        if (string.IsNullOrEmpty(previousStep.AssignedToUserId) && prevTemplate != null)
        {
            previousStep.AssignedToUserId = await ResolveAssigneeAsync(
                prevTemplate, cabinetId, instance.ClientId, userId);
        }

        instance.CurrentStepOrder = previousStep.StepOrder;

        // Declaration status: explicit override → previous entry status → ACorriger
        if (instance.Declaration != null)
        {
            var oldDeclStatut = instance.Declaration.Statut;
            var targetStatut = !string.IsNullOrWhiteSpace(dto.DeclarationStatusOnReject)
                ? dto.DeclarationStatusOnReject!.Trim()
                : (!string.IsNullOrWhiteSpace(prevTemplate?.DeclarationStatusOnEntry)
                    ? prevTemplate!.DeclarationStatusOnEntry
                    : RefCodes.StatutDeclaration.ACorriger);

            instance.Declaration.Statut = targetStatut!;
            instance.Declaration.MotifRefus = reason;
            instance.Declaration.UpdatedBy = userId;
            instance.Declaration.UpdatedAt = DateTime.UtcNow;

            _db.DeclarationHistories.Add(new DeclarationHistory
            {
                DeclarationId = instance.Declaration.Id,
                CabinetId = cabinetId,
                AncienStatut = oldDeclStatut,
                NouveauStatut = targetStatut!,
                UserId = userId,
                Commentaire = $"Refus étape « {currentStep.StepName} » → retour « {previousStep.StepName} » : {reason}",
                CreatedBy = userId
            });
        }

        // Reopen or recreate task on previous step
        if (prevTemplate?.AutoCreateTask == true && prevTemplate.AssignmentType != AssignmentType.AutoSystem)
        {
            TaskItem? prevTask = null;
            if (previousStep.TaskItemId.HasValue)
                prevTask = await _db.TaskItems.FirstOrDefaultAsync(t => t.Id == previousStep.TaskItemId.Value);

            if (prevTask != null)
            {
                prevTask.Statut = "AFaire";
                prevTask.AssignedToUserId = previousStep.AssignedToUserId;
                prevTask.UpdatedAt = DateTime.UtcNow;
                prevTask.UpdatedBy = userId;
                prevTask.Description = (prevTask.Description ?? "") + $"\n[Réouverture] {reason}";
            }
            else
            {
                var clientName = instance.Client?.RaisonSociale
                    ?? (await _db.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == instance.ClientId))?.RaisonSociale
                    ?? "N/A";
                var task = new TaskItem
                {
                    Titre = $"[Workflow] {previousStep.StepName} — {clientName}",
                    Description = $"Étape réouverte après refus: {reason}",
                    ClientId = instance.ClientId,
                    DeclarationId = instance.DeclarationId,
                    AssignedToUserId = previousStep.AssignedToUserId,
                    AssignedByUserId = userId,
                    DateDebut = DateTime.UtcNow,
                    DateEcheance = previousStep.DueDate ?? DateTime.UtcNow.AddHours(prevTemplate.SLAHours ?? 48),
                    Priorite = prevTemplate.TaskPriority,
                    Statut = "AFaire",
                    CabinetId = cabinetId,
                    CreatedBy = userId
                };
                _db.TaskItems.Add(task);
                await _db.SaveChangesAsync();
                previousStep.TaskItemId = task.Id;
            }
        }

        _db.WorkflowHistories.Add(new WorkflowHistory
        {
            WorkflowInstanceId = instanceId,
            WorkflowStepInstanceId = previousStep.Id,
            Action = "StepReopened",
            PreviousStatus = "Completed",
            NewStatus = previousStep.Status.ToString(),
            Comment = $"Retour suite au refus de « {currentStep.StepName} » : {reason}",
            UserId = userId,
            UserName = userName,
            CabinetId = cabinetId,
            CreatedBy = userId
        });

        await _db.SaveChangesAsync();

        // Soft-lock for reopened step assignee
        if (!string.IsNullOrEmpty(previousStep.AssignedToUserId)
            && previousStep.Status is WorkflowStepStatus.InProgress or WorkflowStepStatus.WaitingForValidation)
        {
            try
            {
                await _unitTakes.OpenTakeAsync(new OpenUnitTakeDto
                {
                    UnitTypeCode = UnitTypeCodes.WorkflowStep,
                    UnitEntityId = previousStep.Id,
                    Comment = $"Réouverture après refus: {reason}",
                    TaskItemId = previousStep.TaskItemId,
                    WorkflowStepInstanceId = previousStep.Id,
                    DeclarationId = instance.DeclarationId,
                    ForceRelease = true
                }, cabinetId, previousStep.AssignedToUserId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not open UnitTake on reopened step {StepId}", previousStep.Id);
            }
        }

        _logger.LogInformation(
            "Workflow {InstanceId}: step {Current} rejected by {User}, reopened {Previous}",
            instanceId, currentStep.StepName, userId, previousStep.StepName);

        var reloaded = await _db.WorkflowStepInstances
            .Include(s => s.AssignedToUser)
            .Include(s => s.CompletedByUser)
            .Include(s => s.WorkflowStepTemplate)
            .FirstOrDefaultAsync(s => s.Id == previousStep.Id);

        return ApiResponse<WorkflowStepInstanceDto>.Ok(
            MapStepInstanceDto(reloaded!),
            $"Étape refusée — retour à « {previousStep.StepName} ».");
    }

    /// <summary>
    /// Explicitly claim the current active step (exclusive soft lock).
    /// Fails if another matching-role user already holds it, unless Manager/Gérant forceOverride.
    /// </summary>
    public async Task<ApiResponse<WorkflowStepInstanceDto>> ClaimStepAsync(
        int instanceId, int cabinetId, string userId, bool forceOverride = false, string? reason = null)
    {
        var instance = await _db.WorkflowInstances
            .Include(wi => wi.StepInstances)
                .ThenInclude(si => si.AssignedToUser)
            .FirstOrDefaultAsync(wi => wi.Id == instanceId && wi.CabinetId == cabinetId && !wi.IsDeleted);

        if (instance == null)
            return ApiResponse<WorkflowStepInstanceDto>.Fail("Instance workflow introuvable.");
        if (instance.Status != WorkflowInstanceStatus.Active)
            return ApiResponse<WorkflowStepInstanceDto>.Fail("Le workflow n'est pas actif.");

        await AttachStepTemplatesIgnoreDeletedAsync(instance.StepInstances);

        var currentStep = instance.StepInstances
            .FirstOrDefault(s => s.StepOrder == instance.CurrentStepOrder && !s.IsDeleted)
            ?? instance.StepInstances.FirstOrDefault(s =>
                s.Status is WorkflowStepStatus.InProgress or WorkflowStepStatus.WaitingForValidation or WorkflowStepStatus.Pending);

        if (currentStep == null)
            return ApiResponse<WorkflowStepInstanceDto>.Fail("Aucune étape active à prendre.");

        var template = currentStep.WorkflowStepTemplate;
        if (template == null)
            return ApiResponse<WorkflowStepInstanceDto>.Fail("Modèle d'étape introuvable.");
        if (template.AssignmentType == AssignmentType.AutoSystem)
            return ApiResponse<WorkflowStepInstanceDto>.Fail("Cette étape est automatique — aucune prise manuelle.");
        if (template.AssignmentType == AssignmentType.Client || template.IsClientStep)
            return ApiResponse<WorkflowStepInstanceDto>.Fail("Cette étape est réservée au client (extranet).");

        // Already mine → ensure task EnCours + open UnitTake and return
        if (CanUserAdvanceStep(currentStep, template, userId))
        {
            await EnsureClaimedTaskEnCoursAsync(currentStep, userId, cabinetId, instance.ClientId, instance.DeclarationId, template);
            await _db.SaveChangesAsync();
            var mineTake = await OpenWorkflowStepTakeAsync(currentStep, instance, cabinetId, userId, forceRelease: false, forceReason: null);
            if (!mineTake.Success)
                return ApiResponse<WorkflowStepInstanceDto>.Fail(mineTake.Message ?? "Impossible d'ouvrir la prise d'unité.");
            var mineDto = MapStepInstanceDto(currentStep);
            ApplyOpenTakeToStepDto(mineDto, mineTake.Data);
            return ApiResponse<WorkflowStepInstanceDto>.Ok(mineDto, "Étape déjà à votre charge.");
        }

        var isManager = await UserMatchesRoleHintAsync(userId, "Manager", cabinetId, instance.ClientId);
        var hint = SuggestEmployeeRoleHint(template);
        var roleOk = await UserMatchesRoleHintAsync(userId, hint, cabinetId, instance.ClientId);

        if (!roleOk && !isManager)
            return ApiResponse<WorkflowStepInstanceDto>.Fail(
                $"Vous n'êtes pas éligible pour cette étape (rôle attendu : {hint}).");

        var openTakeResp = await _unitTakes.GetOpenTakeAsync(UnitTypeCodes.WorkflowStep, currentStep.Id, cabinetId);
        var openTake = openTakeResp.Success ? openTakeResp.Data : null;
        var heldByTake = openTake != null
            && !string.Equals(openTake.TakenByUserId, userId, StringComparison.OrdinalIgnoreCase);

        var heldByOther = heldByTake
            || (!string.IsNullOrEmpty(currentStep.AssignedToUserId)
                && !string.Equals(currentStep.AssignedToUserId, userId, StringComparison.OrdinalIgnoreCase)
                && await UserMatchesRoleHintAsync(currentStep.AssignedToUserId, hint, cabinetId, instance.ClientId));

        if (heldByOther && !(isManager && forceOverride))
        {
            var holderName = openTake?.TakenByUserName
                ?? (currentStep.AssignedToUser != null
                    ? $"{currentStep.AssignedToUser.FirstName} {currentStep.AssignedToUser.LastName}".Trim()
                    : await GetUserNameAsync(currentStep.AssignedToUserId));
            return ApiResponse<WorkflowStepInstanceDto>.Fail(
                $"Étape déjà prise par {holderName ?? "un autre collaborateur"}. Seul un Manager/Gérant peut forcer la reprise.");
        }

        if (heldByOther && isManager && forceOverride && string.IsNullOrWhiteSpace(reason))
            return ApiResponse<WorkflowStepInstanceDto>.Fail("Un motif est requis pour forcer la reprise de l'étape.");

        var previousAssignee = currentStep.AssignedToUserId ?? openTake?.TakenByUserId;
        currentStep.AssignedToUserId = userId;
        if (currentStep.Status == WorkflowStepStatus.Pending)
            currentStep.Status = WorkflowStepStatus.InProgress;
        currentStep.StartedAt ??= DateTime.UtcNow;
        currentStep.ActivatedAt ??= DateTime.UtcNow;
        currentStep.UpdatedAt = DateTime.UtcNow;
        currentStep.UpdatedBy = userId;

        await EnsureClaimedTaskEnCoursAsync(currentStep, userId, cabinetId, instance.ClientId, instance.DeclarationId, template);

        _db.WorkflowHistories.Add(new WorkflowHistory
        {
            WorkflowInstanceId = instanceId,
            WorkflowStepInstanceId = currentStep.Id,
            CabinetId = cabinetId,
            Action = heldByOther ? "ForceClaim" : "Claim",
            PreviousStatus = currentStep.Status.ToString(),
            NewStatus = currentStep.Status.ToString(),
            Comment = heldByOther
                ? $"Reprise forcée{(string.IsNullOrWhiteSpace(reason) ? "" : ": " + reason)} (précédent: {previousAssignee})"
                : "Prise en charge de l'étape",
            UserId = userId,
            UserName = await GetUserNameAsync(userId),
            ActionDate = DateTime.UtcNow,
            CreatedBy = userId
        });

        await _db.SaveChangesAsync();

        var takeResult = await OpenWorkflowStepTakeAsync(
            currentStep, instance, cabinetId, userId,
            forceRelease: heldByOther && isManager && forceOverride,
            forceReason: reason);
        if (!takeResult.Success)
            return ApiResponse<WorkflowStepInstanceDto>.Fail(takeResult.Message ?? "Impossible d'ouvrir la prise d'unité.");

        _logger.LogInformation("Step {StepId} claimed by {UserId} (force={Force})", currentStep.Id, userId, forceOverride);

        // Reload assignee nav for DTO
        await _db.Entry(currentStep).Reference(s => s.AssignedToUser).LoadAsync();
        var claimedDto = MapStepInstanceDto(currentStep);
        ApplyOpenTakeToStepDto(claimedDto, takeResult.Data);
        return ApiResponse<WorkflowStepInstanceDto>.Ok(claimedDto, "Étape prise en charge.");
    }

    private async Task EnsureClaimedTaskEnCoursAsync(
        WorkflowStepInstance step, string userId, int cabinetId, int clientId, int? declarationId, WorkflowStepTemplate template)
    {
        if (step.TaskItemId.HasValue)
        {
            var task = await _db.TaskItems.FirstOrDefaultAsync(t => t.Id == step.TaskItemId.Value);
            if (task != null)
            {
                task.AssignedToUserId = userId;
                if (task.Statut == "AFaire" || string.IsNullOrEmpty(task.Statut) || task.Statut == "EnAttenteRevision")
                    task.Statut = "EnCours";
                task.UpdatedAt = DateTime.UtcNow;
                task.UpdatedBy = userId;
            }
            return;
        }

        var clientName = await _db.Clients.Where(c => c.Id == clientId).Select(c => c.RaisonSociale).FirstOrDefaultAsync() ?? "N/A";
        var taskNew = new TaskItem
        {
            Titre = $"[Workflow] {step.StepName} — {clientName}",
            Description = template.Description ?? $"Étape workflow: {step.StepName}",
            ClientId = clientId,
            DeclarationId = declarationId,
            AssignedToUserId = userId,
            AssignedByUserId = userId,
            DateDebut = DateTime.UtcNow,
            DateEcheance = step.DueDate ?? DateTime.UtcNow.AddHours(template.SLAHours ?? 48),
            Priorite = template.TaskPriority ?? "Haute",
            Statut = "EnCours",
            CabinetId = cabinetId,
            CreatedBy = userId
        };
        _db.TaskItems.Add(taskNew);
        await _db.SaveChangesAsync();
        step.TaskItemId = taskNew.Id;
    }

    public async Task<ApiResponse<bool>> ReassignStepAsync(int stepInstanceId, ReassignStepDto dto, int cabinetId, string userId)
    {
        if (string.IsNullOrWhiteSpace(dto.NewAssigneeUserId))
            return ApiResponse<bool>.Fail("Veuillez sélectionner un collaborateur.");

        var step = await _db.WorkflowStepInstances
            .Include(s => s.AssignedToUser)
            .Include(s => s.WorkflowInstance)
            .Include(s => s.WorkflowStepTemplate)
            .FirstOrDefaultAsync(s => s.Id == stepInstanceId && s.CabinetId == cabinetId);

        if (step == null)
            return ApiResponse<bool>.Fail("Étape non trouvée.");
        if (step.Status == WorkflowStepStatus.Completed || step.Status == WorkflowStepStatus.Skipped)
            return ApiResponse<bool>.Fail("Impossible de réassigner une étape terminée.");
        if (step.WorkflowStepTemplate?.IsClientStep == true
            || step.WorkflowStepTemplate?.AssignmentType == AssignmentType.Client
            || step.Status == WorkflowStepStatus.WaitingForClient)
            return ApiResponse<bool>.Fail("Cette étape est réservée au client (extranet).");

        var newAssignee = await _db.Users.FirstOrDefaultAsync(u =>
            u.Id == dto.NewAssigneeUserId && u.CabinetId == cabinetId && u.IsActive && u.ClientId == null);
        if (newAssignee == null)
            return ApiResponse<bool>.Fail("Collaborateur introuvable ou inactif.");

        if (string.Equals(step.AssignedToUserId, dto.NewAssigneeUserId, StringComparison.OrdinalIgnoreCase))
            return ApiResponse<bool>.Fail("Cette étape est déjà assignée à ce collaborateur.");

        var oldAssignee = step.AssignedToUserId;
        var oldName = await GetUserNameAsync(oldAssignee ?? "");
        var newName = $"{newAssignee.FirstName} {newAssignee.LastName}".Trim();

        // Release exclusive lock held by previous assignee so the new one can take over
        await CloseWorkflowStepTakesAsync(step, cabinetId, userId,
            dto.Reason ?? $"Réassignation vers {newName}");

        step.AssignedToUserId = dto.NewAssigneeUserId;

        if (step.TaskItemId.HasValue)
        {
            var task = await _db.TaskItems.FirstOrDefaultAsync(t => t.Id == step.TaskItemId.Value && t.CabinetId == cabinetId);
            if (task != null)
            {
                task.AssignedToUserId = dto.NewAssigneeUserId;
                if (task.Statut == "Termine" || task.Statut == "Annule")
                    task.Statut = "AFaire";
                task.UpdatedAt = DateTime.UtcNow;
                task.UpdatedBy = userId;
            }
        }

        _db.WorkflowHistories.Add(new WorkflowHistory
        {
            WorkflowInstanceId = step.WorkflowInstanceId,
            WorkflowStepInstanceId = step.Id,
            Action = "StepReassigned",
            Comment = dto.Reason ?? $"Réassigné de {oldName} à {newName}",
            UserId = userId,
            UserName = await GetUserNameAsync(userId),
            CabinetId = cabinetId,
            CreatedBy = userId
        });

        await _db.SaveChangesAsync();

        var actorName = await GetUserNameAsync(userId);
        await _notifications.CreateAsync(
            dto.NewAssigneeUserId,
            "Nouvelle tâche assignée",
            $"L'étape '{step.StepName}' vous a été réassignée par {actorName}.",
            "Info", "Workflow", null, cabinetId);

        if (!string.IsNullOrEmpty(oldAssignee)
            && !string.Equals(oldAssignee, dto.NewAssigneeUserId, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(oldAssignee, userId, StringComparison.OrdinalIgnoreCase))
        {
            await _notifications.CreateAsync(
                oldAssignee,
                "Tâche réassignée",
                $"L'étape '{step.StepName}' a été transférée à {newName} par {actorName}.",
                "Info", "Workflow", null, cabinetId);
        }

        return ApiResponse<bool>.Ok(true, $"Étape réassignée à {newName}.");
    }

    public async Task<ApiResponse<WorkflowStepInstanceDto>> GetCurrentStepAsync(int instanceId, int cabinetId)
    {
        var instance = await _db.WorkflowInstances
            .Include(wi => wi.StepInstances.OrderBy(s => s.StepOrder))
                .ThenInclude(si => si.AssignedToUser)
            .FirstOrDefaultAsync(wi => wi.Id == instanceId && wi.CabinetId == cabinetId);

        if (instance == null)
            return ApiResponse<WorkflowStepInstanceDto>.Fail("Instance non trouvée.");

        await AttachStepTemplatesIgnoreDeletedAsync(instance.StepInstances);

        var current = instance.StepInstances
            .FirstOrDefault(s => s.Status == WorkflowStepStatus.InProgress || s.Status == WorkflowStepStatus.WaitingForClient || s.Status == WorkflowStepStatus.WaitingForValidation);

        if (current == null)
            current = instance.StepInstances.FirstOrDefault(s => s.Status == WorkflowStepStatus.Pending);

        if (current == null)
            return ApiResponse<WorkflowStepInstanceDto>.Fail("Aucune étape active.");

        var dto = MapStepInstanceDto(current);
        await EnrichStepsWithOpenTakesAsync(new List<WorkflowStepInstanceDto> { dto }, cabinetId);
        return ApiResponse<WorkflowStepInstanceDto>.Ok(dto);
    }

    public async Task<ApiResponse<List<WorkflowStepInstanceDto>>> GetMyPendingStepsAsync(int cabinetId, string userId, int? clientId = null)
    {
        // Extranet: create missing declarations so Mes déclarations is not empty for open WFs
        if (clientId.HasValue)
            await HealMissingDeclarationsForClientAsync(cabinetId, clientId.Value, userId);

        IQueryable<WorkflowStepInstance> query;

        if (clientId.HasValue)
        {
            // Extranet: client-waiting steps for this client + any steps assigned to the extranet user
            // Do not ThenInclude StepInstances (cycle with WorkflowInstance breaks AsNoTracking / can bloat).
            query = _db.WorkflowStepInstances
                .Include(s => s.WorkflowStepTemplate)
                .Include(s => s.WorkflowInstance).ThenInclude(wi => wi!.Client)
                .Include(s => s.WorkflowInstance).ThenInclude(wi => wi!.Declaration)
                .Include(s => s.WorkflowInstance).ThenInclude(wi => wi!.WorkflowTemplate)
                .Where(s => s.CabinetId == cabinetId
                    && !s.IsDeleted
                    && s.WorkflowInstance != null
                    && s.WorkflowInstance.ClientId == clientId.Value
                    && s.WorkflowInstance.Status == WorkflowInstanceStatus.Active
                    && (
                        // Client portal steps (WaitingForClient or mis-activated InProgress with Client assign)
                        (s.WorkflowStepTemplate != null
                            && (s.WorkflowStepTemplate.IsClientStep
                                || s.WorkflowStepTemplate.AssignmentType == AssignmentType.Client)
                            && (s.Status == WorkflowStepStatus.WaitingForClient
                                || s.Status == WorkflowStepStatus.InProgress
                                || s.Status == WorkflowStepStatus.WaitingForValidation))
                        || ((s.Status == WorkflowStepStatus.InProgress
                                || s.Status == WorkflowStepStatus.Pending
                                || s.Status == WorkflowStepStatus.WaitingForValidation)
                            && (s.AssignedToUserId == userId
                                || (s.WorkflowStepTemplate != null
                                    && s.WorkflowStepTemplate.AssignedToUserIds != null
                                    && s.WorkflowStepTemplate.AssignedToUserIds.Contains(userId))))
                    ));
        }
        else
        {
            query = _db.WorkflowStepInstances
                .Include(s => s.WorkflowStepTemplate)
                .Include(s => s.WorkflowInstance).ThenInclude(wi => wi!.Client)
                .Include(s => s.WorkflowInstance).ThenInclude(wi => wi!.Declaration)
                .Include(s => s.WorkflowInstance).ThenInclude(wi => wi!.WorkflowTemplate)
                .Where(s => s.CabinetId == cabinetId && !s.IsDeleted &&
                    (s.Status == WorkflowStepStatus.InProgress
                        || s.Status == WorkflowStepStatus.Pending
                        || s.Status == WorkflowStepStatus.WaitingForValidation) &&
                    (s.AssignedToUserId == userId
                        || (s.WorkflowStepTemplate != null && s.WorkflowStepTemplate.AssignedToUserIds != null
                            && s.WorkflowStepTemplate.AssignedToUserIds.Contains(userId))));
        }

        var steps = await query
            .OrderBy(s => s.DueDate ?? DateTime.MaxValue)
            .Take(50)
            .ToListAsync();

        var dtos = steps.Select(MapStepInstanceDto).ToList();
        await EnrichStepProgressAsync(dtos);
        return ApiResponse<List<WorkflowStepInstanceDto>>.Ok(dtos);
    }

    private async Task EnrichStepProgressAsync(List<WorkflowStepInstanceDto> dtos)
    {
        if (dtos.Count == 0) return;
        var instanceIds = dtos.Select(d => d.WorkflowInstanceId).Distinct().ToList();
        var progress = await _db.WorkflowStepInstances
            .AsNoTracking()
            .Where(s => instanceIds.Contains(s.WorkflowInstanceId) && !s.IsDeleted)
            .GroupBy(s => s.WorkflowInstanceId)
            .Select(g => new
            {
                InstanceId = g.Key,
                Total = g.Count(),
                Completed = g.Count(x =>
                    x.Status == WorkflowStepStatus.Completed || x.Status == WorkflowStepStatus.Skipped),
                CurrentOrder = g.Select(x => x.WorkflowInstance!.CurrentStepOrder).FirstOrDefault(),
                CurrentName = g
                    .Where(x => x.StepOrder == x.WorkflowInstance!.CurrentStepOrder)
                    .Select(x => x.StepName)
                    .FirstOrDefault()
            })
            .ToListAsync();

        var map = progress.ToDictionary(p => p.InstanceId);
        foreach (var dto in dtos)
        {
            if (!map.TryGetValue(dto.WorkflowInstanceId, out var p)) continue;
            dto.TotalSteps = p.Total;
            dto.CompletedSteps = p.Completed;
            if (!string.IsNullOrEmpty(p.CurrentName))
                dto.CurrentStepName = p.CurrentName;
        }
    }

    // ╔══════════════════════════════════════════════════════════════════════╗
    // ║                    CLIENT WORKFLOW CONFIG                            ║
    // ╚══════════════════════════════════════════════════════════════════════╝

    public async Task<ApiResponse<List<ClientWorkflowConfigDto>>> GetClientConfigsAsync(int clientId, int cabinetId)
    {
        var configs = await _db.ClientWorkflowConfigs
            .Where(c => c.ClientId == clientId && c.CabinetId == cabinetId && !c.IsDeleted)
            .Include(c => c.Client)
            .Include(c => c.WorkflowTemplate)
            .Include(c => c.PrimaryAssignee)
            .Select(c => new ClientWorkflowConfigDto
            {
                Id = c.Id,
                ClientId = c.ClientId,
                ClientName = c.Client != null ? c.Client.RaisonSociale : null,
                DeclarationType = c.DeclarationType.ToString(),
                WorkflowTemplateId = c.WorkflowTemplateId,
                WorkflowTemplateName = c.WorkflowTemplate != null ? c.WorkflowTemplate.Name : null,
                ClientValidationEnabled = c.ClientValidationEnabled,
                DocumentRequestEnabled = c.DocumentRequestEnabled,
                SLAMultiplier = c.SLAMultiplier,
                AutoStartWorkflow = c.AutoStartWorkflow,
                PrimaryAssigneeUserId = c.PrimaryAssigneeUserId,
                PrimaryAssigneeName = c.PrimaryAssignee != null ? c.PrimaryAssignee.FirstName + " " + c.PrimaryAssignee.LastName : null,
                IsActive = c.IsActive
            })
            .ToListAsync();

        return ApiResponse<List<ClientWorkflowConfigDto>>.Ok(configs);
    }

    public async Task<ApiResponse<bool>> DeleteClientConfigAsync(int configId, int clientId, int cabinetId, string userId)
    {
        var cfg = await _db.ClientWorkflowConfigs
            .FirstOrDefaultAsync(c => c.Id == configId && c.ClientId == clientId && c.CabinetId == cabinetId && !c.IsDeleted);
        if (cfg == null)
            return ApiResponse<bool>.Fail("Affectation introuvable.");

        // Soft-delete affectation
        cfg.IsDeleted = true;
        cfg.IsActive = false;
        cfg.UpdatedBy = userId;

        // Deactivate client-owned copy (keep history / running instances intact)
        if (cfg.WorkflowTemplateId.HasValue)
        {
            var tpl = await _db.WorkflowTemplates
                .FirstOrDefaultAsync(t => t.Id == cfg.WorkflowTemplateId.Value
                    && t.CabinetId == cabinetId
                    && t.ClientId == clientId
                    && !t.IsDeleted);
            if (tpl != null)
            {
                tpl.IsActive = false;
                tpl.IsDefault = false;
                tpl.UpdatedBy = userId;
            }
        }

        await _db.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Workflow retiré de l'affectation client.");
    }

    public async Task<ApiResponse<ClientWorkflowConfigDto>> SaveClientConfigAsync(ClientWorkflowConfigCreateDto dto, int cabinetId, string userId)
    {
        if (!Enum.TryParse<WorkflowDeclarationType>(dto.DeclarationType, true, out var declType))
            declType = WorkflowDeclarationType.All;

        // Assigning a cabinet (global) template → clone into a client-owned workflow first
        if (dto.WorkflowTemplateId.HasValue)
        {
            var ensure = await EnsureClientTemplateAsync(dto.ClientId, dto.WorkflowTemplateId.Value, declType.ToString(), cabinetId, userId);
            if (!ensure.Success || ensure.Data == null)
                return ApiResponse<ClientWorkflowConfigDto>.Fail(ensure.Message ?? "Impossible de créer le workflow client.");
            dto.WorkflowTemplateId = ensure.Data.Id;
        }

        var existing = await _db.ClientWorkflowConfigs
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.ClientId == dto.ClientId && c.CabinetId == cabinetId && c.DeclarationType == declType);

        if (existing != null)
        {
            existing.WorkflowTemplateId = dto.WorkflowTemplateId;
            existing.ClientValidationEnabled = dto.ClientValidationEnabled;
            existing.DocumentRequestEnabled = dto.DocumentRequestEnabled;
            existing.SLAMultiplier = dto.SLAMultiplier;
            existing.AutoStartWorkflow = dto.AutoStartWorkflow;
            existing.PrimaryAssigneeUserId = dto.PrimaryAssigneeUserId;
            existing.IsActive = dto.IsActive;
            existing.IsDeleted = false;
            existing.UpdatedBy = userId;

            // Re-activate client-owned template if it was deactivated on delete
            if (dto.WorkflowTemplateId.HasValue)
            {
                var tpl = await _db.WorkflowTemplates
                    .FirstOrDefaultAsync(t => t.Id == dto.WorkflowTemplateId.Value && t.CabinetId == cabinetId && t.ClientId == dto.ClientId);
                if (tpl != null && !tpl.IsActive)
                {
                    tpl.IsActive = true;
                    tpl.UpdatedBy = userId;
                }
            }
        }
        else
        {
            _db.ClientWorkflowConfigs.Add(new ClientWorkflowConfig
            {
                ClientId = dto.ClientId,
                DeclarationType = declType,
                WorkflowTemplateId = dto.WorkflowTemplateId,
                ClientValidationEnabled = dto.ClientValidationEnabled,
                DocumentRequestEnabled = dto.DocumentRequestEnabled,
                SLAMultiplier = dto.SLAMultiplier,
                AutoStartWorkflow = dto.AutoStartWorkflow,
                PrimaryAssigneeUserId = dto.PrimaryAssigneeUserId,
                IsActive = dto.IsActive,
                CabinetId = cabinetId,
                CreatedBy = userId
            });
        }

        await _db.SaveChangesAsync();

        // Auto-start when affectation is saved with the flag (incl. Domiciliation)
        if (dto.AutoStartWorkflow && dto.IsActive && dto.WorkflowTemplateId.HasValue)
        {
            await TryAutoStartClientWorkflowAsync(dto.ClientId, declType, dto.WorkflowTemplateId, cabinetId, userId);
        }

        var configs = await GetClientConfigsAsync(dto.ClientId, cabinetId);
        var saved = configs.Data?.FirstOrDefault(c => c.DeclarationType == declType.ToString());
        return saved != null
            ? ApiResponse<ClientWorkflowConfigDto>.Ok(saved, "Workflow client enregistré (copie personnalisable).")
            : ApiResponse<ClientWorkflowConfigDto>.Fail("Erreur lors de la sauvegarde.");
    }

    /// <summary>
    /// When DOMICILIATION (or another service) becomes active on a client: ensure cabinet template,
    /// attach client config with AutoStart, and start an instance if none is already running.
    /// </summary>
    public async Task<ApiResponse<WorkflowInstanceDto?>> EnsureServiceWorkflowAsync(
        int clientId, string serviceTypeCode, int cabinetId, string userId)
    {
        if (clientId <= 0 || string.IsNullOrWhiteSpace(serviceTypeCode))
            return ApiResponse<WorkflowInstanceDto?>.Ok(null);

        WorkflowDeclarationType? declType = serviceTypeCode.Trim().ToUpperInvariant() switch
        {
            "DOMICILIATION" => WorkflowDeclarationType.Domiciliation,
            _ => null
        };
        if (declType == null)
            return ApiResponse<WorkflowInstanceDto?>.Ok(null);

        // Ensure a cabinet base template exists (seed if missing)
        int? cabinetTplId = await _db.WorkflowTemplates.AsNoTracking()
            .Where(t => t.CabinetId == cabinetId && t.ClientId == null && t.IsActive && !t.IsDeleted
                && t.DeclarationType == declType.Value)
            .OrderByDescending(t => t.IsDefault)
            .ThenBy(t => t.Id)
            .Select(t => (int?)t.Id)
            .FirstOrDefaultAsync();

        if (cabinetTplId == null)
        {
            var def = GetAllDefaultTemplateDefinitions()
                .FirstOrDefault(d => string.Equals(d.DeclarationType, declType.Value.ToString(), StringComparison.OrdinalIgnoreCase));
            if (def != null)
            {
                var created = await CreateTemplateAsync(def, cabinetId, userId);
                if (created.Success && created.Data != null)
                    cabinetTplId = created.Data.Id;
            }
        }

        if (cabinetTplId == null)
            return ApiResponse<WorkflowInstanceDto?>.Fail("Aucun modèle de workflow Domiciliation. Ouvrez le Builder et lancez « Seed ».");

        // Attach template only — a cabinet employee must start the instance (not AutoSystem).
        var cfgDto = new ClientWorkflowConfigCreateDto
        {
            ClientId = clientId,
            DeclarationType = declType.Value.ToString(),
            WorkflowTemplateId = cabinetTplId,
            AutoStartWorkflow = false,
            DocumentRequestEnabled = true,
            ClientValidationEnabled = true,
            IsActive = true,
            SLAMultiplier = 1m
        };

        var save = await SaveClientConfigAsync(cfgDto, cabinetId, userId);
        if (!save.Success)
            return ApiResponse<WorkflowInstanceDto?>.Fail(save.Message ?? "Impossible d'affecter le workflow domiciliation.");

        return ApiResponse<WorkflowInstanceDto?>.Ok(null,
            "Workflow domiciliation affecté. Un collaborateur doit le démarrer manuellement (fiche client → Workflow).");
    }

    private async Task TryAutoStartClientWorkflowAsync(
        int clientId, WorkflowDeclarationType declType, int? templateId, int cabinetId, string userId)
    {
        var hasActive = await _db.WorkflowInstances.AsNoTracking()
            .AnyAsync(i => i.CabinetId == cabinetId && i.ClientId == clientId && !i.IsDeleted
                && i.WorkflowTemplate != null && i.WorkflowTemplate.DeclarationType == declType
                && (i.Status == WorkflowInstanceStatus.Active || i.Status == WorkflowInstanceStatus.Paused));
        if (hasActive) return;

        await StartWorkflowAsync(new StartWorkflowDto
        {
            ClientId = clientId,
            WorkflowTemplateId = templateId,
            DeclarationType = declType.ToString()
        }, cabinetId, userId);
    }

    public async Task<ApiResponse<WorkflowTemplateDto>> EnsureClientTemplateAsync(
        int clientId, int sourceTemplateId, string? declarationType, int cabinetId, string userId)
    {
        var source = await _db.WorkflowTemplates
            .Include(wt => wt.Steps.Where(s => !s.IsDeleted).OrderBy(s => s.StepOrder))
            .Include(wt => wt.Client)
            .FirstOrDefaultAsync(wt => wt.Id == sourceTemplateId && wt.CabinetId == cabinetId && !wt.IsDeleted);

        if (source == null)
            return ApiResponse<WorkflowTemplateDto>.Fail("Modèle source introuvable.");

        // Already the client-owned copy (Personnaliser opened on existing copy) — do not wipe customizations
        if (source.ClientId == clientId)
            return ApiResponse<WorkflowTemplateDto>.Ok(MapTemplateDto(source));

        if (!Enum.TryParse<WorkflowDeclarationType>(declarationType ?? source.DeclarationType.ToString(), true, out var declType))
            declType = source.DeclarationType;

        var client = await _db.Clients.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == clientId && c.CabinetId == cabinetId);
        if (client == null)
            return ApiResponse<WorkflowTemplateDto>.Fail("Client introuvable.");

        // Find existing client-owned copy for this declaration type (incl. deactivated after unassign)
        var existingClientTpls = await _db.WorkflowTemplates
            .Include(wt => wt.Steps.Where(s => !s.IsDeleted).OrderBy(s => s.StepOrder))
            .Where(wt =>
                wt.CabinetId == cabinetId
                && wt.ClientId == clientId
                && wt.DeclarationType == declType
                && !wt.IsDeleted
                && !wt.IsSystem)
            .OrderByDescending(wt => wt.IsActive)
            .ThenByDescending(wt => wt.IsDefault)
            .ThenByDescending(wt => wt.Id)
            .ToListAsync();

        var existingClientTpl = existingClientTpls.FirstOrDefault();

        // Extra duplicates for same client+type → archive
        foreach (var dup in existingClientTpls.Skip(1))
        {
            dup.IsActive = false;
            dup.IsDefault = false;
            if (!dup.Name.Contains("(ancien)"))
                dup.Name = $"{dup.Name} (ancien)";
            dup.UpdatedBy = userId;
        }

        var cloneDto = new WorkflowTemplateCreateDto
        {
            Name = $"{source.Name} — {client.RaisonSociale}",
            Description = source.Description,
            DeclarationType = declType.ToString(),
            ClientId = clientId,
            IsDefault = true,
            Steps = source.Steps.Where(s => !s.IsDeleted).OrderBy(s => s.StepOrder)
                .Select(MapStepToCreateDto).ToList()
        };

        // Cabinet (or other) source assigned → client copy must match that model (not an old stale copy)
        if (existingClientTpl != null)
        {
            if (!existingClientTpl.IsActive)
            {
                existingClientTpl.IsActive = true;
                existingClientTpl.UpdatedBy = userId;
            }

            // Only re-sync when source is a cabinet base (Affecter / pick modèle cabinet)
            if (!source.ClientId.HasValue)
            {
                await _db.SaveChangesAsync();
                var synced = await UpdateTemplateAsync(existingClientTpl.Id, cloneDto, cabinetId, userId);
                if (synced.Success)
                    _logger.LogInformation(
                        "Re-synced client workflow {ClientTplId} from cabinet {SourceId} for client {ClientId}",
                        existingClientTpl.Id, sourceTemplateId, clientId);
                return synced;
            }

            await _db.SaveChangesAsync();
            return ApiResponse<WorkflowTemplateDto>.Ok(MapTemplateDto(existingClientTpl));
        }

        var created = await CreateTemplateAsync(cloneDto, cabinetId, userId);
        if (created.Success)
            _logger.LogInformation("Cloned workflow template {SourceId} → client {ClientId} as {NewId}", sourceTemplateId, clientId, created.Data?.Id);
        return created;
    }

    public async Task<ApiResponse<ClientWorkflowConfigDto>> SaveClientWorkflowCustomizationAsync(
        ClientWorkflowCustomizeDto dto, int cabinetId, string userId)
    {
        if (dto.ClientId <= 0)
            return ApiResponse<ClientWorkflowConfigDto>.Fail("Client invalide.");

        if (!Enum.TryParse<WorkflowDeclarationType>(dto.DeclarationType, true, out var declType))
            declType = WorkflowDeclarationType.All;

        WorkflowTemplateDto? clientTemplate = null;

        if (dto.WorkflowTemplateId.HasValue)
        {
            var existing = await GetTemplateByIdAsync(dto.WorkflowTemplateId.Value, cabinetId);
            if (!existing.Success || existing.Data == null)
                return ApiResponse<ClientWorkflowConfigDto>.Fail(existing.Message ?? "Modèle introuvable.");
            if (existing.Data.ClientId != dto.ClientId)
            {
                // Accidental global id — clone first
                var ensure = await EnsureClientTemplateAsync(dto.ClientId, dto.WorkflowTemplateId.Value, declType.ToString(), cabinetId, userId);
                if (!ensure.Success || ensure.Data == null)
                    return ApiResponse<ClientWorkflowConfigDto>.Fail(ensure.Message ?? "Clone impossible.");
                clientTemplate = ensure.Data;
            }
            else
            {
                clientTemplate = existing.Data;
            }
        }
        else if (dto.SourceTemplateId.HasValue)
        {
            var ensure = await EnsureClientTemplateAsync(dto.ClientId, dto.SourceTemplateId.Value, declType.ToString(), cabinetId, userId);
            if (!ensure.Success || ensure.Data == null)
                return ApiResponse<ClientWorkflowConfigDto>.Fail(ensure.Message ?? "Clone impossible.");
            clientTemplate = ensure.Data;
        }
        else
        {
            return ApiResponse<ClientWorkflowConfigDto>.Fail("Aucun modèle source sélectionné.");
        }

        var updateDto = new WorkflowTemplateCreateDto
        {
            Name = string.IsNullOrWhiteSpace(dto.TemplateName) ? clientTemplate.Name : dto.TemplateName!,
            Description = dto.TemplateDescription ?? clientTemplate.Description,
            DeclarationType = declType.ToString(),
            ClientId = dto.ClientId,
            IsDefault = true,
            Steps = dto.Steps ?? new List<WorkflowStepTemplateCreateDto>()
        };

        if (updateDto.Steps.Count == 0 && clientTemplate.Steps.Count > 0)
        {
            // Keep existing steps if UI posted none (config-only save)
            updateDto.Steps = clientTemplate.Steps.Select(MapStepDtoToCreateDto).ToList();
        }

        ApiResponse<WorkflowTemplateDto> updated;
        try
        {
            updated = await UpdateTemplateAsync(clientTemplate.Id, updateDto, cabinetId, userId);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Customize save failed for client {ClientId} template {TemplateId}", dto.ClientId, clientTemplate.Id);
            return ApiResponse<ClientWorkflowConfigDto>.Fail(
                "Impossible d'enregistrer les étapes (conflit d'ordre d'étapes). Réessayez — si le problème continue, rouvrez Personnaliser.");
        }

        if (!updated.Success || updated.Data == null)
            return ApiResponse<ClientWorkflowConfigDto>.Fail(updated.Message ?? "Erreur lors de la mise à jour des étapes.");

        var configDto = new ClientWorkflowConfigCreateDto
        {
            ClientId = dto.ClientId,
            DeclarationType = declType.ToString(),
            WorkflowTemplateId = updated.Data.Id,
            ClientValidationEnabled = dto.ClientValidationEnabled,
            DocumentRequestEnabled = dto.DocumentRequestEnabled,
            SLAMultiplier = dto.SLAMultiplier,
            AutoStartWorkflow = dto.AutoStartWorkflow,
            PrimaryAssigneeUserId = dto.PrimaryAssigneeUserId,
            IsActive = dto.IsActive
        };

        // Bypass re-clone: point already to client template — call internal upsert without Ensure
        return await UpsertClientConfigAsync(configDto, cabinetId, userId, skipClone: true);
    }

    private async Task<ApiResponse<ClientWorkflowConfigDto>> UpsertClientConfigAsync(
        ClientWorkflowConfigCreateDto dto, int cabinetId, string userId, bool skipClone)
    {
        if (!skipClone)
            return await SaveClientConfigAsync(dto, cabinetId, userId);

        if (!Enum.TryParse<WorkflowDeclarationType>(dto.DeclarationType, true, out var declType))
            declType = WorkflowDeclarationType.All;

        var existing = await _db.ClientWorkflowConfigs
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.ClientId == dto.ClientId && c.CabinetId == cabinetId && c.DeclarationType == declType);

        if (existing != null)
        {
            existing.WorkflowTemplateId = dto.WorkflowTemplateId;
            existing.ClientValidationEnabled = dto.ClientValidationEnabled;
            existing.DocumentRequestEnabled = dto.DocumentRequestEnabled;
            existing.SLAMultiplier = dto.SLAMultiplier;
            existing.AutoStartWorkflow = dto.AutoStartWorkflow;
            existing.PrimaryAssigneeUserId = dto.PrimaryAssigneeUserId;
            existing.IsActive = dto.IsActive;
            existing.IsDeleted = false;
            existing.UpdatedBy = userId;
        }
        else
        {
            _db.ClientWorkflowConfigs.Add(new ClientWorkflowConfig
            {
                ClientId = dto.ClientId,
                DeclarationType = declType,
                WorkflowTemplateId = dto.WorkflowTemplateId,
                ClientValidationEnabled = dto.ClientValidationEnabled,
                DocumentRequestEnabled = dto.DocumentRequestEnabled,
                SLAMultiplier = dto.SLAMultiplier,
                AutoStartWorkflow = dto.AutoStartWorkflow,
                PrimaryAssigneeUserId = dto.PrimaryAssigneeUserId,
                IsActive = dto.IsActive,
                CabinetId = cabinetId,
                CreatedBy = userId
            });
        }

        await _db.SaveChangesAsync();
        var configs = await GetClientConfigsAsync(dto.ClientId, cabinetId);
        var saved = configs.Data?.FirstOrDefault(c => c.DeclarationType == declType.ToString());
        return saved != null
            ? ApiResponse<ClientWorkflowConfigDto>.Ok(saved, "Personnalisation enregistrée pour ce client.")
            : ApiResponse<ClientWorkflowConfigDto>.Fail("Erreur lors de la sauvegarde.");
    }

    private static WorkflowStepTemplateCreateDto MapStepToCreateDto(WorkflowStepTemplate s) => new()
    {
        Name = s.Name,
        Description = s.Description,
        StepOrder = s.StepOrder,
        StepType = s.StepType.ToString(),
        AssignmentType = s.AssignmentType.ToString(),
        AssignedToUserId = s.AssignedToUserId,
        AssignedToUserIds = string.IsNullOrWhiteSpace(s.AssignedToUserIds)
            ? (string.IsNullOrWhiteSpace(s.AssignedToUserId) ? null : new List<string> { s.AssignedToUserId })
            : s.AssignedToUserIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
        AssignedToRoleName = s.AssignedToRoleName,
        AssignedToDepartmentId = s.AssignedToDepartmentId,
        IsClientStep = s.IsClientStep,
        RequiredPermission = s.RequiredPermission,
        SLAHours = s.SLAHours,
        DeadlineAnchor = s.DeadlineAnchor,
        DeadlineOffsetDays = s.DeadlineOffsetDays,
        DeadlineDay = s.DeadlineDay,
        DeadlineMonth = s.DeadlineMonth,
        DeadlineMonthOffset = s.DeadlineMonthOffset,
        DeadlineYearOffset = s.DeadlineYearOffset,
        ApplyDeadlineToDeclaration = s.ApplyDeadlineToDeclaration,
        LifecycleEndMode = s.LifecycleEndMode,
        LifecycleEndDays = s.LifecycleEndDays,
        ActionOnLifecycleEnd = s.ActionOnLifecycleEnd,
        IsRequired = s.IsRequired,
        IsBlocking = s.IsBlocking,
        AutoCreateTask = s.AutoCreateTask,
        TaskType = s.TaskType.ToString(),
        TaskPriority = s.TaskPriority,
        AutoNotify = s.AutoNotify,
        ValidationRulesJson = s.ValidationRulesJson,
        DeclarationStatusOnComplete = s.DeclarationStatusOnComplete,
        DeclarationStatusOnEntry = s.DeclarationStatusOnEntry,
        ActionOnEntry = s.ActionOnEntry.ToString(),
        ActionOnComplete = s.ActionOnComplete.ToString(),
        ActionConfigJson = s.ActionConfigJson,
        CronExpression = s.CronExpression,
        CronDescription = s.CronDescription,
        IsScheduled = s.IsScheduled,
        MaxRetries = s.MaxRetries,
        RetryIntervalHours = s.RetryIntervalHours,
        OnMaxRetriesAction = s.OnMaxRetriesAction,
        RoutingMode = s.RoutingMode.ToString(),
        RoutingConditionsJson = s.RoutingConditionsJson,
        GotoStepOrder = s.GotoStepOrder,
        TriggerSource = s.TriggerSource.ToString(),
        UseServiceContratAffectation = s.UseServiceContratAffectation,
        ServiceContratTypeCode = s.ServiceContratTypeCode
    };

    private static WorkflowStepTemplateCreateDto MapStepDtoToCreateDto(WorkflowStepTemplateDto s) => new()
    {
        Name = s.Name,
        Description = s.Description,
        StepOrder = s.StepOrder,
        StepType = s.StepType,
        AssignmentType = s.AssignmentType,
        AssignedToUserId = s.AssignedToUserId,
        AssignedToUserIds = s.AssignedToUserIdList?.Count > 0
            ? s.AssignedToUserIdList
            : (string.IsNullOrWhiteSpace(s.AssignedToUserId) ? null : new List<string> { s.AssignedToUserId }),
        AssignedToRoleName = s.AssignedToRoleName,
        AssignedToDepartmentId = s.AssignedToDepartmentId,
        IsClientStep = s.IsClientStep,
        RequiredPermission = s.RequiredPermission,
        SLAHours = s.SLAHours,
        DeadlineAnchor = s.DeadlineAnchor,
        DeadlineOffsetDays = s.DeadlineOffsetDays,
        DeadlineDay = s.DeadlineDay,
        DeadlineMonth = s.DeadlineMonth,
        DeadlineMonthOffset = s.DeadlineMonthOffset,
        DeadlineYearOffset = s.DeadlineYearOffset,
        ApplyDeadlineToDeclaration = s.ApplyDeadlineToDeclaration,
        LifecycleEndMode = s.LifecycleEndMode,
        LifecycleEndDays = s.LifecycleEndDays,
        ActionOnLifecycleEnd = s.ActionOnLifecycleEnd,
        IsRequired = s.IsRequired,
        IsBlocking = s.IsBlocking,
        AutoCreateTask = s.AutoCreateTask,
        TaskType = s.TaskType,
        TaskPriority = s.TaskPriority,
        AutoNotify = s.AutoNotify,
        ValidationRulesJson = s.ValidationRulesJson,
        DeclarationStatusOnComplete = s.DeclarationStatusOnComplete,
        DeclarationStatusOnEntry = s.DeclarationStatusOnEntry,
        ActionOnEntry = s.ActionOnEntry,
        ActionOnComplete = s.ActionOnComplete,
        ActionConfigJson = s.ActionConfigJson,
        CronExpression = s.CronExpression,
        CronDescription = s.CronDescription,
        IsScheduled = s.IsScheduled,
        MaxRetries = s.MaxRetries,
        RetryIntervalHours = s.RetryIntervalHours,
        OnMaxRetriesAction = s.OnMaxRetriesAction,
        RoutingMode = s.RoutingMode,
        RoutingConditionsJson = s.RoutingConditionsJson,
        GotoStepOrder = s.GotoStepOrder,
        TriggerSource = s.TriggerSource,
        UseServiceContratAffectation = s.UseServiceContratAffectation,
        ServiceContratTypeCode = s.ServiceContratTypeCode
    };

    // ╔══════════════════════════════════════════════════════════════════════╗
    // ║                    DASHBOARD / MONITORING                            ║
    // ╚══════════════════════════════════════════════════════════════════════╝

    public async Task<ApiResponse<WorkflowDashboardDto>> GetDashboardAsync(int cabinetId, string userId)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var activeCount = await _db.WorkflowInstances.CountAsync(wi => wi.CabinetId == cabinetId && wi.Status == WorkflowInstanceStatus.Active);
        var completedThisMonth = await _db.WorkflowInstances.CountAsync(wi => wi.CabinetId == cabinetId && wi.Status == WorkflowInstanceStatus.Completed && wi.CompletedAt >= monthStart);

        var overdueSteps = await _db.WorkflowStepInstances
            .CountAsync(s => s.CabinetId == cabinetId && s.DueDate < now &&
                s.Status != WorkflowStepStatus.Completed && s.Status != WorkflowStepStatus.Skipped);

        var pendingMyAction = await _db.WorkflowStepInstances
            .CountAsync(s => s.CabinetId == cabinetId && s.AssignedToUserId == userId &&
                (s.Status == WorkflowStepStatus.InProgress || s.Status == WorkflowStepStatus.Pending));

        var waitingForClient = await _db.WorkflowStepInstances
            .CountAsync(s => s.CabinetId == cabinetId && s.Status == WorkflowStepStatus.WaitingForClient);

        var recentWorkflows = await _db.WorkflowInstances
            .Where(wi => wi.CabinetId == cabinetId)
            .Include(wi => wi.WorkflowTemplate)
            .Include(wi => wi.Client)
            .Include(wi => wi.Declaration)
            .Include(wi => wi.StepInstances)
            .OrderByDescending(wi => wi.StartedAt)
            .Take(10)
            .ToListAsync();

        var myPendingSteps = await _db.WorkflowStepInstances
            .Where(s => s.CabinetId == cabinetId && s.AssignedToUserId == userId &&
                (s.Status == WorkflowStepStatus.InProgress || s.Status == WorkflowStepStatus.Pending))
            .Include(s => s.WorkflowStepTemplate)
            .OrderBy(s => s.DueDate ?? DateTime.MaxValue)
            .Take(20)
            .ToListAsync();

        // Status distribution
        var statusDist = await _db.WorkflowInstances
            .Where(wi => wi.CabinetId == cabinetId)
            .GroupBy(wi => wi.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(g => g.Status, g => g.Count);

        return ApiResponse<WorkflowDashboardDto>.Ok(new WorkflowDashboardDto
        {
            ActiveWorkflows = activeCount,
            CompletedThisMonth = completedThisMonth,
            OverdueSteps = overdueSteps,
            PendingMyAction = pendingMyAction,
            WaitingForClient = waitingForClient,
            RecentWorkflows = recentWorkflows.Select(MapInstanceDto).ToList(),
            MyPendingSteps = myPendingSteps.Select(MapStepInstanceDto).ToList(),
            StatusDistribution = statusDist
        });
    }

    public async Task<ApiResponse<List<WorkflowHistoryDto>>> GetHistoryAsync(int instanceId, int cabinetId)
    {
        var history = await _db.WorkflowHistories
            .Where(h => h.WorkflowInstanceId == instanceId && h.CabinetId == cabinetId)
            .Include(h => h.WorkflowStepInstance)
            .OrderByDescending(h => h.ActionDate)
            .Select(h => new WorkflowHistoryDto
            {
                Id = h.Id,
                WorkflowInstanceId = h.WorkflowInstanceId,
                WorkflowStepInstanceId = h.WorkflowStepInstanceId,
                StepName = h.WorkflowStepInstance != null ? h.WorkflowStepInstance.StepName : null,
                Action = h.Action,
                PreviousStatus = h.PreviousStatus,
                NewStatus = h.NewStatus,
                Comment = h.Comment,
                UserName = h.UserName,
                ActionDate = h.ActionDate
            })
            .ToListAsync();

        return ApiResponse<List<WorkflowHistoryDto>>.Ok(history);
    }

    // ╔══════════════════════════════════════════════════════════════════════╗
    // ║                     TEMPLATE RESOLUTION                             ║
    // ╚══════════════════════════════════════════════════════════════════════╝

    public async Task<WorkflowTemplate?> ResolveTemplateAsync(int cabinetId, int clientId, string? declarationType)
    {
        WorkflowDeclarationType? declType = null;
        if (!string.IsNullOrEmpty(declarationType) && Enum.TryParse<WorkflowDeclarationType>(declarationType, true, out var parsed))
            declType = parsed;

        // Priority 1: Client + specific type
        if (declType.HasValue)
        {
            var clientType = await _db.WorkflowTemplates
                .Include(wt => wt.Steps.Where(s => !s.IsDeleted).OrderBy(s => s.StepOrder))
                .FirstOrDefaultAsync(wt => wt.CabinetId == cabinetId && wt.ClientId == clientId && wt.DeclarationType == declType.Value && wt.IsActive && wt.IsDefault);
            if (clientType != null) return clientType;
        }

        // Priority 2: Client + All types
        var clientAll = await _db.WorkflowTemplates
            .Include(wt => wt.Steps.Where(s => !s.IsDeleted).OrderBy(s => s.StepOrder))
            .FirstOrDefaultAsync(wt => wt.CabinetId == cabinetId && wt.ClientId == clientId && wt.DeclarationType == WorkflowDeclarationType.All && wt.IsActive && wt.IsDefault);
        if (clientAll != null) return clientAll;

        // Priority 3: Global + specific type (TVA: Mensuelle vs Trimestrielle from client.TypeTVA)
        if (declType.HasValue)
        {
            if (declType.Value == WorkflowDeclarationType.TVA)
            {
                var typeTva = await _db.Clients.AsNoTracking()
                    .Where(c => c.Id == clientId && c.CabinetId == cabinetId)
                    .Select(c => c.TypeTVA)
                    .FirstOrDefaultAsync();
                var wantTrim = !string.IsNullOrWhiteSpace(typeTva)
                    && typeTva.StartsWith("Trim", StringComparison.OrdinalIgnoreCase);

                var tvaCandidates = await _db.WorkflowTemplates
                    .Include(wt => wt.Steps.Where(s => !s.IsDeleted).OrderBy(s => s.StepOrder))
                    .Where(wt => wt.CabinetId == cabinetId && wt.ClientId == null
                        && wt.DeclarationType == WorkflowDeclarationType.TVA
                        && wt.IsActive && !wt.Name.Contains("(ancien)"))
                    .ToListAsync();

                var byFreq = tvaCandidates.FirstOrDefault(wt =>
                    wantTrim
                        ? wt.Name.Contains("Trimestrielle", StringComparison.OrdinalIgnoreCase)
                        : wt.Name.Contains("Mensuelle", StringComparison.OrdinalIgnoreCase));
                if (byFreq != null) return byFreq;

                var tvaDefault = tvaCandidates.FirstOrDefault(wt => wt.IsDefault) ?? tvaCandidates.FirstOrDefault();
                if (tvaDefault != null) return tvaDefault;
            }

            var globalType = await _db.WorkflowTemplates
                .Include(wt => wt.Steps.Where(s => !s.IsDeleted).OrderBy(s => s.StepOrder))
                .FirstOrDefaultAsync(wt => wt.CabinetId == cabinetId && wt.ClientId == null && wt.DeclarationType == declType.Value && wt.IsActive && wt.IsDefault);
            if (globalType != null) return globalType;
        }

        // Priority 4: Global default (All)
        return await _db.WorkflowTemplates
            .Include(wt => wt.Steps.Where(s => !s.IsDeleted).OrderBy(s => s.StepOrder))
            .FirstOrDefaultAsync(wt => wt.CabinetId == cabinetId && wt.ClientId == null && wt.DeclarationType == WorkflowDeclarationType.All && wt.IsActive && wt.IsDefault);
    }

    // ╔══════════════════════════════════════════════════════════════════════╗
    // ║                        PRIVATE HELPERS                              ║
    // ╚══════════════════════════════════════════════════════════════════════╝

    /// <summary>
    /// Activates a step (transitions from Pending → InProgress), assigns user, creates task, sends notification.
    /// </summary>
    private async Task ActivateStepAsync(int instanceId, int stepOrder, int cabinetId, string triggerUserId, string? preferredAssigneeUserId = null)
    {
        var stepInstance = await _db.WorkflowStepInstances
            .Include(s => s.WorkflowStepTemplate)
            .Include(s => s.WorkflowInstance).ThenInclude(wi => wi!.Declaration)
            .Include(s => s.WorkflowInstance).ThenInclude(wi => wi!.Client)
            .FirstOrDefaultAsync(s => s.WorkflowInstanceId == instanceId && s.StepOrder == stepOrder);

        if (stepInstance == null) return;

        var template = stepInstance.WorkflowStepTemplate;
        if (template == null) return;
        var instance = stepInstance.WorkflowInstance;
        if (instance == null) return;

        string? assigneeUserId = await ResolveAssigneeAsync(
            template, cabinetId, instance.ClientId, preferredAssigneeUserId ?? triggerUserId);
        stepInstance.AssignedToUserId = assigneeUserId;
        stepInstance.ActivatedAt = DateTime.UtcNow;
        stepInstance.StartedAt = DateTime.UtcNow;

        // Ensure a declaration exists for Init / CreateDeclaration before status updates
        if (!instance.DeclarationId.HasValue
            && (template.ActionOnEntry == WorkflowActionType.CreateDeclaration
                || template.StepType == WorkflowStepType.Initialisation
                || !string.IsNullOrWhiteSpace(template.DeclarationStatusOnEntry)))
        {
            await EnsureDeclarationLinkedAsync(instance, cabinetId, triggerUserId);
        }

        if (template.ActionOnEntry != WorkflowActionType.None)
            await ExecuteStepActionAsync(template.ActionOnEntry, instance, stepInstance, cabinetId, triggerUserId);

        if (instance.DeclarationId.HasValue && instance.Declaration == null)
            instance.Declaration = await _db.Declarations.FindAsync(instance.DeclarationId.Value);

        // Clôture: no échéance — system closes via AutoSystem / lifecycle action
        if (template.StepType == WorkflowStepType.ClotureArchivage)
        {
            stepInstance.DueDate = null;
        }
        else
        {
            decimal? slaMultiplier = null;
            var clientCfg = await _db.ClientWorkflowConfigs
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CabinetId == cabinetId && c.ClientId == instance.ClientId && c.IsActive && !c.IsDeleted);
            if (clientCfg != null)
                slaMultiplier = clientCfg.SLAMultiplier;
            stepInstance.DueDate = ComputeStepDueDate(template, instance.Declaration, DateTime.UtcNow, slaMultiplier);
            if (template.ApplyDeadlineToDeclaration && instance.Declaration != null && stepInstance.DueDate.HasValue)
                instance.Declaration.DateLimite = stepInstance.DueDate.Value.Date;
        }

        // Init: stamp declaration lifecycle end + action
        if (template.StepType == WorkflowStepType.Initialisation && instance.Declaration != null)
            ApplyDeclarationLifecycleFromInit(template, instance.Declaration);

        // Entry status first; AutoSystem OnComplete may override afterwards (e.g. Archivee → Cloturee)
        if (template.DeclarationStatusOnEntry != null && instance.Declaration != null)
            instance.Declaration.Statut = template.DeclarationStatusOnEntry;

        var isLegacyRelanceStep = (template.StepType == WorkflowStepType.Relance
                || template.ActionOnEntry == WorkflowActionType.RelanceClient)
            && template.AssignmentType == AssignmentType.AutoSystem
            && template.MaxRetries > 0;

        var scheduleEmbeddedRelance = !isLegacyRelanceStep
            && template.MaxRetries > 0
            && (template.RetryIntervalHours ?? 0) > 0
            && template.StepType != WorkflowStepType.Initialisation
            && template.StepType != WorkflowStepType.ClotureArchivage;

        if (isLegacyRelanceStep)
        {
            // Legacy dedicated Relance step: schedule retries then auto-complete so Collecte can start
            stepInstance.RetryCount = Math.Max(stepInstance.RetryCount, 1);
            stepInstance.LastRetryAt = DateTime.UtcNow;
            stepInstance.NextRetryAt = DateTime.UtcNow.AddHours(template.RetryIntervalHours ?? 72);
            stepInstance.Status = WorkflowStepStatus.Completed;
            stepInstance.CompletedAt = DateTime.UtcNow;
            stepInstance.CompletedByUserId = triggerUserId;
        }
        else if (template.AssignmentType == AssignmentType.AutoSystem)
        {
            stepInstance.Status = WorkflowStepStatus.Completed;
            stepInstance.CompletedAt = DateTime.UtcNow;
            stepInstance.CompletedByUserId = triggerUserId;
            if (template.ActionOnComplete != WorkflowActionType.None)
                await ExecuteStepActionAsync(template.ActionOnComplete, instance, stepInstance, cabinetId, triggerUserId);
            if (template.DeclarationStatusOnComplete != null && instance.Declaration != null)
                instance.Declaration.Statut = template.DeclarationStatusOnComplete;
        }
        else if (template.IsClientStep || template.AssignmentType == AssignmentType.Client)
        {
            if (!template.IsClientStep)
                template.IsClientStep = true;
            stepInstance.Status = WorkflowStepStatus.WaitingForClient;
        }
        else
        {
            stepInstance.Status = WorkflowStepStatus.InProgress;
        }

        // Embedded relance on Demande / Collecte / any waiting step
        if (scheduleEmbeddedRelance
            && stepInstance.Status is WorkflowStepStatus.WaitingForClient or WorkflowStepStatus.InProgress)
        {
            stepInstance.RetryCount = 0;
            stepInstance.NextRetryAt = DateTime.UtcNow.AddHours(template.RetryIntervalHours ?? 360);
        }

        _db.WorkflowHistories.Add(new WorkflowHistory
        {
            WorkflowInstanceId = instanceId,
            WorkflowStepInstanceId = stepInstance.Id,
            Action = isLegacyRelanceStep ? "RelanceScheduled"
                : scheduleEmbeddedRelance ? "RelanceConfigured"
                : template.AssignmentType == AssignmentType.AutoSystem ? "StepAutoCompleted" : "StepActivated",
            PreviousStatus = "Pending",
            NewStatus = stepInstance.Status.ToString(),
            Comment = scheduleEmbeddedRelance
                ? $"Relance auto: 1ère dans {template.RetryIntervalHours ?? 360}h, max {template.MaxRetries}"
                : null,
            UserId = triggerUserId,
            UserName = await GetUserNameAsync(triggerUserId),
            CabinetId = cabinetId,
            CreatedBy = triggerUserId
        });

        if (template.AutoCreateTask && template.AssignmentType != AssignmentType.AutoSystem)
        {
            var clientName = instance.Client?.RaisonSociale ?? "N/A";
            var task = new TaskItem
            {
                Titre = $"[Workflow] {stepInstance.StepName} — {clientName}",
                Description = template.Description ?? $"Étape workflow: {stepInstance.StepName}",
                ClientId = instance.ClientId,
                DeclarationId = instance.DeclarationId,
                AssignedToUserId = assigneeUserId,
                AssignedByUserId = triggerUserId,
                DateDebut = DateTime.UtcNow,
                DateEcheance = stepInstance.DueDate ?? DateTime.UtcNow.AddHours(template.SLAHours ?? 48),
                Priorite = template.TaskPriority,
                Statut = "AFaire",
                CabinetId = cabinetId,
                CreatedBy = triggerUserId
            };
            _db.TaskItems.Add(task);
            await _db.SaveChangesAsync();
            stepInstance.TaskItemId = task.Id;
        }

        if (template.AutoNotify)
        {
            var clientName = instance.Client?.RaisonSociale ?? "";
            var notifyIds = ParseAssigneeIds(template.AssignedToUserIds, assigneeUserId);
            if (notifyIds.Count == 0 && assigneeUserId != null)
                notifyIds.Add(assigneeUserId);
            foreach (var uid in notifyIds.Distinct())
            {
                await _notifications.CreateAsync(
                    uid,
                    $"Workflow: {stepInstance.StepName}",
                    $"L'étape '{stepInstance.StepName}' pour {clientName} vous est assignée.",
                    "Info", "Workflow", null, cabinetId);
            }
        }

        await _db.SaveChangesAsync();

        // Auto-system (including scheduled Relance): advance to next so Collecte can start
        if (template.AssignmentType == AssignmentType.AutoSystem)
        {
            var nextStep = await _db.WorkflowStepInstances
                .Where(s => s.WorkflowInstanceId == instanceId && s.StepOrder > stepOrder && s.Status == WorkflowStepStatus.Pending)
                .OrderBy(s => s.StepOrder)
                .FirstOrDefaultAsync();

            if (nextStep != null)
            {
                instance.CurrentStepOrder = nextStep.StepOrder;
                await _db.SaveChangesAsync();
                await ActivateStepAsync(instanceId, nextStep.StepOrder, cabinetId, triggerUserId);
            }
            else
            {
                instance.Status = WorkflowInstanceStatus.Completed;
                instance.CompletedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
        }
    }

    /// <summary>
    /// Stamps LifecycleEndsAt / LifecycleEndAction on the declaration from the Init step template.
    /// </summary>
    private static void ApplyDeclarationLifecycleFromInit(WorkflowStepTemplate template, Declaration declaration)
    {
        // PeriodEnd: DateFin période, except CNSS/TVA/… where work is in the next-month filing window
        // (ex. CNSS juillet → clôture auto le 10 août = DateLimite).
        var mode = string.IsNullOrWhiteSpace(template.LifecycleEndMode)
            ? "PeriodEnd"
            : template.LifecycleEndMode.Trim();
        if (string.Equals(mode, "None", StringComparison.OrdinalIgnoreCase))
            return;

        declaration.LifecycleEndsAt = MoroccoDeclarationCalendar.GetLifecycleEndDate(
            declaration.Type, declaration.DateFin, declaration.DateLimite);

        declaration.LifecycleEndAction = string.IsNullOrWhiteSpace(template.ActionOnLifecycleEnd)
            ? nameof(WorkflowActionType.ClotureDeclaration)
            : template.ActionOnLifecycleEnd.Trim();
        // Allow re-stamp on reopen only if not already processed
        if (declaration.LifecycleEndedAt.HasValue && declaration.LifecycleEndsAt > DateTime.UtcNow)
            declaration.LifecycleEndedAt = null;
    }

    /// <summary>
    /// Computes step DueDate when the step opens.
    /// OnStepOpen / OnDeclarationOpen: generic from activation (+ SLA / offset days).
    /// ExactCalendar: day-of-month (mensuel), day+month (trimestriel/annuel) relative to period.
    /// </summary>
    private static DateTime? ComputeStepDueDate(
        WorkflowStepTemplate template,
        Declaration? declaration,
        DateTime activatedAt,
        decimal? slaMultiplier)
    {
        var offsetDays = template.DeadlineOffsetDays ?? 0;
        // Sage-like default: legal Moroccan filing calendar when a declaration is linked.
        var anchor = string.IsNullOrWhiteSpace(template.DeadlineAnchor)
            ? (declaration != null ? "CalendrierMaroc" : "OnStepOpen")
            : template.DeadlineAnchor.Trim();

        DateTime? FromOpen()
        {
            DateTime? due = null;
            if (template.SLAHours.HasValue)
            {
                var hours = (double)template.SLAHours.Value;
                if (slaMultiplier is > 0)
                    hours *= (double)slaMultiplier.Value;
                due = activatedAt.AddHours(hours);
            }
            if (offsetDays != 0)
                due = (due ?? activatedAt).AddDays(offsetDays);
            return due;
        }

        return anchor switch
        {
            "None" => null,
            "CalendrierMaroc" => declaration != null
                ? MoroccoDeclarationCalendar.GetFilingDeadline(
                    declaration.Type,
                    declaration.DateDebut,
                    declaration.DateFin)
                : FromOpen(),
            "ExactCalendar" => ComputeExactCalendarDeadline(template, declaration) ?? FromOpen(),
            "PeriodStart" => declaration != null
                ? declaration.DateDebut.Date.AddDays(offsetDays)
                : FromOpen(),
            "PeriodEnd" => declaration != null
                ? declaration.DateFin.Date.AddDays(offsetDays)
                : FromOpen(),
            "DeclarationDateLimite" => declaration != null
                ? declaration.DateLimite.Date.AddDays(offsetDays)
                : FromOpen(),
            // Ouverture générique (étape ou déclaration)
            "OnDeclarationOpen" => FromOpen(),
            _ => FromOpen() // OnStepOpen (default)
        };
    }

    private static DateTime? ComputeExactCalendarDeadline(WorkflowStepTemplate template, Declaration? declaration)
    {
        if (declaration == null) return null;

        var periodEnd = declaration.DateFin.Date;
        var defaultDay = string.Equals(declaration?.Type, "TVA", StringComparison.OrdinalIgnoreCase) ? 26 : 20;
        var day = Math.Clamp(template.DeadlineDay ?? defaultDay, 1, 31);
        var freq = InferDeclarationFrequency(declaration);

        return freq switch
        {
            "Annuelle" => ClampDate(
                periodEnd.Year + (template.DeadlineYearOffset ?? 1),
                template.DeadlineMonth ?? 3,
                day),

            "Trimestrielle" => template.DeadlineMonth is >= 1 and <= 12
                ? ClampDate(
                    ResolveExactYear(periodEnd, template.DeadlineMonth.Value, template.DeadlineYearOffset ?? 0),
                    template.DeadlineMonth.Value,
                    day)
                : ClampDateFromMonth(periodEnd.AddMonths(template.DeadlineMonthOffset ?? 1), day),

            // Mensuelle (default) — jour N du mois suivant (ou + MonthOffset)
            _ => ClampDateFromMonth(periodEnd.AddMonths(template.DeadlineMonthOffset ?? 1), day)
        };
    }

    private static string InferDeclarationFrequency(Declaration declaration)
    {
        var periode = declaration.Periode?.Trim() ?? "";
        if (periode.Contains('Q', StringComparison.OrdinalIgnoreCase) || periode.Contains("-T", StringComparison.OrdinalIgnoreCase))
            return "Trimestrielle";
        if (System.Text.RegularExpressions.Regex.IsMatch(periode, @"^\d{4}$"))
            return "Annuelle";
        if (periode.Length >= 7 && periode[4] == '-')
            return "Mensuelle";

        var spanDays = (declaration.DateFin.Date - declaration.DateDebut.Date).TotalDays;
        if (spanDays > 300) return "Annuelle";
        if (spanDays > 80) return "Trimestrielle";
        return "Mensuelle";
    }

    private static int ResolveExactYear(DateTime periodEnd, int month, int yearOffset)
    {
        var year = periodEnd.Year + yearOffset;
        // If no year offset and target month is before/equal period end month, use next year
        if (yearOffset == 0 && month <= periodEnd.Month)
            year = periodEnd.Year + 1;
        return year;
    }

    private static DateTime ClampDateFromMonth(DateTime monthCursor, int day)
        => ClampDate(monthCursor.Year, monthCursor.Month, day);

    private static DateTime ClampDate(int year, int month, int day)
    {
        month = Math.Clamp(month, 1, 12);
        var dim = DateTime.DaysInMonth(year, month);
        return new DateTime(year, month, Math.Min(Math.Max(day, 1), dim));
    }

    /// <summary>Process declarations whose lifecycle end is due (Hangfire). Returns count processed.</summary>
    public async Task<int> ProcessDueLifecycleEndsAsync()
    {
        var now = DateTime.UtcNow;
        var terminal = new[]
        {
            RefCodes.StatutDeclaration.Cloturee,
            RefCodes.StatutDeclaration.Archivee,
            RefCodes.StatutDeclaration.Suspendue
        };

        var due = await _db.Declarations
            .Where(d => d.LifecycleEndsAt != null
                && d.LifecycleEndsAt <= now
                && d.LifecycleEndedAt == null
                && !terminal.Contains(d.Statut))
            .OrderBy(d => d.LifecycleEndsAt)
            .Take(100)
            .ToListAsync();

        var processed = 0;
        foreach (var decl in due)
        {
            try
            {
                var actionName = string.IsNullOrWhiteSpace(decl.LifecycleEndAction)
                    ? nameof(WorkflowActionType.ClotureDeclaration)
                    : decl.LifecycleEndAction.Trim();
                if (!Enum.TryParse<WorkflowActionType>(actionName, true, out var action)
                    || action == WorkflowActionType.None)
                    action = WorkflowActionType.ClotureDeclaration;

                var instance = await _db.WorkflowInstances
                    .Include(wi => wi.StepInstances)
                    .Include(wi => wi.Declaration)
                    .FirstOrDefaultAsync(wi => wi.DeclarationId == decl.Id
                        && wi.CabinetId == decl.CabinetId
                        && wi.Status == WorkflowInstanceStatus.Active);

                if (instance != null)
                {
                    instance.Declaration ??= decl;
                    var dummyStep = instance.StepInstances.OrderBy(s => s.StepOrder).FirstOrDefault()
                        ?? new WorkflowStepInstance
                        {
                            WorkflowInstanceId = instance.Id,
                            StepOrder = 0,
                            StepName = "LifecycleEnd",
                            Status = WorkflowStepStatus.Completed,
                            CabinetId = decl.CabinetId,
                            CreatedBy = "SYSTEM"
                        };
                    await ExecuteStepActionAsync(action, instance, dummyStep, decl.CabinetId, "SYSTEM");

                    // Clear open relance schedules
                    foreach (var si in instance.StepInstances.Where(s => s.NextRetryAt != null))
                        si.NextRetryAt = null;

                    if (action is WorkflowActionType.ClotureDeclaration or WorkflowActionType.ArchiveDeclaration)
                    {
                        instance.Status = WorkflowInstanceStatus.Completed;
                        instance.CompletedAt = now;
                    }
                }
                else
                {
                    // No active WF — apply action directly on declaration
                    switch (action)
                    {
                        case WorkflowActionType.ArchiveDeclaration:
                            decl.Statut = RefCodes.StatutDeclaration.Archivee;
                            break;
                        case WorkflowActionType.SuspendDeclaration:
                            decl.Statut = RefCodes.StatutDeclaration.Suspendue;
                            break;
                        case WorkflowActionType.SubmitDeclaration:
                            decl.Statut = RefCodes.StatutDeclaration.Soumise;
                            break;
                        case WorkflowActionType.ValidateDeclaration:
                            decl.Statut = RefCodes.StatutDeclaration.Validee;
                            break;
                        case WorkflowActionType.CompleteDeclaration:
                            decl.Statut = RefCodes.StatutDeclaration.Completee;
                            break;
                        default:
                            decl.Statut = RefCodes.StatutDeclaration.Cloturee;
                            break;
                    }
                    decl.UpdatedBy = "SYSTEM";
                }

                decl.LifecycleEndedAt = now;
                processed++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lifecycle end failed for declaration {Id}", decl.Id);
            }
        }

        if (processed > 0)
            await _db.SaveChangesAsync();
        return processed;
    }

    /// <summary>Process due relances on waiting steps (embedded config) + legacy Relance steps. Hangfire.</summary>
    public async Task<int> ProcessDueRelancesAsync()
    {
        var now = DateTime.UtcNow;
        var due = await _db.WorkflowStepInstances
            .Include(s => s.WorkflowStepTemplate)
            .Include(s => s.WorkflowInstance).ThenInclude(wi => wi!.Client)
            .Include(s => s.WorkflowInstance).ThenInclude(wi => wi!.Declaration)
            .Include(s => s.WorkflowInstance).ThenInclude(wi => wi!.StepInstances).ThenInclude(si => si.WorkflowStepTemplate)
            .Where(s => s.NextRetryAt != null && s.NextRetryAt <= now
                && s.WorkflowStepTemplate != null
                && s.WorkflowStepTemplate.MaxRetries > 0
                && s.WorkflowInstance != null
                && s.WorkflowInstance.Status == WorkflowInstanceStatus.Active)
            .ToListAsync();

        // Embedded: step still open. Legacy Relance step: completed but Collecte still waiting.
        due = due.Where(s =>
        {
            var tpl = s.WorkflowStepTemplate!;
            var isLegacy = tpl.StepType == WorkflowStepType.Relance
                || tpl.ActionOnEntry == WorkflowActionType.RelanceClient;
            if (isLegacy && s.Status == WorkflowStepStatus.Completed)
            {
                return s.WorkflowInstance!.StepInstances.Any(si =>
                    si.Status == WorkflowStepStatus.WaitingForClient
                    || (si.WorkflowStepTemplate?.StepType == WorkflowStepType.Collecte
                        && si.Status is WorkflowStepStatus.WaitingForClient or WorkflowStepStatus.InProgress or WorkflowStepStatus.Pending));
            }
            return s.Status is WorkflowStepStatus.WaitingForClient or WorkflowStepStatus.InProgress;
        }).ToList();

        var processed = 0;
        foreach (var step in due)
        {
            var template = step.WorkflowStepTemplate!;
            var instance = step.WorkflowInstance!;
            try
            {
                var declStatut = instance.Declaration?.Statut;
                if (IsDeclarationClosedForRelance(declStatut))
                {
                    step.NextRetryAt = null;
                    await _db.SaveChangesAsync();
                    continue;
                }

                // Step completed meanwhile — stop
                if (step.Status is WorkflowStepStatus.Completed or WorkflowStepStatus.Skipped or WorkflowStepStatus.Failed
                    && template.StepType != WorkflowStepType.Relance
                    && template.ActionOnEntry != WorkflowActionType.RelanceClient)
                {
                    step.NextRetryAt = null;
                    await _db.SaveChangesAsync();
                    continue;
                }

                step.RetryCount++;
                step.LastRetryAt = now;
                await ExecuteStepActionAsync(WorkflowActionType.RelanceClient, instance, step, step.CabinetId, "SYSTEM");

                if (template.MaxRetries > 0 && step.RetryCount >= template.MaxRetries)
                {
                    var action = (template.OnMaxRetriesAction ?? "Suspend").Trim();
                    if (string.Equals(action, "Suspend", StringComparison.OrdinalIgnoreCase))
                    {
                        step.NextRetryAt = null;
                        await SuspendDeclarationAfterMaxRelancesAsync(instance, step, template.MaxRetries, "SYSTEM");
                    }
                    else if (string.Equals(action, "Cancel", StringComparison.OrdinalIgnoreCase))
                    {
                        instance.Status = WorkflowInstanceStatus.Cancelled;
                        instance.CancelledAt = now;
                        step.NextRetryAt = null;
                        _db.WorkflowHistories.Add(new WorkflowHistory
                        {
                            WorkflowInstanceId = instance.Id,
                            WorkflowStepInstanceId = step.Id,
                            Action = "RelanceMaxRetriesCancel",
                            Comment = $"Max relances ({template.MaxRetries}) atteint — workflow annulé.",
                            UserId = "SYSTEM",
                            CabinetId = step.CabinetId,
                            CreatedBy = "SYSTEM"
                        });
                    }
                    else if (string.Equals(action, "NextStep", StringComparison.OrdinalIgnoreCase)
                          || string.Equals(action, "SkipStep", StringComparison.OrdinalIgnoreCase))
                    {
                        step.NextRetryAt = null;
                        if (step.Status is WorkflowStepStatus.WaitingForClient or WorkflowStepStatus.InProgress)
                        {
                            await AdvanceStepAsync(instance.Id, new AdvanceStepDto
                            {
                                Notes = "Avance auto après max relances",
                                ForceSkip = string.Equals(action, "SkipStep", StringComparison.OrdinalIgnoreCase)
                            }, step.CabinetId, "SYSTEM");
                        }
                    }
                    else
                    {
                        step.NextRetryAt = null;
                        var managers = await _db.Users
                            .Where(u => u.CabinetId == step.CabinetId && u.IsActive)
                            .Take(5).ToListAsync();
                        foreach (var mgr in managers)
                        {
                            await _notifications.CreateAsync(mgr.Id,
                                "Relance — escalade",
                                $"Max relances atteint pour {instance.Client?.RaisonSociale ?? "client"} — étape « {step.StepName} » (WF #{instance.Id}).",
                                "Warning", "Workflow", null, step.CabinetId);
                        }
                        _db.WorkflowHistories.Add(new WorkflowHistory
                        {
                            WorkflowInstanceId = instance.Id,
                            WorkflowStepInstanceId = step.Id,
                            Action = "RelanceMaxRetriesEscalate",
                            Comment = $"Max relances ({template.MaxRetries}) — escalade manager.",
                            UserId = "SYSTEM",
                            CabinetId = step.CabinetId,
                            CreatedBy = "SYSTEM"
                        });
                    }
                }
                else
                {
                    step.NextRetryAt = now.AddHours(template.RetryIntervalHours ?? 360);
                    _db.WorkflowHistories.Add(new WorkflowHistory
                    {
                        WorkflowInstanceId = instance.Id,
                        WorkflowStepInstanceId = step.Id,
                        Action = "RelanceExecuted",
                        Comment = $"Relance #{step.RetryCount}/{template.MaxRetries} sur « {step.StepName} » (prochaine: {step.NextRetryAt:u})",
                        UserId = "SYSTEM",
                        CabinetId = step.CabinetId,
                        CreatedBy = "SYSTEM"
                    });
                }

                await _db.SaveChangesAsync();
                processed++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProcessDueRelances failed for step {StepId}", step.Id);
            }
        }

        return processed;
    }

    private static bool IsDeclarationClosedForRelance(string? statut) =>
        string.Equals(statut, RefCodes.StatutDeclaration.Suspendue, StringComparison.OrdinalIgnoreCase)
        || string.Equals(statut, RefCodes.StatutDeclaration.Validee, StringComparison.OrdinalIgnoreCase)
        || string.Equals(statut, RefCodes.StatutDeclaration.Archivee, StringComparison.OrdinalIgnoreCase)
        || string.Equals(statut, RefCodes.StatutDeclaration.Cloturee, StringComparison.OrdinalIgnoreCase)
        || string.Equals(statut, RefCodes.StatutDeclaration.Deposee, StringComparison.OrdinalIgnoreCase)
        || string.Equals(statut, RefCodes.StatutDeclaration.Soumise, StringComparison.OrdinalIgnoreCase)
        || string.Equals(statut, RefCodes.StatutDeclaration.EnvoyeeSimplTva, StringComparison.OrdinalIgnoreCase)
        || string.Equals(statut, RefCodes.StatutDeclaration.EnvoyeeSimplIs, StringComparison.OrdinalIgnoreCase);

    private async Task SuspendDeclarationAfterMaxRelancesAsync(
        WorkflowInstance instance,
        WorkflowStepInstance step,
        int maxRetries,
        string userId)
    {
        instance.Status = WorkflowInstanceStatus.Paused;
        var clientName = instance.Client?.RaisonSociale ?? "client";

        if (instance.Declaration != null
            && !string.Equals(instance.Declaration.Statut, RefCodes.StatutDeclaration.Suspendue, StringComparison.OrdinalIgnoreCase))
        {
            var oldStatut = instance.Declaration.Statut;
            instance.Declaration.Statut = RefCodes.StatutDeclaration.Suspendue;
            instance.Declaration.UpdatedBy = userId;
            instance.Declaration.UpdatedAt = DateTime.UtcNow;

            _db.DeclarationHistories.Add(new DeclarationHistory
            {
                DeclarationId = instance.Declaration.Id,
                CabinetId = step.CabinetId,
                AncienStatut = oldStatut,
                NouveauStatut = RefCodes.StatutDeclaration.Suspendue,
                UserId = userId,
                Commentaire = $"Suspendue après {maxRetries} relance(s) documents sans réponse.",
                CreatedBy = userId
            });
        }

        _db.WorkflowHistories.Add(new WorkflowHistory
        {
            WorkflowInstanceId = instance.Id,
            WorkflowStepInstanceId = step.Id,
            Action = "RelanceMaxRetriesSuspend",
            Comment = $"Max relances ({maxRetries}) atteint — déclaration suspendue, workflow en pause.",
            UserId = userId,
            CabinetId = step.CabinetId,
            CreatedBy = userId
        });

        var notifyIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var si in instance.StepInstances)
        {
            if (!string.IsNullOrEmpty(si.AssignedToUserId))
                notifyIds.Add(si.AssignedToUserId);
        }

        var managers = await _db.Users
            .Where(u => u.CabinetId == step.CabinetId && u.IsActive)
            .Take(5).Select(u => u.Id).ToListAsync();
        foreach (var id in managers)
            notifyIds.Add(id);

        var declUrl = instance.DeclarationId.HasValue
            ? $"/Declarations/Details/{instance.DeclarationId}"
            : null;
        foreach (var uid in notifyIds)
        {
            await _notifications.CreateAsync(uid,
                "Déclaration suspendue — documents manquants",
                $"La déclaration de {clientName} a été suspendue après {maxRetries} relance(s) sans documents (WF #{instance.Id}).",
                "Warning", "Declaration", declUrl, step.CabinetId);
        }
    }

    private async Task ExecuteStepActionAsync(
        WorkflowActionType action,
        WorkflowInstance instance,
        WorkflowStepInstance step,
        int cabinetId,
        string userId)
    {
        switch (action)
        {
            case WorkflowActionType.CreateDeclaration:
                await EnsureDeclarationLinkedAsync(instance, cabinetId, userId);
                break;

            case WorkflowActionType.RequestDocuments:
            case WorkflowActionType.RelanceClient:
            case WorkflowActionType.NotifyClient:
            case WorkflowActionType.PaymentReminder:
            {
                if (action == WorkflowActionType.RequestDocuments && instance.DeclarationId.HasValue
                    && string.Equals(instance.Declaration?.Type, "IS", StringComparison.OrdinalIgnoreCase))
                {
                    await _declarations.EnsureAnnualChecklistAsync(instance.DeclarationId.Value, cabinetId, userId);
                }

                var clientName = instance.Client?.RaisonSociale ?? $"Client #{instance.ClientId}";
                var declTypeLabel = instance.Declaration?.Type ?? "déclaration";
                var title = action switch
                {
                    WorkflowActionType.RequestDocuments => "Demande de documents",
                    WorkflowActionType.RelanceClient => "Relance documents",
                    WorkflowActionType.PaymentReminder => declTypeLabel.Equals("IS", StringComparison.OrdinalIgnoreCase)
                        ? "Rappel de paiement IS / CM"
                        : "Rappel de paiement TVA",
                    _ => "Notification client"
                };
                var body = action switch
                {
                    WorkflowActionType.RequestDocuments =>
                        $"Merci de déposer les pièces pour la déclaration {(instance.Declaration?.Type ?? "TVA")} {instance.Declaration?.Periode} sur l'extranet.",
                    WorkflowActionType.RelanceClient =>
                        $"Rappel : documents manquants pour {instance.Declaration?.Type} {instance.Declaration?.Periode} (tentative {step.RetryCount}).",
                    WorkflowActionType.PaymentReminder =>
                        declTypeLabel.Equals("IS", StringComparison.OrdinalIgnoreCase)
                            ? $"Solde IS / CM à régler pour l'exercice {instance.Declaration?.Periode}. Confirmez le paiement (stub)."
                            : $"TVA due à régler pour {instance.Declaration?.Periode}. Confirmez le paiement (stub).",
                    _ => $"Notification workflow — {step.StepName}"
                };

                var clientUsers = await _db.Users
                    .Where(u => u.ClientId == instance.ClientId && u.IsActive)
                    .Select(u => u.Id).ToListAsync();
                if (clientUsers.Count == 0 && step.AssignedToUserId != null)
                    clientUsers.Add(step.AssignedToUserId);
                if (clientUsers.Count == 0)
                {
                    clientUsers = await _db.Users
                        .Where(u => u.CabinetId == cabinetId && u.IsActive)
                        .OrderBy(u => u.Id).Take(3).Select(u => u.Id).ToListAsync();
                }

                foreach (var uid in clientUsers.Distinct())
                    await _notifications.CreateAsync(uid, $"{title} — {clientName}", body, "Info", "Workflow", null, cabinetId);
                break;
            }

            case WorkflowActionType.SuspendDeclaration:
                await SuspendDeclarationAfterMaxRelancesAsync(instance, step, step.RetryCount > 0 ? step.RetryCount : 0, userId);
                step.NextRetryAt = null;
                break;

            case WorkflowActionType.ArchiveDeclaration:
                if (instance.Declaration != null)
                {
                    instance.Declaration.Statut = RefCodes.StatutDeclaration.Archivee;
                    instance.Declaration.UpdatedBy = userId;
                }
                break;

            case WorkflowActionType.ClotureDeclaration:
                if (instance.Declaration != null)
                {
                    instance.Declaration.Statut = RefCodes.StatutDeclaration.Cloturee;
                    instance.Declaration.UpdatedBy = userId;
                    instance.Declaration.LifecycleEndedAt ??= DateTime.UtcNow;
                }
                break;

            case WorkflowActionType.ExportXML:
            {
                var isIs = string.Equals(instance.Declaration?.Type, "IS", StringComparison.OrdinalIgnoreCase)
                    || instance.WorkflowTemplate?.DeclarationType == WorkflowDeclarationType.IS;
                _db.WorkflowHistories.Add(new WorkflowHistory
                {
                    WorkflowInstanceId = instance.Id,
                    WorkflowStepInstanceId = step.Id,
                    Action = isIs ? "SimplIsStubExport" : "SimplTvaStubExport",
                    Comment = isIs
                        ? $"Pack liasse / ACK stub SIMPL-IS pour déclaration #{instance.DeclarationId} (pas d'appel DGI réel)."
                        : $"XML/ACK stub généré pour déclaration #{instance.DeclarationId} (pas d'appel DGI réel).",
                    UserId = userId,
                    CabinetId = cabinetId,
                    CreatedBy = userId
                });
                if (instance.Declaration != null && isIs)
                    instance.Declaration.Statut = RefCodes.StatutDeclaration.EnvoyeeSimplIs;
                break;
            }

            case WorkflowActionType.CompleteDeclaration:
                if (instance.Declaration != null)
                    instance.Declaration.Statut = RefCodes.StatutDeclaration.DocumentsRecus;
                break;

            case WorkflowActionType.ValidateDeclaration:
                if (instance.Declaration != null)
                    instance.Declaration.Statut = RefCodes.StatutDeclaration.Validee;
                break;

            case WorkflowActionType.SubmitDeclaration:
                if (instance.Declaration != null)
                {
                    var isIsSubmit = string.Equals(instance.Declaration.Type, "IS", StringComparison.OrdinalIgnoreCase);
                    instance.Declaration.Statut = isIsSubmit
                        ? RefCodes.StatutDeclaration.EnvoyeeSimplIs
                        : RefCodes.StatutDeclaration.EnvoyeeSimplTva;
                    instance.Declaration.DateSoumission ??= DateTime.UtcNow;
                }
                break;
        }
    }

    public async Task HealMissingDeclarationsForClientAsync(int cabinetId, int clientId, string userId)
    {
        var orphans = await _db.WorkflowInstances
            .Include(wi => wi.WorkflowTemplate)
            .Where(wi => wi.CabinetId == cabinetId
                && wi.ClientId == clientId
                && wi.Status == WorkflowInstanceStatus.Active
                && wi.DeclarationId == null
                && wi.WorkflowTemplate != null
                && wi.WorkflowTemplate.DeclarationType != WorkflowDeclarationType.All)
            .ToListAsync();

        if (orphans.Count == 0) return;

        foreach (var wi in orphans)
            await EnsureDeclarationLinkedAsync(wi, cabinetId, userId);

        await _db.SaveChangesAsync();
    }

    private async Task EnsureDeclarationLinkedAsync(WorkflowInstance instance, int cabinetId, string userId)
    {
        if (instance.DeclarationId.HasValue) return;
        if (instance.ClientId <= 0) return;

        var templateType = instance.WorkflowTemplate?.DeclarationType
            ?? await _db.WorkflowTemplates.Where(t => t.Id == instance.WorkflowTemplateId).Select(t => (WorkflowDeclarationType?)t.DeclarationType).FirstOrDefaultAsync();
        if (templateType == WorkflowDeclarationType.Domiciliation || templateType == WorkflowDeclarationType.All)
            return;

        var rawType = templateType?.ToString()
            ?? "TVA";

        var declType = DeclarationServiceCatalog.NormalizeType(rawType);
        if (string.Equals(declType, "All", StringComparison.OrdinalIgnoreCase) || string.Equals(declType, "0", StringComparison.OrdinalIgnoreCase))
            declType = "TVA";

        if (!DeclarationServiceCatalog.IsSupported(declType))
        {
            _logger.LogInformation("CreateDeclaration skipped for unsupported type {Type} on WF {Id}", declType, instance.Id);
            return;
        }

        var existing = await _db.Declarations
            .Where(d => d.CabinetId == cabinetId && d.ClientId == instance.ClientId && d.Type == declType
                && d.Statut != RefCodes.StatutDeclaration.Cloturee
                && d.Statut != RefCodes.StatutDeclaration.Archivee)
            .OrderByDescending(d => d.Id)
            .FirstOrDefaultAsync();

        if (existing != null)
        {
            instance.DeclarationId = existing.Id;
            instance.Declaration = existing;
            return;
        }

        var initCfg = await ResolveInitPeriodConfigAsync(instance.WorkflowTemplateId, declType);

        var created = await _declarations.CreateAsync(new CreateDeclarationDto
        {
            ClientId = instance.ClientId,
            Type = declType,
            AnneeFiscale = DateTime.UtcNow.Year,
            Frequency = initCfg.Frequency,
            Periode = initCfg.Periode,
            PeriodStart = initCfg.PeriodStart,
            PeriodEnd = initCfg.PeriodEnd,
            DueMonth = initCfg.DueMonth,
            DueDay = initCfg.DueDay,
            DateLimite = initCfg.DateLimite,
            WindowStartDay = initCfg.WindowStartDay,
            WindowEndDay = initCfg.WindowEndDay,
            WindowMonthOffset = initCfg.WindowMonthOffset,
            DeadlineMonth = initCfg.DeadlineMonth,
            DeadlineYearOffset = initCfg.DeadlineYearOffset
        }, cabinetId, userId);

        if (created.Success && created.Data != null)
        {
            instance.DeclarationId = created.Data.Id;
            instance.Declaration = await _db.Declarations.FindAsync(created.Data.Id);
            _logger.LogInformation("Created {Type} declaration {DeclId} for workflow {WfId}", declType, created.Data.Id, instance.Id);
        }
        else
        {
            _logger.LogWarning("CreateDeclaration failed for WF {Id} type {Type}: {Msg}", instance.Id, declType, created.Message);
        }
    }

    private sealed record InitPeriodConfig(
        string? Frequency,
        string? Periode,
        DateTime? PeriodStart,
        DateTime? PeriodEnd,
        int? DueMonth,
        int? DueDay,
        DateTime? DateLimite,
        int? WindowStartDay,
        int? WindowEndDay,
        int? WindowMonthOffset,
        int? DeadlineMonth,
        int? DeadlineYearOffset);

    /// <summary>
    /// Reads Init periods[] (DateDebut/DateFin/Échéance per row) — due preferred over legacy window*.
    /// </summary>
    private async Task<InitPeriodConfig> ResolveInitPeriodConfigAsync(int workflowTemplateId, string declarationType)
    {
        string? frequency = null;
        string? periode = null;
        DateTime? periodStart = null, periodEnd = null, dateLimite = null;
        int? dueMonth = null, dueDay = null;
        int? startDay = null, endDay = null, monthOffset = null, deadlineMonth = null, deadlineYearOffset = null;
        var patternPeriods = new List<(int StartMonth, int StartDay, int EndMonth, int EndDay, string? Label, int? DueMonth, int? DueDay)>();

        var init = await _db.WorkflowStepTemplates
            .Where(s => s.WorkflowTemplateId == workflowTemplateId
                && !s.IsDeleted
                && s.StepType == WorkflowStepType.Initialisation)
            .OrderBy(s => s.StepOrder)
            .Select(s => new { s.ActionConfigJson, s.CronExpression })
            .FirstOrDefaultAsync();

        if (init != null && !string.IsNullOrWhiteSpace(init.ActionConfigJson))
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(init.ActionConfigJson);
                var root = doc.RootElement;
                if (root.TryGetProperty("periodFrequency", out var pf) && pf.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    var v = pf.GetString();
                    if (!string.IsNullOrWhiteSpace(v)) frequency = v.Trim();
                }
                if (root.TryGetProperty("periodTemplate", out var pt) && pt.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    var v = pt.GetString();
                    if (!string.IsNullOrWhiteSpace(v)) frequency ??= v.Trim();
                }
                if (root.TryGetProperty("periods", out var arr) && arr.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    foreach (var el in arr.EnumerateArray())
                    {
                        string? label = null;
                        if (el.TryGetProperty("label", out var lb) && lb.ValueKind == System.Text.Json.JsonValueKind.String)
                            label = lb.GetString();

                        int sm, sd, em, ed;
                        int? rowDueM = null, rowDueD = null;
                        if (el.TryGetProperty("dueMonth", out var dueMEl) && dueMEl.TryGetInt32(out var dmi))
                            rowDueM = dmi;
                        if (el.TryGetProperty("dueDay", out var dueDEl) && dueDEl.TryGetInt32(out var ddi))
                            rowDueD = ddi;
                        if ((rowDueM == null || rowDueD == null)
                            && el.TryGetProperty("due", out var dueEl) && dueEl.ValueKind == System.Text.Json.JsonValueKind.String
                            && DeclarationPeriodTemplate.TryParseMonthDay(dueEl.GetString(), out var pdm, out var pdd))
                        {
                            rowDueM = pdm; rowDueD = pdd;
                        }

                        if (el.TryGetProperty("startMonth", out var smEl) && smEl.TryGetInt32(out sm)
                            && el.TryGetProperty("startDay", out var sdEl) && sdEl.TryGetInt32(out sd)
                            && el.TryGetProperty("endMonth", out var emEl) && emEl.TryGetInt32(out em)
                            && el.TryGetProperty("endDay", out var edEl) && edEl.TryGetInt32(out ed))
                        {
                            patternPeriods.Add((sm, sd, em, ed, label, rowDueM, rowDueD));
                            continue;
                        }

                        if (!el.TryGetProperty("start", out var sEl) || !el.TryGetProperty("end", out var eEl))
                            continue;
                        if (!DeclarationPeriodTemplate.TryParseMonthDay(sEl.GetString(), out sm, out sd))
                            continue;
                        if (!DeclarationPeriodTemplate.TryParseMonthDay(eEl.GetString(), out em, out ed))
                            continue;
                        patternPeriods.Add((sm, sd, em, ed, label, rowDueM, rowDueD));
                    }
                }
                // legacy offsets (fallback when no per-row due)
                if (root.TryGetProperty("windowStartDay", out var ws) && ws.TryGetInt32(out var wsd)) startDay = wsd;
                if (root.TryGetProperty("windowEndDay", out var we) && we.TryGetInt32(out var wed)) endDay = wed;
                if (root.TryGetProperty("windowMonthOffset", out var wo) && wo.TryGetInt32(out var mo)) monthOffset = mo;
                if (root.TryGetProperty("deadlineMonth", out var dm) && dm.TryGetInt32(out var dmv)) deadlineMonth = dmv;
                if (root.TryGetProperty("deadlineYearOffset", out var dy) && dy.TryGetInt32(out var dyv)) deadlineYearOffset = dyv;
            }
            catch { /* ignore bad json */ }
        }

        if (frequency == null && init != null && !string.IsNullOrWhiteSpace(init.CronExpression))
        {
            var cron = init.CronExpression.Trim();
            var parts = cron.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 5)
            {
                var dom = parts[2]; var mon = parts[3]; var dow = parts[4];
                if (dow == "*")
                {
                    if (mon != "*" && mon.Split(',').Length <= 4) frequency = "Trimestrielle";
                    else if (mon != "*") frequency = "Annuelle";
                    else if (dom != "*") frequency = "Mensuelle";
                }
            }
        }

        frequency ??= "Mensuelle";

        if (patternPeriods.Count > 0)
        {
            var slice = DeclarationPeriodTemplate.ResolvePatternTableAsOf(DateTime.UtcNow, patternPeriods, previousClosed: true);
            if (slice != null)
            {
                periodStart = slice.Start;
                periodEnd = slice.End;
                periode = slice.Label;
                dateLimite = slice.Due;
                if (slice.Due.HasValue)
                {
                    dueMonth = slice.Due.Value.Month;
                    dueDay = slice.Due.Value.Day;
                }
            }
        }
        else
        {
            var resolved = DeclarationPeriodTemplate.ResolveAsOf(DateTime.UtcNow, frequency, previousClosed: true);
            if (resolved != null)
            {
                periodStart = resolved.Start;
                periodEnd = resolved.End;
                periode = resolved.Code;
                dateLimite = DeclarationPeriodTemplate.EndOfMonthAfter(resolved.End);
                dueMonth = dateLimite.Value.Month;
                dueDay = dateLimite.Value.Day;
            }
        }

        // Legacy window defaults only when no per-row due
        if (dueMonth == null)
        {
            var life = MoroccoDeclarationCalendar.GetFrequencyLifecycleDefaults(frequency);
            startDay ??= life.StartDay;
            endDay ??= life.EndDay;
            monthOffset ??= life.Months;
            deadlineYearOffset ??= life.Years;
        }

        return new InitPeriodConfig(
            frequency,
            periode,
            periodStart,
            periodEnd,
            dueMonth,
            dueDay,
            dateLimite,
            startDay,
            endDay,
            monthOffset,
            deadlineMonth,
            deadlineYearOffset);
    }
    private async Task<string?> EvaluateDocumentGateAsync(WorkflowStepTemplate stepTemplate, int declarationId, int cabinetId)
    {
        if (string.IsNullOrWhiteSpace(stepTemplate.ValidationRulesJson))
            return null;

        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(stepTemplate.ValidationRulesJson);
            var root = doc.RootElement;
            var require = root.TryGetProperty("requireDocuments", out var rd) && rd.ValueKind == System.Text.Json.JsonValueKind.True;
            var minDocs = root.TryGetProperty("minDocuments", out var md) && md.TryGetInt32(out var m) ? m : 1;
            var requireChecklist = root.TryGetProperty("requireChecklistComplete", out var rc)
                && rc.ValueKind == System.Text.Json.JsonValueKind.True;

            if (require)
            {
                var count = await _db.Documents.CountAsync(d =>
                    d.CabinetId == cabinetId && d.DeclarationId == declarationId && !d.IsDeleted);
                if (count < minDocs)
                    return $"Documents insuffisants ({count}/{minDocs}). Déposez les pièces ou utilisez « Passer » (ForceSkip) si autorisé.";
            }

            if (requireChecklist)
            {
                var required = await _db.DeclarationChecklistItems
                    .Where(c => c.DeclarationId == declarationId && c.CabinetId == cabinetId && !c.IsDeleted && c.IsRequired)
                    .ToListAsync();
                if (required.Count == 0)
                {
                    await _declarations.EnsureAnnualChecklistAsync(declarationId, cabinetId, "system");
                    required = await _db.DeclarationChecklistItems
                        .Where(c => c.DeclarationId == declarationId && c.CabinetId == cabinetId && !c.IsDeleted && c.IsRequired)
                        .ToListAsync();
                }

                var missing = required.Where(c => !c.IsReceived).Select(c => c.Label).ToList();
                if (missing.Count > 0)
                    return $"Checklist annuelle incomplète ({required.Count - missing.Count}/{required.Count}). Manquant : {string.Join(", ", missing.Take(4))}{(missing.Count > 4 ? "…" : "")}.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Invalid ValidationRulesJson on step template {Id}", stepTemplate.Id);
        }

        return null;
    }

    /// <summary>
    /// If the step has no / wrong assignee for this dossier role, let the eligible user claim it
    /// (e.g. Comptable on Import & calcul after finishing Contrôle).
    /// </summary>
    private async Task TryClaimStepIfEligibleAsync(
        WorkflowStepInstance step, WorkflowStepTemplate? template, WorkflowInstance instance, int cabinetId, string userId)
    {
        if (template == null || template.AssignmentType != AssignmentType.Employee)
            return;
        if (CanUserAdvanceStep(step, template, userId))
            return;

        var clientId = instance.ClientId;
        var hint = SuggestEmployeeRoleHint(template);
        if (!await UserMatchesRoleHintAsync(userId, hint, cabinetId, clientId))
            return;

        var explicitIds = ParseAssigneeIds(template.AssignedToUserIds, template.AssignedToUserId);
        if (explicitIds.Count > 0 && !explicitIds.Contains(userId, StringComparer.OrdinalIgnoreCase))
        {
            // Template hardcodes someone else who actually matches the role → respect it
            foreach (var id in explicitIds)
            {
                if (await UserMatchesRoleHintAsync(id, hint, cabinetId, clientId))
                    return;
            }
            // Explicit IDs point at the wrong role (e.g. Fiscaliste on Calcul) → allow claim
        }
        else if (!string.IsNullOrEmpty(step.AssignedToUserId)
                 && await UserMatchesRoleHintAsync(step.AssignedToUserId, hint, cabinetId, clientId))
        {
            return; // already assigned to another matching Comptable/Fiscaliste
        }

        step.AssignedToUserId = userId;
        if (step.TaskItemId.HasValue)
        {
            var task = await _db.TaskItems.FirstOrDefaultAsync(t => t.Id == step.TaskItemId.Value);
            if (task != null)
            {
                task.AssignedToUserId = userId;
                if (task.Statut == "AFaire" || string.IsNullOrEmpty(task.Statut))
                    task.Statut = "EnCours";
                task.UpdatedAt = DateTime.UtcNow;
                task.UpdatedBy = userId;
            }
        }
        else
        {
            var clientName = await _db.Clients.Where(c => c.Id == clientId).Select(c => c.RaisonSociale).FirstOrDefaultAsync() ?? "N/A";
            var task = new TaskItem
            {
                Titre = $"[Workflow] {step.StepName} — {clientName}",
                Description = template.Description ?? $"Étape workflow: {step.StepName}",
                ClientId = clientId,
                DeclarationId = instance.DeclarationId,
                AssignedToUserId = userId,
                AssignedByUserId = userId,
                DateDebut = DateTime.UtcNow,
                DateEcheance = step.DueDate ?? DateTime.UtcNow.AddHours(template.SLAHours ?? 48),
                Priorite = template.TaskPriority ?? "Haute",
                Statut = "EnCours",
                CabinetId = cabinetId,
                CreatedBy = userId
            };
            _db.TaskItems.Add(task);
            await _db.SaveChangesAsync();
            step.TaskItemId = task.Id;
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Step {StepId} claimed by {UserId} (role hint {Hint})", step.Id, userId, hint);
    }

    /// <summary>
    /// Resolves the actual user to assign to a step based on assignment type.
    /// Employee steps without explicit IDs fall back to contrat équipe / role hints,
    /// preferring the previous completer for consecutive Comptable steps.
    /// </summary>
    private async Task<string?> ResolveAssigneeAsync(
        WorkflowStepTemplate template, int cabinetId, int? clientId, string? preferredUserId = null)
    {
        switch (template.AssignmentType)
        {
            case AssignmentType.Employee:
            {
                var ids = ParseAssigneeIds(template.AssignedToUserIds, template.AssignedToUserId);
                if (ids.Count > 0)
                    return ids.First();

                var roleHint = SuggestEmployeeRoleHint(template);
                if (!string.IsNullOrEmpty(preferredUserId)
                    && await UserMatchesRoleHintAsync(preferredUserId, roleHint, cabinetId, clientId))
                    return preferredUserId;

                var fromContract = await FindContractMemberByRoleAsync(cabinetId, clientId, roleHint);
                if (!string.IsNullOrEmpty(fromContract))
                    return fromContract;

                return await FindUserInIdentityRoleAsync(cabinetId, roleHint);
            }

            case AssignmentType.Role:
                if (!string.IsNullOrEmpty(template.AssignedToRoleName))
                {
                    var userInRole = await FindUserInIdentityRoleAsync(cabinetId, template.AssignedToRoleName);
                    return userInRole;
                }
                return null;

            case AssignmentType.Department:
                if (template.AssignedToDepartmentId.HasValue)
                {
                    var memberInDept = await _db.DepartmentMembers
                        .Where(dm => dm.DepartmentId == template.AssignedToDepartmentId.Value && dm.CabinetId == cabinetId && !dm.IsDeleted)
                        .OrderByDescending(dm => dm.IsLead)
                        .Select(dm => dm.UserId)
                        .FirstOrDefaultAsync();
                    return memberInDept;
                }
                return null;

            case AssignmentType.Client:
                if (clientId.HasValue)
                {
                    // Find any extranet user for this client
                    var extranetUser = await _db.Users
                        .Where(u => u.ClientId == clientId.Value && u.IsActive)
                        .Select(u => u.Id)
                        .FirstOrDefaultAsync();
                    return extranetUser;
                }
                return null;

            case AssignmentType.AutoSystem:
                return null;

            default:
                return template.AssignedToUserId;
        }
    }

    /// <summary>Maps TVA/employee steps to dossier role labels (Fiscaliste / Comptable / Manager).</summary>
    private static string SuggestEmployeeRoleHint(WorkflowStepTemplate template)
    {
        var name = (template.Name ?? "").ToLowerInvariant();
        var type = template.StepType;

        if (type == WorkflowStepType.Controle || type == WorkflowStepType.Saisie || type == WorkflowStepType.Rapprochement
            || name.Contains("contr") || name.Contains("calcul") || name.Contains("import")
            || name.Contains("anomal") || name.Contains("rapproche"))
            return "Comptable";

        if (type == WorkflowStepType.ValidationInterne || name.Contains("revue") || name.Contains("superviseur"))
            return "Manager";

        // Demande pièces, SIMPL, paiement, etc.
        return "Fiscaliste";
    }

    private async Task<bool> UserMatchesRoleHintAsync(string userId, string roleHint, int cabinetId, int? clientId)
    {
        if (clientId.HasValue)
        {
            var onContract = await (
                from m in _db.ContratMembres
                join c in _db.ContratsCommerciaux on m.ContratId equals c.Id
                where !m.IsDeleted && !c.IsDeleted
                      && m.UserId == userId && m.CabinetId == cabinetId
                      && c.ClientId == clientId.Value
                      && (c.Statut == RefCodes.StatutContrat.Valide || c.Statut == RefCodes.StatutContrat.Actif)
                      && m.RoleLabel != null && m.RoleLabel.Contains(roleHint)
                select m.Id).AnyAsync();
            if (onContract) return true;
        }

        return await _db.Users
            .Where(u => u.Id == userId && u.CabinetId == cabinetId && u.IsActive)
            .Join(_db.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => ur)
            .Join(_db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
            .AnyAsync(name => name == roleHint
                || (roleHint == "Manager" && (name == "Manager" || name == "Gerant" || name == "SuperAdmin")));
    }

    private async Task<string?> FindContractMemberByRoleAsync(int cabinetId, int? clientId, string roleHint)
    {
        if (!clientId.HasValue) return null;

        return await (
            from m in _db.ContratMembres
            join c in _db.ContratsCommerciaux on m.ContratId equals c.Id
            where !m.IsDeleted && !c.IsDeleted
                  && m.CabinetId == cabinetId
                  && c.ClientId == clientId.Value
                  && (c.Statut == RefCodes.StatutContrat.Valide || c.Statut == RefCodes.StatutContrat.Actif)
                  && m.RoleLabel != null && m.RoleLabel.Contains(roleHint)
            select m.UserId).FirstOrDefaultAsync();
    }

    private async Task<string?> FindUserInIdentityRoleAsync(int cabinetId, string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName)) return null;

        return await _db.Users
            .Where(u => u.CabinetId == cabinetId && u.IsActive)
            .Join(_db.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { u, ur })
            .Join(_db.Roles, x => x.ur.RoleId, r => r.Id, (x, r) => new { x.u, RoleName = r.Name })
            .Where(x => x.RoleName == roleName)
            .Select(x => x.u.Id)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Strict assignee check for AdvanceStep. SYSTEM always allowed (Hangfire / auto).
    /// Client steps: assigned extranet user or any active user of the same ClientId.
    /// Human steps: AssignedToUserId or template CSV.
    /// AutoSystem steps are not advanced by humans (only SYSTEM).
    /// </summary>
    private static bool CanUserAdvanceStep(WorkflowStepInstance currentStep, WorkflowStepTemplate? stepTemplate, string userId)
    {
        if (string.Equals(userId, "SYSTEM", StringComparison.OrdinalIgnoreCase))
            return true;

        if (stepTemplate?.AssignmentType == AssignmentType.AutoSystem)
            return false;

        if (!string.IsNullOrEmpty(currentStep.AssignedToUserId)
            && string.Equals(currentStep.AssignedToUserId, userId, StringComparison.OrdinalIgnoreCase))
            return true;

        var csvIds = ParseAssigneeIds(stepTemplate?.AssignedToUserIds, stepTemplate?.AssignedToUserId);
        if (csvIds.Contains(userId, StringComparer.OrdinalIgnoreCase))
            return true;

        return false;
    }

    private async Task<bool> CanUserAdvanceStepAsync(
        WorkflowStepInstance currentStep,
        WorkflowStepTemplate? stepTemplate,
        WorkflowInstance instance,
        string userId)
    {
        if (CanUserAdvanceStep(currentStep, stepTemplate, userId))
            return true;

        var isClientStep = stepTemplate?.IsClientStep == true
            || stepTemplate?.AssignmentType == AssignmentType.Client
            || currentStep.Status == WorkflowStepStatus.WaitingForClient;

        if (!isClientStep || instance.ClientId <= 0)
            return false;

        return await _db.Users.AnyAsync(u =>
            u.Id == userId && u.IsActive && u.ClientId == instance.ClientId);
    }

    private static List<string> ParseAssigneeIds(string? csv, string? primary = null)
    {
        var ids = new List<string>();
        if (!string.IsNullOrWhiteSpace(csv))
        {
            foreach (var part in csv.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (!string.IsNullOrWhiteSpace(part) && !ids.Contains(part, StringComparer.OrdinalIgnoreCase))
                    ids.Add(part);
            }
        }
        if (!string.IsNullOrWhiteSpace(primary) && !ids.Contains(primary, StringComparer.OrdinalIgnoreCase))
            ids.Insert(0, primary);
        return ids;
    }

    private static (string? Primary, string? Csv) NormalizeAssigneeIds(List<string>? list, string? primary)
    {
        var ids = new List<string>();
        if (list != null)
        {
            foreach (var id in list.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                var t = id.Trim();
                if (!ids.Contains(t, StringComparer.OrdinalIgnoreCase))
                    ids.Add(t);
            }
        }
        if (!string.IsNullOrWhiteSpace(primary) && !ids.Contains(primary, StringComparer.OrdinalIgnoreCase))
            ids.Insert(0, primary.Trim());
        if (ids.Count == 0) return (null, null);
        return (ids[0], string.Join(",", ids));
    }

    private async Task<string?> GetUserNameAsync(string? userId)
    {
        if (string.IsNullOrEmpty(userId)) return null;
        var user = await _db.Users.FindAsync(userId);
        return user != null ? $"{user.FirstName} {user.LastName}" : null;
    }

    // ╔══════════════════════════════════════════════════════════════════════╗
    // ║                        DTO MAPPING                                  ║
    // ╚══════════════════════════════════════════════════════════════════════╝

    private static WorkflowTemplateDto MapTemplateDto(WorkflowTemplate wt) => new()
    {
        Id = wt.Id,
        Name = wt.Name,
        Description = wt.Description,
        DeclarationType = wt.DeclarationType.ToString(),
        ClientId = wt.ClientId,
        ClientName = wt.Client?.RaisonSociale,
        IsDefault = wt.IsDefault,
        IsActive = wt.IsActive,
        Version = wt.Version,
        StepCount = wt.Steps.Count(s => !s.IsDeleted),
        InstanceCount = wt.Instances?.Count ?? 0,
        CreatedAt = wt.CreatedAt,
        Steps = wt.Steps.Where(s => !s.IsDeleted).OrderBy(s => s.StepOrder).Select(s => new WorkflowStepTemplateDto
        {
            Id = s.Id,
            WorkflowTemplateId = s.WorkflowTemplateId,
            Name = s.Name,
            Description = s.Description,
            StepOrder = s.StepOrder,
            StepType = s.StepType.ToString(),
            AssignmentType = s.AssignmentType.ToString(),
            AssignedToUserId = s.AssignedToUserId,
            AssignedToUserName = s.AssignedToUser != null ? s.AssignedToUser.FirstName + " " + s.AssignedToUser.LastName : null,
            AssignedToUserIds = s.AssignedToUserIds,
            AssignedToUserIdList = ParseAssigneeIds(s.AssignedToUserIds, s.AssignedToUserId),
            AssignedToRoleName = s.AssignedToRoleName,
            AssignedToDepartmentId = s.AssignedToDepartmentId,
            AssignedToDepartmentName = s.AssignedToDepartment?.Name,
            IsClientStep = s.IsClientStep,
            RequiredPermission = s.RequiredPermission,
            SLAHours = s.SLAHours,
            DeadlineAnchor = s.DeadlineAnchor,
            DeadlineOffsetDays = s.DeadlineOffsetDays,
            DeadlineDay = s.DeadlineDay,
            DeadlineMonth = s.DeadlineMonth,
            DeadlineMonthOffset = s.DeadlineMonthOffset,
            DeadlineYearOffset = s.DeadlineYearOffset,
            ApplyDeadlineToDeclaration = s.ApplyDeadlineToDeclaration,
            LifecycleEndMode = s.LifecycleEndMode,
            LifecycleEndDays = s.LifecycleEndDays,
            ActionOnLifecycleEnd = s.ActionOnLifecycleEnd,
            IsRequired = s.IsRequired,
            IsBlocking = s.IsBlocking,
            AutoCreateTask = s.AutoCreateTask,
            TaskType = s.TaskType.ToString(),
            TaskPriority = s.TaskPriority,
            AutoNotify = s.AutoNotify,
            ValidationRulesJson = s.ValidationRulesJson,
            DeclarationStatusOnComplete = s.DeclarationStatusOnComplete,
            DeclarationStatusOnEntry = s.DeclarationStatusOnEntry,
            ActionOnEntry = s.ActionOnEntry.ToString(),
            ActionOnComplete = s.ActionOnComplete.ToString(),
            ActionConfigJson = s.ActionConfigJson,
            CronExpression = s.CronExpression,
            CronDescription = s.CronDescription,
            IsScheduled = s.IsScheduled,
            MaxRetries = s.MaxRetries,
            RetryIntervalHours = s.RetryIntervalHours,
            OnMaxRetriesAction = s.OnMaxRetriesAction,
            RoutingMode = s.RoutingMode.ToString(),
            RoutingConditionsJson = s.RoutingConditionsJson,
            GotoStepOrder = s.GotoStepOrder,
            TriggerSource = s.TriggerSource.ToString(),
            UseServiceContratAffectation = s.UseServiceContratAffectation,
            ServiceContratTypeCode = s.ServiceContratTypeCode
        }).ToList()
    };

    private async Task<ApiResponse<UnitTakeDto>> OpenWorkflowStepTakeAsync(
        WorkflowStepInstance step, WorkflowInstance instance, int cabinetId, string userId,
        bool forceRelease, string? forceReason)
    {
        var stepTake = await _unitTakes.OpenTakeAsync(new OpenUnitTakeDto
        {
            UnitTypeCode = UnitTypeCodes.WorkflowStep,
            UnitEntityId = step.Id,
            Comment = $"Prise étape: {step.StepName}",
            TaskItemId = step.TaskItemId,
            WorkflowStepInstanceId = step.Id,
            DeclarationId = instance.DeclarationId,
            ForceRelease = forceRelease,
            ForceReason = forceReason
        }, cabinetId, userId);

        if (!stepTake.Success)
            return stepTake;

        if (step.TaskItemId.HasValue)
        {
            await _unitTakes.OpenTakeAsync(new OpenUnitTakeDto
            {
                UnitTypeCode = UnitTypeCodes.Task,
                UnitEntityId = step.TaskItemId.Value,
                Comment = $"Lié à l'étape {step.StepName}",
                TaskItemId = step.TaskItemId,
                WorkflowStepInstanceId = step.Id,
                DeclarationId = instance.DeclarationId,
                ForceRelease = forceRelease,
                ForceReason = forceReason
            }, cabinetId, userId);
        }

        if (instance.DeclarationId.HasValue)
        {
            await _unitTakes.OpenTakeAsync(new OpenUnitTakeDto
            {
                UnitTypeCode = UnitTypeCodes.Declaration,
                UnitEntityId = instance.DeclarationId.Value,
                Comment = $"Lié à l'étape {step.StepName}",
                TaskItemId = step.TaskItemId,
                WorkflowStepInstanceId = step.Id,
                DeclarationId = instance.DeclarationId,
                ForceRelease = forceRelease,
                ForceReason = forceReason
            }, cabinetId, userId);
        }

        return stepTake;
    }

    private async Task CloseWorkflowStepTakesAsync(
        WorkflowStepInstance step, int cabinetId, string userId, string? comment)
    {
        await _unitTakes.CloseTakeAsync(new CloseUnitTakeDto
        {
            UnitTypeCode = UnitTypeCodes.WorkflowStep,
            UnitEntityId = step.Id,
            Comment = comment,
            ForceClose = true
        }, cabinetId, userId);

        if (step.TaskItemId.HasValue)
        {
            await _unitTakes.CloseTakeAsync(new CloseUnitTakeDto
            {
                UnitTypeCode = UnitTypeCodes.Task,
                UnitEntityId = step.TaskItemId.Value,
                Comment = comment,
                ForceClose = true
            }, cabinetId, userId);
        }
    }

    private async Task EnrichStepsWithOpenTakesAsync(List<WorkflowStepInstanceDto> steps, int cabinetId)
    {
        if (steps.Count == 0) return;

        var stepType = await _db.UnitTypes.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Code == UnitTypeCodes.WorkflowStep && t.IsActive && !t.IsDeleted);
        if (stepType == null) return;

        var stepIds = steps.Select(s => s.Id).ToList();
        var openTakes = await _db.UnitTakes.AsNoTracking()
            .Include(t => t.TakenByUser)
            .Where(t => t.CabinetId == cabinetId
                        && t.UnitTypeId == stepType.Id
                        && stepIds.Contains(t.UnitEntityId)
                        && t.ClosedAt == null
                        && t.Status == UnitTakeStatuses.Open
                        && !t.IsDeleted)
            .ToListAsync();

        foreach (var step in steps)
        {
            var take = openTakes.FirstOrDefault(t => t.UnitEntityId == step.Id);
            if (take == null) continue;
            ApplyOpenTakeToStepDto(step, MapUnitTakeLite(take));
            // Prefer UnitTake holder as exclusive lock source for UI
            step.AssignedToUserId = take.TakenByUserId;
            if (string.IsNullOrWhiteSpace(step.AssignedToUserName) && take.TakenByUser != null)
                step.AssignedToUserName = $"{take.TakenByUser.FirstName} {take.TakenByUser.LastName}".Trim();
        }
    }

    private static UnitTakeDto MapUnitTakeLite(UnitTake t) => new()
    {
        Id = t.Id,
        UnitTypeId = t.UnitTypeId,
        UnitEntityId = t.UnitEntityId,
        TakenByUserId = t.TakenByUserId,
        TakenByUserName = t.TakenByUser != null
            ? $"{t.TakenByUser.FirstName} {t.TakenByUser.LastName}".Trim()
            : null,
        TakenAt = t.TakenAt,
        ClosedAt = t.ClosedAt,
        Status = t.Status,
        Comment = t.Comment
    };

    private static void ApplyOpenTakeToStepDto(WorkflowStepInstanceDto step, UnitTakeDto? take)
    {
        if (take == null) return;
        step.HasOpenTake = take.IsOpen;
        step.OpenTakeUserId = take.TakenByUserId;
        step.OpenTakeUserName = take.TakenByUserName;
        step.OpenTakeTakenAt = take.TakenAt;
        if (!string.IsNullOrEmpty(take.TakenByUserId))
            step.AssignedToUserId = take.TakenByUserId;
        if (!string.IsNullOrWhiteSpace(take.TakenByUserName))
            step.AssignedToUserName = take.TakenByUserName;
    }

    private static WorkflowInstanceDto MapInstanceDto(WorkflowInstance wi) => new()
    {
        Id = wi.Id,
        WorkflowTemplateId = wi.WorkflowTemplateId,
        WorkflowTemplateName = wi.WorkflowTemplate?.Name,
        DeclarationId = wi.DeclarationId,
        DeclarationType = wi.Declaration?.Type,
        DeclarationPeriode = wi.Declaration?.Periode,
        ClientId = wi.ClientId,
        ClientName = wi.Client?.RaisonSociale,
        CurrentStepOrder = wi.CurrentStepOrder,
        CurrentStepName = wi.StepInstances?.FirstOrDefault(s => s.StepOrder == wi.CurrentStepOrder)?.StepName,
        Status = wi.Status.ToString(),
        StartedByUserName = wi.StartedByUser != null ? wi.StartedByUser.FirstName + " " + wi.StartedByUser.LastName : null,
        StartedAt = wi.StartedAt,
        CompletedAt = wi.CompletedAt,
        TotalSteps = wi.StepInstances?.Count ?? 0,
        CompletedSteps = wi.StepInstances?.Count(s => s.Status == WorkflowStepStatus.Completed || s.Status == WorkflowStepStatus.Skipped) ?? 0,
        StepInstances = wi.StepInstances?.OrderBy(s => s.StepOrder).Select(MapStepInstanceDto).ToList() ?? new(),
        RecentHistory = wi.History?.OrderByDescending(h => h.ActionDate).Take(10).Select(h => new WorkflowHistoryDto
        {
            Id = h.Id,
            WorkflowInstanceId = h.WorkflowInstanceId,
            WorkflowStepInstanceId = h.WorkflowStepInstanceId,
            Action = h.Action,
            PreviousStatus = h.PreviousStatus,
            NewStatus = h.NewStatus,
            Comment = h.Comment,
            UserName = h.UserName,
            ActionDate = h.ActionDate
        }).ToList() ?? new()
    };

    private static WorkflowStepInstanceDto MapStepInstanceDto(WorkflowStepInstance s) => new()
    {
        Id = s.Id,
        WorkflowInstanceId = s.WorkflowInstanceId,
        WorkflowStepTemplateId = s.WorkflowStepTemplateId,
        StepOrder = s.StepOrder,
        StepName = s.StepName,
        StepType = s.WorkflowStepTemplate?.StepType.ToString() ?? "",
        Status = s.Status.ToString(),
        AssignedToUserId = s.AssignedToUserId,
        AssignedToUserName = s.AssignedToUser != null ? s.AssignedToUser.FirstName + " " + s.AssignedToUser.LastName : null,
        AssignedToUserIdList = ParseAssigneeIds(s.WorkflowStepTemplate?.AssignedToUserIds, s.AssignedToUserId),
        CompletedByUserName = s.CompletedByUser != null ? s.CompletedByUser.FirstName + " " + s.CompletedByUser.LastName : null,
        ClientId = s.WorkflowInstance?.ClientId,
        ClientName = s.WorkflowInstance?.Client?.RaisonSociale,
        DeclarationId = s.WorkflowInstance?.DeclarationId,
        DeclarationType = s.WorkflowInstance?.Declaration?.Type ?? s.WorkflowInstance?.WorkflowTemplate?.DeclarationType.ToString(),
        DeclarationPeriode = s.WorkflowInstance?.Declaration?.Periode,
        DeclarationStatut = s.WorkflowInstance?.Declaration?.Statut,
        AssignmentType = s.WorkflowStepTemplate?.AssignmentType.ToString(),
        WorkflowStatus = s.WorkflowInstance?.Status.ToString(),
        CurrentStepName = s.WorkflowInstance?.StepInstances?
            .FirstOrDefault(x => x.StepOrder == s.WorkflowInstance.CurrentStepOrder)?.StepName ?? s.StepName,
        TotalSteps = s.WorkflowInstance?.StepInstances?.Count ?? 0,
        CompletedSteps = s.WorkflowInstance?.StepInstances?
            .Count(x => x.Status == WorkflowStepStatus.Completed || x.Status == WorkflowStepStatus.Skipped) ?? 0,
        ActivatedAt = s.ActivatedAt,
        StartedAt = s.StartedAt,
        CompletedAt = s.CompletedAt,
        DueDate = s.DueDate,
        Notes = s.Notes,
        BlockReason = s.BlockReason,
        TaskItemId = s.TaskItemId,
        IsClientStep = s.WorkflowStepTemplate?.IsClientStep ?? false,
        IsBlocking = s.WorkflowStepTemplate?.IsBlocking ?? true,
        IsRequired = s.WorkflowStepTemplate?.IsRequired ?? true,
        RetryCount = s.RetryCount,
        LastRetryAt = s.LastRetryAt,
        NextRetryAt = s.NextRetryAt,
        TriggerSource = s.TriggerSource,
        RoutedToStepOrder = s.RoutedToStepOrder,
        ActionOnEntry = s.WorkflowStepTemplate?.ActionOnEntry.ToString() ?? "None",
        ActionOnComplete = s.WorkflowStepTemplate?.ActionOnComplete.ToString() ?? "None",
        MaxRetries = s.WorkflowStepTemplate?.MaxRetries ?? 0,
        RoutingMode = s.WorkflowStepTemplate?.RoutingMode.ToString() ?? "Sequential",
        CronExpression = s.WorkflowStepTemplate?.CronExpression,
        CronDescription = s.WorkflowStepTemplate?.CronDescription
    };
}
