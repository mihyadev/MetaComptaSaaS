using MetaCompta.Application.DTOs;
using MetaCompta.BackOffice.Filters;
using MetaCompta.BackOffice.Services;
using MetaCompta.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.BackOffice.Controllers;

[RequireSelectedClient]
public class PayrollController : BaseController
{
    private readonly IPayrollFacade _payroll;
    private readonly IClientFacade _clients;
    private readonly BulletinPdfService _pdf;

    public PayrollController(
        IReferenceDataFacade referenceData,
        IHttpContextAccessor contextAccessor,
        IPayrollFacade payroll,
        IClientFacade clients,
        BulletinPdfService pdf)
        : base(referenceData, contextAccessor)
    {
        _payroll = payroll;
        _clients = clients;
        _pdf = pdf;
    }

    // ===========================================================
    // PERIODES
    // ===========================================================

    [RequirePermission(ClaimDefinitions.PayrollDetail)]
    public async Task<IActionResult> Index(int? clientId = null)
    {
        clientId ??= SelectedClientId;
        var periods = await _payroll.GetAllPeriodsAsync(clientId, SelectedExerciceId);
        ViewBag.Client = SelectedClientId.HasValue ? await _clients.GetByIdAsync(SelectedClientId.Value) : null;
        ViewBag.ClientId = clientId;
        await LoadReferenceDataToViewBagAsync("STATUT_PAIE");
        return View(periods);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.PayrollCreate)]
    public async Task<IActionResult> Initialize(int clientId, int annee, int mois)
    {
        var result = await _payroll.InitializeAsync(clientId, annee, mois);
        if (result?.Success == true)
        {
            TempData["Success"] = result.Message;
            return RedirectToAction("Period", new { id = result.Data?.Id });
        }
        TempData["Error"] = result?.Message;
        if (result?.Errors?.Count > 0)
            TempData["ErrorDetails"] = string.Join("|||", result.Errors);
        return RedirectToAction("Index");
    }

    [RequirePermission(ClaimDefinitions.PayrollDetail)]
    public async Task<IActionResult> Period(int id)
    {
        var period = await _payroll.GetPeriodAsync(id);
        ViewBag.PaySlips = await _payroll.GetPaySlipsAsync(id);
        TempData["CurrentPeriodId"] = id;
        TempData.Keep("CurrentPeriodId");
        await LoadReferenceDataToViewBagAsync("STATUT_PAIE");
        return View(period);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.PayrollEdit)]
    public async Task<IActionResult> Calculate(int id)
    {
        var result = await _payroll.CalculateAllAsync(id);
        SetApiResult(result, null, "Erreur lors du calcul.");
        return RedirectToAction("Period", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.PayrollValidate)]
    public async Task<IActionResult> Validate(int id)
    {
        var result = await _payroll.ValidateAsync(id);
        SetApiResult(result, null, "Erreur lors de la validation.");
        return RedirectToAction("Period", new { id });
    }

    // ===========================================================
    // BULLETIN DE PAIE (view + PDF)
    // ===========================================================

    [RequirePermission(ClaimDefinitions.PayrollDetail)]
    public async Task<IActionResult> Bulletin(int id)
    {
        var bulletin = await _payroll.GetBulletinAsync(id);
        if (bulletin == null)
        {
            TempData["Error"] = "Bulletin introuvable.";
            return RedirectToAction("Index");
        }
        ViewBag.PaySlipId = id;
        return View(bulletin);
    }

    [RequirePermission(ClaimDefinitions.PayrollDetail)]
    public async Task<IActionResult> BulletinPdf(int id)
    {
        var bulletin = await _payroll.GetBulletinAsync(id);
        if (bulletin == null) return NotFound("Bulletin introuvable.");

        var pdfBytes = _pdf.Generate(bulletin);
        var fileName = $"Bulletin_{bulletin.EmployeeNom?.Replace(" ", "_")}_{bulletin.Mois:D2}_{bulletin.Annee}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    // ===========================================================
    // DEMO SEED 2025
    // ===========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.SystemAdmin)]
    public async Task<IActionResult> SeedDemo2025()
    {
        if (!SelectedClientId.HasValue)
        {
            TempData["Error"] = "Veuillez selectionner un client.";
            return RedirectToAction("Index");
        }
        var result = await _payroll.SeedDemo2025Async(SelectedClientId.Value);
        if (result?.Success == true) TempData["Success"] = result.Message ?? "Demo 2025 chargee.";
        else TempData["Error"] = result?.Message ?? "Erreur lors du chargement.";
        return RedirectToAction("Index");
    }

    // ===========================================================
    // RUBRIQUES DE PAIE
    // ===========================================================

    [RequirePermission(ClaimDefinitions.PayrollDetail)]
    public async Task<IActionResult> Rubriques()
    {
        var rubriques = await _payroll.GetRubriquesAsync();
        return View(rubriques);
    }

    [HttpGet]
    [RequirePermission(ClaimDefinitions.PayrollEdit)]
    public async Task<IActionResult> CreateRubrique()
    {
        return View(new RubriqueCreateDto { Actif = true, AfficherSurBulletin = true, OrdreExecution = 100, OrdreAffichage = 100 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.PayrollEdit)]
    public async Task<IActionResult> CreateRubrique(RubriqueCreateDto dto)
    {
        var result = await _payroll.CreateRubriqueAsync(dto);
        if (result?.Success == true)
        {
            TempData["Success"] = "Rubrique creee avec succes.";
            return RedirectToAction("Rubriques");
        }
        TempData["Error"] = result?.Message ?? "Erreur lors de la creation.";
        return View(dto);
    }

    [HttpGet]
    [RequirePermission(ClaimDefinitions.PayrollEdit)]
    public async Task<IActionResult> EditRubrique(int id)
    {
        var rubrique = await _payroll.GetRubriqueAsync(id);
        if (rubrique == null) { TempData["Error"] = "Rubrique introuvable."; return RedirectToAction("Rubriques"); }
        ViewBag.RubriqueId = id;
        ViewBag.RubriqueCode = rubrique.Code;
        ViewBag.IsSysteme = rubrique.Systeme;
        var dto = new RubriqueCreateDto
        {
            Code = rubrique.Code, Libelle = rubrique.Libelle, Type = rubrique.Type,
            Section = rubrique.Section, OrdreExecution = rubrique.OrdreExecution,
            OrdreAffichage = rubrique.OrdreAffichage, Actif = rubrique.Actif,
            AfficherSurBulletin = rubrique.AfficherSurBulletin, Obligatoire = rubrique.Obligatoire,
            FormuleBase = rubrique.FormuleBase, FormuleTaux = rubrique.FormuleTaux,
            FormuleMontant = rubrique.FormuleMontant, FormuleGain = rubrique.FormuleGain,
            FormuleRetenue = rubrique.FormuleRetenue, Description = rubrique.Description,
            DependsDe = rubrique.DependsDe
        };
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.PayrollEdit)]
    public async Task<IActionResult> EditRubrique(int id, RubriqueCreateDto dto)
    {
        var result = await _payroll.UpdateRubriqueAsync(id, dto);
        if (result?.Success == true)
        {
            TempData["Success"] = "Rubrique mise a jour.";
            return RedirectToAction("Rubriques");
        }
        TempData["Error"] = result?.Message ?? "Erreur lors de la mise a jour.";
        ViewBag.RubriqueId = id;
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.PayrollEdit)]
    public async Task<IActionResult> ToggleRubrique(int id)
    {
        var result = await _payroll.ToggleRubriqueAsync(id);
        if (result?.Success == true) TempData["Success"] = "Statut modifie.";
        else TempData["Error"] = "Erreur lors du changement de statut.";
        return RedirectToAction("Rubriques");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.PayrollEdit)]
    public async Task<IActionResult> DeleteRubrique(int id)
    {
        var result = await _payroll.DeleteRubriqueAsync(id);
        if (result?.Success == true) TempData["Success"] = "Rubrique supprimee.";
        else TempData["Error"] = "Erreur lors de la suppression.";
        return RedirectToAction("Rubriques");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.SystemAdmin)]
    public async Task<IActionResult> SeedRubriques()
    {
        var result = await _payroll.SeedRubriquesAsync();
        if (result?.Success == true) TempData["Success"] = result.Message ?? "Rubriques par defaut chargees.";
        else TempData["Error"] = result?.Message ?? "Erreur.";
        return RedirectToAction("Rubriques");
    }

    // ===========================================================
    // CONSTANTES & TAUX
    // ===========================================================

    [RequirePermission(ClaimDefinitions.PayrollDetail)]
    public async Task<IActionResult> Constantes()
    {
        var constantes = await _payroll.GetConstantesAsync();
        return View(constantes);
    }

    [HttpGet]
    [RequirePermission(ClaimDefinitions.PayrollEdit)]
    public IActionResult CreateConstante()
    {
        return View(new ConstanteGlobaleDto { Actif = true, DateEffet = DateTime.Today });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.PayrollEdit)]
    public async Task<IActionResult> CreateConstante(ConstanteGlobaleDto dto)
    {
        var result = await _payroll.CreateConstanteAsync(dto);
        if (result?.Success == true)
        {
            TempData["Success"] = "Constante creee avec succes.";
            return RedirectToAction("Constantes");
        }
        TempData["Error"] = result?.Message ?? "Erreur lors de la creation.";
        return View(dto);
    }

    [HttpGet]
    [RequirePermission(ClaimDefinitions.PayrollEdit)]
    public async Task<IActionResult> EditConstante(int id)
    {
        var constantes = await _payroll.GetConstantesAsync();
        var constante = constantes.FirstOrDefault(c => c.Id == id);
        if (constante == null) { TempData["Error"] = "Constante introuvable."; return RedirectToAction("Constantes"); }
        ViewBag.ConstanteId = id;
        ViewBag.ConstanteCode = constante.Code;
        return View(constante);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.PayrollEdit)]
    public async Task<IActionResult> EditConstante(int id, ConstanteGlobaleDto dto)
    {
        var result = await _payroll.UpdateConstanteAsync(id, dto);
        if (result?.Success == true)
        {
            TempData["Success"] = "Constante mise a jour.";
            return RedirectToAction("Constantes");
        }
        TempData["Error"] = result?.Message ?? "Erreur lors de la mise a jour.";
        ViewBag.ConstanteId = id;
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.PayrollEdit)]
    public async Task<IActionResult> DeleteConstante(int id)
    {
        var result = await _payroll.DeleteConstanteAsync(id);
        if (result?.Success == true) TempData["Success"] = "Constante supprimee.";
        else TempData["Error"] = "Erreur lors de la suppression.";
        return RedirectToAction("Constantes");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.SystemAdmin)]
    public async Task<IActionResult> SeedConstantes()
    {
        var result = await _payroll.SeedConstantesAsync();
        if (result?.Success == true) TempData["Success"] = result.Message ?? "Constantes par defaut chargees.";
        else TempData["Error"] = result?.Message ?? "Erreur.";
        return RedirectToAction("Constantes");
    }


    // DYNAMIC EMPLOYEE VARIABLES (AJAX)
    // ===========================================================

    [HttpGet]
    [RequirePermission(ClaimDefinitions.PayrollDetail)]
    public async Task<IActionResult> GetEmployeeVariables(int employeeId, int annee, int mois)
    {
        var vars = await _payroll.GetVariablesForPeriodAsync(employeeId, annee, mois);
        return Json(vars);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.PayrollEdit)]
    public async Task<IActionResult> SaveEmployeeVariables(int employeeId, int annee, int mois, List<VariableEmployeCreateDto> variables, int periodId)
    {
        var result = await _payroll.UpdateVariablesBulkAsync(employeeId, annee, mois, variables);
        if (result?.Success == true)
            TempData["Success"] = "Variables enregistrees avec succes.";
        else
            TempData["Error"] = result?.Message ?? "Erreur lors de l'enregistrement.";
        return RedirectToAction("Period", new { id = periodId });
    }
}
