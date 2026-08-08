using MetaCompta.Application.DTOs;
using MetaCompta.BackOffice.Filters;
using MetaCompta.BackOffice.Services;
using MetaCompta.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.BackOffice.Controllers;

public class HomeController : BaseController
{
    private readonly IDashboardFacade _dashboard;
    private readonly IComptabiliteFacade _compta;

    public HomeController(
        IReferenceDataFacade referenceData,
        IHttpContextAccessor contextAccessor,
        IDashboardFacade dashboard,
        IComptabiliteFacade compta)
        : base(referenceData, contextAccessor)
    {
        _dashboard = dashboard;
        _compta = compta;
    }

    public IActionResult Workspace()
    {
        // Entry gateway: no left menu until Cabinet or Dossier Client is chosen
        HttpContext.Session.Remove("WorkspaceMode");
        ViewBag.UserName = UserName;
        ViewBag.UserRole = UserRole;
        ViewBag.CabinetName = CabinetName;
        ViewBag.HasClient = SelectedClientId.HasValue;
        ViewBag.SelectedClientName = SelectedClientName;
        return View();
    }

    /// <summary>Enter cabinet administration space (shows cabinet sidebar menu).</summary>
    public IActionResult EnterCabinet()
    {
        HttpContext.Session.SetString("WorkspaceMode", "Cabinet");
        return RedirectToAction(nameof(Index));
    }

    [RequirePermission(ClaimDefinitions.CabinetDetail)]
    public async Task<IActionResult> Index()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("WorkspaceMode")))
            HttpContext.Session.SetString("WorkspaceMode", "Cabinet");

        ViewBag.UserName = UserName;
        ViewBag.UserRole = UserRole;
        ViewBag.CabinetName = CabinetName;

        var data = await _dashboard.GetManagerDashboardAsync();
        return View(data);
    }

    /// <summary>
    /// Sets the global selected exercice in session (AJAX or form).
    /// Accepts exerciceId (preferred) and/or année.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SetExercice(int? exerciceId = null, int? exercice = null, string? returnUrl = null)
    {
        if (!SelectedClientId.HasValue)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.ContentType?.Contains("json") == true)
                return Json(new { success = false, message = "Aucun client sélectionné." });
            return Redirect(returnUrl ?? "/");
        }

        ExerciceComptableDto? exo = null;
        if (exerciceId is > 0)
            exo = await _compta.GetExerciceByIdAsync(exerciceId.Value);

        if (exo == null)
        {
            var list = await _compta.GetExercicesAsync(SelectedClientId.Value);
            if (exercice is > 0)
                exo = list.FirstOrDefault(e => e.Annee == exercice);
            else if (exerciceId is > 0)
                exo = list.FirstOrDefault(e => e.Id == exerciceId);
        }

        if (exo == null || exo.ClientId != SelectedClientId.Value)
        {
            if (IsAjaxRequest())
                return Json(new { success = false, message = "Exercice introuvable." });
            TempData["Error"] = "Exercice introuvable.";
            return Redirect(returnUrl ?? "/");
        }

        SetExerciceSession(exo);

        if (IsAjaxRequest())
            return Json(new
            {
                success = true,
                exerciceId = exo.Id,
                annee = exo.Annee,
                statut = exo.Statut,
                label = exo.Label
            });

        return Redirect(returnUrl ?? Url.Action("Index", "Comptabilite") ?? "/");
    }

    /// <summary>AJAX — create a new exercice for the selected client and select it.</summary>
    [HttpPost]
    [RequirePermission(ClaimDefinitions.ComptabiliteCreate)]
    public async Task<IActionResult> CreateExercice([FromBody] ExerciceCreateDto? dto)
    {
        if (!SelectedClientId.HasValue)
            return Json(new { success = false, message = "Aucun client sélectionné." });

        dto ??= new ExerciceCreateDto();
        dto.ClientId = SelectedClientId.Value;
        if (dto.Annee <= 0) dto.Annee = DateTime.Now.Year + 1;

        var result = await _compta.CreateExerciceAsync(dto);
        if (result?.Success != true || result.Data == null)
            return Json(new { success = false, message = result?.Message ?? "Erreur lors de la création." });

        SetExerciceSession(result.Data);
        return Json(new
        {
            success = true,
            message = result.Message,
            exerciceId = result.Data.Id,
            annee = result.Data.Annee,
            statut = result.Data.Statut,
            label = result.Data.Label
        });
    }

    /// <summary>AJAX — list exercices for the selected client (topbar dropdown).</summary>
    [HttpGet]
    public async Task<IActionResult> ListExercices()
    {
        if (!SelectedClientId.HasValue)
            return Json(Array.Empty<object>());

        await _compta.EnsureLedgerAsync(SelectedClientId.Value);
        var list = await _compta.GetExercicesAsync(SelectedClientId.Value);
        return Json(list.Select(e => new
        {
            e.Id,
            e.Annee,
            e.Statut,
            dateDebut = e.DateDebut.ToString("yyyy-MM-dd"),
            dateFin = e.DateFin.ToString("yyyy-MM-dd"),
            label = e.Label,
            selected = SelectedExerciceId == e.Id
        }));
    }

    private bool IsAjaxRequest()
        => string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase)
           || Request.Headers.Accept.ToString().Contains("application/json", StringComparison.OrdinalIgnoreCase);

    [SkipAuthentication]
    public IActionResult Error() => View();
}
