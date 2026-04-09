using MetaCompta.BackOffice.Filters;
using MetaCompta.BackOffice.Services;
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
        ViewBag.UserName = UserName;
        ViewBag.UserRole = UserRole;
        ViewBag.CabinetName = CabinetName;
        ViewBag.HasClient = SelectedClientId.HasValue;
        ViewBag.SelectedClientName = SelectedClientName;
        return View();
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.UserName = UserName;
        ViewBag.UserRole = UserRole;
        ViewBag.CabinetName = CabinetName;

        var data = await _dashboard.GetManagerDashboardAsync();
        return View(data);
    }

    [SkipAuthentication]
    public IActionResult Error() => View();
}
