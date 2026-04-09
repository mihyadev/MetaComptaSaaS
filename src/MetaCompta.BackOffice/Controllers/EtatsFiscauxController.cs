using MetaCompta.BackOffice.Filters;
using MetaCompta.BackOffice.Services;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.BackOffice.Controllers;

/// <summary>
/// Controller for États Fiscaux — État 9421, Liasse fiscale, Bilan, CPC, Amortissements, États de synthèse.
/// </summary>
[RequireSelectedClient]
public class EtatsFiscauxController : BaseController
{
    private readonly IEtatFiscalFacade _etats;
    private readonly IClientFacade _clients;

    public EtatsFiscauxController(
        IReferenceDataFacade referenceData,
        IHttpContextAccessor contextAccessor,
        IEtatFiscalFacade etats,
        IClientFacade clients)
        : base(referenceData, contextAccessor)
    {
        _etats = etats;
        _clients = clients;
    }

    public async Task<IActionResult> Index(int? clientId = null, string? type = null, int? exercice = null, int page = 1, string? search = null)
    {
        await LoadReferenceDataToViewBagAsync("TYPE_ETAT_FISCAL", "STATUT_ETAT");
        var result = await _etats.GetAllAsync(clientId, type, exercice, page, search);
        ViewBag.Clients = await _clients.GetAllForDropdownAsync();
        ViewBag.ClientId = clientId;
        ViewBag.TypeEtat = type;
        ViewBag.Exercice = exercice;
        ViewBag.Search = search;
        return View(result);
    }

    public async Task<IActionResult> Details(int id)
    {
        await LoadReferenceDataToViewBagAsync("TYPE_ETAT_FISCAL", "STATUT_ETAT");
        var etat = await _etats.GetByIdAsync(id);
        if (etat == null) return NotFound();
        return View(etat);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate(int clientId, string type, int exercice)
    {
        var result = await _etats.GenerateAsync(clientId, type, exercice);
        if (result?.Success == true)
        {
            TempData["Success"] = result.Message;
            return RedirectToAction("Index", new { clientId, type, exercice });
        }
        SetApiError(result, "Erreur lors de la génération de l'état fiscal.");
        return RedirectToAction("Index", new { clientId, type, exercice });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Validate(int id)
    {
        var result = await _etats.ValidateAsync(id);
        if (result?.Success == true)
            TempData["Success"] = result.Message;
        else
            SetApiError(result, "Erreur lors de la validation.");
        return RedirectToAction("Details", new { id });
    }

    public async Task<IActionResult> Etat9421(int? clientId = null, int? exercice = null, int page = 1)
    {
        return await TypedIndex("Etat9421", clientId, exercice, page);
    }

    public async Task<IActionResult> LiasseFiscale(int? clientId = null, int? exercice = null, int page = 1)
    {
        return await TypedIndex("LiasseFiscale", clientId, exercice, page);
    }

    public async Task<IActionResult> Bilan(int? clientId = null, int? exercice = null, int page = 1)
    {
        return await TypedIndex("Bilan", clientId, exercice, page);
    }

    public async Task<IActionResult> CPC(int? clientId = null, int? exercice = null, int page = 1)
    {
        return await TypedIndex("CPC", clientId, exercice, page);
    }

    public async Task<IActionResult> Amortissements(int? clientId = null, int? exercice = null, int page = 1)
    {
        return await TypedIndex("Amortissements", clientId, exercice, page);
    }

    public async Task<IActionResult> EtatsSynthese(int? clientId = null, int? exercice = null, int page = 1)
    {
        return await TypedIndex("EtatsSynthese", clientId, exercice, page);
    }

    private async Task<IActionResult> TypedIndex(string type, int? clientId, int? exercice, int page)
    {
        await LoadReferenceDataToViewBagAsync("TYPE_ETAT_FISCAL", "STATUT_ETAT");
        var result = await _etats.GetAllAsync(clientId, type, exercice, page, null);
        ViewBag.Clients = await _clients.GetAllForDropdownAsync();
        ViewBag.ClientId = clientId;
        ViewBag.TypeEtat = type;
        ViewBag.Exercice = exercice;
        return View("Index", result);
    }
}
