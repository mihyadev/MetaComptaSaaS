using MetaCompta.Application.DTOs;
using MetaCompta.BackOffice.Filters;
using MetaCompta.BackOffice.Services;
using MetaCompta.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.BackOffice.Controllers;

/// <summary>
/// Controller for Tasks — both client-scoped and global task management.
/// </summary>
public class TasksController : BaseController
{
    private readonly ITaskFacade _tasks;
    private readonly IClientFacade _clients;
    private readonly IWorkflowFacade _workflow;
    private readonly IDeclarationFacade _declarations;
    private readonly ICabinetFacade _cabinet;

    public TasksController(
        IReferenceDataFacade referenceData,
        IHttpContextAccessor contextAccessor,
        ITaskFacade tasks,
        IClientFacade clients,
        IWorkflowFacade workflow,
        IDeclarationFacade declarations,
        ICabinetFacade cabinet)
        : base(referenceData, contextAccessor)
    {
        _tasks = tasks;
        _clients = clients;
        _workflow = workflow;
        _declarations = declarations;
        _cabinet = cabinet;
    }

    [RequirePermission(ClaimDefinitions.TaskDetail)]
    public async Task<IActionResult> Index(string? statut = null, int? clientId = null, int page = 1)
    {
        // Espace Client: only tasks of this dossier that are assigned to the client (extranet), not employees
        string? assignedToKind = null;
        if (SelectedClientId.HasValue)
        {
            clientId = SelectedClientId;
            assignedToKind = "Client";
        }

        var result = await _tasks.GetAllAsync(statut: statut, clientId: clientId, page: page, assignedToKind: assignedToKind);
        ViewBag.Clients = await LoadTaskClientFilterAsync();
        ViewBag.Statut = statut;
        ViewBag.ClientId = clientId;
        ViewBag.IsClientSpace = SelectedClientId.HasValue;
        ViewBag.SelectedClientName = SelectedClientName;
        ViewBag.AssignedToKind = assignedToKind;
        await LoadReferenceDataToViewBagAsync("STATUT_TACHE", "PRIORITE_TACHE");
        return View(result);
    }

    [RequirePermission(ClaimDefinitions.TaskDetail)]
    public async Task<IActionResult> MyTasks(string? statut = null, int? clientId = null, int page = 1)
    {
        if (SelectedClientId.HasValue)
            clientId = SelectedClientId;

        var result = await _tasks.GetAllAsync(
            statut: statut,
            assignedTo: "me",
            clientId: clientId,
            page: page);
        ViewBag.Clients = await LoadTaskClientFilterAsync();
        ViewBag.Statut = statut;
        ViewBag.IsClientSpace = SelectedClientId.HasValue;
        ViewBag.SelectedClientName = SelectedClientName;
        ViewBag.ClientId = clientId;
        ViewBag.IsMyTasks = true;
        await LoadReferenceDataToViewBagAsync("STATUT_TACHE", "PRIORITE_TACHE");
        return View("Index", result);
    }

    [RequirePermission(ClaimDefinitions.TaskDetail)]
    public async Task<IActionResult> Overdue(int? clientId = null, int page = 1)
    {
        string? assignedToKind = SelectedClientId.HasValue ? "Client" : null;
        if (SelectedClientId.HasValue)
            clientId = SelectedClientId;

        var result = await _tasks.GetAllAsync(
            statut: "EnRetard",
            clientId: clientId,
            page: page,
            assignedToKind: assignedToKind);
        ViewBag.Clients = await LoadTaskClientFilterAsync();
        ViewBag.Statut = "EnRetard";
        ViewBag.IsClientSpace = SelectedClientId.HasValue;
        ViewBag.SelectedClientName = SelectedClientName;
        ViewBag.ClientId = clientId;
        ViewBag.AssignedToKind = assignedToKind;
        await LoadReferenceDataToViewBagAsync("STATUT_TACHE", "PRIORITE_TACHE");
        return View("Index", result);
    }

