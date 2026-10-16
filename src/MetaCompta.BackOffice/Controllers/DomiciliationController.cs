using MetaCompta.Application.DTOs;
using MetaCompta.BackOffice.Filters;
using MetaCompta.BackOffice.Helpers;
using MetaCompta.BackOffice.Services;
using MetaCompta.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.BackOffice.Controllers;

public class DomiciliationController : BaseController
{
    private readonly IDomiciliationFacade _domiciliation;
    private readonly IClientFacade _clients;
    private readonly IDocumentFacade _documents;
    private readonly ICabinetFacade _cabinet;
    private readonly IApiClient _api;

    public DomiciliationController(
        IReferenceDataFacade referenceData,
        IHttpContextAccessor contextAccessor,
        IDomiciliationFacade domiciliation,
        IClientFacade clients,
        IDocumentFacade documents,
        ICabinetFacade cabinet,
        IApiClient api)
        : base(referenceData, contextAccessor)
    {
        _domiciliation = domiciliation;
        _clients = clients;
        _documents = documents;
        _cabinet = cabinet;
        _api = api;
    }

    [RequirePermission(ClaimDefinitions.DomiciliationView)]
    public async Task<IActionResult> Index()
    {
        // When a dossier client is selected, stay scoped to that company only.
        if (SelectedClientId is int selected && selected > 0)
        {
            var ws = await _domiciliation.GetClientWorkspaceAsync(selected);
            if (ws != null)
                return RedirectToAction(nameof(Client), new { id = selected });

            TempData["Error"] = "Le client sélectionné n'a pas de service de domiciliation actif.";
            return View(new List<DomiciliationDashboardRowDto>());
        }

        var rows = await _domiciliation.GetDashboardAsync() ?? new List<DomiciliationDashboardRowDto>();
        return View(rows);
    }

    [RequirePermission(ClaimDefinitions.DomiciliationView)]
    public async Task<IActionResult> Client(int id)
    {
        var ws = await _domiciliation.GetClientWorkspaceAsync(id);
        if (ws == null)
        {
            TempData["Error"] = "Espace domiciliation introuvable pour ce client.";
            return RedirectToAction(nameof(Index));
        }
        return View(ws);
    }

