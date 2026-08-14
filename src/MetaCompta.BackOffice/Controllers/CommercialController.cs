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

    public CommercialController(
        IReferenceDataFacade referenceData,
        IHttpContextAccessor contextAccessor,
        ICommercialFacade commercial,
        IClientFacade clients,
        ICabinetFacade cabinet)
        : base(referenceData, contextAccessor)
    {
        _commercial = commercial;
        _clients = clients;
        _cabinet = cabinet;
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
        await LoadReferenceDataToViewBagAsync("TYPE_SERVICE");

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
        await LoadReferenceDataToViewBagAsync("TYPE_SERVICE");

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
        return View(contrat);
    }

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
    public async Task<IActionResult> ContratCreate()
    {
        var clients = await _clients.GetAllForDropdownAsync();
        ViewBag.Clients = clients;
        await LoadEmployeesAsync();
        await LoadReferenceDataToViewBagAsync("TYPE_SERVICE");

        return View(new ContratCommercialCreateDto
        {
            DateDebutContrat = DateTime.UtcNow.Date,
            DateFinContrat = new DateTime(DateTime.UtcNow.Year, 12, 31),
            Devise = "MAD",
            ModeleFacturation = "Forfait"
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

        var clients = await _clients.GetAllForDropdownAsync();
        ViewBag.Clients = clients;
        await LoadEmployeesAsync();
        await LoadReferenceDataToViewBagAsync("TYPE_SERVICE");

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

        var clients = await _clients.GetAllForDropdownAsync();
        ViewBag.Clients = clients;
        await LoadEmployeesAsync();
        await LoadReferenceDataToViewBagAsync("TYPE_SERVICE");

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
            ServicesActives = contrat.ServicesActives,
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
        ViewBag.ContratId = id;
        return View("ContratCreate", dto);
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

        var clients = await _clients.GetAllForDropdownAsync();
        ViewBag.Clients = clients;
        await LoadEmployeesAsync();
        await LoadReferenceDataToViewBagAsync("TYPE_SERVICE");
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