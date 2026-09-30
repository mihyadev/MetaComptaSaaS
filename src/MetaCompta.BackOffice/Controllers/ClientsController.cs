using MetaCompta.Application.DTOs;
using MetaCompta.BackOffice.Filters;
using MetaCompta.BackOffice.Services;
using MetaCompta.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.BackOffice.Controllers;

public class ClientsController : BaseController
{
    private readonly IClientFacade _clients;
    private readonly ICommercialFacade _commercial;
    private readonly IWorkflowFacade _workflow;
    private readonly ICabinetFacade _cabinet;
    private readonly IComptabiliteFacade _compta;
    private readonly IDepartmentFacade _departments;

    private static readonly string[] ClientRefCategories = new[]
    {
        "FORME_JURIDIQUE", "STATUT_CLIENT", "TYPE_TVA", "VILLE", "SECTEUR_ACTIVITE"
    };

    public ClientsController(
        IReferenceDataFacade referenceData,
        IHttpContextAccessor contextAccessor,
        IClientFacade clients,
        ICommercialFacade commercial,
        IWorkflowFacade workflow,
        ICabinetFacade cabinet,
        IComptabiliteFacade compta,
        IDepartmentFacade departments)
        : base(referenceData, contextAccessor)
    {
        _clients = clients;
        _commercial = commercial;
        _workflow = workflow;
        _cabinet = cabinet;
        _compta = compta;
        _departments = departments;
    }

    [RequirePermission(ClaimDefinitions.ClientList)]
    public async Task<IActionResult> Index(int page = 1, string? search = null, string? formeJuridique = null, string? statut = null)
    {
        var result = await _clients.GetAllAsync(page, search, statut);

        await LoadReferenceDataToViewBagAsync("FORME_JURIDIQUE", "STATUT_CLIENT");
        ViewBag.Search = search;
        ViewBag.Statut = statut;
        ViewBag.FormeJuridique = formeJuridique;
        return View(result);
    }

    [RequirePermission(ClaimDefinitions.ClientDetail)]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var client = await _clients.GetByIdAsync(id);
            if (client == null)
            {
                TempData["Error"] = "Client introuvable.";
                return RedirectToAction("Index");
            }
            // Ensure ServicesFiduciaires is never null for the view
            client.ServicesFiduciaires ??= new List<ServiceContratDto>();

            // Load validated contracts for sync button display
            var contrats = await _commercial.GetContratsAsync(clientId: id, statut: RefCodes.StatutContrat.Valide);
            ViewBag.ContratsValides = contrats?.Items ?? new List<ContratCommercialDto>();

            // Load all contracts for this client (Contrats tab)
            var allContrats = await _commercial.GetContratsAsync(clientId: id, statut: null);
            ViewBag.AllContrats = allContrats?.Items ?? new List<ContratCommercialDto>();

            // Load client users (Extranet users)
            ViewBag.ClientUsers = await _clients.GetClientUsersAsync(id);

