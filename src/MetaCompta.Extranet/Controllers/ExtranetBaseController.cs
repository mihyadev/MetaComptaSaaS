using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MetaCompta.Application.DTOs;
using MetaCompta.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MetaCompta.Extranet.Controllers;

public abstract class ExtranetBaseController : Controller
{
    protected readonly IHttpClientFactory _httpFactory;
    protected readonly JsonSerializerOptions _jsonOpts = new() { PropertyNameCaseInsensitive = true };

    protected ExtranetBaseController(IHttpClientFactory httpFactory) => _httpFactory = httpFactory;

    protected HttpClient CreateClient()
    {
        var client = _httpFactory.CreateClient("MetaComptaAPI");
        var token = HttpContext.Session.GetString("JwtToken");
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    protected async Task<T?> GetAsync<T>(string url)
    {
        var resp = await CreateClient().GetAsync(url);
        if (!resp.IsSuccessStatusCode) return default;
        var json = await resp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, _jsonOpts);
    }

    protected async Task<TResult?> PostAsync<TInput, TResult>(string url, TInput data)
    {
        var jsonOpts = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
        var content = new StringContent(JsonSerializer.Serialize(data, jsonOpts), Encoding.UTF8, "application/json");
        var resp = await CreateClient().PostAsync(url, content);
        if (!resp.IsSuccessStatusCode) return default;
        var json = await resp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TResult>(json, jsonOpts);
    }

    /// <summary>POST that returns API envelope + raw error body for diagnostics.</summary>
    protected async Task<(ApiResponse<TResult>? Response, int StatusCode, string? ErrorBody)> PostApiAsync<TInput, TResult>(string url, TInput data)
    {
        var jsonOpts = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
        var content = new StringContent(JsonSerializer.Serialize(data, jsonOpts), Encoding.UTF8, "application/json");
        var resp = await CreateClient().PostAsync(url, content);
        var json = await resp.Content.ReadAsStringAsync();
        if (!resp.IsSuccessStatusCode)
            return (null, (int)resp.StatusCode, string.IsNullOrWhiteSpace(json) ? resp.ReasonPhrase : json);

        var parsed = JsonSerializer.Deserialize<ApiResponse<TResult>>(json, jsonOpts);
        return (parsed, (int)resp.StatusCode, null);
    }

    protected async Task<bool> PutAsync<T>(string url, T data)
    {
        var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
        var resp = await CreateClient().PutAsync(url, content);
        return resp.IsSuccessStatusCode;
    }

    protected async Task<bool> DeleteAsync(string url)
    {
        var resp = await CreateClient().DeleteAsync(url);
        return resp.IsSuccessStatusCode;
    }

    protected bool IsAuthenticated => !string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"));

    protected int? SelectedExerciceId =>
        int.TryParse(HttpContext.Session.GetString("SelectedExerciceId"), out var id) ? id : null;

    protected int? SelectedExercice =>
        int.TryParse(HttpContext.Session.GetString("SelectedExercice"), out var y) ? y : null;

    protected string? SelectedExerciceStatut =>
        HttpContext.Session.GetString("SelectedExerciceStatut");

    protected IReadOnlyList<string> ActiveServices
    {
        get
        {
            var raw = HttpContext.Session.GetString("ActiveServices");
            if (string.IsNullOrWhiteSpace(raw)) return Array.Empty<string>();
            return raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }
    }

    protected void SetExerciceSession(ExerciceComptableDto exo)
    {
        HttpContext.Session.SetString("SelectedExercice", exo.Annee.ToString());
        HttpContext.Session.SetString("SelectedExerciceId", exo.Id.ToString());
        HttpContext.Session.SetString("SelectedExerciceStatut", exo.Statut ?? "Ouvert");
    }

    protected void SetActiveServicesSession(IEnumerable<string> services)
    {
        HttpContext.Session.SetString("ActiveServices",
            string.Join(',', services.Distinct(StringComparer.OrdinalIgnoreCase)));
    }

