using MetaCompta.Application.DTOs;
using MetaCompta.BackOffice.Filters;
using MetaCompta.BackOffice.Services;
using MetaCompta.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.BackOffice.Controllers;

/// <summary>
/// Controller for États Fiscaux — catalog + interactive États de Synthèse workspace.
/// </summary>
[RequireSelectedClient]
public class EtatsFiscauxController : BaseController
{
    private readonly IEtatFiscalFacade _etats;
    private readonly IClientFacade _clients;
    private readonly IComptabiliteFacade _compta;

    public EtatsFiscauxController(
        IReferenceDataFacade referenceData,
        IHttpContextAccessor contextAccessor,
        IEtatFiscalFacade etats,
        IClientFacade clients,
        IComptabiliteFacade compta)
        : base(referenceData, contextAccessor)
    {
        _etats = etats;
        _clients = clients;
        _compta = compta;
    }

    [RequirePermission(ClaimDefinitions.ReportView)]
    public async Task<IActionResult> Index(int? clientId = null, string? type = null, int? exercice = null, int page = 1, string? search = null)
    {
        clientId ??= SelectedClientId;
        await LoadReferenceDataToViewBagAsync("TYPE_ETAT_FISCAL", "STATUT_ETAT");
        var result = await _etats.GetAllAsync(clientId, type, exercice, page, search);
        ViewBag.Client = SelectedClientId.HasValue ? await _clients.GetByIdAsync(SelectedClientId.Value) : null;
        ViewBag.ClientId = clientId;
        ViewBag.TypeEtat = type;
        ViewBag.Exercice = exercice;
        ViewBag.Search = search;
        return View(result);
    }

    /// <summary>Interactive États de Synthèse grid (Bilan, CPC, ESG…) under Tenue comptable.</summary>
    [RequirePermission(ClaimDefinitions.ReportView)]
    public async Task<IActionResult> Synthese(string? pane = null)
    {
        var clientId = SelectedClientId;
        if (!clientId.HasValue)
            return RedirectToAction("Workspace", "Home");

        await _compta.EnsureLedgerAsync(clientId.Value);

        if (!SelectedExerciceId.HasValue && SelectedExercice is null)
        {
            var exercices = await _compta.GetExercicesAsync(clientId.Value);
            var pick = exercices
                .Where(e => string.Equals(e.Statut, "Ouvert", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(e => e.Annee)
                .FirstOrDefault()
                ?? exercices.OrderByDescending(e => e.Annee).FirstOrDefault();
            if (pick != null)
                SetExerciceSession(pick);
        }

        var exerciceAnnee = SelectedExercice ?? DateTime.Now.Year;
        var balance = await _compta.GetBalanceAsync(clientId.Value, null, null, SelectedExerciceId);

        ViewBag.Client = await _clients.GetByIdAsync(clientId.Value);
        ViewBag.ClientId = clientId.Value;
        ViewBag.ExerciceAnnee = exerciceAnnee;
        ViewBag.ExerciceId = SelectedExerciceId;
        ViewBag.ExerciceStatut = SelectedExerciceStatut ?? "Ouvert";
        ViewBag.InitialPane = string.IsNullOrWhiteSpace(pane) ? "bilan" : pane.Trim().ToLowerInvariant();
        ViewBag.BalanceJson = System.Text.Json.JsonSerializer.Serialize(
            balance.Select(b => new
            {
                account = b.NumeroCompte,
                name = b.LibelleCompte,
                debit = b.TotalDebit,
                credit = b.TotalCredit,
                soldeD = b.SoldeDebiteur,
                soldeC = b.SoldeCrediteur
            }));
        return View();
    }

    [RequirePermission(ClaimDefinitions.ReportView)]
    public async Task<IActionResult> Details(int id)
    {
        await LoadReferenceDataToViewBagAsync("TYPE_ETAT_FISCAL", "STATUT_ETAT");
        var etat = await _etats.GetByIdAsync(id);
        if (etat == null) return NotFound();
        return View(etat);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.ReportExport)]
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
    [RequirePermission(ClaimDefinitions.DeclarationValidate)]
    public async Task<IActionResult> Validate(int id)
    {
        var result = await _etats.ValidateAsync(id);
        if (result?.Success == true)
            TempData["Success"] = result.Message;
        else
            SetApiError(result, "Erreur lors de la validation.");
        return RedirectToAction("Details", new { id });
    }

    [RequirePermission(ClaimDefinitions.ReportView)]
    public IActionResult Etat9421() => RedirectToAction(nameof(Synthese), new { pane = "9421" });

    [RequirePermission(ClaimDefinitions.ReportView)]
    public IActionResult LiasseFiscale() => RedirectToAction(nameof(Synthese), new { pane = "bilan" });

    [RequirePermission(ClaimDefinitions.ReportView)]
    public IActionResult Bilan() => RedirectToAction(nameof(Synthese), new { pane = "bilan" });

    [RequirePermission(ClaimDefinitions.ReportView)]
    public IActionResult CPC() => RedirectToAction(nameof(Synthese), new { pane = "cpc" });

    [RequirePermission(ClaimDefinitions.ReportView)]
    public IActionResult Amortissements() => RedirectToAction(nameof(Synthese), new { pane = "amort" });

    [RequirePermission(ClaimDefinitions.ReportView)]
    public IActionResult EtatsSynthese() => RedirectToAction(nameof(Synthese), new { pane = "bilan" });
}
