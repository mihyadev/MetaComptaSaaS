using MetaCompta.Application.DTOs;
using MetaCompta.BackOffice.Filters;
using MetaCompta.BackOffice.Services;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.BackOffice.Controllers;

/// <summary>
/// Controller for Juridique — Dossiers juridiques (création société, modification statuts, PV AG, dépôt légal).
/// </summary>
[RequireSelectedClient]
public class JuridiqueController : BaseController
{
    private readonly IJuridiqueFacade _juridique;
    private readonly IClientFacade _clients;

    public JuridiqueController(
        IReferenceDataFacade referenceData,
        IHttpContextAccessor contextAccessor,
        IJuridiqueFacade juridique,
        IClientFacade clients)
        : base(referenceData, contextAccessor)
    {
        _juridique = juridique;
        _clients = clients;
    }

    public async Task<IActionResult> Index(int? clientId = null, string? type = null, string? statut = null, int page = 1, string? search = null)
    {
        clientId ??= SelectedClientId;
        await LoadReferenceDataToViewBagAsync("TYPE_DOSSIER_JURIDIQUE", "STATUT_DOSSIER");
        var result = await _juridique.GetAllAsync(clientId, type, statut, page, search);
        ViewBag.Client = SelectedClientId.HasValue ? await _clients.GetByIdAsync(SelectedClientId.Value) : null;
        ViewBag.TypeDossier = type;
        ViewBag.Statut = statut;
        ViewBag.Search = search;
        return View(result);
    }

    public async Task<IActionResult> Details(int id)
    {
        await LoadReferenceDataToViewBagAsync("TYPE_DOSSIER_JURIDIQUE", "STATUT_DOSSIER");
        var dossier = await _juridique.GetByIdAsync(id);
        if (dossier == null) return NotFound();
        return View(dossier);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadReferenceDataToViewBagAsync("TYPE_DOSSIER_JURIDIQUE");
        ViewBag.Client = SelectedClientId.HasValue ? await _clients.GetByIdAsync(SelectedClientId.Value) : null;
        return View(new DossierJuridiqueCreateDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DossierJuridiqueCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadReferenceDataToViewBagAsync("TYPE_DOSSIER_JURIDIQUE");
            ViewBag.Client = SelectedClientId.HasValue ? await _clients.GetByIdAsync(SelectedClientId.Value) : null;
            return View(dto);
        }
        var result = await _juridique.CreateAsync(dto);
        if (result?.Success == true)
        {
            TempData["Success"] = result.Message;
            return RedirectToAction("Index");
        }
        SetApiError(result, "Erreur lors de la création du dossier juridique.");
        await LoadReferenceDataToViewBagAsync("TYPE_DOSSIER_JURIDIQUE");
        ViewBag.Client = SelectedClientId.HasValue ? await _clients.GetByIdAsync(SelectedClientId.Value) : null;
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id, string statut)
    {
        var result = await _juridique.ChangeStatusAsync(id, statut);
        if (result?.Success == true)
            TempData["Success"] = result.Message;
        else
            SetApiError(result, "Erreur lors du changement de statut.");
        return RedirectToAction("Details", new { id });
    }
}
