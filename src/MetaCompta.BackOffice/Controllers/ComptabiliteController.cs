using MetaCompta.Application.DTOs;
using MetaCompta.BackOffice.Filters;
using MetaCompta.BackOffice.Services;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.BackOffice.Controllers;

/// <summary>
/// Controller for Comptabilité — Écritures, Journaux, Grand Livre, Balance, Rapprochement.
/// All data is scoped to the selected client (injected by RequireSelectedClient filter).
/// </summary>
[RequireSelectedClient]
public class ComptabiliteController : BaseController
{
    private readonly IComptabiliteFacade _compta;
    private readonly IClientFacade _clients;

    public ComptabiliteController(
        IReferenceDataFacade referenceData,
        IHttpContextAccessor contextAccessor,
        IComptabiliteFacade compta,
        IClientFacade clients)
        : base(referenceData, contextAccessor)
    {
        _compta = compta;
        _clients = clients;
    }

    /// <summary>Load selected client info for all views.</summary>
    private async Task LoadClientAsync(int? clientId)
    {
        clientId ??= SelectedClientId;
        if (clientId.HasValue)
        {
            var client = await _clients.GetByIdAsync(clientId.Value);
            ViewBag.Client = client;
        }
        ViewBag.ClientId = clientId;
    }

    public async Task<IActionResult> Ecritures(int? clientId = null, string? journal = null, int page = 1, string? search = null)
    {
        clientId ??= SelectedClientId;
        await LoadClientAsync(clientId);
        var result = await _compta.GetEcrituresAsync(clientId, journal, page, search);
        ViewBag.Journal = journal;
        ViewBag.Search = search;
        return View(result);
    }

    [HttpGet]
    public async Task<IActionResult> CreateEcriture()
    {
        await LoadClientAsync(SelectedClientId);
        return View(new EcritureCreateDto { ClientId = SelectedClientId ?? 0 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEcriture(EcritureCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadClientAsync(SelectedClientId);
            return View(dto);
        }
        var result = await _compta.CreateEcritureAsync(dto);
        if (result?.Success == true)
        {
            TempData["Success"] = result.Message;
            return RedirectToAction("Ecritures");
        }
        SetApiError(result, "Erreur lors de la création de l''écriture.");
        await LoadClientAsync(SelectedClientId);
        return View(dto);
    }

    public async Task<IActionResult> Journaux(int? clientId = null)
    {
        clientId ??= SelectedClientId;
        await LoadClientAsync(clientId);
        var journaux = await _compta.GetJournauxAsync(clientId);
        return View(journaux);
    }

    public async Task<IActionResult> GrandLivre(int? clientId = null, DateTime? dateDebut = null, DateTime? dateFin = null)
    {
        clientId ??= SelectedClientId;
        if (!clientId.HasValue) return RedirectToAction("Ecritures");
        await LoadClientAsync(clientId);
        var data = await _compta.GetGrandLivreAsync(clientId.Value, dateDebut, dateFin);
        ViewBag.DateDebut = dateDebut;
        ViewBag.DateFin = dateFin;
        return View(data);
    }

    public async Task<IActionResult> Balance(int? clientId = null, DateTime? dateDebut = null, DateTime? dateFin = null)
    {
        clientId ??= SelectedClientId;
        if (!clientId.HasValue) return RedirectToAction("Ecritures");
        await LoadClientAsync(clientId);
        var data = await _compta.GetBalanceAsync(clientId.Value, dateDebut, dateFin);
        ViewBag.DateDebut = dateDebut;
        ViewBag.DateFin = dateFin;
        return View(data);
    }

    public async Task<IActionResult> BalanceAgee(int? clientId = null)
    {
        clientId ??= SelectedClientId;
        if (!clientId.HasValue) return RedirectToAction("Ecritures");
        await LoadClientAsync(clientId);
        var data = await _compta.GetBalanceAgeeAsync(clientId.Value);
        return View(data);
    }

    public async Task<IActionResult> Rapprochements(int? clientId = null)
    {
        clientId ??= SelectedClientId;
        await LoadClientAsync(clientId);
        var data = await _compta.GetRapprochementsAsync(clientId);
        return View(data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRapprochement(int clientId, string compteBancaire, string periode)
    {
        var result = await _compta.CreateRapprochementAsync(clientId, compteBancaire, periode);
        if (result?.Success == true)
        {
            TempData["Success"] = result.Message;
            return RedirectToAction("Rapprochements");
        }
        SetApiError(result, "Erreur lors de la création du rapprochement.");
        return RedirectToAction("Rapprochements");
    }
}