            // ── Workflow data (embedded tab) ──
            try
            {
                // Ensure cabinet defaults exist for TVA/IS/IR/CNSS… and hide outdated "(ancien)"
                try { await _workflow.SeedAllDefaultTemplatesAsync(); } catch { /* non-blocking */ }

                var templates = await _workflow.GetTemplatesAsync(
                    clientId: null, page: 1, pageSize: 200,
                    cabinetOnly: true, activeOnly: true, excludeAncien: true);
                ViewBag.WorkflowTemplates = templates?.Items ?? new List<WorkflowTemplateDto>();

                var instances = await _workflow.GetInstancesAsync(clientId: id, page: 1);
                ViewBag.WorkflowInstances = instances?.Items ?? new List<WorkflowInstanceDto>();

                var pendingSteps = await _workflow.GetMyPendingStepsAsync();
                ViewBag.WorkflowPendingSteps = pendingSteps ?? new List<WorkflowStepInstanceDto>();

                ViewBag.WorkflowConfigs = await _workflow.GetClientConfigsAsync(id) ?? new List<ClientWorkflowConfigDto>();

                try
                {
                    var emps = await _cabinet.GetEmployeesAsync(page: 1, pageSize: 100);
                    ViewBag.CabinetEmployees = emps?.Items?.Where(e => e.EstActif).ToList() ?? new List<CabinetEmployeeDto>();
                }
                catch
                {
                    ViewBag.CabinetEmployees = new List<CabinetEmployeeDto>();
                }

                try
                {
                    ViewBag.Departments = await _departments.GetAllAsync() ?? new List<DepartmentDto>();
                }
                catch
                {
                    ViewBag.Departments = new List<DepartmentDto>();
                }
            }
            catch
            {
                // Workflow API may not be ready — degrade gracefully
                ViewBag.WorkflowTemplates = new List<WorkflowTemplateDto>();
                ViewBag.WorkflowInstances = new List<WorkflowInstanceDto>();
                ViewBag.WorkflowPendingSteps = new List<WorkflowStepInstanceDto>();
                ViewBag.WorkflowConfigs = new List<ClientWorkflowConfigDto>();
                ViewBag.CabinetEmployees = new List<CabinetEmployeeDto>();
                ViewBag.Departments = new List<DepartmentDto>();
            }

