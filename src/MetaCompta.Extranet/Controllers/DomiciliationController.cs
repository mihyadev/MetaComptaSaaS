using MetaCompta.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.Extranet.Controllers;

[Route("Domiciliation")]
public class DomiciliationController : ExtranetBaseController
{
    public DomiciliationController(IHttpClientFactory http) : base(http) { }

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index()
    {
        await EnsurePortalContextAsync();
        var courriers = await GetAsync<ApiResponse<List<CourrierDomiciliationDto>>>("api/client-domiciliation/courriers");
        var docs = await GetAsync<ApiResponse<List<DemandeDocumentDomiciliationDto>>>("api/client-domiciliation/document-requests");
        ViewBag.Courriers = courriers?.Data ?? new List<CourrierDomiciliationDto>();
        ViewBag.Documents = docs?.Data ?? new List<DemandeDocumentDomiciliationDto>();
        return View();
    }

    [HttpGet("Courrier/{id:int}")]
    public async Task<IActionResult> Courrier(int id)
    {
        await EnsurePortalContextAsync();
        var courriers = await GetAsync<ApiResponse<List<CourrierDomiciliationDto>>>("api/client-domiciliation/courriers");
        var item = courriers?.Data?.FirstOrDefault(c => c.Id == id);
        if (item == null)
        {
            TempData["Error"] = "Correspondance introuvable ou non notifiée.";
            return RedirectToAction(nameof(Index));
        }
        return View(item);
    }

    [HttpPost("Courrier/{id:int}/Action")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestAction(int id, DemandeActionCreateDto dto)
    {
        var result = await PostAsync<DemandeActionCreateDto, ApiResponse<DemandeActionCourrierDto>>(
            $"api/client-domiciliation/courriers/{id}/actions", dto);
        if (result?.Success == true)
            TempData["Success"] = result.Message ?? "Demande envoyée.";
        else
            TempData["Error"] = result?.Message ?? "Impossible d'envoyer la demande.";
        return RedirectToAction(nameof(Courrier), new { id });
    }

    [HttpGet("Documents/{id:int}")]
    public async Task<IActionResult> DocumentRequest(int id)
    {
        await EnsurePortalContextAsync();
        var docs = await GetAsync<ApiResponse<List<DemandeDocumentDomiciliationDto>>>("api/client-domiciliation/document-requests");
        var item = docs?.Data?.FirstOrDefault(d => d.Id == id);
        if (item == null)
        {
            TempData["Error"] = "Demande introuvable.";
            return RedirectToAction(nameof(Index));
        }
        return View(item);
    }

    [HttpPost("Documents/{id:int}/Attach")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AttachDocument(int id, int documentId)
    {
        var result = await PostAsync<object, ApiResponse<DemandeDocumentDomiciliationDto>>(
            $"api/client-domiciliation/document-requests/{id}/attach/{documentId}", new { });
        if (result?.Success == true)
            TempData["Success"] = result.Message ?? "Document envoyé.";
        else
            TempData["Error"] = result?.Message ?? "Envoi impossible.";
        return RedirectToAction(nameof(DocumentRequest), new { id });
    }
}
