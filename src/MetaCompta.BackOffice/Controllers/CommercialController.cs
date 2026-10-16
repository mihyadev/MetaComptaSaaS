using MetaCompta.Application.DTOs;
using MetaCompta.BackOffice.Filters;
using MetaCompta.BackOffice.Services;
using MetaCompta.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.BackOffice.Controllers;

/// <summary>
/// Contrôleur MVC pour la gestion des documents commerciaux.
/// Cycle de vie : Devis → Bon de Commande → Contrat → Facture + Paiements
/// </summary>
public class CommercialController : BaseController
{
    private readonly ICommercialFacade _commercial;
    private readonly IClientFacade _clients;
    private readonly ICabinetFacade _cabinet;
    private readonly IWorkflowFacade _workflow;

    public CommercialController(
        IReferenceDataFacade referenceData,
        IHttpContextAccessor contextAccessor,
        ICommercialFacade commercial,
        IClientFacade clients,
        ICabinetFacade cabinet,
        IWorkflowFacade workflow)
        : base(referenceData, contextAccessor)
    {
        _commercial = commercial;
        _clients = clients;
        _cabinet = cabinet;
        _workflow = workflow;
    }

    private async Task LoadEmployeesAsync()
    {
        try
        {
            var employees = await _cabinet.GetEmployeesAsync(page: 1, pageSize: 200);
            ViewBag.Employees = employees?.Items ?? new List<CabinetEmployeeDto>();
        }
        catch
        {
            ViewBag.Employees = new List<CabinetEmployeeDto>();
        }
    }

    private async Task LoadCabinetRolesAsync()
    {
        try
        {
            var roles = await _cabinet.GetRolesAsync(page: 1);
            ViewBag.CabinetRoles = (roles?.Items ?? new List<RoleDto>())
                .Where(r => !string.Equals(r.Name, "Client", StringComparison.OrdinalIgnoreCase))
                .OrderBy(r => r.Name)
                .ToList();
        }
        catch
        {
            ViewBag.CabinetRoles = new List<RoleDto>();
        }
    }

    private void LoadServiceGroups()
    {
        ViewBag.ServiceGroups = ContractServiceGroups.All;
        ViewData["ServiceGroups"] = ContractServiceGroups.All;
    }

    private async Task LoadWorkflowTemplatesForContratAsync()
    {
        try
        {
            var page = await _workflow.GetTemplatesAsync(
                declarationType: null,
                clientId: null,
                page: 1,
                pageSize: 200,
                cabinetOnly: true,
                activeOnly: true,
                excludeAncien: true);
            ViewBag.WorkflowTemplates = page?.Items ?? new List<WorkflowTemplateDto>();
        }
        catch
        {
            ViewBag.WorkflowTemplates = new List<WorkflowTemplateDto>();
        }
    }

    private async Task LoadContratServicePickerDataAsync()
    {
        await LoadReferenceDataToViewBagAsync("TYPE_SERVICE");
        LoadServiceGroups();
        await LoadWorkflowTemplatesForContratAsync();
    }

    /// <summary>
    /// Loads clients eligible for Nouveau contrat (without active Valide/Actif contract).
    /// Edit mode keeps the current client selectable even if they already have that contract.
    /// </summary>
    private async Task LoadContratPickerClientsAsync(int? ensureClientId = null)
    {
        var picker = await _clients.GetForContratPickerAsync()
            ?? new ClientContratPickerResultDto();

        var sans = picker.SansContratActif ?? new List<ClientContratPickerItemDto>();
        var avec = picker.AvecContratActif ?? new List<ClientContratPickerItemDto>();

        if (ensureClientId is > 0
            && !sans.Any(c => c.Id == ensureClientId)
            && !avec.Any(c => c.Id == ensureClientId))
        {
            var fallback = await _clients.GetByIdAsync(ensureClientId.Value);
            if (fallback != null)
            {
                sans.Insert(0, new ClientContratPickerItemDto
                {
                    Id = fallback.Id,
                    RaisonSociale = fallback.RaisonSociale,
                    ICE = fallback.ICE,
                    IF = fallback.IF,
                    RC = fallback.RC,
                    NumeroCNSS = fallback.NumeroCNSS,
                    VilleSiegeSocial = fallback.VilleSiegeSocial,
                    HasActiveContract = false
                });
            }
        }
        else if (ensureClientId is > 0 && avec.Any(c => c.Id == ensureClientId))
        {
            // Edit: surface current client even though they hold the active contract being edited
            var current = avec.First(c => c.Id == ensureClientId);
            sans.Insert(0, current);
        }

        ViewBag.ClientsSansContratActif = sans;
        ViewBag.ClientsAvecContratActif = new List<ClientContratPickerItemDto>();
        ViewBag.Clients = sans.Select(c => new ClientDto
        {
            Id = c.Id,
            RaisonSociale = c.RaisonSociale,
            ICE = c.ICE,
            IF = c.IF,
            RC = c.RC,
            NumeroCNSS = c.NumeroCNSS,
            VilleSiegeSocial = c.VilleSiegeSocial
        }).ToList();
    }

    // ════════════════════════════════════════════════════════════════
    //  AJAX — Services déclaratifs du client
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns the active ServiceContrat list for a given client (used in line dropdowns).
    /// </summary>
    [HttpGet]
    [RequirePermission(ClaimDefinitions.CommercialDetail)]
    public async Task<IActionResult> GetClientServices(int clientId)
    {
        if (clientId <= 0)
            return Json(new List<object>());

        var services = await _clients.GetServiceContratsAsync(clientId);
        var result = services
            .Select(s => new { id = s.Id, clientId = s.ClientId, label = s.LibelleService + " (" + s.TypeService + ")", actif = s.EstActifAujourdhui })
            .ToList();
        return Json(result);
    }

    // ════════════════════════════════════════════════════════════════
    //  DASHBOARD COMMERCIAL
    // ════════════════════════════════════════════════════════════════

    [RequirePermission(ClaimDefinitions.CommercialDetail)]
    public async Task<IActionResult> Index()
    {
        var dashboard = await _commercial.GetDashboardAsync();
        return View(dashboard ?? new DashboardCommercialDto());
    }

    // ════════════════════════════════════════════════════════════════
    //  DEVIS
    // ════════════════════════════════════════════════════════════════

    [RequirePermission(ClaimDefinitions.CommercialDetail)]
    public async Task<IActionResult> Devis(int page = 1, int? clientId = null, string? statut = null)
    {
        var result = await _commercial.GetDevisAsync(clientId, statut, page);
        await LoadReferenceDataToViewBagAsync("STATUT_DEVIS");
        ViewBag.ClientId = clientId;
        ViewBag.Statut = statut;
        return View(result);
    }

