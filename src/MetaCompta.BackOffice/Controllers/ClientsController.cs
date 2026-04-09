using MetaCompta.Application.DTOs;
using MetaCompta.BackOffice.Services;
using MetaCompta.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.BackOffice.Controllers;

public class ClientsController : BaseController
{
    private readonly IClientFacade _clients;
    private readonly ICommercialFacade _commercial;
    private readonly IWorkflowFacade _workflow;

    private static readonly string[] ClientRefCategories = new[]
    {
        "FORME_JURIDIQUE", "STATUT_CLIENT", "TYPE_TVA", "VILLE", "SECTEUR_ACTIVITE"
    };

    public ClientsController(
        IReferenceDataFacade referenceData,
        IHttpContextAccessor contextAccessor,
        IClientFacade clients,
        ICommercialFacade commercial,
        IWorkflowFacade workflow)
        : base(referenceData, contextAccessor)
    {
        _clients = clients;
        _commercial = commercial;
        _workflow = workflow;
    }

    public async Task<IActionResult> Index(int page = 1, string? search = null, string? formeJuridique = null, string? statut = null)
    {
        var result = await _clients.GetAllAsync(page, search, statut);

        await LoadReferenceDataToViewBagAsync("FORME_JURIDIQUE", "STATUT_CLIENT");
        ViewBag.Search = search;
        ViewBag.Statut = statut;
        ViewBag.FormeJuridique = formeJuridique;
        return View(result);
    }

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

            // ── Workflow data (embedded tab) ──
            try
            {
                var templates = await _workflow.GetTemplatesAsync(clientId: null, page: 1);
                ViewBag.WorkflowTemplates = templates?.Items ?? new List<WorkflowTemplateDto>();

                var instances = await _workflow.GetInstancesAsync(clientId: id, page: 1);
                ViewBag.WorkflowInstances = instances?.Items ?? new List<WorkflowInstanceDto>();

                var pendingSteps = await _workflow.GetMyPendingStepsAsync();
                ViewBag.WorkflowPendingSteps = pendingSteps ?? new List<WorkflowStepInstanceDto>();
            }
            catch
            {
                // Workflow API may not be ready — degrade gracefully
                ViewBag.WorkflowTemplates = new List<WorkflowTemplateDto>();
                ViewBag.WorkflowInstances = new List<WorkflowInstanceDto>();
                ViewBag.WorkflowPendingSteps = new List<WorkflowStepInstanceDto>();
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

    [HttpGet]
    public async Task<IActionResult> DetailsJson(int id)
    {
        var client = await _clients.GetByIdAsync(id);
        return Json(new { found = client != null, data = client });
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadReferenceDataToViewBagAsync(ClientRefCategories);
        return View(new ClientCreateDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
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
    public async Task<IActionResult> SetClientContext(int? clientId)
    {
        if (clientId.HasValue)
        {
            var client = await _clients.GetByIdAsync(clientId.Value);
            if (client != null)
            {
                HttpContext.Session.SetString("SelectedClientId", client.Id.ToString());
                HttpContext.Session.SetString("SelectedClientName", client.RaisonSociale);

                // Store active services in session for sidebar menu filtering
                var activeServices = await _clients.GetActiveServicesAsync(clientId.Value);
                HttpContext.Session.SetString("ActiveServices", System.Text.Json.JsonSerializer.Serialize(activeServices));

                // Store contract status — gates declaration create/edit in sidebar & views
                var hasActiveContract = await _clients.HasActiveContractAsync(clientId.Value);
                HttpContext.Session.SetString("HasActiveContract", hasActiveContract.ToString());

                return Json(new { success = true, clientName = client.RaisonSociale, clientId = client.Id, activeServices, hasActiveContract });
            }
        }
        // Clear selection
        HttpContext.Session.Remove("SelectedClientId");
        HttpContext.Session.Remove("SelectedClientName");
        HttpContext.Session.Remove("ActiveServices");
        HttpContext.Session.Remove("HasActiveContract");
        return Json(new { success = true, clientName = (string?)null, clientId = (int?)null, activeServices = new List<string>(), hasActiveContract = false });
    }

    /// <summary>
    /// AJAX endpoint — returns all clients for the topbar dropdown search.
    /// </summary>
    [HttpGet]
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
    public async Task<IActionResult> SyncServicesFromContrat(int contratId)
    {
        var result = await _commercial.SyncServicesFromContratAsync(contratId);
        if (result?.Success == true)
        {
            return Json(new { success = true, count = result.Data, message = result.Message });
        }
        return Json(new { success = false, message = result?.Message ?? "Erreur lors de la synchronisation." });
    }
}
