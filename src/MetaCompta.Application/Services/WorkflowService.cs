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
    private readonly ILogger<WorkflowService> _logger;

    public WorkflowService(MetaComptaDbContext db, INotificationService notifications, ILogger<WorkflowService> logger)
    {
        _db = db;
        _notifications = notifications;
        _logger = logger;
    }

    // ╔══════════════════════════════════════════════════════════════════════╗
    // ║                         TEMPLATES                                   ║
    // ╚══════════════════════════════════════════════════════════════════════╝

    public async Task<ApiResponse<PagedResult<WorkflowTemplateDto>>> GetTemplatesAsync(
        int cabinetId, string? declarationType = null, int? clientId = null, int page = 1, int pageSize = 20)
    {
        var query = _db.WorkflowTemplates
            .Where(wt => wt.CabinetId == cabinetId)
            .Include(wt => wt.Steps)
            .Include(wt => wt.Client)
            .AsQueryable();

        if (!string.IsNullOrEmpty(declarationType) && Enum.TryParse<WorkflowDeclarationType>(declarationType, true, out var dt))
            query = query.Where(wt => wt.DeclarationType == dt);

        if (clientId.HasValue)
            query = query.Where(wt => wt.ClientId == clientId.Value || wt.ClientId == null);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(wt => wt.IsDefault).ThenBy(wt => wt.Name)
            .Skip((page - 1) * pageSize).Take(pageSize)
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

        var template = new WorkflowTemplate
        {
            Name = dto.Name,
            Description = dto.Description,
            DeclarationType = declType,
            ClientId = dto.ClientId,
            IsDefault = dto.IsDefault,
            IsActive = true,
            Version = 1,
            CabinetId = cabinetId,
            CreatedBy = userId
        };

        // If setting as default, unset other defaults for same scope
        if (dto.IsDefault)
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

            _db.WorkflowStepTemplates.Add(new WorkflowStepTemplate
            {
                WorkflowTemplateId = template.Id,
                Name = stepDto.Name,
                Description = stepDto.Description,
                StepOrder = order,
                StepType = stepType,
                AssignmentType = assignType,
                AssignedToUserId = stepDto.AssignedToUserId,
                AssignedToRoleName = stepDto.AssignedToRoleName,
                AssignedToDepartmentId = stepDto.AssignedToDepartmentId,
                IsClientStep = stepDto.IsClientStep,
                RequiredPermission = stepDto.RequiredPermission,
                SLAHours = stepDto.SLAHours,
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

        // System templates (Plan Marocain) cannot be modified
        if (template.IsSystem)
            return ApiResponse<WorkflowTemplateDto>.Fail("Ce modèle système (Plan Marocain) ne peut pas être modifié.");

        // Check if any active instances use this template
        var hasActiveInstances = await _db.WorkflowInstances
            .AnyAsync(wi => wi.WorkflowTemplateId == id && wi.Status == WorkflowInstanceStatus.Active);
        if (hasActiveInstances)
            return ApiResponse<WorkflowTemplateDto>.Fail("Impossible de modifier un modèle avec des instances actives. Créez une nouvelle version.");

        if (!Enum.TryParse<WorkflowDeclarationType>(dto.DeclarationType, true, out var declType))
            declType = WorkflowDeclarationType.All;

        template.Name = dto.Name;
        template.Description = dto.Description;
        template.DeclarationType = declType;
        template.ClientId = dto.ClientId;
        template.IsDefault = dto.IsDefault;
        template.Version++;
        template.UpdatedBy = userId;

        // Remove old steps (soft delete)
        foreach (var oldStep in template.Steps.Where(s => !s.IsDeleted))
        {
            oldStep.IsDeleted = true;
            oldStep.DeletedAt = DateTime.UtcNow;
            oldStep.DeletedBy = userId;
        }

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

            _db.WorkflowStepTemplates.Add(new WorkflowStepTemplate
            {
                WorkflowTemplateId = template.Id,
                Name = stepDto.Name,
                Description = stepDto.Description,
                StepOrder = order,
                StepType = stepType,
                AssignmentType = assignType,
                AssignedToUserId = stepDto.AssignedToUserId,
                AssignedToRoleName = stepDto.AssignedToRoleName,
                AssignedToDepartmentId = stepDto.AssignedToDepartmentId,
                IsClientStep = stepDto.IsClientStep,
                RequiredPermission = stepDto.RequiredPermission,
                SLAHours = stepDto.SLAHours,
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

        // System templates (Plan Marocain) cannot be deleted
        if (template.IsSystem)
            return ApiResponse<bool>.Fail("Ce modèle système (Plan Marocain) ne peut pas être supprimé.");

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
    /// All (Standard), TVA, IS, IR, CNSS, Paie, Juridique, Comptabilite
    /// Each template has optimized steps, SLAs, and assignments for its domain.
    /// </summary>
    public async Task<ApiResponse<List<WorkflowTemplateDto>>> SeedAllDefaultTemplatesAsync(int cabinetId, string userId)
    {
        var createdTemplates = new List<WorkflowTemplateDto>();
        var templateDefinitions = GetAllDefaultTemplateDefinitions();

        foreach (var def in templateDefinitions)
        {
            // Skip if a default template already exists for this type
            var exists = await _db.WorkflowTemplates.AnyAsync(t =>
                t.CabinetId == cabinetId && !t.IsDeleted && t.IsDefault && t.DeclarationType.ToString() == def.DeclarationType);
            if (exists) continue;

            var result = await CreateTemplateAsync(def, cabinetId, userId);
            if (result.Success && result.Data != null)
                createdTemplates.Add(result.Data);
        }

        if (!createdTemplates.Any())
            return ApiResponse<List<WorkflowTemplateDto>>.Ok(createdTemplates, "Tous les modèles par défaut existent déjà.");

        return ApiResponse<List<WorkflowTemplateDto>>.Ok(createdTemplates, $"{createdTemplates.Count} modèle(s) par défaut créé(s) avec succès.");
    }

    /// <summary>
    /// Returns all per-type default template definitions.
    /// </summary>
    private static List<WorkflowTemplateCreateDto> GetAllDefaultTemplateDefinitions()
    {
        return new List<WorkflowTemplateCreateDto>
        {
            // ── TVA (Cycle mensuel/trimestriel) ──────────────────────────
            new()
            {
                Name = "TVA — Déclaration de TVA",
                Description = "Workflow optimisé pour les déclarations de TVA : collecte factures, rapprochement, calcul TVA, validation et télédéclaration.",
                DeclarationType = "TVA", IsDefault = true,
                Steps = new()
                {
                    new() { Name = "Ouverture période TVA", StepOrder = 0, StepType = "Initialisation", AssignmentType = "AutoSystem", IsRequired = true, IsBlocking = true, AutoCreateTask = false, AutoNotify = true, SLAHours = 1, Description = "Création automatique du dossier TVA pour la période", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.Brouillon, TaskType = "Standard" },
                    new() { Name = "Demande factures & pièces", StepOrder = 1, StepType = "DemandePieces", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 24, Description = "Envoi de la demande de factures d'achat/vente au client", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.ACompleter, TaskType = "DocumentRequest", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Collecte & numérisation", StepOrder = 2, StepType = "Collecte", AssignmentType = "Client", IsClientStep = true, IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 96, Description = "Le client dépose les factures et relevés bancaires sur l'extranet", TaskType = "ClientAction", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Saisie & rapprochement TVA", StepOrder = 3, StepType = "Saisie", AssignmentType = "Department", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 36, Description = "Saisie des écritures, ventilation TVA déductible/collectée, rapprochement", DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Completee, TaskType = "DataEntry", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Contrôle TVA", StepOrder = 4, StepType = "Controle", AssignmentType = "Role", AssignedToRoleName = "Manager", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 12, Description = "Vérification des montants, cohérence CA déclaré vs facturé, prorata", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.EnRevision, TaskType = "Review", TaskPriority = RefCodes.PrioriteTache.Haute, RequiredPermission = "workflow.validate" },
                    new() { Name = "Validation chef comptable", StepOrder = 5, StepType = "ValidationInterne", AssignmentType = "Role", AssignedToRoleName = "Manager", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 8, Description = "Validation finale du montant TVA à payer/crédit de TVA", DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Validee, TaskType = "Validation", TaskPriority = RefCodes.PrioriteTache.Urgente, RequiredPermission = "workflow.validate" },
                    new() { Name = "Approbation client", StepOrder = 6, StepType = "ValidationClient", AssignmentType = "Client", IsClientStep = true, IsRequired = false, IsBlocking = false, AutoCreateTask = true, AutoNotify = true, SLAHours = 48, Description = "Le client consulte et approuve la déclaration TVA sur l'extranet", TaskType = "ClientAction", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Télédéclaration DGI", StepOrder = 7, StepType = "DepotSoumission", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 12, Description = "Soumission électronique sur Simpl-TVA / portail DGI", DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Soumise, TaskType = "Submission", TaskPriority = RefCodes.PrioriteTache.Urgente },
                    new() { Name = "Clôture & archivage", StepOrder = 8, StepType = "ClotureArchivage", AssignmentType = "AutoSystem", IsRequired = true, IsBlocking = false, AutoCreateTask = false, AutoNotify = true, SLAHours = 24, Description = "Archivage de l'accusé de réception et classement du dossier", TaskType = "Archiving" }
                }
            },

            // ── IS (Impôt sur les Sociétés — cycle annuel) ───────────────
            new()
            {
                Name = "IS — Impôt sur les Sociétés",
                Description = "Workflow pour la déclaration annuelle d'IS : bilan fiscal, retraitements, calcul de l'impôt, liasse fiscale et dépôt.",
                DeclarationType = "IS", IsDefault = true,
                Steps = new()
                {
                    new() { Name = "Ouverture exercice IS", StepOrder = 0, StepType = "Initialisation", AssignmentType = "AutoSystem", IsRequired = true, IsBlocking = true, AutoCreateTask = false, AutoNotify = true, SLAHours = 1, Description = "Initialisation du dossier IS pour l'exercice fiscal", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.Brouillon, TaskType = "Standard" },
                    new() { Name = "Demande documents annuels", StepOrder = 1, StepType = "DemandePieces", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 72, Description = "Demande de la balance générale, inventaires, PV AG, contrats", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.ACompleter, TaskType = "DocumentRequest", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Collecte pièces justificatives", StepOrder = 2, StepType = "Collecte", AssignmentType = "Client", IsClientStep = true, IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 168, Description = "Le client fournit les documents nécessaires à la clôture annuelle", TaskType = "ClientAction", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Retraitements fiscaux", StepOrder = 3, StepType = "Saisie", AssignmentType = "Department", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 96, Description = "Réintégrations, déductions, amortissements, provisions non déductibles", DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Completee, TaskType = "DataEntry", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Contrôle liasse fiscale", StepOrder = 4, StepType = "Controle", AssignmentType = "Role", AssignedToRoleName = "Manager", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 48, Description = "Vérification de cohérence du résultat fiscal, report déficitaire, cotisation minimale", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.EnRevision, TaskType = "Review", TaskPriority = RefCodes.PrioriteTache.Haute, RequiredPermission = "workflow.validate" },
                    new() { Name = "Validation associé / expert", StepOrder = 5, StepType = "ValidationInterne", AssignmentType = "Role", AssignedToRoleName = "Manager", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 48, Description = "Validation finale par l'expert-comptable ou l'associé responsable", DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Validee, TaskType = "Validation", TaskPriority = RefCodes.PrioriteTache.Urgente, RequiredPermission = "workflow.validate" },
                    new() { Name = "Validation client (bilan)", StepOrder = 6, StepType = "ValidationClient", AssignmentType = "Client", IsClientStep = true, IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 120, Description = "Le client approuve le bilan et la liasse fiscale", TaskType = "ClientAction", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Dépôt DGI & paiement", StepOrder = 7, StepType = "DepotSoumission", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 24, Description = "Télédéclaration IS sur Simpl-IS et génération de l'avis d'imposition", DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Soumise, TaskType = "Submission", TaskPriority = RefCodes.PrioriteTache.Urgente },
                    new() { Name = "Clôture & archivage annuel", StepOrder = 8, StepType = "ClotureArchivage", AssignmentType = "AutoSystem", IsRequired = true, IsBlocking = false, AutoCreateTask = false, AutoNotify = true, SLAHours = 72, Description = "Archivage liasse fiscale, accusé DGI, classement exercice", TaskType = "Archiving" }
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
                    new() { Name = "Ouverture période CNSS", StepOrder = 0, StepType = "Initialisation", AssignmentType = "AutoSystem", IsRequired = true, IsBlocking = true, AutoCreateTask = false, AutoNotify = true, SLAHours = 1, Description = "Initialisation automatique du bordereau CNSS du mois", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.Brouillon, TaskType = "Standard" },
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
            template = await ResolveTemplateAsync(cabinetId, dto.ClientId, dto.DeclarationType);
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

        // 3. Load client config for overrides
        ClientWorkflowConfig? clientConfig = null;
        if (dto.ClientId > 0)
        {
            WorkflowDeclarationType? declTypeEnum = null;
            if (!string.IsNullOrEmpty(dto.DeclarationType) && Enum.TryParse<WorkflowDeclarationType>(dto.DeclarationType, true, out var parsed))
                declTypeEnum = parsed;

            clientConfig = await _db.ClientWorkflowConfigs
                .FirstOrDefaultAsync(c => c.ClientId == dto.ClientId && c.CabinetId == cabinetId && c.IsActive &&
                    (c.DeclarationType == (declTypeEnum ?? WorkflowDeclarationType.All) || c.DeclarationType == WorkflowDeclarationType.All));
        }

        // 4. Create workflow instance
        var instance = new WorkflowInstance
        {
            WorkflowTemplateId = template.Id,
            DeclarationId = dto.DeclarationId,
            ClientId = dto.ClientId,
            CurrentStepOrder = 0,
            Status = WorkflowInstanceStatus.Active,
            StartedByUserId = userId,
            StartedAt = DateTime.UtcNow,
            CabinetId = cabinetId,
            CreatedBy = userId
        };

        _db.WorkflowInstances.Add(instance);
        await _db.SaveChangesAsync();

        // 5. Create all step instances from template
        var activeSteps = template.Steps.Where(s => !s.IsDeleted).OrderBy(s => s.StepOrder).ToList();

        // Skip client validation step if disabled in client config
        if (clientConfig != null && !clientConfig.ClientValidationEnabled)
            activeSteps = activeSteps.Where(s => s.StepType != WorkflowStepType.ValidationClient).ToList();
        if (clientConfig != null && !clientConfig.DocumentRequestEnabled)
            activeSteps = activeSteps.Where(s => s.StepType != WorkflowStepType.DemandePieces).ToList();

        foreach (var stepTemplate in activeSteps)
        {
            var slaHours = stepTemplate.SLAHours;
            if (clientConfig != null && slaHours.HasValue)
                slaHours = (int)(slaHours.Value * clientConfig.SLAMultiplier);

            var stepInstance = new WorkflowStepInstance
            {
                WorkflowInstanceId = instance.Id,
                WorkflowStepTemplateId = stepTemplate.Id,
                StepOrder = stepTemplate.StepOrder,
                StepName = stepTemplate.Name,
                Status = WorkflowStepStatus.Pending,
                DueDate = slaHours.HasValue ? DateTime.UtcNow.AddHours(slaHours.Value) : null,
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
            .Include(wi => wi.StepInstances)
                .ThenInclude(si => si.WorkflowStepTemplate)
            .Include(wi => wi.History.OrderByDescending(h => h.ActionDate).Take(20))
            .FirstOrDefaultAsync();

        if (wi == null)
            return ApiResponse<WorkflowInstanceDto>.Fail("Instance de workflow non trouvée.");

        return ApiResponse<WorkflowInstanceDto>.Ok(MapInstanceDto(wi));
    }

    public async Task<ApiResponse<PagedResult<WorkflowInstanceDto>>> GetInstancesAsync(
        int cabinetId, int? clientId = null, string? status = null, int? declarationId = null, int page = 1, int pageSize = 20)
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

        // Cancel all pending steps
        foreach (var step in instance.StepInstances.Where(s => s.Status == WorkflowStepStatus.Pending || s.Status == WorkflowStepStatus.InProgress))
        {
            step.Status = WorkflowStepStatus.Skipped;
            step.Notes = $"Annulé: {reason}";
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
                .ThenInclude(si => si.WorkflowStepTemplate)
            .Include(wi => wi.Declaration)
            .FirstOrDefaultAsync(wi => wi.Id == instanceId && wi.CabinetId == cabinetId);

        if (instance == null)
            return ApiResponse<WorkflowStepInstanceDto>.Fail("Instance non trouvée.");
        if (instance.Status != WorkflowInstanceStatus.Active)
            return ApiResponse<WorkflowStepInstanceDto>.Fail("Le workflow n'est pas actif.");

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
            currentStep.Status = WorkflowStepStatus.Completed;
            currentStep.CompletedAt = DateTime.UtcNow;
            currentStep.CompletedByUserId = userId;
            currentStep.Notes = dto.Notes;

            // Update declaration status if configured
            if (stepTemplate?.DeclarationStatusOnComplete != null && instance.Declaration != null)
            {
                instance.Declaration.Statut = stepTemplate.DeclarationStatusOnComplete;
                instance.Declaration.UpdatedBy = userId;
            }

            _db.WorkflowHistories.Add(new WorkflowHistory
            {
                WorkflowInstanceId = instanceId,
                WorkflowStepInstanceId = currentStep.Id,
                Action = "StepCompleted",
                PreviousStatus = "InProgress",
                NewStatus = "Completed",
                Comment = dto.Comment ?? dto.Notes,
                UserId = userId,
                UserName = await GetUserNameAsync(userId),
                CabinetId = cabinetId,
                CreatedBy = userId
            });
        }

        await _db.SaveChangesAsync();

        // Find and activate next step
        var nextStep = instance.StepInstances
            .Where(s => s.StepOrder > currentStep.StepOrder && s.Status == WorkflowStepStatus.Pending)
            .OrderBy(s => s.StepOrder)
            .FirstOrDefault();

        if (nextStep != null)
        {
            instance.CurrentStepOrder = nextStep.StepOrder;
            await _db.SaveChangesAsync();
            await ActivateStepAsync(instanceId, nextStep.StepOrder, cabinetId, userId);
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

    public async Task<ApiResponse<bool>> ReassignStepAsync(int stepInstanceId, ReassignStepDto dto, int cabinetId, string userId)
    {
        var step = await _db.WorkflowStepInstances
            .Include(s => s.AssignedToUser)
            .FirstOrDefaultAsync(s => s.Id == stepInstanceId && s.CabinetId == cabinetId);

        if (step == null)
            return ApiResponse<bool>.Fail("Étape non trouvée.");
        if (step.Status == WorkflowStepStatus.Completed || step.Status == WorkflowStepStatus.Skipped)
            return ApiResponse<bool>.Fail("Impossible de réassigner une étape terminée.");

        var oldAssignee = step.AssignedToUserId;
        step.AssignedToUserId = dto.NewAssigneeUserId;

        _db.WorkflowHistories.Add(new WorkflowHistory
        {
            WorkflowInstanceId = step.WorkflowInstanceId,
            WorkflowStepInstanceId = step.Id,
            Action = "StepReassigned",
            Comment = dto.Reason ?? $"Réassigné de {oldAssignee} à {dto.NewAssigneeUserId}",
            UserId = userId,
            UserName = await GetUserNameAsync(userId),
            CabinetId = cabinetId,
            CreatedBy = userId
        });

        await _db.SaveChangesAsync();

        // Notify new assignee
        var userName = await GetUserNameAsync(userId);
        await _notifications.CreateAsync(
            dto.NewAssigneeUserId,
            "Nouvelle tâche assignée",
            $"L'étape '{step.StepName}' vous a été assignée par {userName}.",
            "Info", "Workflow", null, cabinetId);

        return ApiResponse<bool>.Ok(true, "Étape réassignée.");
    }

    public async Task<ApiResponse<WorkflowStepInstanceDto>> GetCurrentStepAsync(int instanceId, int cabinetId)
    {
        var instance = await _db.WorkflowInstances
            .Include(wi => wi.StepInstances.OrderBy(s => s.StepOrder))
                .ThenInclude(si => si.AssignedToUser)
            .Include(wi => wi.StepInstances)
                .ThenInclude(si => si.WorkflowStepTemplate)
            .FirstOrDefaultAsync(wi => wi.Id == instanceId && wi.CabinetId == cabinetId);

        if (instance == null)
            return ApiResponse<WorkflowStepInstanceDto>.Fail("Instance non trouvée.");

        var current = instance.StepInstances
            .FirstOrDefault(s => s.Status == WorkflowStepStatus.InProgress || s.Status == WorkflowStepStatus.WaitingForClient || s.Status == WorkflowStepStatus.WaitingForValidation);

        if (current == null)
            current = instance.StepInstances.FirstOrDefault(s => s.Status == WorkflowStepStatus.Pending);

        if (current == null)
            return ApiResponse<WorkflowStepInstanceDto>.Fail("Aucune étape active.");

        return ApiResponse<WorkflowStepInstanceDto>.Ok(MapStepInstanceDto(current));
    }

    public async Task<ApiResponse<List<WorkflowStepInstanceDto>>> GetMyPendingStepsAsync(int cabinetId, string userId, int? clientId = null)
    {
        var query = _db.WorkflowStepInstances
            .Where(s => s.CabinetId == cabinetId && s.AssignedToUserId == userId &&
                (s.Status == WorkflowStepStatus.InProgress || s.Status == WorkflowStepStatus.Pending || s.Status == WorkflowStepStatus.WaitingForValidation));

        // Filter by client if in Espace Client
        if (clientId.HasValue)
            query = query.Where(s => s.WorkflowInstance != null && s.WorkflowInstance.ClientId == clientId.Value);

        var steps = await query
            .Include(s => s.WorkflowInstance).ThenInclude(wi => wi!.Client)
            .Include(s => s.WorkflowInstance).ThenInclude(wi => wi!.Declaration)
            .Include(s => s.WorkflowInstance).ThenInclude(wi => wi!.WorkflowTemplate)
            .Include(s => s.WorkflowStepTemplate)
            .OrderBy(s => s.DueDate ?? DateTime.MaxValue)
            .Take(50)
            .ToListAsync();

        return ApiResponse<List<WorkflowStepInstanceDto>>.Ok(steps.Select(MapStepInstanceDto).ToList());
    }

    // ╔══════════════════════════════════════════════════════════════════════╗
    // ║                    CLIENT WORKFLOW CONFIG                            ║
    // ╚══════════════════════════════════════════════════════════════════════╝

    public async Task<ApiResponse<List<ClientWorkflowConfigDto>>> GetClientConfigsAsync(int clientId, int cabinetId)
    {
        var configs = await _db.ClientWorkflowConfigs
            .Where(c => c.ClientId == clientId && c.CabinetId == cabinetId)
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

    public async Task<ApiResponse<ClientWorkflowConfigDto>> SaveClientConfigAsync(ClientWorkflowConfigCreateDto dto, int cabinetId, string userId)
    {
        if (!Enum.TryParse<WorkflowDeclarationType>(dto.DeclarationType, true, out var declType))
            declType = WorkflowDeclarationType.All;

        var existing = await _db.ClientWorkflowConfigs
            .FirstOrDefaultAsync(c => c.ClientId == dto.ClientId && c.CabinetId == cabinetId && c.DeclarationType == declType);

        if (existing != null)
        {
            existing.WorkflowTemplateId = dto.WorkflowTemplateId;
            existing.ClientValidationEnabled = dto.ClientValidationEnabled;
            existing.DocumentRequestEnabled = dto.DocumentRequestEnabled;
            existing.SLAMultiplier = dto.SLAMultiplier;
            existing.AutoStartWorkflow = dto.AutoStartWorkflow;
            existing.PrimaryAssigneeUserId = dto.PrimaryAssigneeUserId;
            existing.IsActive = true;
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
                IsActive = true,
                CabinetId = cabinetId,
                CreatedBy = userId
            });
        }

        await _db.SaveChangesAsync();

        var configs = await GetClientConfigsAsync(dto.ClientId, cabinetId);
        var saved = configs.Data?.FirstOrDefault(c => c.DeclarationType == declType.ToString());
        return saved != null
            ? ApiResponse<ClientWorkflowConfigDto>.Ok(saved)
            : ApiResponse<ClientWorkflowConfigDto>.Fail("Erreur lors de la sauvegarde.");
    }

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

        // Priority 3: Global + specific type
        if (declType.HasValue)
        {
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
    private async Task ActivateStepAsync(int instanceId, int stepOrder, int cabinetId, string triggerUserId)
    {
        var stepInstance = await _db.WorkflowStepInstances
            .Include(s => s.WorkflowStepTemplate)
            .Include(s => s.WorkflowInstance).ThenInclude(wi => wi!.Declaration)
            .Include(s => s.WorkflowInstance).ThenInclude(wi => wi!.Client)
            .FirstOrDefaultAsync(s => s.WorkflowInstanceId == instanceId && s.StepOrder == stepOrder);

        if (stepInstance == null) return;

        var template = stepInstance.WorkflowStepTemplate;
        if (template == null) return;

        // Resolve assignment
        string? assigneeUserId = await ResolveAssigneeAsync(template, cabinetId, stepInstance.WorkflowInstance?.ClientId);
        stepInstance.AssignedToUserId = assigneeUserId;
        stepInstance.ActivatedAt = DateTime.UtcNow;
        stepInstance.StartedAt = DateTime.UtcNow;

        // Set status
        if (template.AssignmentType == AssignmentType.AutoSystem)
        {
            // Auto-complete system steps
            stepInstance.Status = WorkflowStepStatus.Completed;
            stepInstance.CompletedAt = DateTime.UtcNow;
            stepInstance.CompletedByUserId = triggerUserId;
        }
        else if (template.IsClientStep)
        {
            stepInstance.Status = WorkflowStepStatus.WaitingForClient;
        }
        else
        {
            stepInstance.Status = WorkflowStepStatus.InProgress;
        }

        // Update declaration status on entry
        if (template.DeclarationStatusOnEntry != null && stepInstance.WorkflowInstance?.Declaration != null)
        {
            stepInstance.WorkflowInstance.Declaration.Statut = template.DeclarationStatusOnEntry;
        }

        _db.WorkflowHistories.Add(new WorkflowHistory
        {
            WorkflowInstanceId = instanceId,
            WorkflowStepInstanceId = stepInstance.Id,
            Action = template.AssignmentType == AssignmentType.AutoSystem ? "StepAutoCompleted" : "StepActivated",
            PreviousStatus = "Pending",
            NewStatus = stepInstance.Status.ToString(),
            UserId = triggerUserId,
            UserName = await GetUserNameAsync(triggerUserId),
            CabinetId = cabinetId,
            CreatedBy = triggerUserId
        });

        // Auto-create task
        if (template.AutoCreateTask && template.AssignmentType != AssignmentType.AutoSystem)
        {
            var clientName = stepInstance.WorkflowInstance?.Client?.RaisonSociale ?? "N/A";
            var task = new TaskItem
            {
                Titre = $"[Workflow] {stepInstance.StepName} — {clientName}",
                Description = template.Description ?? $"Étape workflow: {stepInstance.StepName}",
                ClientId = stepInstance.WorkflowInstance?.ClientId,
                DeclarationId = stepInstance.WorkflowInstance?.DeclarationId,
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

        // Auto-notify
        if (template.AutoNotify && assigneeUserId != null)
        {
            var clientName = stepInstance.WorkflowInstance?.Client?.RaisonSociale ?? "";
            await _notifications.CreateAsync(
                assigneeUserId,
                $"Workflow: {stepInstance.StepName}",
                $"L'étape '{stepInstance.StepName}' pour {clientName} vous est assignée.",
                "Info", "Workflow", null, cabinetId);
        }

        await _db.SaveChangesAsync();

        // If auto-system step completed, advance to next
        if (template.AssignmentType == AssignmentType.AutoSystem)
        {
            var nextStep = await _db.WorkflowStepInstances
                .Where(s => s.WorkflowInstanceId == instanceId && s.StepOrder > stepOrder && s.Status == WorkflowStepStatus.Pending)
                .OrderBy(s => s.StepOrder)
                .FirstOrDefaultAsync();

            if (nextStep != null)
            {
                var instance = await _db.WorkflowInstances.FindAsync(instanceId);
                if (instance != null)
                {
                    instance.CurrentStepOrder = nextStep.StepOrder;
                    await _db.SaveChangesAsync();
                }
                await ActivateStepAsync(instanceId, nextStep.StepOrder, cabinetId, triggerUserId);
            }
            else
            {
                // Workflow complete
                var instance = await _db.WorkflowInstances.FindAsync(instanceId);
                if (instance != null)
                {
                    instance.Status = WorkflowInstanceStatus.Completed;
                    instance.CompletedAt = DateTime.UtcNow;
                    await _db.SaveChangesAsync();
                }
            }
        }
    }

    /// <summary>
    /// Resolves the actual user to assign to a step based on assignment type.
    /// </summary>
    private async Task<string?> ResolveAssigneeAsync(WorkflowStepTemplate template, int cabinetId, int? clientId)
    {
        switch (template.AssignmentType)
        {
            case AssignmentType.Employee:
                return template.AssignedToUserId;

            case AssignmentType.Role:
                if (!string.IsNullOrEmpty(template.AssignedToRoleName))
                {
                    var userInRole = await _db.Users
                        .Where(u => u.CabinetId == cabinetId && u.IsActive)
                        .Join(_db.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { u, ur })
                        .Join(_db.Roles, x => x.ur.RoleId, r => r.Id, (x, r) => new { x.u, RoleName = r.Name })
                        .Where(x => x.RoleName == template.AssignedToRoleName)
                        .Select(x => x.u.Id)
                        .FirstOrDefaultAsync();
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
            AssignedToRoleName = s.AssignedToRoleName,
            AssignedToDepartmentId = s.AssignedToDepartmentId,
            AssignedToDepartmentName = s.AssignedToDepartment?.Name,
            IsClientStep = s.IsClientStep,
            RequiredPermission = s.RequiredPermission,
            SLAHours = s.SLAHours,
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
        CompletedByUserName = s.CompletedByUser != null ? s.CompletedByUser.FirstName + " " + s.CompletedByUser.LastName : null,
        ClientId = s.WorkflowInstance?.ClientId,
        ClientName = s.WorkflowInstance?.Client?.RaisonSociale,
        DeclarationId = s.WorkflowInstance?.DeclarationId,
        DeclarationType = s.WorkflowInstance?.Declaration?.Type ?? s.WorkflowInstance?.WorkflowTemplate?.DeclarationType.ToString(),
        AssignmentType = s.WorkflowStepTemplate?.AssignmentType.ToString(),
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