    [RequirePermission(ClaimDefinitions.CommercialDetail)]
    public async Task<IActionResult> DevisDetails(int id)
    {
        var devis = await _commercial.GetDevisByIdAsync(id);
        if (devis == null)
        {
            TempData["Error"] = "Devis introuvable.";
            return RedirectToAction("Devis");
        }
        return View(devis);
    }

    [HttpGet]
    [RequirePermission(ClaimDefinitions.CommercialCreate)]
    public async Task<IActionResult> DevisCreate()
    {
        var clients = await _clients.GetAllForDropdownAsync();
        ViewBag.Clients = clients;

        // Charger tous les services de tous les clients (avec statut actif/inactif)
        ViewBag.ClientServices = await LoadAllClientServicesAsync(clients);

        return View(new DevisCreateDto());
    }

    [HttpPost]
    [RequirePermission(ClaimDefinitions.CommercialCreate)]
    public async Task<IActionResult> DevisCreate(DevisCreateDto dto)
    {
        var result = await _commercial.CreateDevisAsync(dto);
        if (result?.Success == true)
        {
            TempData["Success"] = "Devis créé avec succès.";
            return RedirectToAction("DevisDetails", new { id = result.Data?.Id });
        }
        SetApiError(result, "Erreur lors de la création du devis.");

        var clients = await _clients.GetAllForDropdownAsync();
        ViewBag.Clients = clients;
        ViewBag.ClientServices = await LoadAllClientServicesAsync(clients);

        return View(dto);
    }

    [HttpGet]
    [RequirePermission(ClaimDefinitions.CommercialEdit)]
    public async Task<IActionResult> DevisEdit(int id)
    {
        var devis = await _commercial.GetDevisByIdAsync(id);
        if (devis == null)
        {
            TempData["Error"] = "Devis introuvable.";
            return RedirectToAction("Devis");
        }
        if (devis.Statut != RefCodes.StatutDevis.Brouillon)
        {
            TempData["Error"] = "Seul un devis en brouillon peut être modifié.";
            return RedirectToAction("DevisDetails", new { id });
        }

        var clients = await _clients.GetAllForDropdownAsync();
        ViewBag.Clients = clients;

        // Charger tous les services de tous les clients (avec statut actif/inactif)
        ViewBag.ClientServices = await LoadAllClientServicesAsync(clients);

        var dto = new DevisCreateDto
        {
            ClientId = devis.ClientId,
            Objet = devis.Objet,
            Description = devis.Description,
            Devise = devis.Devise,
            TauxChange = devis.TauxChange,
            DateValidite = devis.DateValidite,
            ConditionsParticulieres = devis.ConditionsParticulieres,
            NotesPourClient = devis.NotesPourClient,
            CommercialId = devis.CommercialId,
            Notes = devis.Notes,
            Lignes = devis.Lignes.Select(l => new LigneDocumentCommercialCreateDto
            {
                TypeLigne = l.TypeLigne,
                TypeService = l.TypeService,
                CodeService = l.CodeService,
                ServiceContratId = l.ServiceContratId,
                Ordre = l.Ordre,
                Description = l.Description,
                DescriptionDetaillee = l.DescriptionDetaillee,
                Quantite = l.Quantite,
                Unite = l.Unite,
                PrixUnitaireHT = l.PrixUnitaireHT,
                RemisePourcentage = l.RemisePourcentage,
                RemiseMontant = l.RemiseMontant,
                TauxTVA = l.TauxTVA
            }).ToList()
        };
        ViewBag.DevisId = id;
        return View(dto);
    }

    [HttpPost]
    [RequirePermission(ClaimDefinitions.CommercialEdit)]
    public async Task<IActionResult> DevisEdit(int id, DevisCreateDto dto)
    {
        var result = await _commercial.UpdateDevisAsync(id, dto);
        if (result?.Success == true)
        {
            TempData["Success"] = "Devis modifié avec succès.";
            return RedirectToAction("DevisDetails", new { id });
        }
        SetApiError(result, "Erreur lors de la modification du devis.");
        ViewBag.Clients = await _clients.GetAllForDropdownAsync();
        ViewBag.DevisId = id;
        return View(dto);
    }

    [HttpPost]
    [RequirePermission(ClaimDefinitions.CommercialValidate)]
    public async Task<IActionResult> DevisSoumettre(int id)
    {
        var result = await _commercial.SoumettreDevisAsync(id);
        SetApiResult(result, "Devis soumis avec succès.", "Erreur lors de la soumission.");
        return RedirectToAction("DevisDetails", new { id });
    }

    [HttpPost]
    [RequirePermission(ClaimDefinitions.CommercialValidate)]
    public async Task<IActionResult> DevisApprouver(int id)
    {
        var result = await _commercial.ApprouverDevisAsync(id);
        SetApiResult(result, "Devis approuvé avec succès.", "Erreur lors de l'approbation.");
        return RedirectToAction("DevisDetails", new { id });
    }

    [HttpPost]
    [RequirePermission(ClaimDefinitions.CommercialValidate)]
    public async Task<IActionResult> DevisRejeter(int id, string motif)
    {
        var result = await _commercial.RejeterDevisAsync(id, motif);
        SetApiResult(result, "Devis rejeté.", "Erreur lors du rejet.");
        return RedirectToAction("DevisDetails", new { id });
    }

    [HttpPost]
    [RequirePermission(ClaimDefinitions.CommercialCreate)]
    public async Task<IActionResult> DevisDupliquer(int id)
    {
        var result = await _commercial.DupliquerDevisAsync(id);
        if (result?.Success == true)
        {
            TempData["Success"] = $"Devis dupliqué avec succès : {result.Data?.Reference}";
            return RedirectToAction("DevisDetails", new { id = result.Data?.Id });
        }
        SetApiError(result, "Erreur lors de la duplication du devis.");
        return RedirectToAction("DevisDetails", new { id });
    }

    [HttpPost]
    [RequirePermission(ClaimDefinitions.CommercialConvert)]
    public async Task<IActionResult> DevisConvertir(int id)
    {
        var result = await _commercial.ConvertirDevisAsync(id);
        if (result?.Success == true)
        {
            TempData["Success"] = $"Bon de commande {result.Data?.Reference} créé avec succès.";
            return RedirectToAction("BonCommandeDetails", new { id = result.Data?.Id });
        }
        SetApiError(result, "Erreur lors de la conversion.");
        return RedirectToAction("DevisDetails", new { id });
    }

