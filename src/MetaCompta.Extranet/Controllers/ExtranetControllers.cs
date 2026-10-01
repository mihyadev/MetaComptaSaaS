using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MetaCompta.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.Extranet.Controllers;

public class HomeController : ExtranetBaseController
{
    public HomeController(IHttpClientFactory http) : base(http) { }

    public async Task<IActionResult> Index()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
            return RedirectToAction("Login", "Account");

        await EnsurePortalContextAsync();
        var exoQs = SelectedExerciceId is > 0 ? $"?exerciceComptableId={SelectedExerciceId}" : "";
        var response = await GetAsync<ApiResponse<ExtranetDashboardDto>>("api/dashboard/extranet" + exoQs);
        ViewBag.Dashboard = response?.Data;
        ViewBag.ClientName = HttpContext.Session.GetString("ClientName");
        ViewBag.UserName = HttpContext.Session.GetString("UserName");
        ViewBag.ExerciceAnnee = SelectedExercice ?? response?.Data?.ExerciceAnnee;
        ViewBag.ExerciceStatut = SelectedExerciceStatut ?? response?.Data?.ExerciceStatut ?? "Ouvert";
        return View();
    }
}

public class ProfileController : ExtranetBaseController
{
    public ProfileController(IHttpClientFactory http) : base(http) { }

    public async Task<IActionResult> Index()
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Account");

        var response = await GetAsync<ApiResponse<ClientDto>>("api/client-profile");
        var client = response?.Data;
        if (client == null) return RedirectToAction("Index", "Home");
        return View(client);
    }
}

public class DeclarationsController : ExtranetBaseController
{
    public DeclarationsController(IHttpClientFactory http) : base(http) { }

