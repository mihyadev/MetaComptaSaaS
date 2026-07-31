using MetaCompta.Application.DTOs;
using MetaCompta.BackOffice.Filters;
using MetaCompta.BackOffice.Helpers;
using MetaCompta.BackOffice.Services;
using MetaCompta.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Xml.Linq;

namespace MetaCompta.BackOffice.Controllers;

[RequireSelectedClient]
public class DeclarationsController : BaseController
{
    // Statuts dans lesquels la déclaration est modifiable (inclut étapes WF TVA: contrôle / calcul)
    private static readonly HashSet<string> EditableStatuts = new(StringComparer.OrdinalIgnoreCase)
    {
        RefCodes.StatutDeclaration.Brouillon,
        RefCodes.StatutDeclaration.ACompleter,
        RefCodes.StatutDeclaration.EnCours,
        RefCodes.StatutDeclaration.EnPreparation,
        RefCodes.StatutDeclaration.CreeeParSysteme,
        RefCodes.StatutDeclaration.DocumentsDemandes,
        RefCodes.StatutDeclaration.DocumentsRecus,
        RefCodes.StatutDeclaration.ControleEnCours,
        RefCodes.StatutDeclaration.CalculTVA,
        RefCodes.StatutDeclaration.EnAttenteCorrection,
        RefCodes.StatutDeclaration.ACorriger,
    };

    private readonly IDeclarationFacade _declarations;
    private readonly IClientFacade _clients;
    private readonly IWorkflowFacade _workflow;
    private readonly IDocumentFacade _documents;
    private readonly IMessageFacade _messages;

    public DeclarationsController(
        IReferenceDataFacade referenceData,
        IHttpContextAccessor contextAccessor,
        IDeclarationFacade declarations,
        IClientFacade clients,
        IWorkflowFacade workflow,
        IDocumentFacade documents,
        IMessageFacade messages)
        : base(referenceData, contextAccessor)
    {
        _declarations = declarations;
        _clients = clients;
        _workflow = workflow;
        _documents = documents;
        _messages = messages;
    }

    // ── INDEX (all types) ───────────────────────────────────────
    [RequirePermission(ClaimDefinitions.DeclarationView)]
    public async Task<IActionResult> Index(string? type = null, string? statut = null, int page = 1)
    {
        var clientId = SelectedClientId;
        var result = await _declarations.GetAllAsync(clientId, type, statut, page);
        await LoadReferenceDataToViewBagAsync("TYPE_DECLARATION", "STATUT_DECLARATION");
        ViewBag.Type = type;
        ViewBag.Statut = statut;
        PopulateContractAccessViewBag();

        return View(result);
    }

    // ── DETAILS ─────────────────────────────────────────────────
    [RequirePermission(ClaimDefinitions.DeclarationView)]
    public async Task<IActionResult> Details(int id)
    {
        var declaration = await _declarations.GetByIdAsync(id);
        if (declaration == null) { TempData["Error"] = "Déclaration introuvable."; return RedirectToAction("Index"); }
        ViewBag.Type = declaration.Type;
        await LoadReferenceDataToViewBagAsync("STATUT_DECLARATION");
        PopulateContractAccessViewBag();

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

        // Load documents for this declaration
        try
        {
            ViewBag.Documents = await _documents.GetByDeclarationIdAsync(id) ?? new List<DocumentDto>();
        }
        catch { ViewBag.Documents = new List<DocumentDto>(); }

        // Load messages for this declaration
        try
        {
            ViewBag.Messages = await _messages.GetByDeclarationAsync(id) ?? new List<MessageDto>();
        }
        catch { ViewBag.Messages = new List<MessageDto>(); }

        // Timeline: workflow history + declaration status history
        try
        {
            if (ViewBag.WorkflowInstance is WorkflowInstanceDto wi)
                ViewBag.WorkflowHistory = await _workflow.GetHistoryAsync(wi.Id) ?? new List<WorkflowHistoryDto>();
            else
                ViewBag.WorkflowHistory = new List<WorkflowHistoryDto>();
        }
        catch { ViewBag.WorkflowHistory = new List<WorkflowHistoryDto>(); }

        try
        {
            ViewBag.DeclarationStatusHistory = await _declarations.GetStatusHistoryAsync(id) ?? new List<DeclarationHistoryDto>();
        }
        catch { ViewBag.DeclarationStatusHistory = new List<DeclarationHistoryDto>(); }

        return View(declaration);
    }

