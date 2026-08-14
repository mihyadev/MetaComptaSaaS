using MetaCompta.Application.DTOs;
using MetaCompta.BackOffice.Filters;
using MetaCompta.BackOffice.Services;
using MetaCompta.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.BackOffice.Controllers;

/// <summary>
/// BackOffice MVC controller for Workflow Engine management.
/// Covers: Dashboard, Templates, Instances, Steps, Client Config.
/// </summary>
public class WorkflowController : BaseController
{
    private readonly IWorkflowFacade _workflow;
    private readonly IClientFacade _clients;
    private readonly IDepartmentFacade _departments;
    private readonly ICabinetFacade _cabinet;

    public WorkflowController(
        IReferenceDataFacade referenceData,
        IHttpContextAccessor contextAccessor,
        IWorkflowFacade workflow,
        IClientFacade clients,
        IDepartmentFacade departments,
        ICabinetFacade cabinet)
        : base(referenceData, contextAccessor)
    {
        _workflow = workflow;
        _clients = clients;
        _departments = departments;
        _cabinet = cabinet;
    }

    // ═══════════════════════════════════════════════════════════
    // DASHBOARD
    // ═══════════════════════════════════════════════════════════

    [RequirePermission(ClaimDefinitions.WorkflowMonitor)]
    public async Task<IActionResult> Index()
    {
        try
        {
            var dashboard = await _workflow.GetDashboardAsync();
            return View(dashboard ?? new WorkflowDashboardDto());
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Erreur chargement du tableau de bord workflow: {ex.Message}";
            return View(new WorkflowDashboardDto());
        }
    }