    public IActionResult Index()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
            return RedirectToAction("Login", "Account");
        return RedirectToAction("Index", "Dossiers");
    }

    public async Task<IActionResult> Details(int id)
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Account");

        var declResponse = await GetAsync<ApiResponse<DeclarationDto>>($"api/client-declarations/{id}");
        var decl = declResponse?.Data;
        if (decl == null) return NotFound();

        // Load documents for this declaration
        var docsResponse = await GetAsync<ApiResponse<List<DocumentDto>>>($"api/documents/declaration/{id}");
        ViewBag.Documents = docsResponse?.Data ?? new List<DocumentDto>();

        // Load messages for this declaration
        var msgsResponse = await GetAsync<ApiResponse<List<MessageDto>>>($"api/messages/declaration/{id}");
        ViewBag.Messages = msgsResponse?.Data ?? new List<MessageDto>();

        // Load notifications count
        var notifCountResp = await GetAsync<ApiResponse<int>>("api/notifications/count");
        ViewBag.UnreadNotifCount = notifCountResp?.Data ?? 0;

        // Load type-specific financial data
        try
        {
            switch (decl.Type?.ToUpper())
            {
                case "TVA":
                    var tva = await GetAsync<ApiResponse<DeclarationTVADto>>($"api/client-declarations/{id}/tva");
                    ViewBag.TVAData = tva?.Data;
                    break;
                case "IS":
                    var isData = await GetAsync<ApiResponse<DeclarationISDto>>($"api/client-declarations/{id}/is");
                    ViewBag.ISData = isData?.Data;
                    var checklist = await GetAsync<ApiResponse<List<DeclarationChecklistItemDto>>>($"api/client-declarations/{id}/checklist");
                    ViewBag.Checklist = checklist?.Data ?? new List<DeclarationChecklistItemDto>();
                    break;
                case "IR":
                    var ir = await GetAsync<ApiResponse<DeclarationIRDto>>($"api/client-declarations/{id}/ir");
                    ViewBag.IRData = ir?.Data;
                    break;
                case "CNSS":
                case "AMO":
                    var cnss = await GetAsync<ApiResponse<DeclarationCNSSDto>>($"api/client-declarations/{id}/cnss");
                    ViewBag.CNSSData = cnss?.Data;
                    var roster = await GetAsync<ApiResponse<CnssEmployeeRosterDto>>($"api/client-declarations/{id}/cnss/employees");
                    ViewBag.CnssRoster = roster?.Data;
                    break;
            }
        }
        catch { /* financial details are supplementary */ }

        // Load workflow instance to check if there's a client step pending
        try
        {
            var wfResponse = await GetAsync<ApiResponse<PagedResult<WorkflowInstanceDto>>>($"api/client-workflow/instances?declarationId={id}&page=1");
            var wfLite = wfResponse?.Data?.Items?.FirstOrDefault(w => w.Status == "Active")
                ?? wfResponse?.Data?.Items?.FirstOrDefault();
            if (wfLite != null)
            {
                var full = await GetAsync<ApiResponse<WorkflowInstanceDto>>($"api/client-workflow/instances/{wfLite.Id}");
                ViewBag.WorkflowInstance = full?.Data ?? wfLite;
                var stepResponse = await GetAsync<ApiResponse<WorkflowStepInstanceDto>>($"api/client-workflow/instances/{wfLite.Id}/current-step");
                ViewBag.CurrentStep = stepResponse?.Data;
            }
        }
        catch { /* workflow is supplementary */ }

        return View(decl);
    }

    public async Task<IActionResult> DownloadDocument(int id)
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Account");

        var client = CreateClient();
        var response = await client.GetAsync($"api/documents/{id}/download");
        if (!response.IsSuccessStatusCode) return NotFound();

        var stream = await response.Content.ReadAsStreamAsync();
        var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
        var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"') ?? "document";
        return File(stream, contentType, fileName);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadDocument(int declarationId, IFormFile file, string titre, string categorie, int? checklistItemId = null)
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Account");

        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Veuillez sélectionner un fichier.";
            return RedirectToAction("Details", new { id = declarationId });
        }

        var client = CreateClient();
        using var content = new MultipartFormDataContent();
        using var stream = file.OpenReadStream();
        var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        content.Add(streamContent, "file", file.FileName);
        content.Add(new StringContent(titre ?? file.FileName), "titre");
        content.Add(new StringContent(categorie ?? "Fiscal"), "categorie");
        content.Add(new StringContent(declarationId.ToString()), "declarationId");
        if (checklistItemId.HasValue)
            content.Add(new StringContent(checklistItemId.Value.ToString()), "checklistItemId");

        var response = await client.PostAsync("api/documents/upload", content);
        TempData[response.IsSuccessStatusCode ? "Success" : "Error"] =
            response.IsSuccessStatusCode ? "Document uploadé avec succès." : "Erreur lors de l'upload.";
        return RedirectToAction("Details", new { id = declarationId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveStep(int declarationId, int workflowInstanceId, string? notes)
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Account");

        var data = new { Notes = notes ?? "Approuvé par le client", Comment = "Validation client via extranet" };
        var http = CreateClient();
        var body = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
        var resp = await http.PostAsync($"api/client-workflow/instances/{workflowInstanceId}/approve", body);
        var json = await resp.Content.ReadAsStringAsync();
        var parsed = JsonSerializer.Deserialize<ApiResponse<WorkflowStepInstanceDto>>(json, _jsonOpts);

        if (resp.IsSuccessStatusCode && parsed?.Success == true)
        {
            TempData["Success"] = "Étape terminée avec succès.";
            return RedirectToAction("Index", "Tasks");
        }

        TempData["Error"] = parsed?.Message ?? "Erreur lors de la validation de l'étape.";
        return RedirectToAction("Details", new { id = declarationId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendMessage(int declarationId, int? clientId, string sujet, string contenu)
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Account");
        if (string.IsNullOrWhiteSpace(contenu))
        {
            TempData["Error"] = "Le message ne peut pas être vide.";
            return RedirectToAction("Details", new { id = declarationId });
        }
        if (!clientId.HasValue && int.TryParse(HttpContext.Session.GetString("ClientId"), out var sid) && sid > 0)
            clientId = sid;

        var dto = new SendMessageDto
        {
            ClientId = clientId,
            DeclarationId = declarationId > 0 ? declarationId : null,
            Sujet = sujet ?? "",
            Contenu = contenu.Trim()
        };
        var (api, status, err) = await PostApiAsync<SendMessageDto, MessageDto>("api/messages", dto);
        if (api?.Success == true && api.Data != null)
            TempData["Success"] = "Message envoyé.";
        else
            TempData["Error"] = api?.Message
                ?? (status >= 400 ? $"Envoi impossible ({status})." : "Erreur lors de l'envoi.")
                ?? "Erreur lors de l'envoi.";
        return RedirectToAction("Details", new { id = declarationId });
    }
}

public class MessagesExtranetController : ExtranetBaseController
{
    public MessagesExtranetController(IHttpClientFactory http) : base(http) { }

    public async Task<IActionResult> Index(int? thread = null)
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Account");

        var clientId = int.TryParse(HttpContext.Session.GetString("ClientId"), out var cid) && cid > 0 ? cid : (int?)null;
        ViewBag.ClientId = clientId;

        // Full client history (not conversations roots-only) so threads show all replies
        List<MessageDto> messages = new();
        if (clientId.HasValue)
        {
            var full = await GetAsync<ApiResponse<List<MessageDto>>>($"api/messages/client/{clientId}");
            messages = full?.Data ?? new List<MessageDto>();
        }
        if (!messages.Any())
        {
            var convResp = await GetAsync<ApiResponse<List<MessageDto>>>("api/messages/conversations");
            messages = convResp?.Data ?? new List<MessageDto>();
        }

        // If a thread is open, ensure we have the full declaration thread
        if (thread is > 0)
        {
            var declMsgs = await GetAsync<ApiResponse<List<MessageDto>>>($"api/messages/declaration/{thread}");
            if (declMsgs?.Data != null && declMsgs.Data.Any())
            {
                var byId = messages.ToDictionary(m => m.Id);
                foreach (var m in declMsgs.Data)
                    byId[m.Id] = m;
                messages = byId.Values.ToList();
            }
        }

        ViewBag.Messages = messages.OrderBy(m => m.CreatedAt).ToList();
        ViewBag.OpenThread = thread;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendMessage(int declarationId, int? clientId, string? sujet, string contenu)
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Account");
        if (string.IsNullOrWhiteSpace(contenu))
        {
            TempData["Error"] = "Le message ne peut pas être vide.";
            return RedirectToAction(nameof(Index), new { thread = declarationId });
        }
        if (declarationId <= 0)
        {
            TempData["Error"] = "Sélectionnez une déclaration pour envoyer un message.";
            return RedirectToAction(nameof(Index));
        }

        if (!clientId.HasValue && int.TryParse(HttpContext.Session.GetString("ClientId"), out var sid) && sid > 0)
            clientId = sid;

        var dto = new SendMessageDto
        {
            ClientId = clientId,
            DeclarationId = declarationId,
            Sujet = sujet?.Trim() ?? "",
            Contenu = contenu.Trim()
        };
        var (api, status, err) = await PostApiAsync<SendMessageDto, MessageDto>("api/messages", dto);
        if (api?.Success == true && api.Data != null)
            TempData["Success"] = "Message envoyé.";
        else
        {
            var detail = api?.Message;
            if (string.IsNullOrWhiteSpace(detail) && !string.IsNullOrWhiteSpace(err) && err.Length < 180)
                detail = err;
            TempData["Error"] = string.IsNullOrWhiteSpace(detail)
                ? $"Erreur lors de l'envoi{(status > 0 ? $" ({status})" : "")}."
                : detail;
        }
        return RedirectToAction(nameof(Index), new { thread = declarationId });
    }
}

public class DocumentsController : ExtranetBaseController
{
    public DocumentsController(IHttpClientFactory http) : base(http) { }

    public async Task<IActionResult> Index()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
            return RedirectToAction("Login", "Account");

        var docsResp = await GetAsync<ApiResponse<PagedResult<DocumentDto>>>("api/documents?pageSize=100");
        var docs = docsResp?.Data?.Items ?? new List<DocumentDto>();
        return View(docs);
    }
}

