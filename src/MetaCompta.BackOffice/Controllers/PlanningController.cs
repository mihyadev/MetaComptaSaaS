using MetaCompta.Application.DTOs;
using MetaCompta.BackOffice.Services;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.BackOffice.Controllers;

/// <summary>
/// Controller for Planning — Calendar / Events.
/// </summary>
public class PlanningController : BaseController
{
    private readonly IPlanningFacade _planning;
    private readonly IClientFacade _clients;

    public PlanningController(
        IReferenceDataFacade referenceData,
        IHttpContextAccessor contextAccessor,
        IPlanningFacade planning,
        IClientFacade clients)
        : base(referenceData, contextAccessor)
    {
        _planning = planning;
        _clients = clients;
    }

    public async Task<IActionResult> Index(int? clientId = null)
    {
        ViewBag.Clients = await _clients.GetAllForDropdownAsync();
        ViewBag.ClientId = clientId;
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetEvents(int? clientId = null, DateTime? start = null, DateTime? end = null)
    {
        var events = await _planning.GetEventsAsync(start, end, clientId);
        return Json(events);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] PlanningEventDto dto)
    {
        var result = await _planning.CreateAsync(dto);
        return Json(result);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update([FromBody] PlanningEventDto dto)
    {
        var result = await _planning.UpdateAsync(dto);
        return Json(result);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _planning.DeleteAsync(id);
        return Json(result);
    }
}
