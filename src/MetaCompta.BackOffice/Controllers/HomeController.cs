using MetaCompta.BackOffice.Filters;
using MetaCompta.BackOffice.Services;
using MetaCompta.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.BackOffice.Controllers;

public class HomeController : BaseController
{
    private readonly IDashboardFacade _dashboard;

    public HomeController(
        IReferenceDataFacade referenceData,
        IHttpContextAccessor contextAccessor,
        IDashboardFacade dashboard)
        : base(referenceData, contextAccessor)
    {
        _dashboard = dashboard;
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SetExercice(int exercice, string? returnUrl = null)
    {
        if (exercice >= 2015 && exercice <= DateTime.Now.Year + 1)
            HttpContext.Session.SetString("SelectedExercice", exercice.ToString());
        return Redirect(returnUrl ?? "/");
    }

    [SkipAuthentication]
    public IActionResult Error() => View();
}