    // ═══════════════════════════════════════════════════════════
    // UNIFIED WORKFLOW BUILDER (Create + View + Edit in ONE page)
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Unified Workflow Builder — single page for creating, viewing, editing templates.
    /// Replaces: CreateTemplate, EditTemplate, TemplateDetails, Templates views.
    /// </summary>
    [RequirePermission(ClaimDefinitions.WorkflowView, ClaimDefinitions.WorkflowEdit, ClaimDefinitions.WorkflowCreate)]
    public async Task<IActionResult> Builder(int? id = null)
    {
        await LoadDropdowns();
        await LoadBuilderCabinetTemplatesAsync();

        if (id.HasValue)
        {
            var template = await _workflow.GetTemplateByIdAsync(id.Value);
            if (template == null)
            {
                TempData["Error"] = "Modèle introuvable.";
                return RedirectToAction("Builder");
            }
            // Builder = cabinet base only (client copies are customized on the client page)
            if (template.ClientId.HasValue)
            {
                TempData["Error"] = "Ce modèle est une copie client. Personnalisez-le depuis la fiche client → Workflow.";
                return RedirectToAction("Details", "Clients", new { id = template.ClientId.Value, tab = "workflow" });
            }
            if (template.Name.Contains("(ancien)", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Ce modèle est archivé (ancien). Utilisez le modèle cabinet actif pour ce type.";
                return RedirectToAction("Builder");
            }
            ViewBag.EditTemplate = template;
            ViewBag.EditTemplateId = id.Value;

            var dto = new WorkflowTemplateCreateDto
            {
                Name = template.Name,
                Description = template.Description,
                DeclarationType = template.DeclarationType,
                ClientId = null,
                IsDefault = true,
                Steps = template.Steps?.Select(s => new WorkflowStepTemplateCreateDto
                {
                    Name = s.Name,
                    Description = s.Description,
                    StepOrder = s.StepOrder,
                    StepType = s.StepType,
                    AssignmentType = s.AssignmentType,
                    AssignedToUserId = s.AssignedToUserId,
                    AssignedToUserIds = s.AssignedToUserIdList?.Count > 0
                        ? s.AssignedToUserIdList
                        : (!string.IsNullOrEmpty(s.AssignedToUserIds)
                            ? s.AssignedToUserIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
                            : (!string.IsNullOrEmpty(s.AssignedToUserId) ? new List<string> { s.AssignedToUserId } : null)),
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
                }).ToList() ?? new()
            };
            return View(dto);
        }

        return View(new WorkflowTemplateCreateDto { ClientId = null, IsDefault = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.WorkflowCreate, ClaimDefinitions.WorkflowEdit)]
    public async Task<IActionResult> BuilderSave(int? id, WorkflowTemplateCreateDto dto)
    {
        // Builder = cabinet base only (one per declaration type). Client copies are customized on the client page.
        dto.ClientId = null;
        dto.IsDefault = true;

        if (!ModelState.IsValid)
        {
            await LoadDropdowns();
            await LoadBuilderCabinetTemplatesAsync();
            if (id.HasValue) ViewBag.EditTemplateId = id.Value;
            return View("Builder", dto);
        }

        if (id.HasValue)
        {
            var existing = await _workflow.GetTemplateByIdAsync(id.Value);
            if (existing == null)
            {
                TempData["Error"] = "Modèle introuvable.";
                return RedirectToAction("Builder");
            }
            if (existing.ClientId.HasValue)
            {
                TempData["Error"] = "Ce modèle est une copie client. Personnalisez-le depuis la fiche client → Workflow.";
                return RedirectToAction("Details", "Clients", new { id = existing.ClientId.Value, tab = "workflow" });
            }

            var result = await _workflow.UpdateTemplateAsync(id.Value, dto);
            SetApiResult(result, "Modèle cabinet mis à jour.", "Erreur lors de la mise à jour.");
            return RedirectToAction("Builder", new { id = id.Value });
        }

        // Upsert: if a cabinet base already exists for this declaration type, update it
        var existingForType = await _workflow.GetTemplatesAsync(
            declarationType: dto.DeclarationType, clientId: null, page: 1, pageSize: 50,
            cabinetOnly: true, activeOnly: true, excludeAncien: true);
        var match = existingForType?.Items?.FirstOrDefault();
        if (match != null)
        {
            var updateResult = await _workflow.UpdateTemplateAsync(match.Id, dto);
            SetApiResult(updateResult, "Modèle cabinet mis à jour (un seul workflow par type de déclaration).", "Erreur lors de la mise à jour.");
            return RedirectToAction("Builder", new { id = match.Id });
        }

        var createResult = await _workflow.CreateTemplateAsync(dto);
        if (createResult?.Success == true)
        {
            TempData["Success"] = createResult.Message ?? "Modèle cabinet créé.";
            var newId = createResult.Data?.Id;
            return newId.HasValue
                ? RedirectToAction("Builder", new { id = newId.Value })
                : RedirectToAction("Builder");
        }
        TempData["Error"] = createResult?.Message ?? "Erreur lors de la création.";
        if (createResult?.Errors?.Count > 0)
            TempData["ErrorDetails"] = string.Join("|||", createResult.Errors);
        await LoadDropdowns();
        await LoadBuilderCabinetTemplatesAsync();
        return View("Builder", dto);
    }

    // ═══════════════════════════════════════════════════════════
    // TEMPLATES (kept for backward compatibility)
    // ═══════════════════════════════════════════════════════════

    [RequirePermission(ClaimDefinitions.WorkflowView)]
    public async Task<IActionResult> Templates(string? declarationType = null, int? clientId = null, int page = 1)
    {
        var result = await _workflow.GetTemplatesAsync(declarationType, clientId, page);
        ViewBag.DeclarationType = declarationType;
        ViewBag.ClientId = clientId;
        return View(result);
    }

    [RequirePermission(ClaimDefinitions.WorkflowView)]
    public async Task<IActionResult> TemplateDetails(int id)
    {
        var template = await _workflow.GetTemplateByIdAsync(id);
        if (template == null)
        {
            TempData["Error"] = "Modèle de workflow introuvable.";
            return RedirectToAction("Templates");
        }
        return View(template);
    }

    [HttpGet]
    [RequirePermission(ClaimDefinitions.WorkflowCreate)]
    public async Task<IActionResult> CreateTemplate()
    {
        await LoadDropdowns();
        return View(new WorkflowTemplateCreateDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.WorkflowCreate)]
    public async Task<IActionResult> CreateTemplate(WorkflowTemplateCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdowns();
            return View(dto);
        }
        var result = await _workflow.CreateTemplateAsync(dto);
        if (result?.Success == true)
        {
            TempData["Success"] = result.Message ?? "Modèle créé avec succès.";
            return RedirectToAction("Templates");
        }
        TempData["Error"] = result?.Message ?? "Erreur lors de la création.";
        if (result?.Errors?.Count > 0)
            TempData["ErrorDetails"] = string.Join("|||", result.Errors);
        await LoadDropdowns();
        return View(dto);
    }

    [HttpGet]
    [RequirePermission(ClaimDefinitions.WorkflowEdit)]
    public async Task<IActionResult> EditTemplate(int id)
    {
        var template = await _workflow.GetTemplateByIdAsync(id);
        if (template == null) return NotFound();

        var dto = new WorkflowTemplateCreateDto
        {
            Name = template.Name,
            Description = template.Description,
            DeclarationType = template.DeclarationType,
            ClientId = template.ClientId,
            IsDefault = template.IsDefault,
            Steps = template.Steps?.Select(s => new WorkflowStepTemplateCreateDto
            {
                Name = s.Name,
                Description = s.Description,
                StepOrder = s.StepOrder,
                StepType = s.StepType,
                AssignmentType = s.AssignmentType,
                AssignedToUserId = s.AssignedToUserId,
                AssignedToUserIds = s.AssignedToUserIdList?.Count > 0
                    ? s.AssignedToUserIdList
                    : (!string.IsNullOrEmpty(s.AssignedToUserIds)
                        ? s.AssignedToUserIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
                        : (!string.IsNullOrEmpty(s.AssignedToUserId) ? new List<string> { s.AssignedToUserId } : null)),
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
                DeclarationStatusOnEntry = s.DeclarationStatusOnEntry
            }).ToList() ?? new()
        };

        ViewBag.TemplateId = id;
        await LoadDropdowns();
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.WorkflowEdit)]
    public async Task<IActionResult> EditTemplate(int id, WorkflowTemplateCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.TemplateId = id;
            await LoadDropdowns();
            return View(dto);
        }
        var result = await _workflow.UpdateTemplateAsync(id, dto);
        if (result?.Success == true)
        {
            TempData["Success"] = result.Message ?? "Modèle mis à jour.";
            return RedirectToAction("TemplateDetails", new { id });
        }
        TempData["Error"] = result?.Message ?? "Erreur lors de la mise à jour.";
        if (result?.Errors?.Count > 0)
            TempData["ErrorDetails"] = string.Join("|||", result.Errors);
        ViewBag.TemplateId = id;
        await LoadDropdowns();
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.WorkflowDelete)]
    public async Task<IActionResult> DeleteTemplate(int id)
    {
        var result = await _workflow.DeleteTemplateAsync(id);
        SetApiResult(result, "Modèle supprimé.", "Erreur lors de la suppression.");
        return RedirectToAction("Templates");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.WorkflowCreate)]
    public async Task<IActionResult> CreateDefaultTemplate()
    {
        var result = await _workflow.CreateDefaultTemplateAsync();
        SetApiResult(result, "Modèle par défaut créé.", "Erreur lors de la création du modèle par défaut.");
        return RedirectToAction("Templates");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.WorkflowCreate)]
    public async Task<IActionResult> SeedAllDefaultTemplates(bool forceReset = false)
    {
        var result = await _workflow.SeedAllDefaultTemplatesAsync(forceReset);
        SetApiResult(result,
            forceReset
                ? "Workflows cabinet réinitialisés : 1 modèle propre par type de déclaration."
                : "Modèles par défaut créés.",
            "Erreur lors de la création des modèles par défaut.");
        return RedirectToAction("Builder");
    }

    // ═══════════════════════════════════════════════════════════
    // INSTANCES (Workflow executions)
    // ═══════════════════════════════════════════════════════════

    [RequirePermission(ClaimDefinitions.WorkflowMonitor)]
    public async Task<IActionResult> Instances(int? clientId = null, string? status = null, int? declarationId = null, int page = 1)
    {
        var result = await _workflow.GetInstancesAsync(clientId, status, declarationId, page, SelectedExerciceId);
        ViewBag.ClientId = clientId;
        ViewBag.Status = status;
        ViewBag.DeclarationId = declarationId;
        return View(result);
    }

    [RequirePermission(ClaimDefinitions.WorkflowView, ClaimDefinitions.WorkflowMonitor)]
    public async Task<IActionResult> InstanceDetails(int id)
    {
        var instance = await _workflow.GetInstanceByIdAsync(id);
        if (instance == null)
        {
            TempData["Error"] = "Instance de workflow introuvable.";
            // Avoid redirect to Instances (needs workflow.monitor) for roles that only have workflow.view
            return RedirectToAction("MyTasks");
        }
        var history = await _workflow.GetHistoryAsync(id);
        ViewBag.History = history;
        try
        {
            var employees = await _cabinet.GetEmployeesAsync(page: 1, pageSize: 200);
            ViewBag.Employees = employees?.Items?.Where(e => e.EstActif).ToList() ?? new List<CabinetEmployeeDto>();
        }
        catch
        {
            ViewBag.Employees = new List<CabinetEmployeeDto>();
        }
        return View(instance);
    }

    [HttpGet]
    [RequirePermission(ClaimDefinitions.WorkflowCreate)]
    public async Task<IActionResult> StartWorkflow()
    {
        var clients = await _clients.GetAllForDropdownAsync();
        var templates = await _workflow.GetTemplatesAsync(page: 1);
        ViewBag.Clients = clients;
        ViewBag.Templates = templates?.Items ?? new List<WorkflowTemplateDto>();
        return View(new StartWorkflowDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.WorkflowCreate)]
    public async Task<IActionResult> StartWorkflow(StartWorkflowDto dto)
    {
        if (!ModelState.IsValid)
        {
            var clients = await _clients.GetAllForDropdownAsync();
            var templates = await _workflow.GetTemplatesAsync(page: 1);
            ViewBag.Clients = clients;
            ViewBag.Templates = templates?.Items ?? new List<WorkflowTemplateDto>();
            return View(dto);
        }
        var result = await _workflow.StartWorkflowAsync(dto);
        if (result?.Success == true)
        {
            TempData["Success"] = result.Message ?? "Workflow démarré avec succès.";
            if (dto.ClientId > 0)
                return RedirectToAction("Details", "Clients", new { id = dto.ClientId, tab = "workflow" });
            return RedirectToAction("InstanceDetails", new { id = result.Data?.Id });
        }
        TempData["Error"] = result?.Message ?? "Erreur lors du démarrage.";
        if (result?.Errors?.Count > 0)
            TempData["ErrorDetails"] = string.Join("|||", result.Errors);
        if (dto.ClientId > 0)
            return RedirectToAction("Details", "Clients", new { id = dto.ClientId, tab = "workflow" });
        return RedirectToAction("Instances");
    }

    /// <summary>Advance step — authenticated user; service enforces assignee-only.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdvanceStep(int instanceId, string? notes = null, string? comment = null)
    {
        var dto = new AdvanceStepDto { Notes = notes, Comment = comment };
        var result = await _workflow.AdvanceStepAsync(instanceId, dto);
        SetApiResult(result, "Étape avancée.", "Erreur lors de l'avancement.");
        return RedirectToAction("InstanceDetails", new { id = instanceId });
    }

    /// <summary>Refuse current step and reopen previous (assignee-only).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectStep(int instanceId, string? reason = null, string? comment = null)
    {
        var dto = new RejectStepDto { Reason = reason, Comment = comment };
        var result = await _workflow.RejectStepAsync(instanceId, dto);
        SetApiResult(result, result?.Message ?? "Étape refusée — retour à la précédente.", "Erreur lors du refus.");
        return RedirectToAction("InstanceDetails", new { id = instanceId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.WorkflowCancel)]
    public async Task<IActionResult> CancelWorkflow(int instanceId, string reason = "")
    {
        var result = await _workflow.CancelWorkflowAsync(instanceId, reason);
        SetApiResult(result, null, "Erreur lors de l'annulation.");
        return RedirectToAction("InstanceDetails", new { id = instanceId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.WorkflowCancel)]
    public async Task<IActionResult> PauseWorkflow(int instanceId)
    {
        var result = await _workflow.PauseWorkflowAsync(instanceId);
        SetApiResult(result, null, "Erreur lors de la mise en pause.");
        return RedirectToAction("InstanceDetails", new { id = instanceId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.WorkflowAdvance)]
    public async Task<IActionResult> ResumeWorkflow(int instanceId)
    {
        var result = await _workflow.ResumeWorkflowAsync(instanceId);
        SetApiResult(result, null, "Erreur lors de la reprise.");
        return RedirectToAction("InstanceDetails", new { id = instanceId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.WorkflowReassign, ClaimDefinitions.TaskAssign)]
    public async Task<IActionResult> ReassignStep(int stepInstanceId, string newAssigneeUserId, string? reason = null, int? returnInstanceId = null, int? returnTaskId = null)
    {
        var dto = new ReassignStepDto { NewAssigneeUserId = newAssigneeUserId, Reason = reason };
        var result = await _workflow.ReassignStepAsync(stepInstanceId, dto);
        SetApiResult(result, result?.Message, "Erreur lors de la réassignation.");
        if (returnTaskId.HasValue)
            return RedirectToAction("Details", "Tasks", new { id = returnTaskId.Value });
        if (returnInstanceId.HasValue)
            return RedirectToAction("InstanceDetails", new { id = returnInstanceId.Value });
        return RedirectToAction("MyTasks");
    }

    // ═══════════════════════════════════════════════════════════
    // MY TASKS (Pending Steps)
    // ═══════════════════════════════════════════════════════════

    [RequirePermission(ClaimDefinitions.WorkflowView)]
    public async Task<IActionResult> MyTasks()
    {
        var steps = await _workflow.GetMyPendingStepsAsync(SelectedClientId);
        ViewBag.IsClientSpace = SelectedClientId.HasValue;
        ViewBag.ClientName = SelectedClientName;
        return View(steps);
    }

    // ═══════════════════════════════════════════════════════════
    // CLIENT CONFIG
    // ═══════════════════════════════════════════════════════════

    [RequirePermission(ClaimDefinitions.WorkflowConfigure)]
    public IActionResult ClientConfig(int clientId)
    {
        // Affectation lives on Clients/Details Workflow tab
        return RedirectToAction("Details", "Clients", new { id = clientId, tab = "workflow" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.WorkflowConfigure)]
    public async Task<IActionResult> SaveClientConfig(ClientWorkflowConfigCreateDto dto)
    {
        var result = await _workflow.SaveClientConfigAsync(dto);
        SetApiResult(result, "Configuration sauvegardée.", "Erreur lors de la sauvegarde de la configuration.");
        return RedirectToAction("Details", "Clients", new { id = dto.ClientId, tab = "workflow" });
    }

    // ── Helpers ──────────────────────────────────────────────

    [HttpGet]
    [RequirePermission(ClaimDefinitions.WorkflowView)]
    public async Task<IActionResult> GetInstanceJson(int id)
    {
        var instance = await _workflow.GetInstanceByIdAsync(id);
        return Json(new { found = instance != null, data = instance });
    }

    private async Task LoadDropdowns()
    {
        ViewBag.Clients = await _clients.GetAllForDropdownAsync();
        ViewBag.Departments = await _departments.GetAllAsync();
        try
        {
            var employees = await _cabinet.GetEmployeesAsync(page: 1, pageSize: 200);
            ViewBag.Employees = employees?.Items?.Where(e => e.EstActif).ToList() ?? new List<CabinetEmployeeDto>();
        }
        catch
        {
            ViewBag.Employees = new List<CabinetEmployeeDto>();
        }
    }

    /// <summary>
    /// Builder sidebar: one active cabinet template per declaration type (no client copies, no anciens).
    /// </summary>
    private async Task LoadBuilderCabinetTemplatesAsync()
    {
        try { await _workflow.SeedAllDefaultTemplatesAsync(); } catch { /* non-blocking */ }

        var all = await _workflow.GetTemplatesAsync(
            declarationType: null, clientId: null, page: 1, pageSize: 100,
            cabinetOnly: true, activeOnly: true, excludeAncien: true);

        var items = (all?.Items ?? new List<WorkflowTemplateDto>())
            .GroupBy(t => t.DeclarationType)
            .Select(g => g.OrderByDescending(t => t.IsDefault).ThenBy(t => t.Id).First())
            .OrderBy(t => t.DeclarationType)
            .ToList();

        ViewBag.AllTemplates = items;
    }
}
