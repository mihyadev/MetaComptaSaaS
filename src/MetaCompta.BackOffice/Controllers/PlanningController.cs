using MetaCompta.Application.DTOs;
using MetaCompta.BackOffice.Filters;
using MetaCompta.BackOffice.Services;
using MetaCompta.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.BackOffice.Controllers;

/// <summary>
/// Controller for Planning — Calendar / Events.
/// </summary>
public class PlanningController : BaseController
{
    private readonly IPlanningFacade _planning;
    private readonly IClientFacade _clients;
    private readonly ITaskFacade _tasks;
    private readonly ICabinetFacade _cabinet;

    public PlanningController(
        IReferenceDataFacade referenceData,
        IHttpContextAccessor contextAccessor,
        IPlanningFacade planning,
        IClientFacade clients,
        ITaskFacade tasks,
        ICabinetFacade cabinet)
        : base(referenceData, contextAccessor)
    {
        _planning = planning;
        _clients = clients;
        _tasks = tasks;
        _cabinet = cabinet;
    }

    [RequirePermission(ClaimDefinitions.TaskDetail)]
    public async Task<IActionResult> Index(int? clientId = null)
    {
        // Espace Client: only show planning elements for the selected client
        if (SelectedClientId.HasValue)
            clientId = SelectedClientId;

        // Range: current week -> +6 weeks for events
        var weekStart = DateTime.Today.AddDays(-(int)(DateTime.Today.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)DateTime.Today.DayOfWeek - 1));
        var rangeEnd = weekStart.AddDays(42);

        var eventsTask    = _planning.GetEventsAsync(weekStart, rangeEnd, clientId, null);
        var myTasksTask   = _tasks.GetAllAsync(assignedTo: "me", clientId: clientId, page: 1);
        var teamTasksTask = _tasks.GetAllAsync(clientId: clientId, page: 1);
        var clientsTask   = SelectedClientId.HasValue
            ? Task.FromResult<List<ClientDto>>(new List<ClientDto>())
            : _clients.GetAllForDropdownAsync();
        var employeesTask = _cabinet.GetEmployeesAsync(page: 1);

        await Task.WhenAll(eventsTask, myTasksTask, teamTasksTask, clientsTask, employeesTask);

        ViewBag.Clients          = clientsTask.Result;
        ViewBag.ClientId         = clientId;
        ViewBag.IsClientSpace    = SelectedClientId.HasValue;
        ViewBag.SelectedClientName = SelectedClientName;
        ViewBag.Events           = eventsTask.Result ?? new List<PlanningEventDto>();
        ViewBag.MyTasks          = myTasksTask.Result?.Items ?? new List<TaskItemDto>();
        ViewBag.TeamTasks        = teamTasksTask.Result?.Items ?? new List<TaskItemDto>();
        ViewBag.Employees        = employeesTask.Result?.Items ?? new List<CabinetEmployeeDto>();
        ViewBag.WeekStart        = weekStart;
        ViewBag.UserDisplayName  = UserName ?? "Vous";
        ViewBag.CurrentUserId    = UserId;
        return View();
    }

    [HttpGet]
    [RequirePermission(ClaimDefinitions.TaskDetail)]
    public async Task<IActionResult> GetEvents(int? clientId = null, DateTime? start = null, DateTime? end = null)
    {
        if (SelectedClientId.HasValue)
            clientId = SelectedClientId;
        var events = await _planning.GetEventsAsync(start, end, clientId);
        return Json(events);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.TaskCreate)]
    public async Task<IActionResult> Create([FromBody] PlanningEventDto dto)
    {
        if (SelectedClientId.HasValue)
            dto.ClientId = SelectedClientId;
        var result = await _planning.CreateAsync(dto);
        return Json(result);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.TaskEdit)]
    public async Task<IActionResult> Update([FromBody] PlanningEventDto dto)
    {
        if (SelectedClientId.HasValue)
            dto.ClientId = SelectedClientId;
        var result = await _planning.UpdateAsync(dto);
        return Json(result);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.TaskEdit)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _planning.DeleteAsync(id);
        return Json(result);
    }
}