            await LoadReferenceDataToViewBagAsync("FORME_JURIDIQUE", "STATUT_CLIENT", "TYPE_TVA");
            return View(client);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Erreur lors du chargement: {ex.Message}";
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.WorkflowConfigure)]
    public async Task<IActionResult> SaveWorkflowAffectation(ClientWorkflowConfigCreateDto dto)
    {
        if (dto.ClientId <= 0)
        {
            TempData["Error"] = "Client invalide.";
            return RedirectToAction("Index");
        }

        var result = await _workflow.SaveClientConfigAsync(dto);
        if (result?.Success == true)
            TempData["Success"] = result.Message ?? "Affectation enregistrée — copie identique au modèle cabinet créée/mise à jour pour ce client.";
        else
            TempData["Error"] = result?.Message ?? "Erreur lors de l'affectation du workflow.";

        return RedirectToAction("Details", new { id = dto.ClientId, tab = "workflow" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.WorkflowConfigure)]
    public async Task<IActionResult> DeleteWorkflowAffectation(int configId, int clientId)
    {
        if (clientId <= 0 || configId <= 0)
        {
            TempData["Error"] = "Affectation invalide.";
            return RedirectToAction("Index");
        }

        var result = await _workflow.DeleteClientConfigAsync(configId, clientId);
        if (result?.Success == true)
            TempData["Success"] = result.Message ?? "Workflow retiré de l'affectation client.";
        else
            TempData["Error"] = result?.Message ?? "Impossible de supprimer l'affectation.";

        return RedirectToAction("Details", new { id = clientId, tab = "workflow" });
    }

    /// <summary>Clone cabinet template → client-owned copy (or return existing) for Personnaliser.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.WorkflowConfigure)]
    public async Task<IActionResult> EnsureClientWorkflow([FromBody] EnsureClientWorkflowDto dto)
    {
        if (dto.ClientId <= 0 || dto.SourceTemplateId <= 0)
            return BadRequest(new { success = false, message = "Client ou modèle invalide." });

        var result = await _workflow.EnsureClientTemplateAsync(dto);
        if (result?.Success != true || result.Data == null)
            return BadRequest(new { success = false, message = result?.Message ?? "Clone impossible." });

        return Json(new { success = true, data = result.Data, message = result.Message });
    }

    /// <summary>Save Personnaliser: client config + editable steps on the client-owned copy.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.WorkflowConfigure)]
    public async Task<IActionResult> SaveClientWorkflowCustomization(ClientWorkflowCustomizeDto dto)
    {
        if (dto.ClientId <= 0)
        {
            TempData["Error"] = "Client invalide.";
            return RedirectToAction("Index");
        }

        var result = await _workflow.SaveClientWorkflowCustomizationAsync(dto);
        if (result?.Success == true)
            TempData["Success"] = result.Message ?? "Personnalisation enregistrée pour ce client.";
        else
            TempData["Error"] = result?.Message ?? "Erreur lors de la personnalisation.";

        return RedirectToAction("Details", new { id = dto.ClientId, tab = "workflow" });
    }

    /// <summary>JSON for Personnaliser panel: template + builder steps.</summary>
    [HttpGet]
    [RequirePermission(ClaimDefinitions.WorkflowView, ClaimDefinitions.WorkflowConfigure, ClaimDefinitions.ClientDetail)]
    public async Task<IActionResult> WorkflowTemplateJson(int id)
    {
        var template = await _workflow.GetTemplateByIdAsync(id);
        if (template == null) return NotFound(new { success = false, message = "Modèle introuvable." });
        return Json(new
        {
            success = true,
            data = template
        });
    }

    [HttpGet]
    [RequirePermission(ClaimDefinitions.ClientDetail)]
    public async Task<IActionResult> DetailsJson(int id)
    {
        var client = await _clients.GetByIdAsync(id);
        return Json(new { found = client != null, data = client });
    }

    [HttpGet]
    [RequirePermission(ClaimDefinitions.ClientCreate)]
    public async Task<IActionResult> Create()
    {
        await LoadReferenceDataToViewBagAsync(ClientRefCategories);
        return View(new ClientCreateDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.ClientCreate)]
    public async Task<IActionResult> Create(ClientCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadReferenceDataToViewBagAsync(ClientRefCategories);
            return View(dto);
        }
        var result = await _clients.CreateAsync(dto);
        if (result?.Success == true)
        {
            TempData["Success"] = result.Message;
            return RedirectToAction("Index");
        }
        SetApiError(result, "Erreur lors de la création du client.");
        await LoadReferenceDataToViewBagAsync(ClientRefCategories);
        return View(dto);
    }

    [HttpGet]
    [RequirePermission(ClaimDefinitions.ClientEdit)]
    public async Task<IActionResult> Edit(int id)
    {
        var client = await _clients.GetByIdAsync(id);
        if (client == null) return NotFound();

        var dto = new ClientCreateDto
        {
            RaisonSociale = client.RaisonSociale,
            ICE = client.ICE,
            IF = client.IF,
            RC = client.RC,
            NumeroCNSS = client.NumeroCNSS,
            NumeroPatente = client.NumeroPatente,
            FormeJuridique = client.FormeJuridique,
            SecteurActivite = client.SecteurActivite,
            CodeActivite = client.CodeActivite,
            CapitalSocial = client.CapitalSocial,
            DateCreation = client.DateCreation,
            DateDebutActivite = client.DateDebutActivite,
            AdresseSiegeSocial = client.AdresseSiegeSocial,
            VilleSiegeSocial = client.VilleSiegeSocial,
            TelephonePrincipal = client.TelephonePrincipal,
            EmailPrincipal = client.EmailPrincipal,
            ContactNom = client.ContactNom,
            ContactFonction = client.ContactFonction,
            ContactTelephone = client.ContactTelephone,
            ContactEmail = client.ContactEmail,
            TypeTVA = client.TypeTVA,
            MoisClotureExercice = client.MoisClotureExercice,
            DateDebutContrat = client.DateDebutContrat,
            ManagerPrincipalId = client.ManagerPrincipalId,
            NotesInternes = client.NotesInternes,
            // ⚠️ Services are managed exclusively through ContratCommercial validation.
            // ServicesFiduciaires is intentionally left empty — read-only display in the view
            // fetches them directly from the ClientDto model.
            ServicesFiduciaires = new List<ServiceContratCreateDto>()
        };
        ViewBag.ClientId = id;
        ViewBag.CurrentStatut = client.Statut;
        ViewBag.ClientServices = client.ServicesFiduciaires ?? new List<ServiceContratDto>();
        await LoadReferenceDataToViewBagAsync(ClientRefCategories);
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.ClientEdit)]
    public async Task<IActionResult> Edit(int id, ClientCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ClientId = id;
            await LoadReferenceDataToViewBagAsync(ClientRefCategories);
            return View(dto);
        }
        var result = await _clients.UpdateAsync(id, dto);
        if (result?.Success == true)
        {
            TempData["Success"] = result.Message;
            return RedirectToAction("Details", new { id });
        }
        SetApiError(result, "Erreur lors de la mise à jour du client.");
        ViewBag.ClientId = id;
        await LoadReferenceDataToViewBagAsync(ClientRefCategories);
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.ClientDelete)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _clients.DeleteAsync(id);
        SetApiResult(result, "Client supprimé avec succès.", "Erreur lors de la suppression du client.");
        return RedirectToAction("Index");
    }

    // ── Global Client Context (AJAX from topbar) ────────────────

    /// <summary>
    /// AJAX endpoint — sets the global selected client in session.
    /// Called from the topbar client selector dropdown.
    /// </summary>
    [HttpPost]
    [RequirePermission(ClaimDefinitions.ClientList)]
    public async Task<IActionResult> SetClientContext(int? clientId)
    {
        if (clientId.HasValue)
        {
            var client = await _clients.GetByIdAsync(clientId.Value);
            if (client != null)
            {
                HttpContext.Session.SetString("SelectedClientId", client.Id.ToString());
                HttpContext.Session.SetString("SelectedClientName", client.RaisonSociale);
                // Client selected — métier space chosen next (ClientSpaces)
                HttpContext.Session.Remove("WorkspaceMode");

                // Auto-pick exercice first, then scope ActiveServices to that exercice
                object? exercicePayload = null;
                try
                {
                    await _compta.EnsureLedgerAsync(client.Id);
                    var exercices = await _compta.GetExercicesAsync(client.Id);
                    var pick = exercices
                        .Where(e => string.Equals(e.Statut, "Ouvert", StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(e => e.Annee)
                        .FirstOrDefault()
                        ?? exercices.OrderByDescending(e => e.Annee).FirstOrDefault();
                    if (pick != null)
                    {
                        SetExerciceSession(pick);
                        exercicePayload = new { pick.Id, pick.Annee, pick.Statut, label = pick.Label };
                    }
                    else
                        ClearExerciceSession();
                }
                catch
                {
                    ClearExerciceSession();
                }

                var access = await _clients.GetContractAccessAsync(clientId.Value, SelectedExerciceId);
                HttpContext.Session.SetString("ActiveServices", System.Text.Json.JsonSerializer.Serialize(access.ActiveServices));
                HttpContext.Session.SetString("HasActiveContract", access.AllowsManage.ToString());
                HttpContext.Session.SetString("ContractAccessMode", access.Mode);
                HttpContext.Session.SetString("ContractAccessReason", access.Reason);
                HttpContext.Session.SetString("ContractAccessReasonLabel", access.ReasonLabel ?? "");

                return Json(new
                {
                    success = true,
                    clientName = client.RaisonSociale,
                    clientId = client.Id,
                    activeServices = access.ActiveServices,
                    hasActiveContract = access.AllowsManage,
                    contractAccessMode = access.Mode,
                    contractAccessReason = access.Reason,
                    exercice = exercicePayload,
                    redirectToClientSpaces = true
                });
            }
        }
        // Clear selection — return to workspace gateway
        HttpContext.Session.Remove("SelectedClientId");
        HttpContext.Session.Remove("SelectedClientName");
        HttpContext.Session.Remove("ActiveServices");
        HttpContext.Session.Remove("HasActiveContract");
        HttpContext.Session.Remove("ContractAccessMode");
        HttpContext.Session.Remove("ContractAccessReason");
        HttpContext.Session.Remove("ContractAccessReasonLabel");
        HttpContext.Session.Remove("WorkspaceMode");
        ClearExerciceSession();
        return Json(new { success = true, clientName = (string?)null, clientId = (int?)null, activeServices = new List<string>(), hasActiveContract = false, redirectToWorkspace = true });
    }

    /// <summary>
    /// AJAX endpoint — returns all clients for the topbar dropdown search.
    /// </summary>
    [HttpGet]
    [RequirePermission(ClaimDefinitions.ClientList)]
    public async Task<IActionResult> SearchClients(string? q)
    {
        var clients = await _clients.GetAllForDropdownAsync();
        if (!string.IsNullOrWhiteSpace(q))
        {
            clients = clients.Where(c => c.RaisonSociale.Contains(q, StringComparison.OrdinalIgnoreCase)
                || (c.ICE ?? "").Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        return Json(clients.Select(c => new { c.Id, c.RaisonSociale, c.ICE, c.FormeJuridique, c.Statut }).Take(20));
    }

    /// <summary>
    /// AJAX endpoint — auto-generate declarations from active service contracts.
    /// Called from Edit page "Générer Déclarations" button.
    /// </summary>
    [HttpPost]
    [RequirePermission(ClaimDefinitions.DeclarationCreate)]
    public async Task<IActionResult> GenerateDeclarations(int clientId, int exercice)
    {
        var request = new Application.DTOs.GenerateDeclarationsRequestDto
        {
            ClientId = clientId,
            ExerciceFiscal = exercice
        };
        var result = await _clients.GenerateDeclarationsAsync(clientId, request);
        if (result?.Success == true)
        {
            return Json(new { success = true, totalGenerated = result.Data?.TotalGenerated ?? 0, warnings = result.Data?.Warnings ?? new List<string>() });
        }
        return Json(new { success = false, message = result?.Message ?? "Erreur lors de la génération." });
    }

    /// <summary>
    /// AJAX endpoint — get service contracts for a client.
    /// </summary>
    [HttpGet]
    [RequirePermission(ClaimDefinitions.CommercialDetail, ClaimDefinitions.ClientDetail)]
    public async Task<IActionResult> GetServiceContrats(int clientId, int? exerciceFiscal = null)
    {
        var services = await _clients.GetServiceContratsAsync(clientId, exerciceFiscal);
        return Json(services);
    }

    /// <summary>
    /// AJAX endpoint — sync services from a validated contract.
    /// Re-activates services for clients whose contracts were validated before the contract-driven architecture.
    /// </summary>
    [HttpPost]
    [RequirePermission(ClaimDefinitions.CommercialEdit)]
    public async Task<IActionResult> SyncServicesFromContrat(int contratId)
    {
        var result = await _commercial.SyncServicesFromContratAsync(contratId);
        if (result?.Success == true)
        {
            return Json(new { success = true, count = result.Data, message = result.Message });
        }
        return Json(new { success = false, message = result?.Message ?? "Erreur lors de la synchronisation." });
    }

    // ── Client Users (Extranet) ────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.UserManage)]
    public async Task<IActionResult> CreateClientUser(int clientId, string email, string password, string firstName, string lastName, string? telephone)
    {
        var dto = new ClientUserCreateDto
        {
            Email = email,
            Password = password,
            FirstName = firstName,
            LastName = lastName,
            Telephone = telephone
        };
        var result = await _clients.CreateClientUserAsync(clientId, dto);
        if (result?.Success == true)
            TempData["Success"] = result.Message;
        else
            TempData["Error"] = result?.Message ?? "Erreur lors de la création de l'utilisateur.";
        return RedirectToAction("Details", new { id = clientId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.UserManage)]
    public async Task<IActionResult> ToggleClientUser(int clientId, string userId)
    {
        var result = await _clients.ToggleClientUserActiveAsync(clientId, userId);
        if (result?.Success == true)
            TempData["Success"] = result.Message;
        else
            TempData["Error"] = result?.Message ?? "Erreur lors de la mise à jour.";
        return RedirectToAction("Details", new { id = clientId });
    }

    // ── Contrats Clients ────────────────

    [RequirePermission(ClaimDefinitions.CommercialDetail, ClaimDefinitions.ClientDetail)]
    public async Task<IActionResult> Contrats(int? clientId, string? statut, int page = 1)
    {
        var contrats = await _commercial.GetContratsAsync(clientId: clientId, statut: statut, page: page);
        ViewBag.Clients = await _clients.GetAllForDropdownAsync();
        ViewBag.SelectedClientId = clientId;
        ViewBag.SelectedStatut = statut;
        return View(contrats);
    }
}
