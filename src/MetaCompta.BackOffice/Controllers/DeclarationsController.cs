using MetaCompta.Application.DTOs;
using MetaCompta.BackOffice.Filters;
using MetaCompta.BackOffice.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Xml.Linq;

namespace MetaCompta.BackOffice.Controllers;

[RequireSelectedClient]
public class DeclarationsController : BaseController
{
    // Statuts dans lesquels la declaration est modifiable
    private static readonly HashSet<string> EditableStatuts = new(StringComparer.OrdinalIgnoreCase)
    {
        "Brouillon", "ACompleter", "EnCours", "EnPreparation"
    };

    private readonly IDeclarationFacade _declarations;
    private readonly IClientFacade _clients;
    private readonly IWorkflowFacade _workflow;

    public DeclarationsController(
        IReferenceDataFacade referenceData,
        IHttpContextAccessor contextAccessor,
        IDeclarationFacade declarations,
        IClientFacade clients,
        IWorkflowFacade workflow)
        : base(referenceData, contextAccessor)
    {
        _declarations = declarations;
        _clients = clients;
        _workflow = workflow;
    }

    // ── INDEX (all types) ───────────────────────────────────────
    public async Task<IActionResult> Index(string? type = null, string? statut = null, int page = 1)
    {
        var clientId = SelectedClientId;
        var result = await _declarations.GetAllAsync(clientId, type, statut, page);
        await LoadReferenceDataToViewBagAsync("TYPE_DECLARATION", "STATUT_DECLARATION");
        ViewBag.Type = type;
        ViewBag.Statut = statut;

        // Contract status from session — gates create/edit buttons
        var hasActiveContract = bool.TryParse(HttpContext.Session.GetString("HasActiveContract"), out var hac) && hac;
        ViewBag.HasActiveContract = hasActiveContract;

        return View(result);
    }

    // ── DETAILS ─────────────────────────────────────────────────
    public async Task<IActionResult> Details(int id)
    {
        var declaration = await _declarations.GetByIdAsync(id);
        if (declaration == null) { TempData["Error"] = "Déclaration introuvable."; return RedirectToAction("Index"); }
        ViewBag.Type = declaration.Type;
        await LoadReferenceDataToViewBagAsync("STATUT_DECLARATION");

        // Contract status from session — gates edit/status buttons in the view
        var hasActiveContract = bool.TryParse(HttpContext.Session.GetString("HasActiveContract"), out var hac) && hac;
        ViewBag.HasActiveContract = hasActiveContract;

        // Load type-specific data
        switch (declaration.Type)
        {
            case "TVA":
                ViewBag.TVAData = await _declarations.GetTVADataAsync(id);
                break;
            case "IS":
                ViewBag.ISData = await _declarations.GetISDataAsync(id);
                break;
            case "IR":
                ViewBag.IRData = await _declarations.GetIRDataAsync(id);
                break;
            case "CNSS":
                ViewBag.CNSSData = await _declarations.GetCNSSDataAsync(id);
                break;
        }

        // Load workflow instance for this declaration (if any)
        try
        {
            var wfInstances = await _workflow.GetInstancesAsync(clientId: declaration.ClientId, declarationId: id, page: 1);
            var activeWf = wfInstances?.Items?.FirstOrDefault(w => w.Status == "Active" || w.Status == "Paused");
            if (activeWf != null)
            {
                ViewBag.WorkflowInstance = activeWf;
                var currentStep = await _workflow.GetCurrentStepAsync(activeWf.Id);
                ViewBag.WorkflowCurrentStep = currentStep;
            }
            else if (wfInstances?.Items?.Any() == true)
            {
                ViewBag.WorkflowInstance = wfInstances.Items.First();
            }
            // Load available templates for this client + type
            var templates = await _workflow.GetTemplatesAsync(declarationType: declaration.Type, clientId: declaration.ClientId, page: 1);
            ViewBag.WorkflowTemplates = templates?.Items ?? new List<WorkflowTemplateDto>();
        }
        catch { /* Workflow data is supplementary, don't block Details page */ }

        // Populate ActionBar ViewBag
        await LoadWorkflowActionBarAsync(declaration);

        return View(declaration);
    }