    /// <summary>Prendre / Ouvrir the current workflow step (exclusive soft lock).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClaimWorkflowStep(int declarationId, int workflowInstanceId, bool forceOverride = false, string? reason = null)
    {
        var result = await _workflow.ClaimStepAsync(workflowInstanceId, forceOverride, reason);
        if (result?.Success == true)
            TempData["Success"] = result.Message ?? "Étape prise en charge.";
        else
            TempData["Error"] = result?.Message ?? "Impossible de prendre cette étape.";
        return RedirectToAction("Details", new { id = declarationId });
    }

    // ── SEND MESSAGE ────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.DeclarationView, ClaimDefinitions.DeclarationEdit)]
    public async Task<IActionResult> SendMessage(int declarationId, int clientId, string sujet, string contenu)
    {
        if (string.IsNullOrWhiteSpace(contenu))
        {
            TempData["Error"] = "Le message ne peut pas être vide.";
            return RedirectToAction("Details", new { id = declarationId });
        }
        var dto = new SendMessageDto
        {
            ClientId = clientId,
            DeclarationId = declarationId,
            Sujet = sujet,
            Contenu = contenu
        };
        await _messages.SendAsync(dto);
        TempData["Success"] = "Message envoyé.";
        return RedirectToAction("Details", new { id = declarationId });
    }

