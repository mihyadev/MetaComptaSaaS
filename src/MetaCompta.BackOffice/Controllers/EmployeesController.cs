using MetaCompta.Application.DTOs;
using MetaCompta.BackOffice.Filters;
using MetaCompta.BackOffice.Services;
using MetaCompta.BackOffice.Helpers;
using MetaCompta.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.BackOffice.Controllers;

[RequireSelectedClient]
[RequireWorkspaceMode(WorkspaceModes.Paie)]
public class EmployeesController : BaseController
{
    private readonly IEmployeeFacade _employees;
    private readonly IClientFacade _clients;
    private readonly IPayrollFacade _payroll;

    private static readonly string[] EmpRefCategories = new[]
    {
        "TYPE_CONTRAT", "SITUATION_FAMILIALE", "SEXE", "MODE_PAIEMENT", "STATUT_EMPLOYE", "VILLE"
    };

    public EmployeesController(
        IReferenceDataFacade referenceData,
        IHttpContextAccessor contextAccessor,
        IEmployeeFacade employees,
        IClientFacade clients,
        IPayrollFacade payroll)
        : base(referenceData, contextAccessor)
    {
        _employees = employees;
        _clients = clients;
        _payroll = payroll;
    }

    private async Task LoadClientsAsync()
    {
        try
        {
            ViewBag.Client = SelectedClientId.HasValue ? await _clients.GetByIdAsync(SelectedClientId.Value) : null;
        }
        catch
        {
            ViewBag.Client = null;
        }
    }

    [RequirePermission(ClaimDefinitions.EmployeeList, ClaimDefinitions.EmployeeListByClient)]
    public async Task<IActionResult> Index(int? clientId = null, int page = 1, string? search = null, string? typeContrat = null)
    {
        clientId ??= SelectedClientId;
        var result = await _employees.GetAllAsync(clientId, page, search);

        await LoadClientsAsync();
        await LoadReferenceDataToViewBagAsync("TYPE_CONTRAT", "STATUT_EMPLOYE");
        ViewBag.Search = search;
        ViewBag.ClientId = clientId;
        ViewBag.TypeContrat = typeContrat;
        return View(result);
    }

    [RequirePermission(ClaimDefinitions.EmployeeDetail)]
    public async Task<IActionResult> Details(int id)
    {
        var employee = await _employees.GetByIdAsync(id);
        if (employee == null) return NotFound();
        await LoadReferenceDataToViewBagAsync("TYPE_CONTRAT", "STATUT_EMPLOYE", "SITUATION_FAMILIALE", "MODE_PAIEMENT");
        return View(employee);
    }

    [HttpGet]
    [RequirePermission(ClaimDefinitions.EmployeeCreate)]
    public async Task<IActionResult> Create(int? clientId = null)
    {
        clientId ??= SelectedClientId;
        await LoadClientsAsync();
        await LoadReferenceDataToViewBagAsync(EmpRefCategories);
        var dto = new EmployeeCreateDto();
        if (clientId.HasValue) dto.ClientId = clientId.Value;
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.EmployeeCreate)]
    public async Task<IActionResult> Create(EmployeeCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadClientsAsync();
            await LoadReferenceDataToViewBagAsync(EmpRefCategories);
            return View(dto);
        }
        var result = await _employees.CreateAsync(dto);
        if (result?.Success == true)
        {
            TempData["Success"] = result.Message;
            return RedirectToAction("Index");
        }
        SetApiError(result, "Erreur lors de la création de l'employé.");
        await LoadClientsAsync();
        await LoadReferenceDataToViewBagAsync(EmpRefCategories);
        return View(dto);
    }

    [HttpGet]
    [RequirePermission(ClaimDefinitions.EmployeeEdit)]
    public async Task<IActionResult> Edit(int id)
    {
        var employee = await _employees.GetByIdAsync(id);
        if (employee == null) return NotFound();

        var dto = new EmployeeCreateDto
        {
            CIN = employee.CIN,
            Nom = employee.Nom,
            Prenom = employee.Prenom,
            ClientId = employee.ClientId,
            NumeroCNSS = employee.NumeroCNSS,
            DateEmbauche = employee.DateEmbauche,
            TypeContrat = employee.TypeContrat,
            Poste = employee.Poste,
            Departement = employee.Departement,
            SalaireBase = employee.SalaireBase,
            SituationFamiliale = employee.SituationFamiliale,
            NombreEnfantsACharge = employee.NombreEnfantsACharge,
            TelephonePortable = employee.TelephonePortable,
            Email = employee.Email,
            ModePaiement = employee.ModePaiement,
            RIB = employee.RIB,
            NomBanque = employee.NomBanque
        };

        await LoadClientsAsync();
        await LoadReferenceDataToViewBagAsync(EmpRefCategories);
        ViewBag.EmployeeId = id;
        ViewBag.EmployeeName = employee.FullName;
        ViewBag.CurrentStatut = employee.Statut;
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.EmployeeEdit)]
    public async Task<IActionResult> Edit(int id, EmployeeCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadClientsAsync();
            await LoadReferenceDataToViewBagAsync(EmpRefCategories);
            ViewBag.EmployeeId = id;
            return View(dto);
        }
        var result = await _employees.UpdateAsync(id, dto);
        if (result?.Success == true)
        {
            TempData["Success"] = result.Message;
            return RedirectToAction("Details", new { id });
        }
        SetApiError(result, "Erreur lors de la mise à jour de l'employé.");
        await LoadClientsAsync();
        await LoadReferenceDataToViewBagAsync(EmpRefCategories);
        ViewBag.EmployeeId = id;
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.EmployeeDelete)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _employees.DeleteAsync(id);
        SetApiResult(result, "Employé supprimé.", "Erreur lors de la suppression.");
        return RedirectToAction("Index");
    }


    // ================================================================
    //  AFFECTATION RUBRIQUES (AJAX endpoints for Employee Details tab)
    // ================================================================

    [HttpGet]
    [RequirePermission(ClaimDefinitions.PayrollDetail, ClaimDefinitions.EmployeeDetail)]
    public async Task<IActionResult> GetAffectations(int employeeId)
    {
        var list = await _payroll.GetAffectationsAsync(employeeId);
        return Json(list);
    }

    [HttpGet]
    [RequirePermission(ClaimDefinitions.PayrollDetail, ClaimDefinitions.EmployeeDetail)]
    public async Task<IActionResult> GetRubriquesDisponibles(int employeeId, int? clientId = null)
    {
        var list = await _payroll.GetRubriquesDisponiblesAsync(employeeId, clientId);
        return Json(list);
    }

    [HttpPost]
    [RequirePermission(ClaimDefinitions.PayrollEdit)]
    public async Task<IActionResult> AddAffectation([FromBody] AffectationRubriqueCreateDto dto)
    {
        var result = await _payroll.AddAffectationAsync(dto);
        return Json(result);
    }

    [HttpPut]
    [RequirePermission(ClaimDefinitions.PayrollEdit)]
    public async Task<IActionResult> UpdateAffectation(int id, [FromBody] AffectationRubriqueCreateDto dto)
    {
        var result = await _payroll.UpdateAffectationAsync(id, dto);
        return Json(result);
    }

    [HttpDelete]
    [RequirePermission(ClaimDefinitions.PayrollEdit)]
    public async Task<IActionResult> RemoveAffectation(int id)
    {
        var result = await _payroll.RemoveAffectationAsync(id);
        return Json(result);
    }
}