    [HttpGet]
    [RequirePermission(ClaimDefinitions.CommercialConvert)]
    public async Task<IActionResult> DevisConvertirEnContrat(int id)
    {
        var devis = await _commercial.GetDevisByIdAsync(id);
        if (devis == null)
        {
            TempData["Error"] = "Devis introuvable.";
            return RedirectToAction("Devis");
        }
        ViewBag.Devis = devis;
        await LoadEmployeesAsync();
        await LoadContratServicePickerDataAsync();

        return View(new ConvertDevisEnContratDto
        {
            DevisId = id,
            ModeleFacturation = "Forfait",
            DateDebutContrat = DateTime.UtcNow
        });
    }

    [HttpPost]
    [RequirePermission(ClaimDefinitions.CommercialConvert)]
    public async Task<IActionResult> DevisConvertirEnContrat(ConvertDevisEnContratDto dto)
    {
        var result = await _commercial.ConvertirDevisEnContratAsync(dto.DevisId, dto);
        if (result?.Success == true)
        {
            TempData["Success"] = $"Contrat {result.Data?.Reference} créé avec succès depuis le devis.";
            return RedirectToAction("ContratDetails", new { id = result.Data?.Id });
        }
        SetApiError(result, "Erreur lors de la conversion en contrat.");
        return RedirectToAction("DevisDetails", new { id = dto.DevisId });
    }

    [HttpPost]
    [RequirePermission(ClaimDefinitions.CommercialDelete)]
    public async Task<IActionResult> DevisSupprimer(int id)
    {
        var result = await _commercial.DeleteDevisAsync(id);
        SetApiResult(result, "Devis supprimé avec succès.", "Erreur lors de la suppression.");
        return RedirectToAction("Devis");
    }

    // ════════════════════════════════════════════════════════════════
    //  BONS DE COMMANDE
    // ════════════════════════════════════════════════════════════════

    [RequirePermission(ClaimDefinitions.CommercialDetail)]
    public async Task<IActionResult> BonsCommande(int page = 1, int? clientId = null, string? statut = null)
    {
        var result = await _commercial.GetBonsCommandeAsync(clientId, statut, page);
        await LoadReferenceDataToViewBagAsync("STATUT_BON_COMMANDE");
        ViewBag.ClientId = clientId;
        ViewBag.Statut = statut;
        return View(result);
    }

    [RequirePermission(ClaimDefinitions.CommercialDetail)]
    public async Task<IActionResult> BonCommandeDetails(int id)
    {
        var bc = await _commercial.GetBonCommandeByIdAsync(id);
        if (bc == null)
        {
            TempData["Error"] = "Bon de commande introuvable.";
            return RedirectToAction("BonsCommande");
        }
        return View(bc);
    }

    [HttpPost]
    [RequirePermission(ClaimDefinitions.CommercialValidate)]
    public async Task<IActionResult> BonCommandeConfirmer(int id)
    {
        var result = await _commercial.ConfirmerBonCommandeAsync(id);
        SetApiResult(result, "Bon de commande confirmé.", "Erreur lors de la confirmation.");
        return RedirectToAction("BonCommandeDetails", new { id });
    }

    [HttpPost]
    [RequirePermission(ClaimDefinitions.CommercialValidate)]
    public async Task<IActionResult> BonCommandeApprouver(int id)
    {
        var result = await _commercial.ApprouverBonCommandeAsync(id);
        SetApiResult(result, "Bon de commande approuvé.", "Erreur lors de l'approbation.");
        return RedirectToAction("BonCommandeDetails", new { id });
    }

    [HttpPost]
    [RequirePermission(ClaimDefinitions.CommercialCreate)]
    public async Task<IActionResult> BonCommandeDupliquer(int id)
    {
        var result = await _commercial.DupliquerBonCommandeAsync(id);
        if (result?.Success == true)
        {
            TempData["Success"] = $"Bon de commande dupliqué avec succès : {result.Data?.Reference}";
            return RedirectToAction("BonCommandeDetails", new { id = result.Data?.Id });
        }
        SetApiError(result, "Erreur lors de la duplication du bon de commande.");
        return RedirectToAction("BonCommandeDetails", new { id });
    }

    [HttpGet]
    [RequirePermission(ClaimDefinitions.CommercialConvert)]
    public async Task<IActionResult> BonCommandeConvertir(int id)
    {
        var bc = await _commercial.GetBonCommandeByIdAsync(id);
        if (bc == null)
        {
            TempData["Error"] = "Bon de commande introuvable.";
            return RedirectToAction("BonsCommande");
        }
        ViewBag.BonCommande = bc;
        await LoadEmployeesAsync();
        await LoadContratServicePickerDataAsync();

        return View(new ConvertBonCommandeEnContratDto
        {
            BonCommandeId = id,
            ModeleFacturation = "Forfait",
            DateDebutContrat = DateTime.UtcNow
        });
    }

    [HttpPost]
    [RequirePermission(ClaimDefinitions.CommercialConvert)]
    public async Task<IActionResult> BonCommandeConvertir(ConvertBonCommandeEnContratDto dto)
    {
        var result = await _commercial.ConvertirBonCommandeAsync(dto);
        if (result?.Success == true)
        {
            TempData["Success"] = $"Contrat {result.Data?.Reference} créé avec succès.";
            return RedirectToAction("ContratDetails", new { id = result.Data?.Id });
        }
        SetApiError(result, "Erreur lors de la conversion.");
        return RedirectToAction("BonCommandeDetails", new { id = dto.BonCommandeId });
    }

    // ════════════════════════════════════════════════════════════════
    //  CONTRATS
    // ════════════════════════════════════════════════════════════════

    [RequirePermission(ClaimDefinitions.CommercialDetail)]
    public async Task<IActionResult> Contrats(int page = 1, int? clientId = null, string? statut = null)
    {
        var result = await _commercial.GetContratsAsync(clientId, statut, page);
        await LoadReferenceDataToViewBagAsync("STATUT_CONTRAT");
        await LoadContratPickerClientsAsync();
        ViewBag.ClientId = clientId;
        ViewBag.Statut = statut;
        return View(result);
    }

    [RequirePermission(ClaimDefinitions.CommercialDetail)]
    public async Task<IActionResult> ContratDetails(int id)
    {
        var contrat = await _commercial.GetContratByIdAsync(id);
        if (contrat == null)
        {
            TempData["Error"] = "Contrat introuvable.";
            return RedirectToAction("Contrats");
        }
        await LoadEmployeesAsync();
        await LoadCabinetRolesAsync();
        await LoadWorkflowTemplatesForContratAsync();
        ViewBag.WorkflowProposals = await BuildWorkflowProposalsAsync(contrat);
        return View(contrat);
    }

