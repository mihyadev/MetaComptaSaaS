using System.Net.Http;
using System.Text;
using System.Text.Json;
using MetaCompta.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.Extranet.Controllers;

public class AccountController : ExtranetBaseController
{
    private static readonly JsonSerializerOptions CamelJson = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public AccountController(IHttpClientFactory http) : base(http) { }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        var client = CreateClient();
        var content = new StringContent(
            JsonSerializer.Serialize(new { email, password, portal = "extranet" }, CamelJson),
            Encoding.UTF8,
            "application/json");
        var resp = await client.PostAsync("api/auth/login", content);
        var json = await resp.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<TokenDto>>(json, _jsonOpts);

        if (!resp.IsSuccessStatusCode || result?.Success != true || result.Data == null)
        {
            ModelState.AddModelError("", result?.Message ?? "Email ou mot de passe incorrect");
            return View();
        }

        if (result.Data.User?.ClientId == null)
        {
            ModelState.AddModelError("", "Compte cabinet — utilisez le BackOffice.");
            return View();
        }

        HttpContext.Session.SetString("JwtToken", result.Data.AccessToken);
        HttpContext.Session.SetString("UserName", result.Data.User?.FullName ?? email);
        HttpContext.Session.SetString("UserRole", result.Data.User?.Role ?? "Client");
        HttpContext.Session.SetString("UserEmail", result.Data.User?.Email ?? email);
        HttpContext.Session.SetString("ClientName", result.Data.User?.ClientName ?? "");
        HttpContext.Session.SetString("ClientId", result.Data.User?.ClientId?.ToString() ?? "");
        HttpContext.Session.SetString("CabinetName", result.Data.User?.CabinetName ?? "");

        try
        {
            var ctx = await GetAsync<ApiResponse<ExtranetContextDto>>("api/client-context");
            if (ctx?.Data?.CurrentExercice != null)
                SetExerciceSession(ctx.Data.CurrentExercice);
            if (ctx?.Data?.ActiveServices != null)
                SetActiveServicesSession(ctx.Data.ActiveServices);
        }
        catch { /* context loads on next request */ }

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> Activate(string userId, string token)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
        {
            ViewBag.TokenInvalid = true;
            return View(new ActivateAccountDto());
        }

        var client = CreateClient();
        var url = $"api/auth/activation-info?userId={Uri.EscapeDataString(userId)}&token={Uri.EscapeDataString(token)}";
        var response = await client.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<ActivationInfoDto>>(content, _jsonOpts);

        if (result?.Success != true || result.Data == null)
        {
            ViewBag.TokenInvalid = true;
            return View(new ActivateAccountDto { UserId = userId, Token = token });
        }

        ViewBag.FullName = result.Data.FullName;
        ViewBag.Email = result.Data.Email;
        ViewBag.AlreadyActivated = result.Data.AlreadyActivated;
        ViewBag.TokenInvalid = !result.Data.AlreadyActivated && !result.Data.TokenValid;

        return View(new ActivateAccountDto { UserId = userId, Token = token });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(ActivateAccountDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.TokenInvalid = false;
            return View(dto);
        }

        var client = CreateClient();
        var payload = new StringContent(JsonSerializer.Serialize(dto, CamelJson), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("api/auth/activate", payload);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<bool>>(content, _jsonOpts);

        if (result?.Success == true)
        {
            TempData["Success"] = result.Message ?? "Compte activé. Vous pouvez vous connecter.";
            return RedirectToAction(nameof(Login));
        }

        ModelState.AddModelError("", result?.Message ?? "Activation impossible.");
        if (result?.Errors != null)
            foreach (var err in result.Errors)
                ModelState.AddModelError("", err);

        ViewBag.TokenInvalid = false;
        return View(dto);
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}
