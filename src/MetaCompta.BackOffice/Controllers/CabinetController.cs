using MetaCompta.Application.DTOs;
using MetaCompta.BackOffice.Services;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.BackOffice.Controllers;

/// <summary>
/// Controller for Cabinet — Employés du cabinet, Rôles & Permissions, Cabinet Info, Profil.
/// Full CRUD with claims-based access.
/// </summary>
public class CabinetController : BaseController
{
    private readonly ICabinetFacade _cabinet;

    public CabinetController(
        IReferenceDataFacade referenceData,
        IHttpContextAccessor contextAccessor,
        ICabinetFacade cabinet)
        : base(referenceData, contextAccessor)
    {
        _cabinet = cabinet;
    }

    // ═══════════════════════════════════════════════════════════
    // EMPLOYEES
    // ═══════════════════════════════════════════════════════════

    public async Task<IActionResult> Index(string? departement = null, int page = 1, string? search = null)
    {
        await LoadReferenceDataToViewBagAsync("DEPARTEMENT_CABINET");
        var result = await _cabinet.GetEmployeesAsync(departement, page, search);
        ViewBag.Departement = departement;
        ViewBag.Search = search;
        return View(result);
    }

    [HttpGet]
    public async Task<IActionResult> CreateEmployee()
    {
        var claims = await _cabinet.GetClaimsByDomainAsync();
        var roles = await _cabinet.GetRolesAsync(1, null);
        ViewBag.ClaimsByDomain = claims;
        ViewBag.Roles = roles?.Items ?? new List<RoleDto>();
        return View(new CabinetEmployeeCreateDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEmployee(CabinetEmployeeCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            var claims = await _cabinet.GetClaimsByDomainAsync();
            var roles = await _cabinet.GetRolesAsync(1, null);
            ViewBag.ClaimsByDomain = claims;
            ViewBag.Roles = roles?.Items ?? new List<RoleDto>();
            return View(dto);
        }

        var result = await _cabinet.CreateEmployeeAsync(dto);
        if (result?.Success == true)
        {
            TempData["Success"] = $"Employé {dto.FirstName} {dto.LastName} créé avec succès.";
            return RedirectToAction(nameof(Index));
        }

        SetApiError(result, "Erreur lors de la création.");
        if (result?.Errors?.Any() == true)
            foreach (var err in result.Errors) ModelState.AddModelError("", err);

        var claimsReload = await _cabinet.GetClaimsByDomainAsync();
        var rolesReload = await _cabinet.GetRolesAsync(1, null);
        ViewBag.ClaimsByDomain = claimsReload;
        ViewBag.Roles = rolesReload?.Items ?? new List<RoleDto>();
        return View(dto);
    }

    [HttpGet]
    public async Task<IActionResult> EditEmployee(string id)
    {
        var employee = await _cabinet.GetEmployeeByIdAsync(id);
        if (employee == null) return NotFound();

        var claims = await _cabinet.GetClaimsByDomainAsync();
        var roles = await _cabinet.GetRolesAsync(1, null);
        ViewBag.ClaimsByDomain = claims;
        ViewBag.Roles = roles?.Items ?? new List<RoleDto>();
        ViewBag.EmployeeId = id;
        ViewBag.EmployeeEmail = employee.Email;

        var dto = new CabinetEmployeeUpdateDto
        {
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            CIN = employee.CIN,
            Telephone = employee.Telephone,
            DateNaissance = employee.DateNaissance,
            LieuNaissance = employee.LieuNaissance,
            Adresse = employee.Adresse,
            Sexe = employee.Sexe,
            SituationFamiliale = employee.SituationFamiliale,
            Poste = employee.Poste,
            Departement = employee.Departement,
            NumeroCNSS = employee.NumeroCNSS,
            DateEmbauche = employee.DateEmbauche,
            TypeContrat = employee.TypeContrat,
            Role = employee.Role ?? "Assistant",
            IsActive = employee.EstActif,
            Permissions = employee.Permissions ?? new List<string>()
        };
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEmployee(string id, CabinetEmployeeUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            var claims = await _cabinet.GetClaimsByDomainAsync();
            var roles = await _cabinet.GetRolesAsync(1, null);
            ViewBag.ClaimsByDomain = claims;
            ViewBag.Roles = roles?.Items ?? new List<RoleDto>();
            ViewBag.EmployeeId = id;
            return View(dto);
        }

        var result = await _cabinet.UpdateEmployeeAsync(id, dto);
        if (result?.Success == true)
        {
            TempData["Success"] = "Employé modifié avec succès.";
            return RedirectToAction(nameof(Index));
        }

        SetApiError(result, "Erreur lors de la modification.");
        var claimsReload = await _cabinet.GetClaimsByDomainAsync();
        var rolesReload = await _cabinet.GetRolesAsync(1, null);
        ViewBag.ClaimsByDomain = claimsReload;
        ViewBag.Roles = rolesReload?.Items ?? new List<RoleDto>();
        ViewBag.EmployeeId = id;
        return View(dto);
    }

    public async Task<IActionResult> EmployeeDetails(string id)
    {
        var employee = await _cabinet.GetEmployeeByIdAsync(id);
        if (employee == null) return NotFound();
        return View(employee);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleEmployee(string id)
    {
        var result = await _cabinet.ToggleEmployeeActiveAsync(id);
        TempData[result?.Success == true ? "SuccessMessage" : "ErrorMessage"] =
            result?.Message ?? "Opération effectuée.";
        return RedirectToAction(nameof(Index));
    }

    // ═══════════════════════════════════════════════════════════
    // ROLES & PERMISSIONS
    // ═══════════════════════════════════════════════════════════

    public async Task<IActionResult> Roles(int page = 1, string? search = null)
    {
        var result = await _cabinet.GetRolesAsync(page, search);
        var claims = await _cabinet.GetClaimsByDomainAsync();
        ViewBag.Search = search;
        ViewBag.ClaimsByDomain = claims;
        return View(result);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRole(string name, string? description, List<string> permissions)
    {
        var result = await _cabinet.CreateRoleAsync(name, description, permissions ?? new List<string>());
        TempData[result?.Success == true ? "SuccessMessage" : "ErrorMessage"] =
            result?.Success == true ? $"Rôle '{name}' créé avec succès." : (result?.Message ?? "Erreur.");
        return RedirectToAction(nameof(Roles));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateRolePermissions(string roleId, List<string> permissions)
    {
        var result = await _cabinet.UpdateRolePermissionsAsync(roleId, permissions ?? new List<string>());
        TempData[result?.Success == true ? "SuccessMessage" : "ErrorMessage"] =
            result?.Success == true ? "Permissions mises à jour." : (result?.Message ?? "Erreur.");
        return RedirectToAction(nameof(Roles));
    }

    // ═══════════════════════════════════════════════════════════
    // EQUIPES (Teams — grouped employees view)
    // ═══════════════════════════════════════════════════════════

    public async Task<IActionResult> Equipes(int page = 1, string? search = null)
    {
        var result = await _cabinet.GetEmployeesAsync(null, page, search);
        ViewBag.Search = search;
        return View(result);
    }

    // ═══════════════════════════════════════════════════════════
    // CABINET INFO
    // ═══════════════════════════════════════════════════════════

    [HttpGet]
    public async Task<IActionResult> EditCabinet()
    {
        var cabinet = await _cabinet.GetCabinetAsync();
        if (cabinet == null) return NotFound();

        var dto = new CabinetEditDto
        {
            Id = cabinet.Id,
            Name = cabinet.Name,
            ICE = cabinet.ICE,
            IF = cabinet.IF,
            RC = cabinet.RC,
            Address = cabinet.Address,
            City = cabinet.City,
            Phone = cabinet.Phone,
            Email = cabinet.Email,
            LogoUrl = cabinet.LogoUrl
        };
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCabinet(CabinetEditDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var result = await _cabinet.UpdateCabinetAsync(dto);
        if (result?.Success == true)
        {
            TempData["Success"] = "Informations du cabinet mises à jour.";
            return RedirectToAction(nameof(EditCabinet));
        }

        SetApiError(result, "Erreur lors de la mise à jour.");
        return View(dto);
    }

    // ═══════════════════════════════════════════════════════════
    // AUDIT LOGS
    // ═══════════════════════════════════════════════════════════

    public async Task<IActionResult> AuditLogs(string? utilisateur = null, string? action = null, DateTime? dateDebut = null, DateTime? dateFin = null, int page = 1)
    {
        var result = await _cabinet.GetAuditLogsAsync(utilisateur, action, dateDebut, dateFin, page);
        ViewBag.Utilisateur = utilisateur;
        ViewBag.Action = action;
        ViewBag.DateDebut = dateDebut;
        ViewBag.DateFin = dateFin;
        return View(result);
    }

    // ═══════════════════════════════════════════════════════════
    // PROFILE
    // ═══════════════════════════════════════════════════════════

    public async Task<IActionResult> Profile()
    {
        var profile = await _cabinet.GetProfileAsync();
        if (profile == null) return NotFound();
        return View(profile);
    }
}
