using MetaCompta.Application.DTOs;
using MetaCompta.BackOffice.Filters;
using MetaCompta.BackOffice.Helpers;
using MetaCompta.BackOffice.Services;
using MetaCompta.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.BackOffice.Controllers;

public class HomeController : BaseController
{
    /// <summary>
    /// BackOffice mini-apps (métier spaces). Declarations = suivi of ALL déclarations (fiscal + social).
    /// Compta / Paie / Juridique = operational management apps (not declaration suivi).
    /// </summary>
    public static readonly string[] DomainSpaces = { "Declarations", "Compta", "Paie", "Juridique" };

    private readonly IDashboardFacade _dashboard;
    private readonly IComptabiliteFacade _compta;
    private readonly IConfiguration _configuration;

    public HomeController(
        IReferenceDataFacade referenceData,
        IHttpContextAccessor contextAccessor,
        IDashboardFacade dashboard,
        IComptabiliteFacade compta,
        IConfiguration configuration)
        : base(referenceData, contextAccessor)
    {
        _dashboard = dashboard;
        _compta = compta;
        _configuration = configuration;
    }

    public IActionResult Workspace(string? space = null)
    {
        // Entry gateway: no left menu until Cabinet or a métier space is chosen
        HttpContext.Session.Remove("WorkspaceMode");
        var pending = NormalizeDomain(space)
            ?? NormalizeDomain(HttpContext.Session.GetString("PendingWorkspaceDomain"));
        if (pending == null)
        {
            // Fresh gateway (ex. « Changer d'espace ») — reset métier focus
            HttpContext.Session.Remove("PendingWorkspaceDomain");
            HttpContext.Session.Remove("WorkspaceDomain");
        }
        else
        {
            HttpContext.Session.SetString("PendingWorkspaceDomain", pending);
            HttpContext.Session.SetString("WorkspaceDomain", pending);
        }

        ViewBag.UserName = UserName;
        ViewBag.UserRole = UserRole;
        ViewBag.CabinetName = CabinetName;
        ViewBag.HasClient = SelectedClientId.HasValue;
        ViewBag.SelectedClientName = SelectedClientName;
        ViewBag.PendingDomain = pending;
        ViewBag.CanDeclarationsSpace = PermissionHelper.HasAnyDomain(HttpContext.Session, "tva", "ir", "is", "cnss");
        ViewBag.CanComptaSpace = PermissionHelper.HasAnyDomain(HttpContext.Session, "comptabilite");
        ViewBag.CanPaieSpace = PermissionHelper.HasAnyDomain(HttpContext.Session, "payroll", "employee");
        ViewBag.CanJuridiqueSpace = PermissionHelper.HasAnyDomain(HttpContext.Session, "juridique");
        return View();
    }

    /// <summary>Enter cabinet administration space (shows cabinet sidebar menu).</summary>
    public IActionResult EnterCabinet()
    {
        HttpContext.Session.Remove("PendingWorkspaceDomain");
        HttpContext.Session.Remove("WorkspaceDomain");
        HttpContext.Session.SetString("WorkspaceMode", "Cabinet");
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Enter a métier mini-app (Declarations / Compta / Paie / Juridique).
    /// If a client is already selected, open the landing page; otherwise return to gateway to pick a client.
    /// </summary>
    public IActionResult EnterDomain(string domain)
    {
        var normalized = NormalizeDomain(domain);
        if (normalized == null)
            return RedirectToAction(nameof(Workspace));

        HttpContext.Session.SetString("PendingWorkspaceDomain", normalized);
        HttpContext.Session.SetString("WorkspaceDomain", normalized);
        HttpContext.Session.Remove("WorkspaceMode");

        if (SelectedClientId.HasValue)
        {
            HttpContext.Session.SetString("WorkspaceMode", "Client");
            HttpContext.Session.Remove("PendingWorkspaceDomain");
            return Redirect(DomainLandingUrl(normalized));
        }

        return RedirectToAction(nameof(Workspace), new { space = normalized });
    }

    public static string? NormalizeDomain(string? domain)
    {
        if (string.IsNullOrWhiteSpace(domain)) return null;
        var raw = domain.Trim();
        // Legacy alias: Fiscal → Suivi Déclarations (all déclarations)
        if (string.Equals(raw, "Fiscal", StringComparison.OrdinalIgnoreCase))
            return "Declarations";
        return DomainSpaces.FirstOrDefault(d =>
            string.Equals(d, raw, StringComparison.OrdinalIgnoreCase));
    }

    public static string DomainLandingUrl(string domain) => domain switch
    {
        "Compta" => "/Comptabilite",
        "Paie" => "/Payroll",
        "Juridique" => "/Juridique",
        _ => "/Declarations" // Declarations: suivi fiscal + social
    };

    public static string DomainThemeClass(string? domain) => domain switch
    {
        "Declarations" => "app-declarations",
        "Compta" => "app-compta",
        "Paie" => "app-paie",
        "Juridique" => "app-juridique",
        _ => "app-cabinet"
    };

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