    // ── UPLOAD DOCUMENT ─────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.DocumentUpload)]
    public async Task<IActionResult> UploadDocument(int declarationId, IFormFile file, string titre, string categorie)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Veuillez sélectionner un fichier.";
            return RedirectToAction("Details", new { id = declarationId });
        }

        var declaration = await _declarations.GetByIdAsync(declarationId);
        using var stream = file.OpenReadStream();
        var result = await _documents.UploadAsync(stream, file.FileName, file.ContentType, file.Length,
            titre ?? file.FileName, categorie ?? "Fiscal", declaration?.ClientId, declarationId);

        TempData[result?.Success == true ? "Success" : "Error"] =
            result?.Success == true ? "Document uploadé avec succès." : (result?.Message ?? "Erreur lors de l'upload.");
        return RedirectToAction("Details", new { id = declarationId });
    }

    // ── CREATE (generic via modal) ──────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.DeclarationCreate)]
    public async Task<IActionResult> Create(string type, int? anneeFiscale = null)
    {
        var clientId = SelectedClientId ?? 0;
        if (clientId == 0) { TempData["Error"] = "Aucun client sélectionné."; return RedirectToAction("Index"); }
        if (!RequiresManageAccess()) return RedirectToAction("Index", new { type });
        if (!RequiresPermission(PermissionHelper.Declaration.Create)) return RedirectToAction("Index", new { type });

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
    [RequirePermission(ClaimDefinitions.DeclarationCreate)]
    public async Task<IActionResult> CreateTVA()
    {
        return await Create("TVA", null);
    }

    // ── DELETE ───────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.DeclarationDelete)]
    public async Task<IActionResult> Delete(int id)
    {
        if (!RequiresManageAccess()) return RedirectToAction("Index");
        var canDelete = PermissionHelper.HasPermission(HttpContext.Session, PermissionHelper.Declaration.Delete)
            || PermissionHelper.HasPermission(HttpContext.Session, PermissionHelper.Declaration.Edit);
        if (!canDelete)
        {
            TempData["Error"] = "Vous n'avez pas la permission de supprimer cette déclaration.";
            return RedirectToAction("Details", new { id });
        }
        var result = await _declarations.DeleteAsync(id);
        SetApiResult(result, "Déclaration supprimée.", "Erreur lors de la suppression.");
        return RedirectToAction("Index");
    }

    // ═══════════════════════════════════════════════════════════
    //  TVA EDIT
    // ═══════════════════════════════════════════════════════════
    [HttpGet]
    [RequirePermission(ClaimDefinitions.TvaEdit, ClaimDefinitions.DeclarationEdit)]
    public async Task<IActionResult> EditTVA(int id)
    {
        var declaration = await _declarations.GetByIdAsync(id);
        if (declaration == null) { TempData["Error"] = "Déclaration introuvable."; return RedirectToAction("Index"); }
        if (!EditableStatuts.Contains(declaration.Statut ?? "")) { TempData["Error"] = "Modification impossible : la déclaration est dans un statut verrouillé (déjà validée / soumise / clôturée)."; return RedirectToAction("Details", new { id }); }
        ViewBag.Type = "TVA";
        PopulateContractAccessViewBag();
        if (!(ViewBag.CanEditDeclaration as bool? ?? false))
        {
            TempData["Error"] = ViewBag.IsConsultMode == true
                ? (ViewBag.ContractAccessReasonLabel as string ?? "Modification impossible : mode consultation.")
                : "Vous n'avez pas la permission de modifier cette déclaration.";
            return RedirectToAction("Details", new { id });
        }
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
    [RequirePermission(ClaimDefinitions.TvaEdit, ClaimDefinitions.DeclarationEdit)]
    public async Task<IActionResult> EditTVA(int id, DeclarationTVADto dto)
    {
        if (!RequiresManageAccess()) return RedirectToAction("Details", new { id });
        if (!RequiresPermission(PermissionHelper.Declaration.Edit)) return RedirectToAction("Details", new { id });
        var declCheck = await _declarations.GetByIdAsync(id);
        if (declCheck != null && !EditableStatuts.Contains(declCheck.Statut ?? ""))
        { TempData["Error"] = "Modification interdite dans ce statut."; return RedirectToAction("Details", new { id }); }
        // Maroc: Facturée − Déductible − Crédit déclaration précédente
        var result = await _declarations.SaveTVAAjustementsAsync(id, dto.TVACollecteeTotale, dto.TVARecuperableTotale, dto.CreditPrecedent);
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
    [RequirePermission(ClaimDefinitions.IsEdit, ClaimDefinitions.DeclarationEdit)]
    public async Task<IActionResult> EditIS(int id)
    {
        var declaration = await _declarations.GetByIdAsync(id);
        if (declaration == null) { TempData["Error"] = "Déclaration introuvable."; return RedirectToAction("Index"); }
        if (!EditableStatuts.Contains(declaration.Statut ?? "")) { TempData["Error"] = "Modification impossible : la déclaration est dans un statut verrouillé (déjà validée / soumise / clôturée)."; return RedirectToAction("Details", new { id }); }
        ViewBag.Type = "IS";
        PopulateContractAccessViewBag();
        if (!(ViewBag.CanEditDeclaration as bool? ?? false))
        {
            TempData["Error"] = ViewBag.IsConsultMode == true
                ? (ViewBag.ContractAccessReasonLabel as string ?? "Modification impossible : mode consultation.")
                : "Vous n'avez pas la permission de modifier cette déclaration.";
            return RedirectToAction("Details", new { id });
        }
        ViewBag.Declaration = declaration;
        await LoadWorkflowActionBarAsync(declaration);
        var isData = await _declarations.GetISDataAsync(id);
        return View(isData ?? new DeclarationISDto { DeclarationId = id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.IsEdit, ClaimDefinitions.DeclarationEdit)]
    public async Task<IActionResult> EditIS(int id, DeclarationISDto dto)
    {
        if (!RequiresManageAccess()) return RedirectToAction("Details", new { id });
        if (!RequiresPermission(PermissionHelper.Declaration.Edit)) return RedirectToAction("Details", new { id });
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
    [RequirePermission(ClaimDefinitions.IrEdit, ClaimDefinitions.DeclarationEdit)]
    public async Task<IActionResult> EditIR(int id)
    {
        var declaration = await _declarations.GetByIdAsync(id);
        if (declaration == null) { TempData["Error"] = "Déclaration introuvable."; return RedirectToAction("Index"); }
        if (!EditableStatuts.Contains(declaration.Statut ?? "")) { TempData["Error"] = "Modification impossible : la déclaration est dans un statut verrouillé (déjà validée / soumise / clôturée)."; return RedirectToAction("Details", new { id }); }
        ViewBag.Type = "IR";
        PopulateContractAccessViewBag();
        if (!(ViewBag.CanEditDeclaration as bool? ?? false))
        {
            TempData["Error"] = ViewBag.IsConsultMode == true
                ? (ViewBag.ContractAccessReasonLabel as string ?? "Modification impossible : mode consultation.")
                : "Vous n'avez pas la permission de modifier cette déclaration.";
            return RedirectToAction("Details", new { id });
        }
        ViewBag.Declaration = declaration;
        await LoadWorkflowActionBarAsync(declaration);
        var irData = await _declarations.GetIRDataAsync(id);
        return View(irData ?? new DeclarationIRDto { DeclarationId = id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.IrEdit, ClaimDefinitions.DeclarationEdit)]
    public async Task<IActionResult> EditIR(int id, DeclarationIRDto dto)
    {
        if (!RequiresManageAccess()) return RedirectToAction("Details", new { id });
        if (!RequiresPermission(PermissionHelper.Declaration.Edit)) return RedirectToAction("Details", new { id });
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
    [RequirePermission(ClaimDefinitions.CnssEdit, ClaimDefinitions.DeclarationEdit)]
    public async Task<IActionResult> EditCNSS(int id)
    {
        var declaration = await _declarations.GetByIdAsync(id);
        if (declaration == null) { TempData["Error"] = "Déclaration introuvable."; return RedirectToAction("Index"); }
        if (!EditableStatuts.Contains(declaration.Statut ?? "")) { TempData["Error"] = "Modification impossible : la déclaration est dans un statut verrouillé (déjà validée / soumise / clôturée)."; return RedirectToAction("Details", new { id }); }
        ViewBag.Type = "CNSS";
        PopulateContractAccessViewBag();
        if (!(ViewBag.CanEditDeclaration as bool? ?? false))
        {
            TempData["Error"] = ViewBag.IsConsultMode == true
                ? (ViewBag.ContractAccessReasonLabel as string ?? "Modification impossible : mode consultation.")
                : "Vous n'avez pas la permission de modifier cette déclaration.";
            return RedirectToAction("Details", new { id });
        }
        ViewBag.Declaration = declaration;
        await LoadWorkflowActionBarAsync(declaration);
        var cnssData = await _declarations.GetCNSSDataAsync(id);
        return View(cnssData ?? new DeclarationCNSSDto { DeclarationId = id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.CnssEdit, ClaimDefinitions.DeclarationEdit)]
    public async Task<IActionResult> EditCNSS(int id, DeclarationCNSSDto dto)
    {
        if (!RequiresManageAccess()) return RedirectToAction("Details", new { id });
        if (!RequiresPermission(PermissionHelper.Declaration.Edit)) return RedirectToAction("Details", new { id });
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
    [RequirePermission(ClaimDefinitions.CnssEdit, ClaimDefinitions.DeclarationEdit)]
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
    [RequirePermission(ClaimDefinitions.DeclarationEdit, ClaimDefinitions.DeclarationSubmit, ClaimDefinitions.DeclarationValidate)]
    public async Task<IActionResult> ChangeStatus(int id, string newStatut, string? comment = null)
    {
        if (!RequiresManageAccess()) return RedirectToAction("Details", new { id });
        if (!RequiresAnyPermission(
                PermissionHelper.Declaration.Edit,
                PermissionHelper.Declaration.Submit,
                PermissionHelper.Declaration.Validate))
        {
            TempData["Error"] = "Vous n'avez pas la permission de modifier le statut.";
            return RedirectToAction("Details", new { id });
        }

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
    [RequirePermission(ClaimDefinitions.WorkflowCreate)]
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

    /// <summary>
    /// Advance the current workflow step for a declaration.
    /// Authenticated BackOffice users only; API/service enforces assignee-only.
    /// </summary>
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
    [RequirePermission(ClaimDefinitions.WorkflowCancel)]
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
    [RequirePermission(ClaimDefinitions.WorkflowAdvance)]
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
    [RequirePermission(ClaimDefinitions.WorkflowCancel)]
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
    [RequirePermission(ClaimDefinitions.TvaEdit, ClaimDefinitions.DeclarationEdit)]
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
    [RequirePermission(ClaimDefinitions.TvaEdit, ClaimDefinitions.DeclarationEdit)]
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
    [RequirePermission(ClaimDefinitions.DeclarationEdit)]
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
    [RequirePermission(ClaimDefinitions.TvaEdit, ClaimDefinitions.DeclarationEdit)]
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
    [RequirePermission(ClaimDefinitions.DeclarationSubmit)]
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
