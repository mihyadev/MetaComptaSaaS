using MetaCompta.Application.DTOs;
using MetaCompta.BackOffice.Services;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.BackOffice.Controllers;

/// <summary>
/// BackOffice MVC controller for Department management.
/// CRUD + member management.
/// </summary>
public class DepartmentsController : BaseController
{
    private readonly IDepartmentFacade _departments;
    private readonly ICabinetFacade _cabinet;

    public DepartmentsController(
        IReferenceDataFacade referenceData,
        IHttpContextAccessor contextAccessor,
        IDepartmentFacade departments,
        ICabinetFacade cabinet)
        : base(referenceData, contextAccessor)
    {
        _departments = departments;
        _cabinet = cabinet;
    }

    // ═══════════════════════════════════════════════════════════
    // LIST
    // ═══════════════════════════════════════════════════════════

    public async Task<IActionResult> Index()
    {
        try
        {
            var departments = await _departments.GetAllAsync();
            return View(departments);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Erreur: {ex.Message}";
            return View(new List<DepartmentDto>());
        }
    }

    // ═══════════════════════════════════════════════════════════
    // DETAILS
    // ═══════════════════════════════════════════════════════════

    public async Task<IActionResult> Details(int id)
    {
        var dept = await _departments.GetByIdAsync(id);
        if (dept == null)
        {
            TempData["Error"] = "Département introuvable.";
            return RedirectToAction("Index");
        }
        var members = await _departments.GetMembersAsync(id);
        ViewBag.Members = members;
        return View(dept);
    }

    // ═══════════════════════════════════════════════════════════
    // CREATE
    // ═══════════════════════════════════════════════════════════

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadEmployeeDropdown();
        return View(new DepartmentCreateDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DepartmentCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadEmployeeDropdown();
            return View(dto);
        }
        var result = await _departments.CreateAsync(dto);
        if (result?.Success == true)
        {
            TempData["Success"] = result.Message ?? "Département créé avec succès.";
            return RedirectToAction("Index");
        }
        TempData["Error"] = result?.Message ?? "Erreur lors de la création.";
        if (result?.Errors?.Count > 0)
            TempData["ErrorDetails"] = string.Join("|||", result.Errors);
        await LoadEmployeeDropdown();
        return View(dto);
    }

    // ═══════════════════════════════════════════════════════════
    // EDIT
    // ═══════════════════════════════════════════════════════════

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var dept = await _departments.GetByIdAsync(id);
        if (dept == null) return NotFound();

        var dto = new DepartmentCreateDto
        {
            Name = dept.Name,
            Code = dept.Code,
            Description = dept.Description,
            HeadOfDepartmentUserId = dept.HeadOfDepartmentUserId
        };
        ViewBag.DepartmentId = id;
        await LoadEmployeeDropdown();
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DepartmentCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.DepartmentId = id;
            await LoadEmployeeDropdown();
            return View(dto);
        }
        var result = await _departments.UpdateAsync(id, dto);
        if (result?.Success == true)
        {
            TempData["Success"] = result.Message ?? "Département mis à jour.";
            return RedirectToAction("Details", new { id });
        }
        TempData["Error"] = result?.Message ?? "Erreur lors de la mise à jour.";
        if (result?.Errors?.Count > 0)
            TempData["ErrorDetails"] = string.Join("|||", result.Errors);
        ViewBag.DepartmentId = id;
        await LoadEmployeeDropdown();
        return View(dto);
    }

    // ═══════════════════════════════════════════════════════════
    // DELETE
    // ═══════════════════════════════════════════════════════════

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _departments.DeleteAsync(id);
        SetApiResult(result, "Département supprimé.", "Erreur lors de la suppression.");
        return RedirectToAction("Index");
    }

    // ═══════════════════════════════════════════════════════════
    // MEMBERS
    // ═══════════════════════════════════════════════════════════

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMember(int departmentId, string userId, bool isLead = false)
    {
        var result = await _departments.AddMemberAsync(departmentId, userId, isLead);
        SetApiResult(result, "Membre ajouté.", "Erreur lors de l'ajout du membre.");
        return RedirectToAction("Details", new { id = departmentId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveMember(int departmentId, string userId)
    {
        var result = await _departments.RemoveMemberAsync(departmentId, userId);
        SetApiResult(result, "Membre retiré.", "Erreur lors du retrait du membre.");
        return RedirectToAction("Details", new { id = departmentId });
    }

    // ── Helpers ──────────────────────────────────────────────

    private async Task LoadEmployeeDropdown()
    {
        try
        {
            var employees = await _cabinet.GetEmployeesAsync(page: 1, search: null);
            ViewBag.Employees = employees?.Items ?? new List<CabinetEmployeeDto>();
        }
        catch
        {
            ViewBag.Employees = new List<CabinetEmployeeDto>();
        }
    }
}