public class PaySlipsController : ExtranetBaseController
{
    public PaySlipsController(IHttpClientFactory http) : base(http) { }

    public async Task<IActionResult> Index()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
            return RedirectToAction("Login", "Account");

        await EnsurePortalContextAsync();
        if (!HasPaieService(ActiveServices))
        {
            TempData["Error"] = "Les bulletins de paie ne sont pas inclus dans vos services actifs.";
            return RedirectToAction("Index", "Home");
        }

        // Payroll API is BackOffice-only for now — show empty state until client myslips is available
        ViewBag.PaySlipsUnavailable = true;
        return View(new List<PaySlipDto>());
    }
}
public class BadgeCountsController : ExtranetBaseController
{
    public BadgeCountsController(IHttpClientFactory http) : base(http) { }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!IsAuthenticated) return Json(new { notifications = 0, messages = 0, tasks = 0 });

        int notifs = 0, msgs = 0, tasks = 0;
        try
        {
            var nResp = await GetAsync<ApiResponse<int>>("api/notifications/count");
            notifs = nResp?.Data ?? 0;
        }
        catch { }
        try
        {
            var mResp = await GetAsync<ApiResponse<int>>("api/messages/unread-count");
            msgs = mResp?.Data ?? 0;
        }
        catch { }
        try
        {
            var tResp = await GetAsync<ApiResponse<int>>("api/client-workflow/my-tasks/count");
            tasks = tResp?.Data ?? 0;
        }
        catch { }

        return Json(new { notifications = notifs, messages = msgs, tasks });
    }
}

