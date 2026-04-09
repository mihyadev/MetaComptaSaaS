using MetaCompta.Application.DTOs;
using MetaCompta.BackOffice.Services;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.BackOffice.Controllers;

/// <summary>
/// Controller for Tasks — both client-scoped and global task management.
/// </summary>
public class TasksController : BaseController
{
    private readonly ITaskFacade _tasks;
    private readonly IClientFacade _clients;

    public TasksController(
        IReferenceDataFacade referenceData,
        IHttpContextAccessor contextAccessor,
        ITaskFacade tasks,
        IClientFacade clients)
        : base(referenceData, contextAccessor)
    {
        _tasks = tasks;
        _clients = clients;
    }

    public async Task<IActionResult> Index(string? statut = null, int? clientId = null, int page = 1)
    {
        var result = await _tasks.GetAllAsync(statut: statut, clientId: clientId, page: page);
        ViewBag.Clients = await _clients.GetAllForDropdownAsync();
        ViewBag.Statut = statut;
        ViewBag.ClientId = clientId;
        await LoadReferenceDataToViewBagAsync("STATUT_TACHE", "PRIORITE_TACHE");
        return View(result);
    }

    public async Task<IActionResult> MyTasks(int page = 1)
    {
        var result = await _tasks.GetAllAsync(assignedTo: "me", page: page);
        await LoadReferenceDataToViewBagAsync("STATUT_TACHE", "PRIORITE_TACHE");
        return View("Index", result);
    }

    public async Task<IActionResult> Overdue(int page = 1)
    {
        var result = await _tasks.GetAllAsync(statut: "EnRetard", page: page);
        ViewBag.Statut = "EnRetard";
        await LoadReferenceDataToViewBagAsync("STATUT_TACHE", "PRIORITE_TACHE");
        return View("Index", result);
    }

    public async Task<IActionResult> Details(int id)
    {
        var task = await _tasks.GetByIdAsync(id);
        if (task == null) return NotFound();
        return View(task);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? clientId = null)
    {
        ViewBag.Clients = await _clients.GetAllForDropdownAsync();
        await LoadReferenceDataToViewBagAsync("STATUT_TACHE", "PRIORITE_TACHE");
        var dto = new TaskItemDto();
        if (clientId.HasValue) dto.ClientId = clientId;
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TaskItemDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Clients = await _clients.GetAllForDropdownAsync();
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
        ViewBag.Clients = await _clients.GetAllForDropdownAsync();
        await LoadReferenceDataToViewBagAsync("STATUT_TACHE", "PRIORITE_TACHE");
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id, string newStatut)
    {
        var result = await _tasks.ChangeStatusAsync(id, newStatut);
        SetApiResult(result, null, "Erreur lors du changement de statut.");
        return RedirectToAction("Details", new { id });
    }
}
