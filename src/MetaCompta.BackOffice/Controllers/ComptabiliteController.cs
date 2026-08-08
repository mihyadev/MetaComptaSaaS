using System.Text;
using MetaCompta.Application.DTOs;
using MetaCompta.BackOffice.Filters;
using MetaCompta.BackOffice.Services;
using MetaCompta.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.BackOffice.Controllers;

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
            ViewBag.Client = await _clients.GetByIdAsync(clientId.Value);
        ViewBag.ClientId = clientId;
        ViewBag.ExerciceAnnee = SelectedExercice;
        ViewBag.ExerciceId = SelectedExerciceId;
        ViewBag.ExerciceStatut = SelectedExerciceStatut;
    }

    private async Task EnsureLedgerAsync(int? clientId)
    {
        clientId ??= SelectedClientId;
        if (clientId is > 0)
            await _compta.EnsureLedgerAsync(clientId.Value);
    }

    private async Task EnsureExerciceContextAsync(int? clientId)
    {
        clientId ??= SelectedClientId;
        if (clientId is not > 0) return;
        await EnsureLedgerAsync(clientId);
        if (SelectedExerciceId is > 0) return;

        var exercices = await _compta.GetExercicesAsync(clientId.Value);
        var pick = exercices
            .Where(e => string.Equals(e.Statut, "Ouvert", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(e => e.Annee)
            .FirstOrDefault()
            ?? exercices.OrderByDescending(e => e.Annee).FirstOrDefault();
        if (pick != null)
        {
            SetExerciceSession(pick);
            ViewBag.ExerciceAnnee = pick.Annee;
            ViewBag.ExerciceId = pick.Id;
            ViewBag.ExerciceStatut = pick.Statut;
        }
    }

    private static FileContentResult CsvFile(string fileName, string content)
    {
        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(content)).ToArray();
        return new FileContentResult(bytes, "text/csv; charset=utf-8") { FileDownloadName = fileName };
    }

    [RequirePermission(ClaimDefinitions.ComptabiliteDetail)]
    public async Task<IActionResult> Index(int? clientId = null)
    {
        clientId ??= SelectedClientId;
        if (!clientId.HasValue) return RedirectToAction("Ecritures");
        await LoadClientAsync(clientId);
        await EnsureExerciceContextAsync(clientId);
        var dash = await _compta.GetDashboardAsync(clientId.Value, SelectedExerciceId) ?? new ComptaDashboardDto();
        return View(dash);
    }

    [RequirePermission(ClaimDefinitions.ComptabiliteDetail)]
    public async Task<IActionResult> PlanComptable(int? clientId = null, string? search = null)
    {
        clientId ??= SelectedClientId;
        if (!clientId.HasValue) return RedirectToAction("Index");
        await LoadClientAsync(clientId);
        await EnsureLedgerAsync(clientId);
        ViewBag.Search = search;
        return View(await _compta.GetComptesAsync(clientId.Value, search));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.ComptabiliteCreate)]
    public async Task<IActionResult> CreateCompte(CompteComptableCreateDto dto)
    {
        dto.ClientId = SelectedClientId ?? dto.ClientId;
        var result = await _compta.CreateCompteAsync(dto);
        if (result?.Success == true) TempData["Success"] = result.Message;
        else SetApiError(result, "Erreur lors de la création du compte.");
        return RedirectToAction("PlanComptable");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.ComptabiliteEdit)]
    public async Task<IActionResult> UpdateCompte(int id, string libelle, string? nature, bool estActif = true)
    {
        var result = await _compta.UpdateCompteAsync(id, new CompteComptableUpdateDto
        {
            Libelle = libelle,
            Nature = nature,
            EstActif = estActif
        });
        if (result?.Success == true) TempData["Success"] = result.Message;
        else SetApiError(result, "Erreur mise à jour compte.");
        return RedirectToAction("PlanComptable");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.ComptabiliteCreate)]
    public async Task<IActionResult> ImportPlan(IFormFile? file, string? csvText)
    {
        if (SelectedClientId is not > 0) return RedirectToAction("PlanComptable");
        var content = await ReadCsvUploadAsync(file, csvText);
        var result = await _compta.ImportComptesAsync(SelectedClientId.Value, content);
        SetImportTempData(result, "Import échoué.");
        return RedirectToAction("PlanComptable");
    }

    [RequirePermission(ClaimDefinitions.ComptabiliteDetail)]
    public async Task<IActionResult> ExportPlan(int? clientId = null)
    {
        clientId ??= SelectedClientId;
        if (!clientId.HasValue) return RedirectToAction("PlanComptable");
        var comptes = await _compta.GetComptesAsync(clientId.Value);
        var sb = new StringBuilder();
        sb.AppendLine("Numero;Libelle;Classe;Nature;Actif");
        foreach (var c in comptes)
            sb.AppendLine($"{c.Numero};{c.Libelle};{c.Classe};{c.Nature};{(c.EstActif ? "1" : "0")}");
        return CsvFile($"plan-pcgm-client{clientId}.csv", sb.ToString());
    }

    [RequirePermission(ClaimDefinitions.ComptabiliteDetail)]
    public async Task<IActionResult> Ecritures(int? clientId = null, string? journal = null, int page = 1, string? search = null, string? statut = null)
    {
        clientId ??= SelectedClientId;
        await LoadClientAsync(clientId);
        await EnsureExerciceContextAsync(clientId);
        ViewBag.Journal = journal;
        ViewBag.Search = search;
        ViewBag.Statut = statut;
        ViewBag.Journaux = clientId is > 0 ? await _compta.GetJournauxAsync(clientId) : new List<JournalDto>();
        return View(await _compta.GetEcrituresAsync(clientId, journal, page, search, statut, SelectedExerciceId));
    }

    [RequirePermission(ClaimDefinitions.ComptabiliteDetail)]
    public async Task<IActionResult> Details(int id)
    {
        await LoadClientAsync(SelectedClientId);
        var detail = await _compta.GetEcritureByIdAsync(id);
        if (detail == null)
        {
            TempData["Error"] = "Écriture introuvable.";
            return RedirectToAction("Ecritures");
        }
        return View(detail);
    }

    [HttpGet]
    [RequirePermission(ClaimDefinitions.ComptabiliteCreate)]
    public async Task<IActionResult> CreateEcriture()
    {
        await LoadClientAsync(SelectedClientId);
        await EnsureExerciceContextAsync(SelectedClientId);
        if (SelectedClientId is > 0)
            ViewBag.Comptes = await _compta.GetComptesAsync(SelectedClientId.Value);
        ViewBag.Journaux = SelectedClientId is > 0
            ? await _compta.GetJournauxAsync(SelectedClientId)
            : new List<JournalDto>();
        ViewBag.ExerciceCloture = string.Equals(SelectedExerciceStatut, "Cloture", StringComparison.OrdinalIgnoreCase)
            || string.Equals(SelectedExerciceStatut, "Clôturé", StringComparison.OrdinalIgnoreCase);
        return View(new EcritureCreateDto
        {
            ClientId = SelectedClientId ?? 0,
            ExerciceComptableId = SelectedExerciceId,
            DateEcriture = DateTime.Today,
            Journal = "OD",
            Lignes = new List<LigneEcritureDto> { new(), new() },
            Valider = true
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.ComptabiliteCreate)]
    public async Task<IActionResult> CreateEcriture(EcritureCreateDto dto)
    {
        dto.ClientId = SelectedClientId ?? dto.ClientId;
        dto.ExerciceComptableId ??= SelectedExerciceId;
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

    [HttpGet]
    [RequirePermission(ClaimDefinitions.ComptabiliteEdit)]
    public async Task<IActionResult> EditEcriture(int id)
    {
        await LoadClientAsync(SelectedClientId);
        await EnsureLedgerAsync(SelectedClientId);
        var detail = await _compta.GetEcritureByIdAsync(id);
        if (detail == null)
        {
            TempData["Error"] = "Écriture introuvable.";
            return RedirectToAction("Ecritures");
        }
        if (detail.Statut is "Validé" or "Validee")
        {
            TempData["Error"] = "Écriture validée — modification impossible.";
            return RedirectToAction("Details", new { id });
        }
        if (SelectedClientId is > 0)
            ViewBag.Comptes = await _compta.GetComptesAsync(SelectedClientId.Value);
        ViewBag.Journaux = await _compta.GetJournauxAsync(SelectedClientId);
        ViewBag.EcritureId = id;
        return View("CreateEcriture", new EcritureCreateDto
        {
            ClientId = detail.ClientId,
            Journal = detail.Journal,
            DateEcriture = detail.DateEcriture,
            Libelle = detail.Libelle,
            NumeroPiece = detail.NumeroPiece,
            Reference = detail.Reference,
            Lignes = detail.Lignes.Count >= 2 ? detail.Lignes : new List<LigneEcritureDto> { new(), new() },
            Valider = false
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.ComptabiliteEdit)]
    public async Task<IActionResult> EditEcriture(int id, EcritureCreateDto dto)
    {
        dto.ClientId = SelectedClientId ?? dto.ClientId;
        var result = await _compta.UpdateEcritureAsync(id, dto);
        if (result?.Success == true)
        {
            TempData["Success"] = result.Message;
            return RedirectToAction("Details", new { id });
        }
        SetApiError(result, "Erreur lors de la modification.");
        await LoadClientAsync(SelectedClientId);
        if (SelectedClientId is > 0)
            ViewBag.Comptes = await _compta.GetComptesAsync(SelectedClientId.Value);
        ViewBag.Journaux = await _compta.GetJournauxAsync(SelectedClientId);
        ViewBag.EcritureId = id;
        return View("CreateEcriture", dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.ComptabiliteEdit)]
    public async Task<IActionResult> DeleteEcriture(int id)
    {
        var result = await _compta.DeleteEcritureAsync(id);
        if (result?.Success == true) TempData["Success"] = result.Message;
        else SetApiError(result, "Suppression impossible.");
        return RedirectToAction("Ecritures");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.ComptabiliteCreate)]
    public async Task<IActionResult> ValidateEcriture(int id)
    {
        var result = await _compta.ValidateEcritureAsync(id);
        if (result?.Success == true) TempData["Success"] = result.Message;
        else SetApiError(result, "Erreur lors de la validation.");
        return RedirectToAction("Ecritures");
    }

    [RequirePermission(ClaimDefinitions.ComptabiliteDetail)]
    public async Task<IActionResult> ExportEcritures(int? clientId = null, string? journal = null, string? statut = null)
    {
        clientId ??= SelectedClientId;
        if (!clientId.HasValue) return RedirectToAction("Ecritures");
        var page = await _compta.GetEcrituresAsync(clientId, journal, 1, null, statut, SelectedExerciceId);
        var headers = new List<EcritureComptableDto>();
        if (page != null)
        {
            headers.AddRange(page.Items);
            for (var p = 2; p <= Math.Max(1, page.TotalPages); p++)
            {
                var next = await _compta.GetEcrituresAsync(clientId, journal, p, null, statut, SelectedExerciceId);
                if (next?.Items != null) headers.AddRange(next.Items);
            }
        }
        var sb = new StringBuilder();
        sb.AppendLine("Piece;Date;Journal;Libelle;Reference;NumCompte;LibelleCompte;Debit;Credit;Valider");
        foreach (var e in headers)
        {
            var detail = await _compta.GetEcritureByIdAsync(e.Id);
            if (detail?.Lignes == null || detail.Lignes.Count == 0)
            {
                sb.AppendLine($"{e.NumeroPiece};{e.DateEcriture:yyyy-MM-dd};{e.Journal};\"{e.Libelle}\";{e.Reference};{e.NumCompte};\"{e.LibelleCompte}\";{e.Debit};{e.Credit};{(e.Statut is "Validé" or "Validee" ? "1" : "0")}");
                continue;
            }
            foreach (var l in detail.Lignes)
            {
                sb.AppendLine($"{detail.NumeroPiece};{detail.DateEcriture:yyyy-MM-dd};{detail.Journal};\"{detail.Libelle}\";{detail.Reference};{l.NumCompte};\"{l.LibelleCompte}\";{l.Debit};{l.Credit};{(detail.Statut is "Validé" or "Validee" ? "1" : "0")}");
            }
        }
        return CsvFile($"ecritures-client{clientId}.csv", sb.ToString());
    }

    [RequirePermission(ClaimDefinitions.ComptabiliteDetail)]
    public IActionResult DownloadTemplateEcritures()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Piece;Date;Journal;Libelle;Reference;NumCompte;LibelleCompte;Debit;Credit;Valider");
        sb.AppendLine("OD-2026-0001;2026-06-15;OD;Achat fournitures;;6111;Achats;1200;0;0");
        sb.AppendLine("OD-2026-0001;2026-06-15;OD;Achat fournitures;;4411;Fournisseurs;0;1200;0");
        return CsvFile("modele-ecritures.csv", sb.ToString());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.ComptabiliteCreate)]
    public async Task<IActionResult> ImportEcritures(IFormFile? file, string? csvText, bool valider = false)
    {
        if (SelectedClientId is not > 0) return RedirectToAction("Ecritures");
        var content = await ReadCsvUploadAsync(file, csvText);
        var result = await _compta.ImportEcrituresAsync(SelectedClientId.Value, content, SelectedExerciceId, valider);
        SetImportTempData(result, "Import écritures échoué.");
        return RedirectToAction("Ecritures");
    }

    [RequirePermission(ClaimDefinitions.ComptabiliteDetail)]
    public IActionResult DownloadTemplateJournaux()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Code;Libelle;Type;ProchainNumero;Actif");
        sb.AppendLine("OD;Opérations diverses;OD;1;1");
        return CsvFile("modele-journaux.csv", sb.ToString());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.ComptabiliteEdit)]
    public async Task<IActionResult> ImportJournaux(IFormFile? file, string? csvText)
    {
        if (SelectedClientId is not > 0) return RedirectToAction("Journaux");
        var content = await ReadCsvUploadAsync(file, csvText);
        var result = await _compta.ImportJournauxAsync(SelectedClientId.Value, content);
        SetImportTempData(result, "Import journaux échoué.");
        return RedirectToAction("Journaux");
    }

    [RequirePermission(ClaimDefinitions.ComptabiliteDetail)]
    public IActionResult DownloadTemplateRapprochements()
    {
        var sb = new StringBuilder();
        sb.AppendLine("CompteBancaire;Periode;SoldeBanque;Libelle");
        sb.AppendLine("5141;2026-06;12500.00;BQ CIH juin");
        return CsvFile("modele-rapprochements.csv", sb.ToString());
    }

    [RequirePermission(ClaimDefinitions.ComptabiliteDetail)]
    public IActionResult DownloadTemplateSoldesOuverture()
    {
        var sb = new StringBuilder();
        sb.AppendLine("NumCompte;Libelle;SoldeDebiteur;SoldeCrediteur");
        sb.AppendLine("5141;Banque;10000;0");
        sb.AppendLine("1111;Capital;0;10000");
        return CsvFile("modele-soldes-ouverture.csv", sb.ToString());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.ComptabiliteCreate)]
    public async Task<IActionResult> ImportSoldesOuverture(IFormFile? file, string? csvText)
    {
        if (SelectedClientId is not > 0) return RedirectToAction("Balance");
        var content = await ReadCsvUploadAsync(file, csvText);
        var result = await _compta.ImportSoldesOuvertureAsync(SelectedClientId.Value, content, SelectedExerciceId);
        SetImportTempData(result, "Import soldes d'ouverture échoué.");
        return RedirectToAction("Balance");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.ComptabiliteCreate)]
    public async Task<IActionResult> ImportRapprochements(IFormFile? file, string? csvText)
    {
        if (SelectedClientId is not > 0) return RedirectToAction("Rapprochements");
        var content = await ReadCsvUploadAsync(file, csvText);
        var result = await _compta.ImportRapprochementsAsync(SelectedClientId.Value, content);
        SetImportTempData(result, "Import rapprochements échoué.");
        return RedirectToAction("Rapprochements");
    }

    [RequirePermission(ClaimDefinitions.ComptabiliteDetail)]
    public IActionResult DownloadTemplatePlan()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Numero;Libelle;Classe;Nature;Actif");
        sb.AppendLine("6118;Autres achats;6;General;1");
        return CsvFile("modele-plan-comptable.csv", sb.ToString());
    }

    private static async Task<string> ReadCsvUploadAsync(IFormFile? file, string? csvText)
    {
        if (file is { Length: > 0 })
        {
            using var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8);
            return await reader.ReadToEndAsync();
        }
        return csvText ?? "";
    }

    private void SetImportTempData(ApiResponse<CompteImportResultDto>? result, string failFallback)
    {
        if (result?.Success == true)
        {
            var msg = result.Message ?? "Import terminé.";
            if (result.Data?.Messages?.Count > 0)
                msg += " " + string.Join(" ", result.Data.Messages.Take(5));
            TempData["Success"] = msg;
        }
        else
            SetApiError(result, failFallback);
    }

    [RequirePermission(ClaimDefinitions.ComptabiliteDetail)]
    public async Task<IActionResult> Journaux(int? clientId = null)
    {
        clientId ??= SelectedClientId;
        await LoadClientAsync(clientId);
        await EnsureLedgerAsync(clientId);
        return View(await _compta.GetJournauxAsync(clientId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.ComptabiliteEdit)]
    public async Task<IActionResult> UpdateJournal(int id, string? libelle, int? prochainNumero, bool estActif = true)
    {
        var result = await _compta.UpdateJournalAsync(id, new JournalUpdateDto
        {
            Libelle = libelle,
            ProchainNumero = prochainNumero,
            EstActif = estActif
        });
        if (result?.Success == true) TempData["Success"] = result.Message;
        else SetApiError(result, "Erreur mise à jour journal.");
        return RedirectToAction("Journaux");
    }

    [RequirePermission(ClaimDefinitions.ComptabiliteDetail)]
    public async Task<IActionResult> GrandLivre(int? clientId = null, DateTime? dateDebut = null, DateTime? dateFin = null)
    {
        clientId ??= SelectedClientId;
        if (!clientId.HasValue) return RedirectToAction("Index");
        await LoadClientAsync(clientId);
        await EnsureExerciceContextAsync(clientId);
        ViewBag.DateDebut = dateDebut;
        ViewBag.DateFin = dateFin;
        return View(await _compta.GetGrandLivreAsync(clientId.Value, dateDebut, dateFin, SelectedExerciceId));
    }

    [RequirePermission(ClaimDefinitions.ComptabiliteDetail)]
    public async Task<IActionResult> Balance(int? clientId = null, DateTime? dateDebut = null, DateTime? dateFin = null)
    {
        clientId ??= SelectedClientId;
        if (!clientId.HasValue) return RedirectToAction("Index");
        await LoadClientAsync(clientId);
        await EnsureExerciceContextAsync(clientId);
        ViewBag.DateDebut = dateDebut;
        ViewBag.DateFin = dateFin;
        return View(await _compta.GetBalanceAsync(clientId.Value, dateDebut, dateFin, SelectedExerciceId));
    }

    [RequirePermission(ClaimDefinitions.ComptabiliteDetail)]
    public async Task<IActionResult> ExportBalance(int? clientId = null, DateTime? dateDebut = null, DateTime? dateFin = null)
    {
        clientId ??= SelectedClientId;
        if (!clientId.HasValue) return RedirectToAction("Balance");
        var data = await _compta.GetBalanceAsync(clientId.Value, dateDebut, dateFin, SelectedExerciceId);
        var sb = new StringBuilder();
        sb.AppendLine("Compte;Libelle;Debit;Credit;SoldeDebiteur;SoldeCrediteur");
        foreach (var b in data)
            sb.AppendLine($"{b.NumeroCompte};{b.LibelleCompte};{b.TotalDebit};{b.TotalCredit};{b.SoldeDebiteur};{b.SoldeCrediteur}");
        return CsvFile($"balance-client{clientId}.csv", sb.ToString());
    }

    [RequirePermission(ClaimDefinitions.ComptabiliteDetail)]
    public async Task<IActionResult> BalanceAgee(int? clientId = null)
    {
        clientId ??= SelectedClientId;
        if (!clientId.HasValue) return RedirectToAction("Index");
        await LoadClientAsync(clientId);
        return View(await _compta.GetBalanceAgeeAsync(clientId.Value));
    }

    [RequirePermission(ClaimDefinitions.ComptabiliteDetail)]
    public async Task<IActionResult> Rapprochements(int? clientId = null)
    {
        clientId ??= SelectedClientId;
        await LoadClientAsync(clientId);
        await EnsureLedgerAsync(clientId);
        return View(await _compta.GetRapprochementsAsync(clientId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.ComptabiliteCreate)]
    public async Task<IActionResult> CreateRapprochement(int clientId, string compteBancaire, string periode, decimal? soldeBanque = null)
    {
        clientId = SelectedClientId ?? clientId;
        var result = await _compta.CreateRapprochementAsync(clientId, compteBancaire, periode, soldeBanque);
        if (result?.Success == true) TempData["Success"] = result.Message;
        else SetApiError(result, "Erreur lors de la création du rapprochement.");
        return RedirectToAction("Rapprochements");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.ComptabiliteCreate)]
    public async Task<IActionResult> MarkRapproche(int id, decimal? soldeBanque = null, decimal? soldeCompta = null)
    {
        var result = await _compta.UpdateRapprochementAsync(id, new RapprochementUpdateDto
        {
            SoldeBanque = soldeBanque,
            SoldeCompta = soldeCompta,
            EstRapproche = true
        });
        if (result?.Success == true) TempData["Success"] = result.Message;
        else SetApiError(result, "Erreur mise à jour rapprochement.");
        return RedirectToAction("Rapprochements");
    }
}
