using MetaCompta.Application.DTOs;
using MetaCompta.BackOffice.Filters;
using MetaCompta.BackOffice.Services;
using MetaCompta.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.BackOffice.Controllers;

/// <summary>
/// Comptabilité — Plan PCGM, écritures, journaux, grand livre, balance, rapprochement.
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

    private async Task EnsureLedgerAsync(int? clientId)
    {
        clientId ??= SelectedClientId;
        if (clientId is > 0)
            await _compta.EnsureLedgerAsync(clientId.Value);
    }

    [RequirePermission(ClaimDefinitions.ComptabiliteDetail)]
    public async Task<IActionResult> PlanComptable(int? clientId = null, string? search = null)
    {
        clientId ??= SelectedClientId;
        if (!clientId.HasValue) return RedirectToAction("Ecritures");
        await LoadClientAsync(clientId);
        await EnsureLedgerAsync(clientId);
        var comptes = await _compta.GetComptesAsync(clientId.Value, search);
        ViewBag.Search = search;
        return View(comptes);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.ComptabiliteCreate)]
    public async Task<IActionResult> CreateCompte(CompteComptableCreateDto dto)
    {
        dto.ClientId = SelectedClientId ?? dto.ClientId;
        var result = await _compta.CreateCompteAsync(dto);
        if (result?.Success == true)
            TempData["Success"] = result.Message;
        else
            SetApiError(result, "Erreur lors de la création du compte.");
        return RedirectToAction("PlanComptable");
    }

    [RequirePermission(ClaimDefinitions.ComptabiliteDetail)]
    public async Task<IActionResult> Ecritures(int? clientId = null, string? journal = null, int page = 1, string? search = null)
    {
        clientId ??= SelectedClientId;
        await LoadClientAsync(clientId);
        await EnsureLedgerAsync(clientId);
        var result = await _compta.GetEcrituresAsync(clientId, journal, page, search);
        ViewBag.Journal = journal;
        ViewBag.Search = search;
        return View(result);
    }

    [HttpGet]
    [RequirePermission(ClaimDefinitions.ComptabiliteCreate)]
    public async Task<IActionResult> CreateEcriture()
    {
        await LoadClientAsync(SelectedClientId);
        await EnsureLedgerAsync(SelectedClientId);
        if (SelectedClientId is > 0)
            ViewBag.Comptes = await _compta.GetComptesAsync(SelectedClientId.Value);
        ViewBag.Journaux = SelectedClientId is > 0
            ? await _compta.GetJournauxAsync(SelectedClientId)
            : new List<JournalDto>();
        return View(new EcritureCreateDto
        {
            ClientId = SelectedClientId ?? 0,
            DateEcriture = DateTime.Today,
            Journal = "OD",
            Lignes = new List<LigneEcritureDto>
            {
                new(),
                new()
            },
            Valider = true
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.ComptabiliteCreate)]
    public async Task<IActionResult> CreateEcriture(EcritureCreateDto dto)
    {
        dto.ClientId = SelectedClientId ?? dto.ClientId;
        if (!ModelState.IsValid)
        {
            await LoadClientAsync(SelectedClientId);
            if (SelectedClientId is > 0)
                ViewBag.Comptes = await _compta.GetComptesAsync(SelectedClientId.Value);
            ViewBag.Journaux = await _compta.GetJournauxAsync(SelectedClientId);
            return View(dto);
        }
        var result = await _compta.CreateEcritureAsync(dto);
        if (result?.Success == true)
        {
            TempData["Success"] = result.Message;
            return RedirectToAction("Ecritures");
        }
        SetApiError(result, "Erreur lors de la création de l'écriture.");
        await LoadClientAsync(SelectedClientId);
        if (SelectedClientId is > 0)
            ViewBag.Comptes = await _compta.GetComptesAsync(SelectedClientId.Value);
        ViewBag.Journaux = await _compta.GetJournauxAsync(SelectedClientId);
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.ComptabiliteCreate)]
    public async Task<IActionResult> ValidateEcriture(int id)
    {
        var result = await _compta.ValidateEcritureAsync(id);
        if (result?.Success == true)
            TempData["Success"] = result.Message;
        else
            SetApiError(result, "Erreur lors de la validation.");
        return RedirectToAction("Ecritures");
    }

    [RequirePermission(ClaimDefinitions.ComptabiliteDetail)]
    public async Task<IActionResult> Journaux(int? clientId = null)
    {
        clientId ??= SelectedClientId;
        await LoadClientAsync(clientId);
        await EnsureLedgerAsync(clientId);
        var journaux = await _compta.GetJournauxAsync(clientId);
        return View(journaux);
    }

    [RequirePermission(ClaimDefinitions.ComptabiliteDetail)]
    public async Task<IActionResult> GrandLivre(int? clientId = null, DateTime? dateDebut = null, DateTime? dateFin = null)
    {
        clientId ??= SelectedClientId;
        if (!clientId.HasValue) return RedirectToAction("Ecritures");
        await LoadClientAsync(clientId);
        await EnsureLedgerAsync(clientId);
        var data = await _compta.GetGrandLivreAsync(clientId.Value, dateDebut, dateFin);
        ViewBag.DateDebut = dateDebut;
        ViewBag.DateFin = dateFin;
        return View(data);
    }

    [RequirePermission(ClaimDefinitions.ComptabiliteDetail)]
    public async Task<IActionResult> Balance(int? clientId = null, DateTime? dateDebut = null, DateTime? dateFin = null)
    {
        clientId ??= SelectedClientId;
        if (!clientId.HasValue) return RedirectToAction("Ecritures");
        await LoadClientAsync(clientId);
        await EnsureLedgerAsync(clientId);
        var data = await _compta.GetBalanceAsync(clientId.Value, dateDebut, dateFin);
        ViewBag.DateDebut = dateDebut;
        ViewBag.DateFin = dateFin;
        return View(data);
    }

    [RequirePermission(ClaimDefinitions.ComptabiliteDetail)]
    public async Task<IActionResult> BalanceAgee(int? clientId = null)
    {
        clientId ??= SelectedClientId;
        if (!clientId.HasValue) return RedirectToAction("Ecritures");
        await LoadClientAsync(clientId);
        var data = await _compta.GetBalanceAgeeAsync(clientId.Value);
        return View(data);
    }

    [RequirePermission(ClaimDefinitions.ComptabiliteDetail)]
    public async Task<IActionResult> Rapprochements(int? clientId = null)
    {
        clientId ??= SelectedClientId;
        await LoadClientAsync(clientId);
        await EnsureLedgerAsync(clientId);
        var data = await _compta.GetRapprochementsAsync(clientId);
        return View(data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.ComptabiliteCreate)]
    public async Task<IActionResult> CreateRapprochement(int clientId, string compteBancaire, string periode, decimal? soldeBanque = null)
    {
        clientId = SelectedClientId ?? clientId;
        var result = await _compta.CreateRapprochementAsync(clientId, compteBancaire, periode, soldeBanque);
        if (result?.Success == true)
        {
            TempData["Success"] = result.Message;
            return RedirectToAction("Rapprochements");
        }
        SetApiError(result, "Erreur lors de la création du rapprochement.");
        return RedirectToAction("Rapprochements");
    }
}
