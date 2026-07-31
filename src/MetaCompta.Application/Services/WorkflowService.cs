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
    /// Refresh TVA default template: deactivate old default (keep in-flight instances), create new aligned template.
    /// </summary>
    public async Task<ApiResponse<WorkflowTemplateDto>> RefreshTvaDefaultTemplateAsync(int cabinetId, string userId)
    {
        var tvaDef = GetAllDefaultTemplateDefinitions()
            .First(d => d.DeclarationType == "TVA");

        var oldDefaults = await _db.WorkflowTemplates
            .Include(t => t.Steps)
            .Where(t => t.CabinetId == cabinetId && !t.IsDeleted
                && t.DeclarationType == WorkflowDeclarationType.TVA && t.IsDefault)
            .ToListAsync();

        var expectedSteps = tvaDef.Steps.Count;
        var alreadyAligned = oldDefaults.Any(t => t.Steps.Count(s => !s.IsDeleted) >= expectedSteps
            && t.Steps.Any(s => !s.IsDeleted && s.StepType == WorkflowStepType.Relance));

        if (alreadyAligned)
            return ApiResponse<WorkflowTemplateDto>.Ok(MapTemplateDto(oldDefaults.First(t => t.IsDefault)), "Modèle TVA déjà aligné.");

        foreach (var old in oldDefaults)
        {
            old.IsDefault = false;
            // Keep Active if in-flight instances exist so they continue; otherwise deactivate
            var hasActive = await _db.WorkflowInstances.AnyAsync(wi =>
                wi.WorkflowTemplateId == old.Id && wi.Status == WorkflowInstanceStatus.Active);
            if (!hasActive)
                old.IsActive = false;
            old.Name = old.Name.Contains("(ancien)") ? old.Name : $"{old.Name} (ancien)";
            old.UpdatedBy = userId;
        }

        await _db.SaveChangesAsync();

        tvaDef.Name = "TVA — Déclaration de TVA";
        var created = await CreateTemplateAsync(tvaDef, cabinetId, userId);
        if (!created.Success || created.Data == null)
            return ApiResponse<WorkflowTemplateDto>.Fail(created.Message ?? "Échec refresh modèle TVA.");

        _logger.LogInformation("TVA template refreshed for cabinet {CabinetId}: new template {Id}", cabinetId, created.Data.Id);
        return ApiResponse<WorkflowTemplateDto>.Ok(created.Data, $"Modèle TVA rafraîchi (cabinet {cabinetId}). Anciens modèles désactivés comme défaut.");
    }

    /// <summary>
    /// Returns all per-type default template definitions.
    /// </summary>
    private static List<WorkflowTemplateCreateDto> GetAllDefaultTemplateDefinitions()
    {
        return new List<WorkflowTemplateCreateDto>
        {
            // ── TVA (Cycle mensuel/trimestriel — end-to-end aligned) ─────
            new()
            {
                Name = "TVA — Déclaration de TVA",
                Description = "Workflow TVA bout-en-bout : création, demande/relance documents, contrôle, calcul, revue, SIMPL-TVA (stub), paiement, rapprochement et clôture.",
                DeclarationType = "TVA", IsDefault = true,
                Steps = new()
                {
                    new() { Name = "Ouverture / création dossier", StepOrder = 0, StepType = "Initialisation", AssignmentType = "AutoSystem", IsRequired = true, IsBlocking = true, AutoCreateTask = false, AutoNotify = true, SLAHours = 1, Description = "Création automatique du dossier TVA pour la période", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.CreeeParSysteme, ActionOnEntry = "CreateDeclaration", TaskType = "Standard" },
                    new() { Name = "Demande documents", StepOrder = 1, StepType = "DemandePieces", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 24, Description = "Envoi de la demande de factures d'achat/vente au client", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.DocumentsDemandes, ActionOnEntry = "RequestDocuments", TaskType = "DocumentRequest", TaskPriority = RefCodes.PrioriteTache.Haute, ValidationRulesJson = "{\"requireDocuments\":false,\"minDocuments\":0}" },
                    new() { Name = "Relance documents", StepOrder = 2, StepType = "Relance", AssignmentType = "AutoSystem", IsRequired = false, IsBlocking = false, AutoCreateTask = false, AutoNotify = true, SLAHours = 72, Description = "Relances automatiques tant que les documents manquent", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.DocumentsDemandes, ActionOnEntry = "RelanceClient", MaxRetries = 3, RetryIntervalHours = 48, OnMaxRetriesAction = "EscalateToManager", TaskType = "Standard" },
                    new() { Name = "Réception documents (client)", StepOrder = 3, StepType = "Collecte", AssignmentType = "Client", IsClientStep = true, IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 96, Description = "Le client dépose les factures et relevés bancaires sur l'extranet", DeclarationStatusOnComplete = RefCodes.StatutDeclaration.DocumentsRecus, TaskType = "ClientAction", TaskPriority = RefCodes.PrioriteTache.Haute, ValidationRulesJson = "{\"requireDocuments\":true,\"minDocuments\":1}" },
                    new() { Name = "Contrôle exhaustivité", StepOrder = 4, StepType = "Controle", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 24, Description = "Vérification exhaustivité pièces; branche EnAttenteCorrection si incomplet", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.ControleEnCours, TaskType = "Review", TaskPriority = RefCodes.PrioriteTache.Haute, ValidationRulesJson = "{\"requireDocuments\":true,\"minDocuments\":1}" },
                    new() { Name = "Import & calcul TVA", StepOrder = 5, StepType = "Saisie", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 36, Description = "Import factures, ventilation et calcul TVA (réutilise CalculateTVA)", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.CalculTVA, DeclarationStatusOnComplete = RefCodes.StatutDeclaration.CalculTVA, TaskType = "DataEntry", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Traitement anomalies", StepOrder = 6, StepType = "Controle", AssignmentType = "Employee", IsRequired = false, IsBlocking = false, AutoCreateTask = true, AutoNotify = true, SLAHours = 24, Description = "Correction des anomalies (placeholder jusqu'au moteur d'anomalies)", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.EnAttenteCorrection, TaskType = "Review", TaskPriority = RefCodes.PrioriteTache.Moyenne },
                    new() { Name = "Revue superviseur", StepOrder = 7, StepType = "ValidationInterne", AssignmentType = "Role", AssignedToRoleName = "Manager", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 12, Description = "Approuver → Validée ; Refuser → À corriger", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.EnRevueSuperviseur, DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Validee, TaskType = "Validation", TaskPriority = RefCodes.PrioriteTache.Urgente, RequiredPermission = "workflow.validate" },
                    new() { Name = "Soumission SIMPL-TVA", StepOrder = 8, StepType = "DepotSoumission", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 12, Description = "Stub SIMPL-TVA : XML local + accusé ACK placeholder", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.EnvoyeeSimplTva, DeclarationStatusOnComplete = RefCodes.StatutDeclaration.EnvoyeeSimplTva, ActionOnComplete = "ExportXML", TaskType = "Submission", TaskPriority = RefCodes.PrioriteTache.Urgente },
                    new() { Name = "Dépôt confirmé", StepOrder = 9, StepType = "DepotSoumission", AssignmentType = "AutoSystem", IsRequired = true, IsBlocking = true, AutoCreateTask = false, AutoNotify = true, SLAHours = 1, Description = "Confirmation dépôt après ACK stub", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.Deposee, DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Deposee, TaskType = "Standard" },
                    new() { Name = "Paiement (si TVA due)", StepOrder = 10, StepType = "Paiement", AssignmentType = "Employee", IsRequired = false, IsBlocking = false, AutoCreateTask = true, AutoNotify = true, SLAHours = 48, Description = "Stub paiement : confirmer → Payée (ignorer si crédit de TVA)", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.APayer, DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Payee, ActionOnEntry = "PaymentReminder", TaskType = "Standard", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Rapprochement", StepOrder = 11, StepType = "Rapprochement", AssignmentType = "Employee", IsRequired = true, IsBlocking = true, AutoCreateTask = true, AutoNotify = true, SLAHours = 24, Description = "Rapprochement comptable (stub lien compta)", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.Rapprochee, DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Rapprochee, TaskType = "Review", TaskPriority = RefCodes.PrioriteTache.Haute },
                    new() { Name = "Archivage & clôture", StepOrder = 12, StepType = "ClotureArchivage", AssignmentType = "AutoSystem", IsRequired = true, IsBlocking = false, AutoCreateTask = false, AutoNotify = true, SLAHours = 24, Description = "Archivage puis clôture du dossier", DeclarationStatusOnEntry = RefCodes.StatutDeclaration.Archivee, DeclarationStatusOnComplete = RefCodes.StatutDeclaration.Cloturee, ActionOnComplete = "ArchiveDeclaration", TaskType = "Archiving" }
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

        var dto = MapInstanceDto(wi);
        await EnrichStepsWithOpenTakesAsync(dto.StepInstances, cabinetId);
        return ApiResponse<WorkflowInstanceDto>.Ok(dto);
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

            // Stop further relances once documents are received / collecte done
            if (stepTemplate?.StepType == WorkflowStepType.Collecte
                || stepTemplate?.DeclarationStatusOnComplete == RefCodes.StatutDeclaration.DocumentsRecus)
            {
                foreach (var relance in instance.StepInstances.Where(s =>
                    s.WorkflowStepTemplate?.ActionOnEntry == WorkflowActionType.RelanceClient && s.NextRetryAt != null))
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
    /// Explicitly claim the current active step (exclusive soft lock).
    /// Fails if another matching-role user already holds it, unless Manager/Gérant forceOverride.
    /// </summary>
    public async Task<ApiResponse<WorkflowStepInstanceDto>> ClaimStepAsync(
        int instanceId, int cabinetId, string userId, bool forceOverride = false, string? reason = null)
    {
        var instance = await _db.WorkflowInstances
            .Include(wi => wi.StepInstances)
                .ThenInclude(si => si.WorkflowStepTemplate)
            .Include(wi => wi.StepInstances)
                .ThenInclude(si => si.AssignedToUser)
            .FirstOrDefaultAsync(wi => wi.Id == instanceId && wi.CabinetId == cabinetId && !wi.IsDeleted);

        if (instance == null)
            return ApiResponse<WorkflowStepInstanceDto>.Fail("Instance workflow introuvable.");
        if (instance.Status != WorkflowInstanceStatus.Active)
            return ApiResponse<WorkflowStepInstanceDto>.Fail("Le workflow n'est pas actif.");

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

        var dto = MapStepInstanceDto(current);
        await EnrichStepsWithOpenTakesAsync(new List<WorkflowStepInstanceDto> { dto }, cabinetId);
        return ApiResponse<WorkflowStepInstanceDto>.Ok(dto);
    }

    public async Task<ApiResponse<List<WorkflowStepInstanceDto>>> GetMyPendingStepsAsync(int cabinetId, string userId, int? clientId = null)
    {
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
                        (s.Status == WorkflowStepStatus.WaitingForClient
                            && s.WorkflowStepTemplate != null
                            && (s.WorkflowStepTemplate.IsClientStep
                                || s.WorkflowStepTemplate.AssignmentType == AssignmentType.Client))
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

        if (template.ActionOnEntry != WorkflowActionType.None)
            await ExecuteStepActionAsync(template.ActionOnEntry, instance, stepInstance, cabinetId, triggerUserId);

        if (instance.DeclarationId.HasValue && instance.Declaration == null)
            instance.Declaration = await _db.Declarations.FindAsync(instance.DeclarationId.Value);

        // Entry status first; AutoSystem OnComplete may override afterwards (e.g. Archivee → Cloturee)
        if (template.DeclarationStatusOnEntry != null && instance.Declaration != null)
            instance.Declaration.Statut = template.DeclarationStatusOnEntry;

        var isRelanceSchedule = template.AssignmentType == AssignmentType.AutoSystem
            && template.ActionOnEntry == WorkflowActionType.RelanceClient
            && template.MaxRetries > 0;

        if (isRelanceSchedule)
        {
            // Schedule retries then auto-complete so Collecte (client) can start immediately
            stepInstance.RetryCount = Math.Max(stepInstance.RetryCount, 1);
            stepInstance.LastRetryAt = DateTime.UtcNow;
            stepInstance.NextRetryAt = DateTime.UtcNow.AddHours(template.RetryIntervalHours ?? 48);
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
        else if (template.IsClientStep)
        {
            stepInstance.Status = WorkflowStepStatus.WaitingForClient;
        }
        else
        {
            stepInstance.Status = WorkflowStepStatus.InProgress;
        }

        _db.WorkflowHistories.Add(new WorkflowHistory
        {
            WorkflowInstanceId = instanceId,
            WorkflowStepInstanceId = stepInstance.Id,
            Action = isRelanceSchedule ? "RelanceScheduled"
                : template.AssignmentType == AssignmentType.AutoSystem ? "StepAutoCompleted" : "StepActivated",
            PreviousStatus = "Pending",
            NewStatus = stepInstance.Status.ToString(),
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

    /// <summary>Process due Relance steps (Hangfire). Returns count processed.</summary>
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
                && s.WorkflowStepTemplate.ActionOnEntry == WorkflowActionType.RelanceClient
                && s.WorkflowInstance != null
                && s.WorkflowInstance.Status == WorkflowInstanceStatus.Active)
            .ToListAsync();

        // Only while client document step still open
        due = due.Where(s => s.WorkflowInstance!.StepInstances.Any(si =>
            si.Status == WorkflowStepStatus.WaitingForClient
            || (si.WorkflowStepTemplate?.StepType == WorkflowStepType.Collecte
                && si.Status is WorkflowStepStatus.WaitingForClient or WorkflowStepStatus.InProgress or WorkflowStepStatus.Pending))).ToList();

        var processed = 0;
        foreach (var step in due)
        {
            var template = step.WorkflowStepTemplate!;
            var instance = step.WorkflowInstance!;
            try
            {
                step.RetryCount++;
                step.LastRetryAt = now;
                await ExecuteStepActionAsync(WorkflowActionType.RelanceClient, instance, step, step.CabinetId, "SYSTEM");

                if (template.MaxRetries > 0 && step.RetryCount >= template.MaxRetries)
                {
                    var action = (template.OnMaxRetriesAction ?? "EscalateToManager").Trim();
                    if (string.Equals(action, "Suspend", StringComparison.OrdinalIgnoreCase))
                    {
                        instance.Status = WorkflowInstanceStatus.Paused;
                        step.NextRetryAt = null;
                        _db.WorkflowHistories.Add(new WorkflowHistory
                        {
                            WorkflowInstanceId = instance.Id,
                            WorkflowStepInstanceId = step.Id,
                            Action = "RelanceMaxRetriesSuspend",
                            Comment = $"Max relances ({template.MaxRetries}) atteint — workflow suspendu.",
                            UserId = "SYSTEM",
                            CabinetId = step.CabinetId,
                            CreatedBy = "SYSTEM"
                        });
                    }
                    else if (string.Equals(action, "NextStep", StringComparison.OrdinalIgnoreCase)
                          || string.Equals(action, "SkipStep", StringComparison.OrdinalIgnoreCase))
                    {
                        step.NextRetryAt = null;
                        await AdvanceStepAsync(instance.Id, new AdvanceStepDto
                        {
                            Notes = "Avance auto après max relances",
                            ForceSkip = string.Equals(action, "SkipStep", StringComparison.OrdinalIgnoreCase)
                        }, step.CabinetId, "SYSTEM");
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
                                "Relance TVA — escalade",
                                $"Max relances atteint pour {instance.Client?.RaisonSociale ?? "client"} (WF #{instance.Id}).",
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
                    step.NextRetryAt = now.AddHours(template.RetryIntervalHours ?? 48);
                    _db.WorkflowHistories.Add(new WorkflowHistory
                    {
                        WorkflowInstanceId = instance.Id,
                        WorkflowStepInstanceId = step.Id,
                        Action = "RelanceExecuted",
                        Comment = $"Relance #{step.RetryCount} (prochaine: {step.NextRetryAt:u})",
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
                var clientName = instance.Client?.RaisonSociale ?? $"Client #{instance.ClientId}";
                var title = action switch
                {
                    WorkflowActionType.RequestDocuments => "Demande de documents",
                    WorkflowActionType.RelanceClient => "Relance documents",
                    WorkflowActionType.PaymentReminder => "Rappel de paiement TVA",
                    _ => "Notification client"
                };
                var body = action switch
                {
                    WorkflowActionType.RequestDocuments =>
                        $"Merci de déposer les pièces pour la déclaration {(instance.Declaration?.Type ?? "TVA")} {instance.Declaration?.Periode} sur l'extranet.",
                    WorkflowActionType.RelanceClient =>
                        $"Rappel : documents manquants pour {instance.Declaration?.Type} {instance.Declaration?.Periode} (tentative {step.RetryCount}).",
                    WorkflowActionType.PaymentReminder =>
                        $"TVA due à régler pour {instance.Declaration?.Periode}. Confirmez le paiement (stub).",
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

            case WorkflowActionType.ArchiveDeclaration:
                if (instance.Declaration != null)
                {
                    instance.Declaration.Statut = RefCodes.StatutDeclaration.Archivee;
                    instance.Declaration.UpdatedBy = userId;
                }
                break;

            case WorkflowActionType.ExportXML:
                _db.WorkflowHistories.Add(new WorkflowHistory
                {
                    WorkflowInstanceId = instance.Id,
                    WorkflowStepInstanceId = step.Id,
                    Action = "SimplTvaStubExport",
                    Comment = $"XML/ACK stub généré pour déclaration #{instance.DeclarationId} (pas d'appel DGI réel).",
                    UserId = userId,
                    CabinetId = cabinetId,
                    CreatedBy = userId
                });
                break;

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
                    instance.Declaration.Statut = RefCodes.StatutDeclaration.EnvoyeeSimplTva;
                    instance.Declaration.DateSoumission ??= DateTime.UtcNow;
                }
                break;
        }
    }

    private async Task EnsureDeclarationLinkedAsync(WorkflowInstance instance, int cabinetId, string userId)
    {
        if (instance.DeclarationId.HasValue) return;
        if (instance.ClientId <= 0) return;

        var declType = instance.WorkflowTemplate?.DeclarationType.ToString()
            ?? (await _db.WorkflowTemplates.Where(t => t.Id == instance.WorkflowTemplateId).Select(t => t.DeclarationType.ToString()).FirstOrDefaultAsync())
            ?? "TVA";

        if (!string.Equals(declType, "TVA", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("CreateDeclaration skipped for non-TVA type {Type} on WF {Id}", declType, instance.Id);
            return;
        }

        var existing = await _db.Declarations
            .Where(d => d.CabinetId == cabinetId && d.ClientId == instance.ClientId && d.Type == "TVA"
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

        var created = await _declarations.CreateTVAAsync(instance.ClientId, cabinetId, userId);
        if (created.Success && created.Data != null)
        {
            instance.DeclarationId = created.Data.Id;
            instance.Declaration = await _db.Declarations.FindAsync(created.Data.Id);
            _logger.LogInformation("Created TVA declaration {DeclId} for workflow {WfId}", created.Data.Id, instance.Id);
        }
        else
        {
            _logger.LogWarning("CreateDeclaration failed for WF {Id}: {Msg}", instance.Id, created.Message);
        }
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
            if (!require) return null;

            var count = await _db.Documents.CountAsync(d =>
                d.CabinetId == cabinetId && d.DeclarationId == declarationId && !d.IsDeleted);
            if (count < minDocs)
                return $"Documents insuffisants ({count}/{minDocs}). Déposez les pièces ou utilisez « Passer » (ForceSkip) si autorisé.";
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