    /// <summary>
    /// Client dropdown for task filters. In dossier mode show the selected client only;
    /// otherwise load all clients (needed for Fiscaliste / specialized roles on Mes tâches).
    /// </summary>
    private async Task<List<ClientDto>> LoadTaskClientFilterAsync()
    {
        if (SelectedClientId.HasValue)
        {
            try
            {
                var one = await _clients.GetByIdAsync(SelectedClientId.Value);
                if (one != null)
                    return new List<ClientDto> { one };
            }
            catch { /* fall through */ }
            return new List<ClientDto>
            {
                new() { Id = SelectedClientId.Value, RaisonSociale = SelectedClientName ?? $"Client #{SelectedClientId}" }
            };
        }

        return await _clients.GetAllForDropdownAsync();
    }

    [RequirePermission(ClaimDefinitions.TaskDetail)]
    public async Task<IActionResult> Details(int id)
    {
        var task = await _tasks.GetByIdAsync(id);
        if (task == null) return NotFound();

        if (task.IsWorkflowTask && task.WorkflowInstanceId.HasValue)
        {
            try
            {
                var wf = await _workflow.GetInstanceByIdAsync(task.WorkflowInstanceId.Value);
                ViewBag.ActionBarWorkflow = wf;
                var step = await _workflow.GetCurrentStepAsync(task.WorkflowInstanceId.Value);
                ViewBag.ActionBarCurrentStep = step;
                if (task.DeclarationId.HasValue)
                {
                    var decl = await _declarations.GetByIdAsync(task.DeclarationId.Value);
                    ViewBag.ActionBarDeclaration = decl;
                }
                ViewBag.ActionBarUserRole = HttpContext.Session.GetString("UserRole") ?? "";
                ViewBag.ActionBarUserId = HttpContext.Session.GetString("UserId") ?? "";
                ViewBag.ActionBarIsClientPortal = false;
                PopulateContractAccessViewBag();
            }
            catch { /* supplementary */ }
        }

        try
        {
            var employees = await _cabinet.GetEmployeesAsync(page: 1, pageSize: 200);
            ViewBag.Employees = employees?.Items?.Where(e => e.EstActif).ToList() ?? new List<CabinetEmployeeDto>();
        }
        catch
        {
            ViewBag.Employees = new List<CabinetEmployeeDto>();
        }

        return View(task);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.WorkflowReassign, ClaimDefinitions.TaskAssign)]
    public async Task<IActionResult> ReassignWorkflowStep(int taskId, int stepInstanceId, string newAssigneeUserId, string? reason = null)
    {
        var dto = new ReassignStepDto { NewAssigneeUserId = newAssigneeUserId, Reason = reason };
        var result = await _workflow.ReassignStepAsync(stepInstanceId, dto);
        if (result?.Success == true)
            TempData["Success"] = result.Message ?? "Tâche réassignée.";
        else
            TempData["Error"] = result?.Message ?? "Erreur lors de la réassignation.";
        return RedirectToAction("Details", new { id = taskId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.TaskDetail)]
    public async Task<IActionResult> ClaimWorkflowStep(int taskId, int workflowInstanceId, bool forceOverride = false, string? reason = null)
    {
        var result = await _workflow.ClaimStepAsync(workflowInstanceId, forceOverride, reason);
        if (result?.Success == true)
            TempData["Success"] = result.Message ?? "Étape prise en charge.";
        else
            TempData["Error"] = result?.Message ?? "Impossible de prendre cette étape.";
        return RedirectToAction("Details", new { id = taskId });
    }

    [HttpGet]
    [RequirePermission(ClaimDefinitions.TaskCreate)]
    public async Task<IActionResult> Create(int? clientId = null)
    {
        if (SelectedClientId.HasValue)
            clientId = SelectedClientId;
        ViewBag.Clients = SelectedClientId.HasValue
            ? new List<ClientDto>()
            : await _clients.GetAllForDropdownAsync();
        await LoadReferenceDataToViewBagAsync("STATUT_TACHE", "PRIORITE_TACHE");
        var dto = new TaskItemDto();
        if (clientId.HasValue) dto.ClientId = clientId;
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.TaskCreate)]
    public async Task<IActionResult> Create(TaskItemDto dto)
    {
        if (SelectedClientId.HasValue)
            dto.ClientId = SelectedClientId;
        if (!ModelState.IsValid)
        {
            ViewBag.Clients = SelectedClientId.HasValue
                ? new List<ClientDto>()
                : await _clients.GetAllForDropdownAsync();
            await LoadReferenceDataToViewBagAsync("STATUT_TACHE", "PRIORITE_TACHE");
            return View(dto);
        }
        var result = await _tasks.CreateAsync(dto);
        if (result?.Success == true)
        {
            TempData["Success"] = result.Message;
            return RedirectToAction("Index");
        }
        SetApiError(result, "Erreur lors de la création de la tâche.");
        ViewBag.Clients = SelectedClientId.HasValue
            ? new List<ClientDto>()
            : await _clients.GetAllForDropdownAsync();
        await LoadReferenceDataToViewBagAsync("STATUT_TACHE", "PRIORITE_TACHE");
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.TaskEdit)]
    public async Task<IActionResult> ChangeStatus(int id, string newStatut)
    {
        var result = await _tasks.ChangeStatusAsync(id, newStatut);
        SetApiResult(result, null, "Erreur lors du changement de statut.");
        return RedirectToAction("Details", new { id });
    }

    /// <summary>Complete workflow step from a task — service enforces assignee-only.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.TaskDetail)]
    public async Task<IActionResult> ValidateWorkflowStep(int taskId, int workflowInstanceId, string? notes)
    {
        // 1. Mark the task as done
        await _tasks.ChangeStatusAsync(taskId, "Termine");
        // 2. Advance the workflow to the next step
        var result = await _tasks.CompleteWorkflowStepAsync(workflowInstanceId, notes);
        if (result?.Success == true)
            TempData["Success"] = "Étape validée et workflow avancé avec succès.";
        else
            TempData["Error"] = result?.Message ?? "Erreur lors de la validation de l'étape.";
        return RedirectToAction("MyTasks");
    }

    /// <summary>Refuse current workflow step from a task and reopen the previous step.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.TaskDetail)]
    public async Task<IActionResult> RejectWorkflowStep(int taskId, int workflowInstanceId, string? reason = null, string? comment = null)
    {
        var dto = new RejectStepDto { Reason = reason, Comment = comment };
        var result = await _workflow.RejectStepAsync(workflowInstanceId, dto);
        if (result?.Success == true)
        {
            var msg = result.Message ?? "Étape refusée.";
            TempData["Success"] = msg;
            // Correction on same step (first human step) → stay on task
            if (msg.Contains("Correction demandée", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("pas d'étape précédente", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Details", new { id = taskId });
            }

            // Sent back to previous step → this task is no longer current
            await _tasks.ChangeStatusAsync(taskId, "Annule");
            return RedirectToAction("MyTasks");
        }

        TempData["Error"] = result?.Message ?? "Erreur lors du refus de l'étape.";
        return RedirectToAction("Details", new { id = taskId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.TaskDetail)]
    public async Task<IActionResult> AddComment(int id, string contenu)
    {
        if (string.IsNullOrWhiteSpace(contenu))
        {
            TempData["Error"] = "Le commentaire ne peut pas être vide.";
            return RedirectToAction("Details", new { id });
        }
        var result = await _tasks.AddCommentAsync(id, contenu);
        if (result?.Success == true)
            TempData["Success"] = "Commentaire ajouté.";
        else
            TempData["Error"] = result?.Message ?? "Erreur lors de l'ajout du commentaire.";
        return RedirectToAction("Details", new { id });
    }
}
