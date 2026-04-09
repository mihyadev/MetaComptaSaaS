using MetaCompta.Application.DTOs;
using MetaCompta.BackOffice.Filters;
using MetaCompta.BackOffice.Services;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.BackOffice.Controllers;

[RequireSelectedClient]
public class EmployeesController : BaseController
{
    private readonly IEmployeeFacade _employees;
    private readonly IClientFacade _clients;

    private static readonly string[] EmpRefCategories = new[]
    {
        "TYPE_CONTRAT", "SITUATION_FAMILIALE", "SEXE", "MODE_PAIEMENT", "STATUT_EMPLOYE", "VILLE"
    };

    public EmployeesController(
        IReferenceDataFacade referenceData,
        IHttpContextAccessor contextAccessor,
        IEmployeeFacade employees,
        IClientFacade clients)
        : base(referenceData, contextAccessor)
    {
        _employees = employees;
        _clients = clients;
    }

    private async Task LoadClientsAsync()
    {
        try
        {
            ViewBag.Clients = await _clients.GetAllForDropdownAsync();
        }
        catch
        {
            ViewBag.Clients = new List<ClientDto>();
        }
    }

    public async Task<IActionResult> Index(int? clientId = null, int page = 1, string? search = null, string? typeContrat = null)
    {
        var result = await _employees.GetAllAsync(clientId, page, search);

        await LoadClientsAsync();
        await LoadReferenceDataToViewBagAsync("TYPE_CONTRAT", "STATUT_EMPLOYE");
        ViewBag.Search = search;
        ViewBag.ClientId = clientId;
        ViewBag.TypeContrat = typeContrat;
        return View(result);
    }

    public async Task<IActionResult> Details(int id)
    {
        var employee = await _employees.GetByIdAsync(id);
        if (employee == null) return NotFound();
        await LoadReferenceDataToViewBagAsync("TYPE_CONTRAT", "STATUT_EMPLOYE", "SITUATION_FAMILIALE", "MODE_PAIEMENT");
        return View(employee);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? clientId = null)
    {
        await LoadClientsAsync();
        await LoadReferenceDataToViewBagAsync(EmpRefCategories);
        var dto = new EmployeeCreateDto();
        if (clientId.HasValue) dto.ClientId = clientId.Value;
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
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
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _employees.DeleteAsync(id);
        SetApiResult(result, "Employé supprimé.", "Erreur lors de la suppression.");
        return RedirectToAction("Index");
    }
}
