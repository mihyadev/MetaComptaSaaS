using MetaCompta.Application.DTOs;
using MetaCompta.BackOffice.Filters;
using MetaCompta.BackOffice.Services;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.BackOffice.Controllers;

/// <summary>
/// Controller for Document management — Pièces justificatives, Contrats, Formulaires.
/// </summary>
[RequireSelectedClient]
public class DocumentsController : BaseController
{
    private readonly IDocumentFacade _documents;
    private readonly IClientFacade _clients;

    public DocumentsController(
        IReferenceDataFacade referenceData,
        IHttpContextAccessor contextAccessor,
        IDocumentFacade documents,
        IClientFacade clients)
        : base(referenceData, contextAccessor)
    {
        _documents = documents;
        _clients = clients;
    }

    public async Task<IActionResult> Index(int? clientId = null, string? categorie = null, int page = 1, string? search = null)
    {
        clientId ??= SelectedClientId;
        var result = await _documents.GetAllAsync(clientId, categorie, page, search);
        ViewBag.Client = SelectedClientId.HasValue ? await _clients.GetByIdAsync(SelectedClientId.Value) : null;
        ViewBag.ClientId = clientId;
        ViewBag.Categorie = categorie;
        ViewBag.Search = search;
        await LoadReferenceDataToViewBagAsync("CATEGORIE_DOCUMENT", "STATUT_DOCUMENT");
        return View(result);
    }

    public async Task<IActionResult> Details(int id)
    {
        var doc = await _documents.GetByIdAsync(id);
        if (doc == null) return NotFound();
        return View(doc);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id, string newStatut, string? motif = null)
    {
        var result = await _documents.ChangeStatusAsync(id, newStatut, motif);
        SetApiResult(result, null, "Erreur lors du changement de statut.");
        return RedirectToAction("Details", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleShare(int id)
    {
        var result = await _documents.ToggleShareAsync(id);
        SetApiResult(result, null, "Erreur lors du partage.");
        return RedirectToAction("Details", new { id });
    }
}
