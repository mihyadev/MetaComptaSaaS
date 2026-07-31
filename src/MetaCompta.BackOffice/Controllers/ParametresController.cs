using MetaCompta.BackOffice.Filters;
using MetaCompta.BackOffice.Services;
using MetaCompta.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.BackOffice.Controllers;

/// <summary>
/// Controller for Paramètres — Configuration générale, Templates, Notifications, Intégrations, Périodes comptables.
/// </summary>
public class ParametresController : BaseController
{
    private readonly IParametresFacade _parametres;

    public ParametresController(
        IReferenceDataFacade referenceData,
        IHttpContextAccessor contextAccessor,
        IParametresFacade parametres)
        : base(referenceData, contextAccessor)
    {
        _parametres = parametres;
    }

    [RequirePermission(ClaimDefinitions.SystemAdmin)]
    public async Task<IActionResult> Index()
    {
        var settings = await _parametres.GetAllAsync();
        return View(settings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.SystemAdmin)]
    public async Task<IActionResult> UpdateSetting(string cle, string valeur)
    {
        var result = await _parametres.UpdateSettingAsync(cle, valeur);
        if (result?.Success == true)
            TempData["Success"] = result.Message;
        else
            SetApiError(result, "Erreur lors de la mise à jour du paramètre.");
        return RedirectToAction("Index");
    }

    [RequirePermission(ClaimDefinitions.SystemAdmin)]
    public async Task<IActionResult> Templates()
    {
        var templates = await _parametres.GetTemplatesAsync();
        return View(templates);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.SystemAdmin)]
    public async Task<IActionResult> UpdateTemplate(int id, string contenu)
    {
        var result = await _parametres.UpdateTemplateAsync(id, contenu);
        if (result?.Success == true)
            TempData["Success"] = result.Message;
        else
            SetApiError(result, "Erreur lors de la mise à jour du template.");
        return RedirectToAction("Templates");
    }

    [RequirePermission(ClaimDefinitions.SystemAdmin)]
    public IActionResult Notifications()
    {
        return View();
    }

    [RequirePermission(ClaimDefinitions.SystemAdmin)]
    public IActionResult Integrations()
    {
        return View();
    }

    [RequirePermission(ClaimDefinitions.SystemAdmin)]
    public IActionResult Periodes()
    {
        return View();
    }
}