    // ── CREATE (generic via modal) ──────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string type, int? anneeFiscale = null)
    {
        var clientId = SelectedClientId ?? 0;
        if (clientId == 0) { TempData["Error"] = "Aucun client sélectionné."; return RedirectToAction("Index"); }
        ApiResponse<DeclarationDto>? result = null;

        switch (type.ToUpper())
        {
            case "TVA":
                result = await _declarations.CreateTVAAsync(clientId);
                break;
            case "IS":
                result = await _declarations.CreateISAsync(clientId, anneeFiscale ?? DateTime.Now.Year);
                break;
            case "IR":
                result = await _declarations.CreateIRAsync(clientId, anneeFiscale ?? DateTime.Now.Year);
                break;
            case "CNSS":
                result = await _declarations.CreateCNSSAsync(clientId);
                break;
            default:
                TempData["Error"] = $"Type de déclaration '{type}' non supporté.";
                return RedirectToAction("Index", new { type });
        }

        if (result?.Success == true && result.Data != null)
        {
            TempData["Success"] = result.Message ?? $"Déclaration {type} créée avec succès.";
            // Redirect to the appropriate edit page
            return type.ToUpper() switch
            {
                "TVA" => RedirectToAction("EditTVA", new { id = result.Data.Id }),
                "IS" => RedirectToAction("EditIS", new { id = result.Data.Id }),
                "IR" => RedirectToAction("EditIR", new { id = result.Data.Id }),
                "CNSS" => RedirectToAction("EditCNSS", new { id = result.Data.Id }),
                _ => RedirectToAction("Details", new { id = result.Data.Id })
            };
        }

        TempData["Error"] = result?.Message ?? "Erreur lors de la création.";
        if (result?.Errors?.Count > 0)
            TempData["ErrorDetails"] = string.Join("|||", result.Errors);
        return RedirectToAction("Index", new { type });
    }

    // ── LEGACY TVA CREATE (backward compat) ─────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTVA()
    {
        return await Create("TVA", null);
    }