public class NotificationsController : ExtranetBaseController
{
    public NotificationsController(IHttpClientFactory http) : base(http) { }

    public async Task<IActionResult> Index()
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Account");

        var resp = await GetAsync<ApiResponse<List<NotificationDto>>>("api/notifications?count=50");
        ViewBag.Notifications = resp?.Data ?? new List<NotificationDto>();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> MarkAsRead(int id, string? returnUrl)
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Account");

        var client = CreateClient();
        await client.PutAsync($"api/notifications/{id}/read", null);
        return Redirect(returnUrl ?? "/Notifications");
    }

    [HttpPost]
    public async Task<IActionResult> MarkAllAsRead()
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Account");

        var client = CreateClient();
        await client.PutAsync("api/notifications/read-all", null);
        return RedirectToAction("Index");
    }
}

public class TasksController : ExtranetBaseController
{
    public TasksController(IHttpClientFactory http) : base(http) { }

    public async Task<IActionResult> Index()
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Account");

        var client = CreateClient();
        var http = await client.GetAsync("api/client-workflow/my-tasks");
        if (!http.IsSuccessStatusCode)
        {
            ViewBag.LoadError = $"Impossible de charger vos tâches (HTTP {(int)http.StatusCode}). Réessayez ou contactez votre cabinet.";
            return View(new List<WorkflowStepInstanceDto>());
        }

