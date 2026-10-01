using MetaCompta.Application.DTOs;
using MetaCompta.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.Extranet.Controllers;

[Route("Dossiers")]
public class DossiersController : ExtranetBaseController
{
    public DossiersController(IHttpClientFactory http) : base(http) { }

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index()
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Account");

        await EnsurePortalContextAsync();
        var dossiers = ViewBag.PortalDossiers as List<ExtranetDossierSummaryDto>
                       ?? new List<ExtranetDossierSummaryDto>();
        ViewBag.ExerciceAnnee = SelectedExercice;
        ViewBag.ExerciceStatut = SelectedExerciceStatut ?? "Ouvert";
        return View(dossiers);
    }

    [HttpGet("{type}")]
    public async Task<IActionResult> Type(string type)
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Account");
        if (string.Equals(type, "SwitchExercice", StringComparison.OrdinalIgnoreCase)
            || string.Equals(type, "Index", StringComparison.OrdinalIgnoreCase))
            return RedirectToAction(nameof(Index));

        var normalized = DeclarationServiceCatalog.NormalizeType(type);
        var entry = DeclarationServiceCatalog.Get(normalized);

        var response = await GetAsync<ApiResponse<PagedResult<DeclarationDto>>>(
            BuildDeclarationsUrl(normalized, pageSize: 200));
        var declarations = response?.Data?.Items ?? new List<DeclarationDto>();

        ViewBag.DossierType = normalized;
        ViewBag.DossierLabel = entry?.DisplayName ?? normalized;
        ViewBag.DossierFrequency = entry?.Frequency ?? "";
        ViewBag.DossierIcon = DossierIcon(normalized);
        ViewBag.DossierAccent = DossierAccent(normalized);
        ViewBag.ExerciceAnnee = SelectedExercice;
        ViewBag.ExerciceStatut = SelectedExerciceStatut ?? "Ouvert";

        return View("Type", declarations);
    }

    [HttpPost("SwitchExercice")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SwitchExercice(int exerciceId, string? returnUrl = null)
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Account");

        var resp = await GetAsync<ApiResponse<List<ExerciceComptableDto>>>("api/client-exercices");
        var exo = resp?.Data?.FirstOrDefault(e => e.Id == exerciceId);
        if (exo != null)
            SetExerciceSession(exo);

        await EnsurePortalContextAsync();

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }
}