    // ── DELETE ───────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _declarations.DeleteAsync(id);
        SetApiResult(result, "Déclaration supprimée.", "Erreur lors de la suppression.");
        return RedirectToAction("Index");
    }

    // ═══════════════════════════════════════════════════════════
    //  TVA EDIT
    // ═══════════════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> EditTVA(int id)
    {
        var declaration = await _declarations.GetByIdAsync(id);
        if (declaration == null) { TempData["Error"] = "Déclaration introuvable."; return RedirectToAction("Index"); }
        if (!EditableStatuts.Contains(declaration.Statut ?? "")) { TempData["Error"] = "Modification impossible : la déclaration est dans un statut verrouillé. Seuls Brouillon, À Compléter, En Cours et En Préparation sont modifiables."; return RedirectToAction("Details", new { id }); }
        ViewBag.Type = "TVA";
        var hasActiveContract = bool.TryParse(HttpContext.Session.GetString("HasActiveContract"), out var hac) && hac;
        if (!hasActiveContract) { TempData["Error"] = "Modification impossible : aucun contrat actif pour ce client."; return RedirectToAction("Details", new { id }); }
        ViewBag.Declaration = declaration;
        if (declaration.ClientId > 0)
            ViewBag.Client = await _clients.GetByIdAsync(declaration.ClientId);
        ViewBag.CabinetName = CabinetName;
        await LoadWorkflowActionBarAsync(declaration);
        var tvaData = await _declarations.GetTVADataAsync(id);
        return View(tvaData ?? new DeclarationTVADto { DeclarationId = id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTVA(int id, DeclarationTVADto dto)
    {
        var declCheck = await _declarations.GetByIdAsync(id);
        if (declCheck != null && !EditableStatuts.Contains(declCheck.Statut ?? ""))
        { TempData["Error"] = "Modification interdite dans ce statut."; return RedirectToAction("Details", new { id }); }
        // Only update manual adjustment fields (Immobilisations + Crédit Précédent),
        // then recalculate totals from lines. Never overwrite line-calculated totals.
        var result = await _declarations.SaveTVAAjustementsAsync(id, dto.TVARecuperableImmobilisations, dto.CreditPrecedent);
        if (result?.Success == true)
        {
            TempData["Success"] = "Ajustements TVA enregistrés et totaux recalculés.";
            return RedirectToAction("EditTVA", new { id });
        }
        SetApiError(result, "Erreur lors de la sauvegarde des ajustements TVA.");
        ViewBag.Type = "TVA";
        ViewBag.Declaration = await _declarations.GetByIdAsync(id);
        if (ViewBag.Declaration is DeclarationDto decl && decl.ClientId > 0)
            ViewBag.Client = await _clients.GetByIdAsync(decl.ClientId);
        ViewBag.CabinetName = CabinetName;
        var tvaData = await _declarations.GetTVADataAsync(id);
        return View(tvaData ?? dto);
    }

    // ═══════════════════════════════════════════════════════════
    //  IS EDIT
    // ═══════════════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> EditIS(int id)
    {
        var declaration = await _declarations.GetByIdAsync(id);
        if (declaration == null) { TempData["Error"] = "Déclaration introuvable."; return RedirectToAction("Index"); }
        if (!EditableStatuts.Contains(declaration.Statut ?? "")) { TempData["Error"] = "Modification impossible : la déclaration est dans un statut verrouillé. Seuls Brouillon, À Compléter, En Cours et En Préparation sont modifiables."; return RedirectToAction("Details", new { id }); }
        ViewBag.Type = "IS";
        var hasActiveContract = bool.TryParse(HttpContext.Session.GetString("HasActiveContract"), out var hac) && hac;
        if (!hasActiveContract) { TempData["Error"] = "Modification impossible : aucun contrat actif pour ce client."; return RedirectToAction("Details", new { id }); }
        ViewBag.Declaration = declaration;
        await LoadWorkflowActionBarAsync(declaration);
        var isData = await _declarations.GetISDataAsync(id);
        return View(isData ?? new DeclarationISDto { DeclarationId = id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditIS(int id, DeclarationISDto dto)
    {
        var declCheck = await _declarations.GetByIdAsync(id);
        if (declCheck != null && !EditableStatuts.Contains(declCheck.Statut ?? ""))
        { TempData["Error"] = "Modification interdite dans ce statut."; return RedirectToAction("Details", new { id }); }
        var result = await _declarations.SaveISDataAsync(id, dto);
        if (result?.Success == true)
        {
            TempData["Success"] = "Données IS sauvegardées avec succès.";
            return RedirectToAction("Details", new { id });
        }
        SetApiError(result, "Erreur lors de la sauvegarde IS.");
        ViewBag.Type = "IS";
        ViewBag.Declaration = await _declarations.GetByIdAsync(id);
        return View(dto);
    }

    // ═══════════════════════════════════════════════════════════
    //  IR EDIT
    // ═══════════════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> EditIR(int id)
    {
        var declaration = await _declarations.GetByIdAsync(id);
        if (declaration == null) { TempData["Error"] = "Déclaration introuvable."; return RedirectToAction("Index"); }
        if (!EditableStatuts.Contains(declaration.Statut ?? "")) { TempData["Error"] = "Modification impossible : la déclaration est dans un statut verrouillé. Seuls Brouillon, À Compléter, En Cours et En Préparation sont modifiables."; return RedirectToAction("Details", new { id }); }
        ViewBag.Type = "IR";
        var hasActiveContract = bool.TryParse(HttpContext.Session.GetString("HasActiveContract"), out var hac) && hac;
        if (!hasActiveContract) { TempData["Error"] = "Modification impossible : aucun contrat actif pour ce client."; return RedirectToAction("Details", new { id }); }
        ViewBag.Declaration = declaration;
        await LoadWorkflowActionBarAsync(declaration);
        var irData = await _declarations.GetIRDataAsync(id);
        return View(irData ?? new DeclarationIRDto { DeclarationId = id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditIR(int id, DeclarationIRDto dto)
    {
        var declCheck = await _declarations.GetByIdAsync(id);
        if (declCheck != null && !EditableStatuts.Contains(declCheck.Statut ?? ""))
        { TempData["Error"] = "Modification interdite dans ce statut."; return RedirectToAction("Details", new { id }); }
        var result = await _declarations.SaveIRDataAsync(id, dto);
        if (result?.Success == true)
        {
            TempData["Success"] = "Données IR sauvegardées avec succès.";
            return RedirectToAction("Details", new { id });
        }
        SetApiError(result, "Erreur lors de la sauvegarde IR.");
        ViewBag.Type = "IR";
        ViewBag.Declaration = await _declarations.GetByIdAsync(id);
        return View(dto);
    }

    // ═══════════════════════════════════════════════════════════
    //  CNSS EDIT
    // ═══════════════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> EditCNSS(int id)
    {
        var declaration = await _declarations.GetByIdAsync(id);
        if (declaration == null) { TempData["Error"] = "Déclaration introuvable."; return RedirectToAction("Index"); }
        if (!EditableStatuts.Contains(declaration.Statut ?? "")) { TempData["Error"] = "Modification impossible : la déclaration est dans un statut verrouillé. Seuls Brouillon, À Compléter, En Cours et En Préparation sont modifiables."; return RedirectToAction("Details", new { id }); }
        ViewBag.Type = "CNSS";
        var hasActiveContract = bool.TryParse(HttpContext.Session.GetString("HasActiveContract"), out var hac) && hac;
        if (!hasActiveContract) { TempData["Error"] = "Modification impossible : aucun contrat actif pour ce client."; return RedirectToAction("Details", new { id }); }
        ViewBag.Declaration = declaration;
        await LoadWorkflowActionBarAsync(declaration);
        var cnssData = await _declarations.GetCNSSDataAsync(id);
        return View(cnssData ?? new DeclarationCNSSDto { DeclarationId = id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCNSS(int id, DeclarationCNSSDto dto)
    {
        var declCheck = await _declarations.GetByIdAsync(id);
        if (declCheck != null && !EditableStatuts.Contains(declCheck.Statut ?? ""))
        { TempData["Error"] = "Modification interdite dans ce statut."; return RedirectToAction("Details", new { id }); }
        var result = await _declarations.SaveCNSSDataAsync(id, dto);
        if (result?.Success == true)
        {
            TempData["Success"] = "Données CNSS sauvegardées avec succès.";
            return RedirectToAction("Details", new { id });
        }
        SetApiError(result, "Erreur lors de la sauvegarde CNSS.");
        ViewBag.Type = "CNSS";
        ViewBag.Declaration = await _declarations.GetByIdAsync(id);
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CalculateCNSS(int id)
    {
        var cnssData = await _declarations.CalculateCNSSAsync(id);
        if (cnssData != null)
        {
            var result = await _declarations.SaveCNSSDataAsync(id, cnssData);
            TempData["Success"] = "Calcul CNSS effectué et sauvegardé.";
        }
        else
        {
            TempData["Error"] = "Erreur lors du calcul CNSS.";
        }
        return RedirectToAction("EditCNSS", new { id });
    }

    // ── STATUS ──────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id, string newStatut, string? comment = null)
    {
        var result = await _declarations.ChangeStatusAsync(id, newStatut, comment);
        SetApiResult(result, null, "Erreur lors du changement de statut.");
        return RedirectToAction("Details", new { id });
    }

    // ═══════════════════════════════════════════════════════════
    //  WORKFLOW ACTIONS ON DECLARATIONS
    // ═══════════════════════════════════════════════════════════

    /// <summary>Start a workflow for a specific declaration.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartWorkflow(int declarationId, int? workflowTemplateId = null)
    {
        var declaration = await _declarations.GetByIdAsync(declarationId);
        if (declaration == null) { TempData["Error"] = "Déclaration introuvable."; return RedirectToAction("Index"); }

        var dto = new StartWorkflowDto
        {
            DeclarationId = declarationId,
            ClientId = declaration.ClientId,
            DeclarationType = declaration.Type,
            WorkflowTemplateId = workflowTemplateId
        };

        var result = await _workflow.StartWorkflowAsync(dto);
        if (result?.Success == true)
        {
            TempData["Success"] = result.Message ?? "Workflow démarré avec succès pour cette déclaration.";
        }
        else
        {
            TempData["Error"] = result?.Message ?? "Erreur lors du démarrage du workflow.";
            if (result?.Errors?.Count > 0)
                TempData["ErrorDetails"] = string.Join("|||", result.Errors);
        }
        return RedirectToAction("Details", new { id = declarationId });
    }

    /// <summary>Advance the current workflow step for a declaration.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdvanceWorkflowStep(int declarationId, int workflowInstanceId, string? notes = null, string? comment = null, bool forceSkip = false)
    {
        var dto = new AdvanceStepDto { Notes = notes, Comment = comment, ForceSkip = forceSkip };
        var result = await _workflow.AdvanceStepAsync(workflowInstanceId, dto);
        if (result?.Success == true)
        {
            TempData["Success"] = "Étape workflow avancée avec succès.";
        }
        else
        {
            TempData["Error"] = result?.Message ?? "Erreur lors de l'avancement de l'étape.";
        }
        return RedirectToAction("Details", new { id = declarationId });
    }

    /// <summary>Pause the workflow for a declaration.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PauseDeclarationWorkflow(int declarationId, int workflowInstanceId)
    {
        var result = await _workflow.PauseWorkflowAsync(workflowInstanceId);
        if (result?.Success == true) TempData["Success"] = "Workflow mis en pause.";
        else TempData["Error"] = result?.Message ?? "Erreur lors de la mise en pause.";
        return RedirectToAction("Details", new { id = declarationId });
    }

    /// <summary>Resume the workflow for a declaration.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResumeDeclarationWorkflow(int declarationId, int workflowInstanceId)
    {
        var result = await _workflow.ResumeWorkflowAsync(workflowInstanceId);
        if (result?.Success == true) TempData["Success"] = "Workflow repris.";
        else TempData["Error"] = result?.Message ?? "Erreur lors de la reprise.";
        return RedirectToAction("Details", new { id = declarationId });
    }

    /// <summary>Cancel the workflow for a declaration.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelDeclarationWorkflow(int declarationId, int workflowInstanceId, string reason = "")
    {
        var result = await _workflow.CancelWorkflowAsync(workflowInstanceId, reason);
        if (result?.Success == true) TempData["Success"] = "Workflow annulé.";
        else TempData["Error"] = result?.Message ?? "Erreur lors de l'annulation.";
        return RedirectToAction("Details", new { id = declarationId });
    }

    // ═══════════════════════════════════════════════════════════
    //  TVA INVOICE LINES (Lignes Factures)
    // ═══════════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTVALigne(int declarationId, TVALigneFactureDto ligne)
    {
        ligne.DeclarationTVAId = declarationId;
        ligne.MontantTVA = ligne.MontantHT * ligne.TauxTVA / 100m;
        ligne.MontantTTC = ligne.MontantHT + ligne.MontantTVA;
        var result = await _declarations.AddTVALigneAsync(declarationId, ligne);
        if (result?.Success == true)
            TempData["Success"] = "Ligne ajoutée avec succès.";
        else
            TempData["Error"] = result?.Message ?? "Erreur lors de l'ajout de la ligne.";
        return RedirectToAction("EditTVA", new { id = declarationId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTVALigne(int declarationId, int ligneId)
    {
        var result = await _declarations.DeleteTVALigneAsync(declarationId, ligneId);
        if (result?.Success == true)
            TempData["Success"] = "Ligne supprimée.";
        else
            TempData["Error"] = result?.Message ?? "Erreur lors de la suppression.";
        return RedirectToAction("EditTVA", new { id = declarationId });
    }


    // ═══════════════════════════════════════════════════════════
    //  CSV IMPORT / TEMPLATE for TVA Invoice Lines
    // ═══════════════════════════════════════════════════════════
    [HttpGet]
    public IActionResult DownloadTVATemplate()
    {
        var csv = new StringBuilder();
        csv.AppendLine("TypePiece;NumeroFacture;DateFacture;FournisseurClient;ICE;IdentifiantFiscal;Description;MontantHT;TauxTVA;TypeOperation;Regime");
        csv.AppendLine("Facture;FAC-001;2026-01-15;ACME SARL;001234567890123;12345678;Achat marchandises;10000.00;20;Achat;Encaissement");
        csv.AppendLine("Facture;FAC-002;2026-01-20;Client XYZ;009876543210987;87654321;Vente services;5000.00;20;Vente;Encaissement");
        csv.AppendLine("Avoir;AV-001;2026-01-25;ACME SARL;001234567890123;12345678;Retour marchandises;2000.00;20;Achat;Encaissement");
        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
        return File(bytes, "text/csv", "Modele_Import_TVA_Lignes.csv");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportTVALignes(int declarationId, IFormFile fichierCSV)
    {
        if (fichierCSV == null || fichierCSV.Length == 0)
        {
            TempData["Error"] = "Veuillez sélectionner un fichier CSV.";
            return RedirectToAction("EditTVA", new { id = declarationId });
        }

        var lignes = new List<TVALigneFactureDto>();
        var errors = new List<string>();
        int lineNum = 0;

        using (var reader = new StreamReader(fichierCSV.OpenReadStream(), Encoding.UTF8))
        {
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                lineNum++;
                if (lineNum == 1) continue; // skip header

                if (string.IsNullOrWhiteSpace(line)) continue;

                var cols = line.Split(';');
                if (cols.Length < 11)
                {
                    errors.Add($"Ligne {lineNum}: nombre de colonnes insuffisant ({cols.Length}/11).");
                    continue;
                }

                try
                {
                    var dto = new TVALigneFactureDto
                    {
                        TypePiece = cols[0].Trim(),
                        NumeroFacture = cols[1].Trim(),
                        DateFacture = DateTime.TryParse(cols[2].Trim(), out var dt) ? dt : DateTime.Today,
                        FournisseurClient = cols[3].Trim(),
                        ICE = cols[4].Trim(),
                        IdentifiantFiscal = cols[5].Trim(),
                        Description = cols[6].Trim(),
                        MontantHT = decimal.TryParse(cols[7].Trim().Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var ht) ? ht : 0m,
                        TauxTVA = decimal.TryParse(cols[8].Trim().Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var tx) ? tx : 20m,
                        TypeOperation = cols[9].Trim(),
                        Regime = cols[10].Trim()
                    };
                    dto.MontantTVA = Math.Round(dto.MontantHT * dto.TauxTVA / 100m, 2);
                    dto.MontantTTC = dto.MontantHT + dto.MontantTVA;
                    lignes.Add(dto);
                }
                catch (Exception ex)
                {
                    errors.Add($"Ligne {lineNum}: {ex.Message}");
                }
            }
        }

        if (lignes.Count == 0)
        {
            TempData["Error"] = "Aucune ligne valide trouvée dans le fichier." + (errors.Count > 0 ? " Erreurs: " + string.Join(" | ", errors.Take(5)) : "");
            return RedirectToAction("EditTVA", new { id = declarationId });
        }

        var result = await _declarations.ImportTVALignesAsync(declarationId, lignes);
        if (result?.Success == true)
        {
            TempData["Success"] = $"{lignes.Count} ligne(s) importée(s) avec succès.";
            if (errors.Count > 0)
                TempData["Warning"] = $"{errors.Count} ligne(s) ignorée(s): " + string.Join(" | ", errors.Take(5));
        }
        else
        {
            TempData["Error"] = result?.Message ?? "Erreur lors de l'importation.";
        }
        return RedirectToAction("EditTVA", new { id = declarationId });
    }


    // ═══════════════════════════════════════════════════════════
    //  EDI XML EXPORT - Télédéclaration TVA Maroc
    // ═══════════════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> ExportTVAXml(int id)
    {
        var declaration = await _declarations.GetByIdAsync(id);
        if (declaration == null) { TempData["Error"] = "Déclaration introuvable."; return RedirectToAction("Index"); }
        var tvaData = await _declarations.GetTVADataAsync(id);
        if (tvaData == null) { TempData["Error"] = "Données TVA introuvables."; return RedirectToAction("Details", new { id }); }
        ClientDto? client = null;
        if (declaration.ClientId > 0)
            client = await _clients.GetByIdAsync(declaration.ClientId);
        var cabinetName = CabinetName ?? "Cabinet Comptable";
        var xml = BuildTVAXml(declaration, tvaData, client, cabinetName);
        var bytes = Encoding.UTF8.GetBytes(xml);
        var fileName = $"TVA_{declaration.Periode}_{client?.ICE ?? "UNKNOWN"}_{DateTime.Now:yyyyMMdd}.xml";
        return File(bytes, "application/xml", fileName);
    }

    private static string BuildTVAXml(DeclarationDto decl, DeclarationTVADto tva, ClientDto? client, string cabinetName)
    {
        var ns = XNamespace.Get("http://www.tax.gov.ma/simpl/tva");
        var doc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"),
            new XElement(ns + "DeclarationTVA",
                new XAttribute("version", "1.0"),
                new XElement(ns + "Identifiant",
                    new XElement(ns + "ICE", client?.ICE ?? ""),
                    new XElement(ns + "IdentifiantFiscal", client?.IF ?? ""),
                    new XElement(ns + "RC", client?.RC ?? ""),
                    new XElement(ns + "RaisonSociale", client?.RaisonSociale ?? ""),
                    new XElement(ns + "Adresse", client?.AdresseSiegeSocial ?? ""),
                    new XElement(ns + "Ville", client?.VilleSiegeSocial ?? ""),
                    new XElement(ns + "NumeroPatente", client?.NumeroPatente ?? ""),
                    new XElement(ns + "NumeroCNSS", client?.NumeroCNSS ?? "")
                ),
                new XElement(ns + "Cabinet",
                    new XElement(ns + "NomCabinet", cabinetName)
                ),
                new XElement(ns + "Periode",
                    new XElement(ns + "DateDebut", decl.DateDebut.ToString("yyyy-MM-dd")),
                    new XElement(ns + "DateFin", decl.DateFin.ToString("yyyy-MM-dd")),
                    new XElement(ns + "Periodicite", decl.Periode),
                    new XElement(ns + "RegimeTVA", client?.TypeTVA ?? "Mensuelle")
                ),
                new XElement(ns + "ChiffreAffaires",
                    new XElement(ns + "CA_Taux20", tva.CAHT_20),
                    new XElement(ns + "CA_Taux14", tva.CAHT_14),
                    new XElement(ns + "CA_Taux10", tva.CAHT_10),
                    new XElement(ns + "CA_Taux7", tva.CAHT_7),
                    new XElement(ns + "CA_Exonere", tva.CAExonere)
                ),
                new XElement(ns + "TVACollectee",
                    new XElement(ns + "TVA_Taux20", tva.TVA_20),
                    new XElement(ns + "TVA_Taux14", tva.TVA_14),
                    new XElement(ns + "TVA_Taux10", tva.TVA_10),
                    new XElement(ns + "TVA_Taux7", tva.TVA_7),
                    new XElement(ns + "TotalTVACollectee", tva.TVACollecteeTotale)
                ),
                new XElement(ns + "TVARecuperable",
                    new XElement(ns + "TVACharges", tva.TVARecuperableCharges),
                    new XElement(ns + "TVAImmobilisations", tva.TVARecuperableImmobilisations),
                    new XElement(ns + "CreditReportPrecedent", tva.CreditPrecedent),
                    new XElement(ns + "TotalTVARecuperable", tva.TVARecuperableTotale)
                ),
                new XElement(ns + "Resultat",
                    new XElement(ns + "TVANette", tva.TVANette),
                    new XElement(ns + "TVAaPayer", tva.TVAaPayer),
                    new XElement(ns + "CreditTVA", tva.CreditTVA)
                ),
                new XElement(ns + "LignesFactures",
                    tva.LignesFactures.Select(l =>
                        new XElement(ns + "Ligne",
                            new XElement(ns + "TypePiece", l.TypePiece),
                            new XElement(ns + "NumeroFacture", l.NumeroFacture ?? ""),
                            new XElement(ns + "DateFacture", l.DateFacture.ToString("yyyy-MM-dd")),
                            new XElement(ns + "FournisseurClient", l.FournisseurClient ?? ""),
                            new XElement(ns + "ICE", l.ICE ?? ""),
                            new XElement(ns + "IdentifiantFiscal", l.IdentifiantFiscal ?? ""),
                            new XElement(ns + "Description", l.Description ?? ""),
                            new XElement(ns + "MontantHT", l.MontantHT),
                            new XElement(ns + "TauxTVA", l.TauxTVA),
                            new XElement(ns + "MontantTVA", l.MontantTVA),
                            new XElement(ns + "MontantTTC", l.MontantTTC),
                            new XElement(ns + "TypeOperation", l.TypeOperation)
                        )
                    )
                )
            )
        );
        return doc.ToString();
    }


    // ???????????????????????????????????????????????????????????
    //  GENERIC WORKFLOW ACTION BAR DATA LOADER
    //  Populates ViewBag for _WorkflowActionBar partial view
    // ???????????????????????????????????????????????????????????
    private async Task LoadWorkflowActionBarAsync(DeclarationDto declaration)
    {
        ViewBag.ActionBarDeclaration = declaration;
        ViewBag.ActionBarUserRole = HttpContext.Session.GetString("UserRole") ?? "";
        ViewBag.ActionBarUserId = HttpContext.Session.GetString("UserId") ?? "";
        ViewBag.ActionBarIsClientPortal = false;

        try
        {
            // Load workflow instance for this declaration
            var wfInstances = await _workflow.GetInstancesAsync(
                clientId: declaration.ClientId,
                declarationId: declaration.Id,
                page: 1);
            var activeWf = wfInstances?.Items?.FirstOrDefault(w => w.Status == "Active" || w.Status == "Paused");
            if (activeWf == null && wfInstances?.Items?.Any() == true)
                activeWf = wfInstances.Items.OrderByDescending(w => w.StartedAt).First();

            if (activeWf != null)
            {
                ViewBag.ActionBarWorkflow = activeWf;
                var currentStep = await _workflow.GetCurrentStepAsync(activeWf.Id);
                ViewBag.ActionBarCurrentStep = currentStep;
            }

            // Load available templates
            var templates = await _workflow.GetTemplatesAsync(
                declarationType: declaration.Type,
                clientId: declaration.ClientId,
                page: 1);
            ViewBag.ActionBarTemplates = templates?.Items ?? new List<WorkflowTemplateDto>();
        }
        catch { /* Workflow data is supplementary */ }
    }

}