        var json = await http.Content.ReadAsStringAsync();
        var resp = JsonSerializer.Deserialize<ApiResponse<List<WorkflowStepInstanceDto>>>(json, _jsonOpts);
        var tasks = resp?.Data ?? new List<WorkflowStepInstanceDto>();
        if (resp?.Success == false && !string.IsNullOrEmpty(resp.Message))
            ViewBag.LoadError = resp.Message;
        return View(tasks);
    }

    /// <summary>Open a client workflow step — same workbench as cabinet task: docs, message, terminer.</summary>
    public async Task<IActionResult> Details(int id)
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Account");

        var myTasks = await GetAsync<ApiResponse<List<WorkflowStepInstanceDto>>>("api/client-workflow/my-tasks");
        var step = myTasks?.Data?.FirstOrDefault(s => s.Id == id);

        WorkflowInstanceDto? instance = null;
        if (step != null)
        {
            var instResp = await GetAsync<ApiResponse<WorkflowInstanceDto>>($"api/client-workflow/instances/{step.WorkflowInstanceId}");
            instance = instResp?.Data;
        }
        else
        {
            // Fallback: treat id as workflow instance id (deep links / bookmarks)
            var instResp = await GetAsync<ApiResponse<WorkflowInstanceDto>>($"api/client-workflow/instances/{id}");
            instance = instResp?.Data;
            if (instance != null)
            {
                var cur = await GetAsync<ApiResponse<WorkflowStepInstanceDto>>($"api/client-workflow/instances/{id}/current-step");
                step = cur?.Data ?? instance.StepInstances?
                    .FirstOrDefault(s => s.Status is "WaitingForClient" or "InProgress" or "WaitingForValidation");
            }
        }

        if (step == null || instance == null)
        {
            TempData["Error"] = "Tâche introuvable ou déjà terminée.";
            return RedirectToAction("Index");
        }

        ViewBag.Step = step;
        ViewBag.WorkflowInstance = instance;

        if (step.DeclarationId.HasValue || instance.DeclarationId.HasValue)
        {
            var declId = step.DeclarationId ?? instance.DeclarationId!.Value;
            var decl = await GetAsync<ApiResponse<DeclarationDto>>($"api/client-declarations/{declId}");
            ViewBag.Declaration = decl?.Data;
            var docs = await GetAsync<ApiResponse<List<DocumentDto>>>($"api/documents/declaration/{declId}");
            ViewBag.Documents = docs?.Data ?? new List<DocumentDto>();
            var msgs = await GetAsync<ApiResponse<List<MessageDto>>>($"api/messages/declaration/{declId}");
            ViewBag.Messages = msgs?.Data ?? new List<MessageDto>();
        }
        else
        {
            ViewBag.Documents = new List<DocumentDto>();
            ViewBag.Messages = new List<MessageDto>();
        }

        return View(step);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadDocument(int stepId, int declarationId, IFormFile file, string titre, string categorie)
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Account");

        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Veuillez sélectionner un fichier.";
            return RedirectToAction("Details", new { id = stepId });
        }

        var client = CreateClient();
        using var content = new MultipartFormDataContent();
        using var stream = file.OpenReadStream();
        var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        content.Add(streamContent, "file", file.FileName);
        content.Add(new StringContent(titre ?? file.FileName), "titre");
        content.Add(new StringContent(categorie ?? "Fiscal"), "categorie");
        content.Add(new StringContent(declarationId.ToString()), "declarationId");

        var response = await client.PostAsync("api/documents/upload", content);
        TempData[response.IsSuccessStatusCode ? "Success" : "Error"] =
            response.IsSuccessStatusCode ? "Document envoyé." : "Erreur lors de l'envoi du document.";
        return RedirectToAction("Details", new { id = stepId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendMessage(int stepId, int declarationId, int? clientId, string sujet, string contenu)
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Account");

        var msg = new { ClientId = clientId, DeclarationId = declarationId, Sujet = sujet, Contenu = contenu };
        var http = CreateClient();
        var body = new StringContent(JsonSerializer.Serialize(msg), Encoding.UTF8, "application/json");
        var resp = await http.PostAsync("api/messages", body);
        if (!resp.IsSuccessStatusCode)
        {
            TempData["Error"] = "Erreur lors de l'envoi du message.";
            return RedirectToAction("Details", new { id = stepId });
        }

        var json = await resp.Content.ReadAsStringAsync();
        var parsed = JsonSerializer.Deserialize<ApiResponse<object>>(json, _jsonOpts);
        TempData[parsed?.Success != false ? "Success" : "Error"] =
            parsed?.Success != false ? "Message envoyé." : (parsed?.Message ?? "Erreur lors de l'envoi du message.");
        return RedirectToAction("Details", new { id = stepId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteStep(int stepId, int workflowInstanceId, string? notes)
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Account");

        var data = new
        {
            Notes = string.IsNullOrWhiteSpace(notes) ? "Terminé par le client via Mes tâches" : notes.Trim(),
            Comment = "Validation client via extranet"
        };
        var http = CreateClient();
        var body = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
        var resp = await http.PostAsync($"api/client-workflow/instances/{workflowInstanceId}/approve", body);
        var json = await resp.Content.ReadAsStringAsync();
        var parsed = JsonSerializer.Deserialize<ApiResponse<WorkflowStepInstanceDto>>(json, _jsonOpts);

        if (resp.IsSuccessStatusCode && parsed?.Success == true)
        {
            TempData["Success"] = "Tâche terminée. Merci — votre cabinet poursuit le traitement.";
            return RedirectToAction("Index");
        }

        TempData["Error"] = parsed?.Message
            ?? (resp.IsSuccessStatusCode ? "Impossible de terminer la tâche." : $"Erreur HTTP {(int)resp.StatusCode}.");
        return RedirectToAction("Details", new { id = stepId });
    }

    public async Task<IActionResult> DownloadDocument(int id, int stepId)
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Account");

        var client = CreateClient();
        var response = await client.GetAsync($"api/documents/{id}/download");
        if (!response.IsSuccessStatusCode) return NotFound();

        var stream = await response.Content.ReadAsStreamAsync();
        var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
        var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"') ?? "document";
        return File(stream, contentType, fileName);
    }
}

public class ContratsController : ExtranetBaseController
{
    public ContratsController(IHttpClientFactory http) : base(http) { }

    public async Task<IActionResult> Index()
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Account");

        var list = await GetAsync<ApiResponse<PagedResult<ContratCommercialDto>>>("api/client-contrats?pageSize=50");
        var items = list?.Data?.Items ?? new List<ContratCommercialDto>();

        // Prefer active/validated contract; else latest
        var preferred = items
            .OrderByDescending(c => c.Statut is "Valide" or "Actif" ? 2 : c.Statut == "Signe" ? 1 : 0)
            .ThenByDescending(c => c.DateEmission)
            .FirstOrDefault();

        if (preferred == null)
        {
            ViewBag.HasContract = false;
            return View((ContratCommercialDto?)null);
        }

        var detail = await GetAsync<ApiResponse<ContratCommercialDto>>($"api/client-contrats/{preferred.Id}");
        var contrat = detail?.Data ?? preferred;
        ViewBag.HasContract = true;

        var decls = await GetAsync<ApiResponse<PagedResult<DeclarationDto>>>($"api/client-contrats/{preferred.Id}/declarations?pageSize=50");
        ViewBag.Declarations = decls?.Data?.Items ?? new List<DeclarationDto>();

        return View(contrat);
    }
}