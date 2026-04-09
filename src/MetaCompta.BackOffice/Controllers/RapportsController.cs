using MetaCompta.BackOffice.Filters;
using MetaCompta.BackOffice.Services;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.BackOffice.Controllers;

/// <summary>
/// Controller for Rapports — Financiers, Fiscaux, Sociaux, Activités, Personnalisés.
/// </summary>
[RequireSelectedClient]
public class RapportsController : BaseController
{
    private readonly IRapportFacade _rapports;
    private readonly IClientFacade _clients;

    public RapportsController(
        IReferenceDataFacade referenceData,
        IHttpContextAccessor contextAccessor,
        IRapportFacade rapports,
        IClientFacade clients)
        : base(referenceData, contextAccessor)
    {
        _rapports = rapports;
        _clients = clients;
    }

    public async Task<IActionResult> Index(int? clientId = null, string? type = null, int page = 1, string? search = null)
    {
        clientId ??= SelectedClientId;
        var result = await _rapports.GetAllAsync(clientId, type, page, search);
        ViewBag.TypeRapport = type;
        ViewBag.Search = search;
        ViewBag.ClientId = clientId;
        return View(result);
    }

    public async Task<IActionResult> Financiers(int? clientId = null, int page = 1, string? search = null) =>
        await TypedIndex("Financier", clientId, page, search);

    public async Task<IActionResult> Fiscaux(int? clientId = null, int page = 1, string? search = null) =>
        await TypedIndex("Fiscal", clientId, page, search);

    public async Task<IActionResult> Sociaux(int? clientId = null, int page = 1, string? search = null) =>
        await TypedIndex("Social", clientId, page, search);

    public async Task<IActionResult> Activites(int? clientId = null, int page = 1, string? search = null) =>
        await TypedIndex("Activite", clientId, page, search);

    public async Task<IActionResult> Personnalises(int? clientId = null, int page = 1, string? search = null) =>
        await TypedIndex("Personnalise", clientId, page, search);

    private async Task<IActionResult> TypedIndex(string type, int? clientId, int page, string? search)
    {
        clientId ??= SelectedClientId;
        var result = await _rapports.GetAllAsync(clientId, type, page, search);
        ViewBag.TypeRapport = type;
        ViewBag.Search = search;
        ViewBag.ClientId = clientId;
        return View("Index", result);
    }

    public async Task<IActionResult> Details(int id)
    {
        var rapport = await _rapports.GetByIdAsync(id);
        if (rapport == null) return NotFound();
        return View(rapport);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate(string type, int? clientId = null, DateTime? dateDebut = null, DateTime? dateFin = null)
    {
        clientId ??= SelectedClientId;
        var result = await _rapports.GenerateAsync(type, clientId, dateDebut, dateFin);
        if (result?.Success == true)
        {
            TempData["Success"] = result.Message;
            return RedirectToAction("Index", new { type });
        }
        SetApiError(result, "Erreur lors de la génération du rapport.");
        return RedirectToAction("Index", new { type });
    }
}
