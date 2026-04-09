using MetaCompta.Application.DTOs;
using MetaCompta.BackOffice.Filters;
using MetaCompta.BackOffice.Services;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.BackOffice.Controllers;

[RequireSelectedClient]
public class PayrollController : BaseController
{
    private readonly IPayrollFacade _payroll;
    private readonly IClientFacade _clients;

    public PayrollController(
        IReferenceDataFacade referenceData,
        IHttpContextAccessor contextAccessor,
        IPayrollFacade payroll,
        IClientFacade clients)
        : base(referenceData, contextAccessor)
    {
        _payroll = payroll;
        _clients = clients;
    }

    public async Task<IActionResult> Index(int? clientId = null)
    {
        clientId ??= SelectedClientId;
        var periods = await _payroll.GetAllPeriodsAsync(clientId);

        ViewBag.Client = SelectedClientId.HasValue ? await _clients.GetByIdAsync(SelectedClientId.Value) : null;
        ViewBag.ClientId = clientId;
        await LoadReferenceDataToViewBagAsync("STATUT_PAIE");
        return View(periods);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
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

    public async Task<IActionResult> Period(int id)
    {
        var period = await _payroll.GetPeriodAsync(id);
        ViewBag.PaySlips = await _payroll.GetPaySlipsAsync(id);
        await LoadReferenceDataToViewBagAsync("STATUT_PAIE");
        return View(period);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Calculate(int id)
    {
        var result = await _payroll.CalculateAllAsync(id);
        SetApiResult(result, null, "Erreur lors du calcul.");
        return RedirectToAction("Period", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Validate(int id)
    {
        var result = await _payroll.ValidateAsync(id);
        SetApiResult(result, null, "Erreur lors de la validation.");
        return RedirectToAction("Period", new { id });
    }
}