    [HttpGet]
    [RequirePermission(ClaimDefinitions.DomiciliationReceive)]
    public async Task<IActionResult> Receive(int? clientId = null)
    {
        var effectiveClientId = ResolveClientId(clientId);
        await PrepareClientPickerAsync(effectiveClientId);
        ViewBag.Staff = (await _cabinet.GetEmployeesAsync(pageSize: 200))?.Items ?? new();
        return View(new CourrierReceiveDto
        {
            ClientId = effectiveClientId,
            DateReception = DateTime.Today
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.DomiciliationReceive)]
    public async Task<IActionResult> Receive(CourrierReceiveDto dto, List<IFormFile>? scanFiles)
    {
        dto.ClientId = ResolveClientId(dto.ClientId > 0 ? dto.ClientId : null);
        var files = (scanFiles ?? new()).Where(f => f is { Length: > 0 }).ToList();

        var result = await _domiciliation.ReceiveAsync(dto);
        if (result?.Success != true || result.Data == null)
        {
            SetApiError(result, "Impossible d'enregistrer le courrier.");
            await PrepareClientPickerAsync(dto.ClientId);
            ViewBag.Staff = (await _cabinet.GetEmployeesAsync(pageSize: 200))?.Items ?? new();
            return View(dto);
        }

        var courrierId = result.Data.Id;
        var added = 0;
        foreach (var file in files)
        {
            var uploaded = await UploadScanForClientAsync(dto.ClientId, file, dto.Expediteur ?? dto.TypeCourrier);
            if (uploaded == null) continue;
            var attach = await _domiciliation.AttachScanAsync(courrierId, uploaded.Id);
            if (attach?.Success == true) added++;
        }

        TempData["Success"] = added > 0
            ? $"Courrier enregistré — {added} document(s) ajouté(s) à la photothèque."
            : (result.Message ?? "Courrier enregistré.");
        return RedirectToAction(nameof(Courrier), new { id = courrierId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.DomiciliationReceive, ClaimDefinitions.DomiciliationProcess)]
    public async Task<IActionResult> AttachScan(int id, List<IFormFile>? scanFiles)
    {
        var files = (scanFiles ?? new()).Where(f => f is { Length: > 0 }).ToList();
        if (files.Count == 0)
        {
            TempData["Error"] = "Veuillez sélectionner au moins un fichier (PDF ou image).";
            return RedirectToAction(nameof(Courrier), new { id });
        }

        var courrier = await _domiciliation.GetCourrierAsync(id);
        if (courrier == null) return NotFound();

        var added = 0;
        string? lastError = null;
        foreach (var file in files)
        {
            var uploaded = await UploadScanForClientAsync(courrier.ClientId, file, courrier.Expediteur ?? courrier.TypeCourrier);
            if (uploaded == null) { lastError = TempData["Error"] as string; continue; }
            var result = await _domiciliation.AttachScanAsync(id, uploaded.Id);
            if (result?.Success == true) added++;
            else lastError = result?.Message;
        }

        if (added > 0)
            TempData["Success"] = $"{added} document(s) ajouté(s) à la photothèque.";
        else
            TempData["Error"] = lastError ?? "Aucun document n'a pu être ajouté.";
        return RedirectToAction(nameof(Courrier), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.DomiciliationReceive, ClaimDefinitions.DomiciliationProcess)]
    public async Task<IActionResult> RemovePiece(int id, int pieceId)
    {
        var result = await _domiciliation.RemovePieceAsync(id, pieceId);
        if (result?.Success == true)
            TempData["Success"] = result.Message ?? "Document retiré.";
        else
            SetApiError(result, "Suppression impossible.");
        return RedirectToAction(nameof(Courrier), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.DomiciliationReceive, ClaimDefinitions.DomiciliationProcess)]
    public async Task<IActionResult> SetPrimaryPiece(int id, int pieceId)
    {
        var result = await _domiciliation.SetPrimaryPieceAsync(id, pieceId);
        if (result?.Success == true)
            TempData["Success"] = result.Message ?? "Document principal défini.";
        else
            SetApiError(result, "Action impossible.");
        return RedirectToAction(nameof(Courrier), new { id });
    }

    [HttpGet]
    [RequirePermission(ClaimDefinitions.DomiciliationView)]
    public async Task<IActionResult> PreviewDocument(int documentId)
    {
        var bytes = await _api.GetBytesAsync($"api/documents/{documentId}/download");
        if (bytes == null || bytes.Length == 0) return NotFound();

        var doc = await _documents.GetByIdAsync(documentId);
        var contentType = doc?.ContentType ?? "application/octet-stream";
        return File(bytes, contentType);
    }

    private async Task<DocumentDto?> UploadScanForClientAsync(int clientId, IFormFile scanFile, string? label)
    {
        await using var stream = scanFile.OpenReadStream();
        var titre = string.IsNullOrWhiteSpace(label)
            ? $"Scan courrier — {scanFile.FileName}"
            : $"Scan — {label} — {scanFile.FileName}";
        var upload = await _documents.UploadAsync(
            stream, scanFile.FileName, scanFile.ContentType ?? "application/octet-stream",
            scanFile.Length, titre, "Domiciliation", clientId);
        if (upload?.Success != true || upload.Data == null)
        {
            TempData["Error"] = upload?.Message ?? "Échec de l'upload du scan.";
            return null;
        }
        return upload.Data;
    }

    [RequirePermission(ClaimDefinitions.DomiciliationView)]
    public async Task<IActionResult> Courrier(int id)
    {
        var c = await _domiciliation.GetCourrierAsync(id);
        if (c == null) return NotFound();

        ViewBag.Staff = (await _cabinet.GetEmployeesAsync(pageSize: 200))?.Items ?? new();
        ViewBag.ActionBarUserId = UserId ?? "";
        // Explicit bools so Razor action bar never loses buttons via dynamic cast
        ViewBag.CanDomiciliationProcess = PermissionHelper.HasPermission(HttpContext.Session, ClaimDefinitions.DomiciliationProcess);
        ViewBag.CanDomiciliationNotify = PermissionHelper.HasPermission(HttpContext.Session, ClaimDefinitions.DomiciliationNotify);
        ViewBag.CanDomiciliationView = PermissionHelper.HasPermission(HttpContext.Session, ClaimDefinitions.DomiciliationView);
        var timeline = await _domiciliation.GetTimelineAsync(c.ClientId, 80);
        ViewBag.CourrierTimeline = timeline
            .Where(t => t.RelatedType == "Courrier" && t.RelatedId == id
                || t.RelatedType == "DemandeAction" && c.ActionsClient.Any(a => a.Id == t.RelatedId))
            .OrderByDescending(t => t.At)
            .Take(40)
            .ToList();
        return View(c);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.DomiciliationProcess, ClaimDefinitions.DomiciliationNotify)]
    public async Task<IActionResult> AdvanceCourrier(int id, CourrierStatusActionDto dto)
    {
        // Notify can be done with domiciliation.notify alone; other actions need process.
        var action = (dto.Action ?? "").Trim().ToLowerInvariant();
        if (action == "notify")
        {
            var canNotify = PermissionHelper.HasPermission(HttpContext.Session, ClaimDefinitions.DomiciliationNotify)
                || PermissionHelper.HasPermission(HttpContext.Session, ClaimDefinitions.DomiciliationProcess);
            if (!canNotify)
            {
                TempData["Error"] = "Droit insuffisant pour notifier le client.";
                return RedirectToAction(nameof(Courrier), new { id });
            }
        }
        else if (!PermissionHelper.HasPermission(HttpContext.Session, ClaimDefinitions.DomiciliationProcess))
        {
            TempData["Error"] = "Droit insuffisant pour traiter ce courrier.";
            return RedirectToAction(nameof(Courrier), new { id });
        }

        var result = await _domiciliation.AdvanceCourrierAsync(id, dto);
        if (result?.Success == true)
            TempData["Success"] = result.Message ?? "Statut mis à jour.";
        else
            SetApiError(result, "Action impossible.");
        return RedirectToAction(nameof(Courrier), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.DomiciliationProcess)]
    public async Task<IActionResult> AdvanceAction(int actionId, int courrierId, DemandeActionAdvanceDto dto)
    {
        var result = await _domiciliation.AdvanceActionAsync(actionId, dto);
        if (result?.Success == true)
            TempData["Success"] = result.Message ?? "Demande mise à jour.";
        else
            SetApiError(result, "Action impossible.");
        return RedirectToAction(nameof(Courrier), new { id = courrierId });
    }

    [HttpGet]
    [RequirePermission(ClaimDefinitions.DomiciliationRequest)]
    public async Task<IActionResult> RequestDocument(int? clientId = null)
    {
        var effectiveClientId = ResolveClientId(clientId);
        await PrepareClientPickerAsync(effectiveClientId);
        ViewBag.Staff = (await _cabinet.GetEmployeesAsync(pageSize: 200))?.Items ?? new();
        return View(new DemandeDocumentCreateDto
        {
            ClientId = effectiveClientId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.DomiciliationRequest)]
    public async Task<IActionResult> RequestDocument(DemandeDocumentCreateDto dto)
    {
        dto.ClientId = ResolveClientId(dto.ClientId > 0 ? dto.ClientId : null);

        var result = await _domiciliation.CreateDocumentRequestAsync(dto);
        if (result?.Success == true)
        {
            TempData["Success"] = result.Message ?? "Demande créée.";
            return RedirectToAction(nameof(Client), new { id = dto.ClientId });
        }
        SetApiError(result, "Impossible de créer la demande.");
        await PrepareClientPickerAsync(dto.ClientId);
        ViewBag.Staff = (await _cabinet.GetEmployeesAsync(pageSize: 200))?.Items ?? new();
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(ClaimDefinitions.DomiciliationProcess)]
    public async Task<IActionResult> ReviewDocument(int id, int clientId, DemandeDocumentReviewDto dto)
    {
        var result = await _domiciliation.ReviewDocumentAsync(id, dto);
        if (result?.Success == true)
            TempData["Success"] = result.Message ?? "Revue enregistrée.";
        else
            SetApiError(result, "Revue impossible.");
        return RedirectToAction(nameof(Client), new { id = clientId });
    }

    /// <summary>Prefer session-selected dossier client, then explicit route/form value.</summary>
    private int ResolveClientId(int? clientId)
    {
        if (SelectedClientId is int selected && selected > 0)
            return selected;
        return clientId is > 0 ? clientId.Value : 0;
    }

    /// <summary>
    /// Locks the client picker when a dossier is already selected (or a clientId was passed).
    /// Cabinet-wide picker only when no company context exists.
    /// </summary>
    private async Task PrepareClientPickerAsync(int effectiveClientId)
    {
        var all = await _clients.GetAllForDropdownAsync() ?? new List<ClientDto>();
        var locked = effectiveClientId > 0;
        ViewBag.ClientLocked = locked;

        if (locked)
        {
            var name = SelectedClientName;
            if (string.IsNullOrWhiteSpace(name))
                name = all.FirstOrDefault(c => c.Id == effectiveClientId)?.RaisonSociale;
            ViewBag.LockedClientName = name ?? $"Client #{effectiveClientId}";
            ViewBag.Clients = all.Where(c => c.Id == effectiveClientId).ToList();
        }
        else
        {
            ViewBag.LockedClientName = null;
            ViewBag.Clients = all;
        }
    }
}
