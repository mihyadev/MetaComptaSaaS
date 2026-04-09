using System.Net.Http;
using System.Text;
using System.Text.Json;
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
        // Get cabinets count for dashboard
        var cabinets = await GetAsync<ApiResponse<object>>("api/admin/cabinets?pageSize=1");
        ViewBag.CabinetsData = cabinets;
        return View();
    }
}

public class CabinetsController : AdminBaseController
{
    public CabinetsController(IHttpClientFactory http) : base(http) { }

    public async Task<IActionResult> Index(int page = 1, string? search = null)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"))) return RedirectToAction("Login", "Account");
        var url = $"api/admin/cabinets?page={page}";
        if (!string.IsNullOrWhiteSpace(search)) url += $"&search={search}";
        var result = await GetAsync<ApiResponse<object>>("api/admin/cabinets?page=" + page + (string.IsNullOrWhiteSpace(search) ? "" : "&search=" + search));
        ViewBag.CabinetsResult = result;
        ViewBag.Search = search;
        return View();
    }

    public async Task<IActionResult> Details(int id)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"))) return RedirectToAction("Login", "Account");
        var result = await GetAsync<ApiResponse<object>>($"api/admin/cabinets/{id}");
        ViewBag.CabinetDetail = result;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ToggleActive(int id)
    {
        await PutAsync($"api/admin/cabinets/{id}/toggle", new { });
        TempData["Success"] = "Statut du cabinet modifié.";
        return RedirectToAction("Index");
    }
}

public class ReferenceDataController : AdminBaseController
{
    public ReferenceDataController(IHttpClientFactory http) : base(http) { }

    public async Task<IActionResult> Index()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"))) return RedirectToAction("Login", "Account");
        var tvaRates = await GetAsync<ApiResponse<object>>("api/referencedata/TVA_RATE");
        var cnssRates = await GetAsync<ApiResponse<object>>("api/referencedata/CNSS_RATE");
        ViewBag.TVARates = tvaRates;
        ViewBag.CNSSRates = cnssRates;
        return View();
    }
}

public class AuditLogsController : AdminBaseController
{
    public AuditLogsController(IHttpClientFactory http) : base(http) { }

    public async Task<IActionResult> Index(int page = 1, string? action = null)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"))) return RedirectToAction("Login", "Account");
        var url = $"api/admin/auditlogs?page={page}";
        if (!string.IsNullOrWhiteSpace(action)) url += $"&action={action}";
        var result = await GetAsync<ApiResponse<object>>(url);
        ViewBag.AuditResult = result;
        ViewBag.ActionFilter = action;
        return View();
    }
}