    /// <summary>Auto proposal: each active service → default template + steps with team actors.</summary>
    private async Task<List<ContratWorkflowProposalDto>> BuildWorkflowProposalsAsync(ContratCommercialDto contrat)
    {
        var templates = (ViewBag.WorkflowTemplates as List<WorkflowTemplateDto>) ?? new();
        var employees = (ViewBag.Employees as List<CabinetEmployeeDto>) ?? new();
        var membres = (contrat.Membres ?? new List<ContratMembreDto>())
            .Select(m => (m.UserId, m.RoleLabel))
            .ToList();

        // Prefer client-owned configs if already customized
        List<ClientWorkflowConfigDto> clientConfigs = new();
        try
        {
            clientConfigs = await _workflow.GetClientConfigsAsync(contrat.ClientId) ?? new();
        }
        catch { /* ignore */ }

        var list = new List<ContratWorkflowProposalDto>();
        foreach (var svc in contrat.ServicesActives ?? new List<ContratServiceActiveLine>())
        {
            if (string.IsNullOrWhiteSpace(svc.TypeService)) continue;
            var decl = ContratWorkflowAutoAssign.MapDeclarationType(svc.TypeService);
            if (decl == null) continue;

            var assigneeId = !string.IsNullOrWhiteSpace(svc.PrimaryAssigneeUserId)
                ? svc.PrimaryAssigneeUserId
                : ContratWorkflowAutoAssign.ResolveAssignee(membres, svc.TypeService);

            var assigneeName = employees.FirstOrDefault(e => e.Id == assigneeId)?.NomComplet
                ?? contrat.Membres?.FirstOrDefault(m => m.UserId == assigneeId)?.UserName;
            var assigneeRole = ContratWorkflowAutoAssign.RoleOfMember(membres, assigneeId);

            var cfg = clientConfigs.FirstOrDefault(c =>
                string.Equals(c.DeclarationType, decl, StringComparison.OrdinalIgnoreCase) && c.IsActive);

            WorkflowTemplateDto? tplSummary = null;
            if (cfg?.WorkflowTemplateId is int cfgTplId)
            {
                tplSummary = new WorkflowTemplateDto
                {
                    Id = cfgTplId,
                    Name = cfg.WorkflowTemplateName ?? $"Workflow {decl}",
                    DeclarationType = decl
                };
            }
            else
            {
                var typed = templates
                    .Where(t => string.Equals(t.DeclarationType, decl, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(t => t.IsDefault)
                    .ThenBy(t => t.Id)
                    .ToList();
                tplSummary = typed.FirstOrDefault()
                          ?? templates
                              .Where(t => string.Equals(t.DeclarationType, "All", StringComparison.OrdinalIgnoreCase))
                              .OrderByDescending(t => t.IsDefault)
                              .ThenBy(t => t.Id)
                              .FirstOrDefault();
            }

            var steps = new List<ContratWorkflowStepProposalDto>();
            if (tplSummary != null)
            {
                var full = await _workflow.GetTemplateByIdAsync(tplSummary.Id);
                if (full?.Steps != null)
                {
                    foreach (var s in full.Steps.OrderBy(x => x.StepOrder))
                    {
                        var assignmentType = string.IsNullOrWhiteSpace(s.AssignmentType)
                            ? (s.IsClientStep ? "Client" : "Employee")
                            : s.AssignmentType;
                        var isClient = s.IsClientStep
                            || string.Equals(assignmentType, "Client", StringComparison.OrdinalIgnoreCase);
                        // Init + DemandePieces are always system; also honor AutoSystem
                        var isSystem = !isClient && (
                            string.Equals(assignmentType, "AutoSystem", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(s.StepType, "Initialisation", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(s.StepType, "DemandePieces", StringComparison.OrdinalIgnoreCase));
                        if (isSystem)
                            assignmentType = "AutoSystem";

                        var userIds = new List<string>();
                        var roleNames = new List<string>();

                        // Client / System steps have no human actors to pick
                        if (!isClient && !isSystem)
                        {
                            if (s.AssignedToUserIdList?.Count > 0)
                                userIds.AddRange(s.AssignedToUserIdList.Where(id => !string.IsNullOrWhiteSpace(id)));
                            else if (!string.IsNullOrWhiteSpace(s.AssignedToUserId))
                                userIds.Add(s.AssignedToUserId);

                            // Prefer team members; drop outsiders, fall back to primary assignee
                            if (membres.Count > 0)
                            {
                                userIds = userIds
                                    .Where(id => membres.Any(m => m.UserId == id))
                                    .Distinct(StringComparer.OrdinalIgnoreCase)
                                    .ToList();
                                if (userIds.Count == 0 && !string.IsNullOrWhiteSpace(assigneeId))
                                    userIds.Add(assigneeId);
                            }
                            else if (userIds.Count == 0 && !string.IsNullOrWhiteSpace(assigneeId))
                            {
                                userIds.Add(assigneeId);
                            }

                            if (!string.IsNullOrWhiteSpace(s.AssignedToRoleName))
                            {
                                roleNames = s.AssignedToRoleName
                                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                    .Where(r => !string.Equals(r, "Client", StringComparison.OrdinalIgnoreCase))
                                    .Distinct(StringComparer.OrdinalIgnoreCase)
                                    .ToList();
                            }
                        }

                        steps.Add(new ContratWorkflowStepProposalDto
                        {
                            StepOrder = s.StepOrder,
                            Name = s.Name,
                            StepType = s.StepType,
                            AssignmentType = assignmentType,
                            IsClientStep = isClient,
                            AssignedToUserId = userIds.FirstOrDefault(),
                            AssignedToUserIds = userIds,
                            AssignedToRoleNames = roleNames
                        });
                    }
                    tplSummary.Name = full.Name;
                }
            }

            if (cfg?.PrimaryAssigneeUserId != null)
            {
                assigneeId = cfg.PrimaryAssigneeUserId;
                assigneeName = employees.FirstOrDefault(e => e.Id == assigneeId)?.NomComplet
                    ?? contrat.Membres?.FirstOrDefault(m => m.UserId == assigneeId)?.UserName;
                assigneeRole = ContratWorkflowAutoAssign.RoleOfMember(membres, assigneeId);
            }

            list.Add(new ContratWorkflowProposalDto
            {
                TypeService = svc.TypeService,
                Libelle = svc.LibelleService
                    ?? ContractServiceGroups.LabelForCode(svc.TypeService),
                DeclarationType = decl,
                AssigneeUserId = assigneeId,
                AssigneeName = assigneeName,
                AssigneeRole = assigneeRole,
                WorkflowTemplateId = tplSummary?.Id,
                WorkflowTemplateName = tplSummary?.Name,
                HasTemplate = tplSummary != null,
                HasAssignee = !string.IsNullOrWhiteSpace(assigneeId),
                Steps = steps
            });
        }

        return list;
    }

    [HttpPost]
    [RequirePermission(ClaimDefinitions.CommercialEdit)]
    public async Task<IActionResult> ContratSaveWorkflowAssignments(int id, ContratWorkflowAssignmentsSaveDto dto)
    {
        var contrat = await _commercial.GetContratByIdAsync(id);
        if (contrat == null)
        {
            TempData["Error"] = "Contrat introuvable.";
            return RedirectToAction("Contrats");
        }

        dto.ContratId = id;
        var teamIds = new HashSet<string>(
            (contrat.Membres ?? new List<ContratMembreDto>()).Select(m => m.UserId),
            StringComparer.OrdinalIgnoreCase);

        var errors = new List<string>();
        var saved = 0;

        foreach (var svc in dto.Services ?? new List<ContratWorkflowServiceAssignmentDto>())
        {
            if (svc.SourceTemplateId <= 0 || string.IsNullOrWhiteSpace(svc.DeclarationType))
                continue;

            var full = await _workflow.GetTemplateByIdAsync(svc.SourceTemplateId);
            if (full?.Steps == null || full.Steps.Count == 0)
            {
                errors.Add($"{svc.TypeService}: modèle sans étapes.");
                continue;
            }

            var stepAssign = (svc.Steps ?? new List<ContratWorkflowStepAssignmentDto>())
                .ToDictionary(s => s.StepOrder, s => s);

            var primary = svc.PrimaryAssigneeUserId;
            if (!string.IsNullOrWhiteSpace(primary) && teamIds.Count > 0 && !teamIds.Contains(primary))
                primary = teamIds.First();

            var steps = full.Steps.OrderBy(s => s.StepOrder).Select(s =>
            {
                var create = MapStepToCreate(s);
                if (s.IsClientStep || string.Equals(s.AssignmentType, "Client", StringComparison.OrdinalIgnoreCase))
                {
                    create.AssignmentType = "Client";
                    create.AssignedToUserId = null;
                    create.AssignedToUserIds = null;
                    create.AssignedToRoleName = null;
                    return create;
                }

                // System / automatic steps (Init + DemandePieces): no employee or profile selection
                if (string.Equals(s.AssignmentType, "AutoSystem", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(s.StepType, "Initialisation", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(s.StepType, "DemandePieces", StringComparison.OrdinalIgnoreCase))
                {
                    create.AssignmentType = "AutoSystem";
                    create.AssignedToUserId = null;
                    create.AssignedToUserIds = null;
                    create.AssignedToRoleName = null;
                    create.IsClientStep = false;
                    return create;
                }

                stepAssign.TryGetValue(s.StepOrder, out var assign);
                var actorIds = new List<string>();
                if (assign?.AssignedToUserIds?.Count > 0)
                    actorIds.AddRange(assign.AssignedToUserIds.Where(id => !string.IsNullOrWhiteSpace(id)));
                else if (!string.IsNullOrWhiteSpace(assign?.AssignedToUserId))
                    actorIds.Add(assign.AssignedToUserId);

                actorIds = actorIds
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Where(id => teamIds.Count == 0 || teamIds.Contains(id))
                    .ToList();

                if (actorIds.Count == 0 && !string.IsNullOrWhiteSpace(primary))
                    actorIds.Add(primary);

                var roleNames = (assign?.AssignedToRoleNames ?? new List<string>())
                    .Where(r => !string.IsNullOrWhiteSpace(r)
                                && !string.Equals(r, "Client", StringComparison.OrdinalIgnoreCase))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (actorIds.Count > 0)
                {
                    create.AssignmentType = "Employee";
                    create.AssignedToUserId = actorIds[0];
                    create.AssignedToUserIds = actorIds;
                    create.AssignedToRoleName = roleNames.Count > 0 ? string.Join(",", roleNames) : null;
                }
                else if (roleNames.Count > 0)
                {
                    create.AssignmentType = "Role";
                    create.AssignedToUserId = null;
                    create.AssignedToUserIds = null;
                    create.AssignedToRoleName = string.Join(",", roleNames);
                }
                else
                {
                    create.AssignmentType = string.IsNullOrWhiteSpace(s.AssignmentType) ? "Employee" : s.AssignmentType;
                    create.AssignedToUserId = null;
                    create.AssignedToUserIds = null;
                    create.AssignedToRoleName = null;
                }

                return create;
            }).ToList();

            if (string.IsNullOrWhiteSpace(primary) && steps.Any(st => !string.IsNullOrWhiteSpace(st.AssignedToUserId)))
                primary = steps.First(st => !string.IsNullOrWhiteSpace(st.AssignedToUserId)).AssignedToUserId;

            var result = await _workflow.SaveClientWorkflowCustomizationAsync(new ClientWorkflowCustomizeDto
            {
                ClientId = contrat.ClientId,
                DeclarationType = svc.DeclarationType,
                SourceTemplateId = svc.SourceTemplateId,
                TemplateName = $"{full.Name} — {contrat.ClientName ?? contrat.ClientId.ToString()}",
                AutoStartWorkflow = false,
                PrimaryAssigneeUserId = primary,
                IsActive = true,
                Steps = steps
            });

            if (result?.Success == true)
                saved++;
            else
                errors.Add($"{svc.TypeService}: {result?.Message ?? "échec"}");
        }

        if (saved > 0 && errors.Count == 0)
            TempData["Success"] = $"{saved} workflow(s) enregistré(s) avec acteurs par étape.";
        else if (saved > 0)
            TempData["Success"] = $"{saved} workflow(s) enregistré(s). Attention: {string.Join(" · ", errors)}";
        else
            TempData["Error"] = errors.Count > 0
                ? string.Join(" · ", errors)
                : "Aucun workflow enregistré.";

        return RedirectToAction("ContratDetails", new { id });
    }

    private static WorkflowStepTemplateCreateDto MapStepToCreate(WorkflowStepTemplateDto s) => new()
    {
        Name = s.Name,
        Description = s.Description,
        StepOrder = s.StepOrder,
        StepType = s.StepType,
        AssignmentType = s.AssignmentType,
        AssignedToUserId = s.AssignedToUserId,
        AssignedToUserIds = s.AssignedToUserIdList?.Count > 0
            ? s.AssignedToUserIdList
            : (string.IsNullOrWhiteSpace(s.AssignedToUserId) ? null : new List<string> { s.AssignedToUserId }),
        AssignedToRoleName = s.AssignedToRoleName,
        AssignedToDepartmentId = s.AssignedToDepartmentId,
        IsClientStep = s.IsClientStep,
        RequiredPermission = s.RequiredPermission,
        SLAHours = s.SLAHours,
        DeadlineAnchor = s.DeadlineAnchor,
        DeadlineOffsetDays = s.DeadlineOffsetDays,
        DeadlineDay = s.DeadlineDay,
        DeadlineMonth = s.DeadlineMonth,
        DeadlineMonthOffset = s.DeadlineMonthOffset,
        DeadlineYearOffset = s.DeadlineYearOffset,
        ApplyDeadlineToDeclaration = s.ApplyDeadlineToDeclaration,
        LifecycleEndMode = s.LifecycleEndMode,
        LifecycleEndDays = s.LifecycleEndDays,
        ActionOnLifecycleEnd = s.ActionOnLifecycleEnd,
        IsRequired = s.IsRequired,
        IsBlocking = s.IsBlocking,
        AutoCreateTask = s.AutoCreateTask,
        TaskType = s.TaskType,
        TaskPriority = s.TaskPriority,
        AutoNotify = s.AutoNotify,
        ValidationRulesJson = s.ValidationRulesJson,
        DeclarationStatusOnComplete = s.DeclarationStatusOnComplete,
        DeclarationStatusOnEntry = s.DeclarationStatusOnEntry,
        ActionOnEntry = s.ActionOnEntry,
        ActionOnComplete = s.ActionOnComplete,
        ActionConfigJson = s.ActionConfigJson,
        CronExpression = s.CronExpression,
        CronDescription = s.CronDescription,
        IsScheduled = s.IsScheduled,
        MaxRetries = s.MaxRetries,
        RetryIntervalHours = s.RetryIntervalHours,
        OnMaxRetriesAction = s.OnMaxRetriesAction,
        RoutingMode = s.RoutingMode,
        RoutingConditionsJson = s.RoutingConditionsJson,
        GotoStepOrder = s.GotoStepOrder,
        TriggerSource = s.TriggerSource,
        UseServiceContratAffectation = s.UseServiceContratAffectation,
        ServiceContratTypeCode = s.ServiceContratTypeCode
    };

    [HttpPost]
    [RequirePermission(ClaimDefinitions.CommercialEdit)]
    public async Task<IActionResult> ContratEquipe(int id, ContratEquipeUpdateDto dto)
    {
        var result = await _commercial.UpdateContratEquipeAsync(id, dto);
        if (result?.Success == true)
            TempData["Success"] = "Équipe du contrat mise à jour.";
        else
            SetApiError(result, "Impossible de mettre à jour l'équipe.");
        return RedirectToAction("ContratDetails", new { id });
    }

    [HttpGet]
    [RequirePermission(ClaimDefinitions.CommercialCreate)]
    public async Task<IActionResult> ContratCreate(int? clientId = null)
    {
        await LoadContratPickerClientsAsync(clientId);
        await LoadEmployeesAsync();
        await LoadCabinetRolesAsync();
        await LoadContratServicePickerDataAsync();

        return View(new ContratCommercialCreateDto
        {
            ClientId = clientId ?? 0,
            DateDebutContrat = DateTime.UtcNow.Date,
            DateFinContrat = new DateTime(DateTime.UtcNow.Year, 12, 31),
            Devise = "MAD",
            ModeleFacturation = RefCodes.ModeleFacturation.Forfait,
            FrequenceFacturation = RefCodes.FrequenceFacturation.Mensuel
        });
    }

    [HttpPost]
    [RequirePermission(ClaimDefinitions.CommercialCreate)]
    public async Task<IActionResult> ContratCreate(ContratCommercialCreateDto dto)
    {
        var result = await _commercial.CreateContratAsync(dto);
        if (result?.Success == true)
        {
            TempData["Success"] = $"Contrat {result.Data?.Reference} créé avec succès.";
            return RedirectToAction("ContratDetails", new { id = result.Data?.Id });
        }
        SetApiError(result, "Erreur lors de la création du contrat.");

        await LoadContratPickerClientsAsync(dto.ClientId > 0 ? dto.ClientId : null);
        await LoadEmployeesAsync();
        await LoadCabinetRolesAsync();
        await LoadContratServicePickerDataAsync();

        return View(dto);
    }

    [HttpGet]
    [RequirePermission(ClaimDefinitions.CommercialEdit)]
    public async Task<IActionResult> ContratEdit(int id)
    {
        var contrat = await _commercial.GetContratByIdAsync(id);
        if (contrat == null)
        {
            TempData["Error"] = "Contrat introuvable.";
            return RedirectToAction("Contrats");
        }
        if (contrat.EstSigne || (contrat.Statut != RefCodes.StatutContrat.Brouillon && contrat.Statut != RefCodes.StatutContrat.EnCours))
        {
            TempData["Error"] = "Seul un contrat en brouillon ou en cours (non signé) peut être modifié.";
            return RedirectToAction("ContratDetails", new { id });
        }

        await LoadContratPickerClientsAsync(contrat.ClientId);
        await LoadEmployeesAsync();
        await LoadCabinetRolesAsync();
        await LoadContratServicePickerDataAsync();

        var dto = new ContratCommercialCreateDto
        {
            ClientId = contrat.ClientId,
            Objet = contrat.Objet,
            Description = contrat.Description,
            Devise = contrat.Devise,
            DateDebutContrat = contrat.DateDebutContrat,
            DateFinContrat = contrat.DateFinContrat,
            ModeleFacturation = contrat.ModeleFacturation,
            FrequenceFacturation = contrat.FrequenceFacturation,
            ConditionsParticulieres = null,
            Notes = contrat.Notes,
            ResponsableId = null,
            Membres = contrat.Membres?.Select(m => new ContratMembreInputDto
            {
                UserId = m.UserId,
                RoleLabel = m.RoleLabel
            }).ToList() ?? new List<ContratMembreInputDto>(),
            MembreIds = contrat.Membres?.Select(m => m.UserId).ToList() ?? new List<string>(),
            ServicesActives = contrat.ServicesActives ?? new List<ContratServiceActiveLine>(),
            Lignes = contrat.Lignes.Select(l => new LigneDocumentCommercialCreateDto
            {
                TypeLigne = l.TypeLigne,
                TypeService = l.TypeService,
                CodeService = l.CodeService,
                ServiceContratId = l.ServiceContratId,
                Ordre = l.Ordre,
                Description = l.Description,
                DescriptionDetaillee = l.DescriptionDetaillee,
                Quantite = l.Quantite,
                Unite = l.Unite,
                PrixUnitaireHT = l.PrixUnitaireHT,
                RemisePourcentage = l.RemisePourcentage,
                RemiseMontant = l.RemiseMontant,
                TauxTVA = l.TauxTVA,
                ModeFacturation = l.ModeFacturation
            }).ToList()
        };

        // Hydrate workflow template + step actors from client configs (same source as ContratDetails),
        // so the Workflows tab is editable with current affectations — not only bare ServicesActives JSON.
        await HydrateServicesActivesWorkflowAsync(dto, contrat);

        ViewBag.ContratId = id;
        return View("ContratCreate", dto);
    }

    /// <summary>
    /// Fills missing WorkflowTemplateId / PrimaryAssigneeUserId / StepAssignments on ServicesActives
    /// from existing client workflow configs + default templates (edit form restore).
    /// </summary>
    private async Task HydrateServicesActivesWorkflowAsync(
        ContratCommercialCreateDto dto,
        ContratCommercialDto contrat)
    {
        if (dto.ServicesActives == null || dto.ServicesActives.Count == 0)
            return;

        List<ContratWorkflowProposalDto> proposals;
        try
        {
            proposals = await BuildWorkflowProposalsAsync(contrat);
        }
        catch
        {
            return;
        }

        foreach (var line in dto.ServicesActives)
        {
            if (string.IsNullOrWhiteSpace(line.TypeService)) continue;
            var p = proposals.FirstOrDefault(x =>
                string.Equals(x.TypeService, line.TypeService, StringComparison.OrdinalIgnoreCase));
            if (p == null) continue;

            if (!line.WorkflowTemplateId.HasValue && p.WorkflowTemplateId.HasValue)
                line.WorkflowTemplateId = p.WorkflowTemplateId;

            if (string.IsNullOrWhiteSpace(line.PrimaryAssigneeUserId) && !string.IsNullOrWhiteSpace(p.AssigneeUserId))
                line.PrimaryAssigneeUserId = p.AssigneeUserId;

            var hasSteps = line.StepAssignments?.Any(s =>
                (s.AssignedToUserIds?.Count > 0)
                || !string.IsNullOrWhiteSpace(s.AssignedToUserId)
                || (s.AssignedToRoleNames?.Count > 0)) == true;

            if (!hasSteps && p.Steps.Count > 0)
            {
                line.StepAssignments = p.Steps.Select(s => new ContratWorkflowStepAssignmentDto
                {
                    StepOrder = s.StepOrder,
                    AssignedToUserId = s.AssignedToUserIds?.FirstOrDefault() ?? s.AssignedToUserId,
                    AssignedToUserIds = s.AssignedToUserIds?.Count > 0
                        ? s.AssignedToUserIds
                        : (!string.IsNullOrWhiteSpace(s.AssignedToUserId)
                            ? new List<string> { s.AssignedToUserId! }
                            : new List<string>()),
                    AssignedToRoleNames = s.AssignedToRoleNames?.Count > 0
                        ? s.AssignedToRoleNames
                        : new List<string>()
                }).ToList();
            }
        }
    }

    [HttpPost]
    [RequirePermission(ClaimDefinitions.CommercialEdit)]
    public async Task<IActionResult> ContratEdit(int id, ContratCommercialCreateDto dto)
    {
        var result = await _commercial.UpdateContratAsync(id, dto);
        if (result?.Success == true)
        {
            TempData["Success"] = "Contrat modifié avec succès.";
            return RedirectToAction("ContratDetails", new { id });
        }
        SetApiError(result, "Erreur lors de la modification du contrat.");

        await LoadContratPickerClientsAsync(dto.ClientId > 0 ? dto.ClientId : null);
        await LoadEmployeesAsync();
        await LoadCabinetRolesAsync();
        await LoadContratServicePickerDataAsync();
        ViewBag.ContratId = id;
        return View("ContratCreate", dto);
    }

    [HttpPost]
    [RequirePermission(ClaimDefinitions.CommercialSign)]
    public async Task<IActionResult> ContratSigner(int id, ContratSignatureDto dto)
    {
        var result = await _commercial.SignerContratAsync(id, dto);
        SetApiResult(result, "Contrat signé avec succès.", "Erreur lors de la signature.");
        return RedirectToAction("ContratDetails", new { id });
    }

    [HttpPost]
    [RequirePermission(ClaimDefinitions.CommercialValidate)]
    public async Task<IActionResult> ContratValider(int id)
    {
        var result = await _commercial.ValiderContratAsync(id);
        SetApiResult(result, "Contrat validé et services activés.", "Erreur lors de la validation.");
        return RedirectToAction("ContratDetails", new { id });
    }

    [HttpPost]
    [RequirePermission(ClaimDefinitions.CommercialEdit)]
    public async Task<IActionResult> ContratSuspendre(int id)
    {
        var result = await _commercial.SuspendreContratAsync(id);
        SetApiResult(result, "Contrat suspendu.", "Erreur lors de la suspension.");
        return RedirectToAction("ContratDetails", new { id });
    }

    [HttpPost]
    [RequirePermission(ClaimDefinitions.CommercialEdit)]
    public async Task<IActionResult> ContratReactiver(int id)
    {
        var result = await _commercial.ReactiverContratAsync(id);
        SetApiResult(result, "Contrat réactivé et services restaurés.", "Erreur lors de la réactivation.");
        return RedirectToAction("ContratDetails", new { id });
    }

    [HttpPost]
    [RequirePermission(ClaimDefinitions.CommercialEdit)]
    public async Task<IActionResult> ContratTerminer(int id)
    {
        var result = await _commercial.TerminerContratAsync(id);
        SetApiResult(result, "Contrat terminé.", "Erreur lors de la clôture.");
        return RedirectToAction("ContratDetails", new { id });
    }

    [HttpPost]
    [RequirePermission(ClaimDefinitions.CommercialDelete)]
    public async Task<IActionResult> ContratSupprimer(int id)
    {
        var result = await _commercial.DeleteContratAsync(id);
        SetApiResult(result, "Contrat supprimé avec succès.", "Erreur lors de la suppression.");
        return result?.Success == true
            ? RedirectToAction("Contrats")
            : RedirectToAction("ContratDetails", new { id });
    }

    [HttpPost]
    [RequirePermission(ClaimDefinitions.CommercialCreate)]
    public async Task<IActionResult> ContratDupliquer(int id)
    {
        var result = await _commercial.DupliquerContratAsync(id);
        if (result?.Success == true)
        {
            TempData["Success"] = $"Contrat dupliqué avec succès : {result.Data?.Reference}";
            return RedirectToAction("ContratDetails", new { id = result.Data?.Id });
        }
        SetApiError(result, "Erreur lors de la duplication du contrat.");
        return RedirectToAction("ContratDetails", new { id });
    }

    /// <summary>Create a rectificatif (amendment) — editable draft; original kept until validate.</summary>
    [HttpPost]
    [RequirePermission(ClaimDefinitions.CommercialCreate)]
    public async Task<IActionResult> ContratRectificatif(int id, string? motif = null)
    {
        var result = await _commercial.CreateRectificatifContratAsync(id, motif);
        if (result?.Success == true)
        {
            TempData["Success"] = result.Data?.EstRectificatif == true
                ? $"Rectificatif {result.Data.Reference} créé — éditez, signez et validez pour remplacer l'original."
                : $"Contrat {result.Data?.Reference} ouvert.";
            return RedirectToAction("ContratEdit", new { id = result.Data?.Id });
        }
        SetApiError(result, "Impossible de créer le rectificatif.");
        return RedirectToAction("ContratDetails", new { id });
    }

    // ════════════════════════════════════════════════════════════════
    //  FACTURES
    // ════════════════════════════════════════════════════════════════

    [RequirePermission(ClaimDefinitions.FactureDetail)]
    public async Task<IActionResult> Factures(int page = 1, int? clientId = null, string? statut = null)
    {
        var result = await _commercial.GetFacturesAsync(clientId, statut, page);
        await LoadReferenceDataToViewBagAsync("STATUT_FACTURE");
        ViewBag.ClientId = clientId;
        ViewBag.Statut = statut;
        return View(result);
    }

    [RequirePermission(ClaimDefinitions.FactureDetail)]
    public async Task<IActionResult> FactureDetails(int id)
    {
        var facture = await _commercial.GetFactureByIdAsync(id);
        if (facture == null)
        {
            TempData["Error"] = "Facture introuvable.";
            return RedirectToAction("Factures");
        }
        return View(facture);
    }

    [HttpGet]
    [RequirePermission(ClaimDefinitions.FactureCreate)]
    public async Task<IActionResult> FactureCreate(int? contratId = null)
    {
        // Mode contrat : facture depuis un contrat existant
        if (contratId.HasValue)
        {
            var contrat = await _commercial.GetContratByIdAsync(contratId.Value);
            if (contrat == null)
            {
                TempData["Error"] = "Contrat introuvable.";
                return RedirectToAction("Contrats");
            }
            ViewBag.Contrat = contrat;
            return View(new FactureCreateDto
            {
                ContratId = contratId.Value,
                DateEcheance = DateTime.UtcNow.AddDays(30),
                PeriodeDebut = DateTime.UtcNow.Date,
                PeriodeFin = DateTime.UtcNow.Date.AddMonths(1).AddDays(-1)
            });
        }

        // Mode manuel : facture libre (sans contrat)
        var clients = await _clients.GetAllForDropdownAsync();
        ViewBag.Clients = clients;

        ViewBag.ClientServices = await LoadAllClientServicesAsync(clients);
        ViewBag.IsManual = true;

        return View(new FactureCreateDto
        {
            DateEcheance = DateTime.UtcNow.AddDays(30)
        });
    }

    [HttpPost]
    [RequirePermission(ClaimDefinitions.FactureCreate)]
    public async Task<IActionResult> FactureCreate(FactureCreateDto dto)
    {
        var result = await _commercial.CreateFactureAsync(dto);
        if (result?.Success == true)
        {
            TempData["Success"] = $"Facture {result.Data?.Reference} créée avec succès.";
            return RedirectToAction("FactureDetails", new { id = result.Data?.Id });
        }
        SetApiError(result, "Erreur lors de la création de la facture.");

        // Reload context depending on mode
        if (dto.ContratId.HasValue)
        {
            var contrat = await _commercial.GetContratByIdAsync(dto.ContratId.Value);
            ViewBag.Contrat = contrat!;
        }
        else
        {
            var clients = await _clients.GetAllForDropdownAsync();
            ViewBag.Clients = clients;
            var allServices = new List<object>();
            foreach (var client in clients)
            {
                var services = await _clients.GetServiceContratsAsync(client.Id);
                allServices.AddRange(services
                    .Select(s => new { id = s.Id, clientId = s.ClientId, label = s.LibelleService + " (" + s.TypeService + ")", actif = s.EstActifAujourdhui })
                    .Cast<object>());
            }
            ViewBag.ClientServices = allServices;
            ViewBag.IsManual = true;
        }
        return View(dto);
    }

    [HttpPost]
    [RequirePermission(ClaimDefinitions.FactureSend)]
    public async Task<IActionResult> FactureEnvoyer(int id)
    {
        var result = await _commercial.EnvoyerFactureAsync(id);
        SetApiResult(result, "Facture envoyée.", "Erreur lors de l'envoi.");
        return RedirectToAction("FactureDetails", new { id });
    }

    [HttpPost]
    [RequirePermission(ClaimDefinitions.FactureCancel)]
    public async Task<IActionResult> FactureAnnuler(int id)
    {
        var result = await _commercial.AnnulerFactureAsync(id);
        SetApiResult(result, "Facture annulée.", "Erreur lors de l'annulation.");
        return RedirectToAction("FactureDetails", new { id });
    }

    [HttpPost]
    [RequirePermission(ClaimDefinitions.FactureCreate)]
    public async Task<IActionResult> FactureDupliquer(int id)
    {
        var result = await _commercial.DupliquerFactureAsync(id);
        if (result?.Success == true)
        {
            TempData["Success"] = $"Facture dupliquée avec succès : {result.Data?.Reference}";
            return RedirectToAction("FactureDetails", new { id = result.Data?.Id });
        }
        SetApiError(result, "Erreur lors de la duplication de la facture.");
        return RedirectToAction("FactureDetails", new { id });
    }

    // ════════════════════════════════════════════════════════════════
    //  PAIEMENTS
    // ════════════════════════════════════════════════════════════════

    [HttpPost]
    [RequirePermission(ClaimDefinitions.FactureManagePayments)]
    public async Task<IActionResult> EnregistrerPaiement(int factureId, PaiementCreateDto dto)
    {
        dto.FactureId = factureId;
        var result = await _commercial.EnregistrerPaiementAsync(factureId, dto);
        SetApiResult(result, "Paiement enregistré avec succès.", "Erreur lors de l'enregistrement du paiement.");
        return RedirectToAction("FactureDetails", new { id = factureId });
    }

    // Helper - Charger les services de tous les clients avec statut contrat
    private async Task<List<object>> LoadAllClientServicesAsync(IEnumerable<ClientDto> clients)
    {
        var allServices = new List<object>();
        foreach (var client in clients)
        {
            var services = await _clients.GetServiceContratsAsync(client.Id);
            allServices.AddRange(services
                .Select(s => new
                {
                    id = s.Id,
                    clientId = s.ClientId,
                    label = s.LibelleService + " (" + s.TypeService + ")",
                    actif = s.EstActifAujourdhui
                })
                .Cast<object>());
        }
        return allServices;
    }
}