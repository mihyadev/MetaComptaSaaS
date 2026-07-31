using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Linq;
using MetaCompta.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.Admin.Controllers;

public class AccountController : AdminBaseController
{
    public AccountController(IHttpClientFactory http) : base(http) { }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        var client = CreateClient();
        var content = new StringContent(JsonSerializer.Serialize(new { email, password }), Encoding.UTF8, "application/json");
        var resp = await client.PostAsync("api/auth/login", content);
        if (!resp.IsSuccessStatusCode) { ModelState.AddModelError("", "Identifiants invalides"); return View(); }
        var json = await resp.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<TokenDto>>(json, _jsonOpts);
        if (result?.Success == true && result.Data != null && result.Data.User?.Role == "PlatformAdmin")
        {
            HttpContext.Session.SetString("JwtToken", result.Data.AccessToken);
            HttpContext.Session.SetString("UserName", result.Data.User?.FullName ?? email);
            HttpContext.Session.SetString("UserRole", result.Data.User!.Role!);
            return RedirectToAction("Index", "Home");
        }
        ModelState.AddModelError("", result?.Message ?? "Accès réservé aux administrateurs de la plateforme");
        return View();
    }

    public IActionResult Logout() { HttpContext.Session.Clear(); return RedirectToAction("Login"); }
}

public class HomeController : AdminBaseController
{
    public HomeController(IHttpClientFactory http) : base(http) { }

    public async Task<IActionResult> Index()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"))) return RedirectToAction("Login", "Account");
        var cabinets = await GetAsync<ApiResponse<AdminPagedResult>>("api/admin/cabinets?pageSize=100");
        ViewBag.CabinetsResult = cabinets;
        return View();
    }
}

public class CabinetsController : AdminBaseController
{
    public CabinetsController(IHttpClientFactory http) : base(http) { }

    public async Task<IActionResult> Index(int page = 1, string? search = null)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"))) return RedirectToAction("Login", "Account");
        var url = "api/admin/cabinets?page=" + page + "&pageSize=20";
        if (!string.IsNullOrWhiteSpace(search)) url += "&search=" + Uri.EscapeDataString(search);
        var result = await GetAsync<ApiResponse<AdminPagedResult>>(url);
        ViewBag.CabinetsResult = result;
        ViewBag.Search = search;
        ViewBag.Page = page;
        return View();
    }

    [HttpGet]
    public IActionResult Create()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"))) return RedirectToAction("Login", "Account");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CabinetCreateDto dto)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"))) return RedirectToAction("Login", "Account");
        var ok = await PostAsync("api/admin/cabinets", dto);
        TempData[ok ? "Success" : "Error"] = ok ? "Cabinet créé avec succès." : "Erreur lors de la création du cabinet.";
        return RedirectToAction("Index");
    }

    [HttpPost, ActionName("Edit")]
    public async Task<IActionResult> EditPost(int id, [FromForm] CabinetEditAdminDto dto)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"))) return RedirectToAction("Login", "Account");
        var ok = await PutAsync($"api/admin/cabinets/{id}", dto);
        TempData[ok ? "Success" : "Error"] = ok ? "Cabinet modifié." : "Erreur lors de la modification.";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"))) return RedirectToAction("Login", "Account");
        var result = await GetAsync<ApiResponse<JsonElement>>($"api/admin/cabinets/{id}");
        if (result?.Data.ValueKind != JsonValueKind.Object) { TempData["Error"] = "Cabinet non trouvé."; return RedirectToAction("Index"); }
        ViewBag.Cabinet = result.Data;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ToggleActive(int id)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"))) return RedirectToAction("Login", "Account");
        await PutAsync($"api/admin/cabinets/{id}/toggle", new { });
        TempData["Success"] = "Statut du cabinet modifié.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"))) return RedirectToAction("Login", "Account");
        var ok = await DeleteAsync($"api/admin/cabinets/{id}");
        TempData[ok ? "Success" : "Error"] = ok ? "Cabinet supprimé." : "Impossible de supprimer ce cabinet (clients associés ?).";
        return RedirectToAction("Index");
    }
}

public class ReferenceDataController : AdminBaseController
{
    public ReferenceDataController(IHttpClientFactory http) : base(http) { }

    public async Task<IActionResult> Index()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"))) return RedirectToAction("Login", "Account");
        var result = await GetAsync<ApiResponse<Dictionary<string, List<RefDataItem>>>>("api/referencedata/all");
        ViewBag.AllData = result?.Data;
        return View();
    }

    [HttpGet]
    public IActionResult Create(string? category = null)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"))) return RedirectToAction("Login", "Account");
        ViewBag.SelectedCategory = category;
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"))) return RedirectToAction("Login", "Account");
        var result = await GetAsync<ApiResponse<Dictionary<string, List<RefDataItem>>>>("api/referencedata/all");
        var item = result?.Data?.Values.SelectMany(v => v).FirstOrDefault(r => r.Id == id);
        if (item == null) { TempData["Error"] = "Entrée non trouvée."; return RedirectToAction("Index"); }
        ViewBag.RefItem = item;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Save([FromForm] int id, [FromForm] string category, [FromForm] string code, [FromForm] string label, [FromForm] decimal? numericValue, [FromForm] string? value, [FromForm] int sortOrder)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"))) return RedirectToAction("Login", "Account");
        var data = new { id, category, code, label, numericValue, value, sortOrder, isActive = true };
        var ok = await PostAsync("api/referencedata", data);
        TempData[ok ? "Success" : "Error"] = ok ? "Enregistré." : "Erreur.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"))) return RedirectToAction("Login", "Account");
        var ok = await DeleteAsync($"api/referencedata/{id}");
        TempData[ok ? "Success" : "Error"] = ok ? "Supprimé." : "Erreur.";
        return RedirectToAction("Index");
    }
}

public class AuditLogsController : AdminBaseController
{
    public AuditLogsController(IHttpClientFactory http) : base(http) { }

    public async Task<IActionResult> Index(int page = 1, string? action = null)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"))) return RedirectToAction("Login", "Account");
        var url = $"api/admin/auditlogs?page={page}&pageSize=50";
        if (!string.IsNullOrWhiteSpace(action)) url += "&action=" + Uri.EscapeDataString(action);
        var result = await GetAsync<ApiResponse<AdminPagedResult>>(url);
        ViewBag.AuditResult = result;
        ViewBag.ActionFilter = action;
        ViewBag.Page = page;
        return View();
    }
}

// Helper DTOs for deserialization
public class AdminPagedResult
{
    public List<JsonElement> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class RefDataItem
{
    public int Id { get; set; }
    public string Category { get; set; } = "";
    public string Code { get; set; } = "";
    public string Label { get; set; } = "";
    public string? Value { get; set; }
    public decimal? NumericValue { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}