    protected string BuildDeclarationsUrl(string? type = null, int page = 1, int pageSize = 100)
    {
        var qs = new List<string> { $"page={page}", $"pageSize={pageSize}" };
        if (!string.IsNullOrWhiteSpace(type))
            qs.Add($"type={Uri.EscapeDataString(DeclarationServiceCatalog.NormalizeType(type))}");
        if (SelectedExerciceId is > 0)
            qs.Add($"exerciceComptableId={SelectedExerciceId}");
        return "api/client-declarations?" + string.Join('&', qs);
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (IsAuthenticated && context.Controller is not BadgeCountsController)
        {
            try
            {
                var n = await GetAsync<ApiResponse<int>>("api/notifications/count");
                HttpContext.Items["BadgeNotif"] = n?.Data ?? 0;
            }
            catch { HttpContext.Items["BadgeNotif"] = 0; }
            try
            {
                var m = await GetAsync<ApiResponse<int>>("api/messages/unread-count");
                HttpContext.Items["BadgeMsg"] = m?.Data ?? 0;
            }
            catch { HttpContext.Items["BadgeMsg"] = 0; }
            try
            {
                var t = await GetAsync<ApiResponse<int>>("api/client-workflow/my-tasks/count");
                HttpContext.Items["BadgeTasks"] = t?.Data ?? 0;
            }
            catch { HttpContext.Items["BadgeTasks"] = 0; }

            await EnsurePortalContextAsync();
        }

        await next();
    }

    protected async Task EnsurePortalContextAsync()
    {
        var exoQs = SelectedExerciceId is > 0 ? $"?exerciceComptableId={SelectedExerciceId}" : "";
        var ctxResp = await GetAsync<ApiResponse<ExtranetContextDto>>("api/client-context" + exoQs);
        var ctx = ctxResp?.Data;
        if (ctx == null) return;

        if (ctx.CurrentExercice != null)
        {
            if (SelectedExerciceId != ctx.CurrentExercice.Id)
                SetExerciceSession(ctx.CurrentExercice);
        }

        SetActiveServicesSession(ctx.ActiveServices ?? new List<string>());

        ViewBag.PortalExercices = ctx.Exercices ?? new List<ExerciceComptableDto>();
        ViewBag.PortalDossiers = ctx.DossierSummaries ?? new List<ExtranetDossierSummaryDto>();
        ViewBag.PortalActiveServices = ctx.ActiveServices ?? new List<string>();
        ViewBag.SelectedExerciceId = SelectedExerciceId;
        ViewBag.SelectedExercice = SelectedExercice;
        ViewBag.SelectedExerciceStatut = SelectedExerciceStatut ?? ctx.CurrentExercice?.Statut ?? "Ouvert";
        ViewBag.ShowPaySlipsMenu = HasPaieService(ctx.ActiveServices);
    }

    protected static bool HasPaieService(IEnumerable<string>? services)
    {
        if (services == null) return false;
        return services.Any(s =>
            string.Equals(s, "PAIE", StringComparison.OrdinalIgnoreCase)
            || string.Equals(s, "CNSS", StringComparison.OrdinalIgnoreCase)
            || string.Equals(s, "BULLETIN", StringComparison.OrdinalIgnoreCase));
    }

    protected static string DossierIcon(string? type) => type?.ToUpperInvariant() switch
    {
        "TVA" => "bi-percent",
        "IS" or "DRF" or "CM" or "ACOMPTES" or "REG_IS" => "bi-bank",
        "IR" or "IR_SAL" or "RAS_FONC" or "RAS_DIV" => "bi-person-badge",
        "CNSS" or "AMO" => "bi-shield-check",
        "DTS" => "bi-file-earmark-person",
        _ => "bi-folder2-open"
    };

    protected static string DossierAccent(string? type) => type?.ToUpperInvariant() switch
    {
        "TVA" => "tva",
        "IS" or "DRF" or "CM" or "ACOMPTES" or "REG_IS" => "is",
        "IR" or "IR_SAL" or "RAS_FONC" or "RAS_DIV" => "ir",
        "CNSS" or "AMO" or "DTS" => "cnss",
        _ => "default"
    };
}
