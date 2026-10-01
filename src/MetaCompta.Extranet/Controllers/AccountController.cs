using System.Net.Http;
using System.Text;
using System.Text.Json;
using MetaCompta.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.Extranet.Controllers;

public class AccountController : ExtranetBaseController
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
        if (!resp.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Email ou mot de passe incorrect");
            return View();
        }
        var json = await resp.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<TokenDto>>(json, _jsonOpts);
        if (result?.Success == true && result.Data != null)
        {
            HttpContext.Session.SetString("JwtToken", result.Data.AccessToken);
            HttpContext.Session.SetString("UserName", result.Data.User?.FullName ?? email);
            HttpContext.Session.SetString("UserRole", result.Data.User?.Role ?? "Client");
            HttpContext.Session.SetString("UserEmail", result.Data.User?.Email ?? email);
            HttpContext.Session.SetString("ClientName", result.Data.User?.ClientName ?? "");
            HttpContext.Session.SetString("ClientId", result.Data.User?.ClientId?.ToString() ?? "");
            HttpContext.Session.SetString("CabinetName", result.Data.User?.CabinetName ?? "");

            // Prefetch portal exercice + dossiers for the first authenticated hit
            try
            {
                var ctx = await GetAsync<ApiResponse<ExtranetContextDto>>("api/client-context");
                if (ctx?.Data?.CurrentExercice != null)
                    SetExerciceSession(ctx.Data.CurrentExercice);
                if (ctx?.Data?.ActiveServices != null)
                    SetActiveServicesSession(ctx.Data.ActiveServices);
            }
            catch { /* context loads on next request */ }
        }
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}
