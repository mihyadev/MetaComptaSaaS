using System.Text;
using System.Text.Json;
using MetaCompta.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.BackOffice.Controllers;

public class AccountController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AccountController(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
            return RedirectToAction("Workspace", "Home");
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Activate(string userId, string token)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
        {
            ViewBag.TokenInvalid = true;
            return View(new ActivateAccountDto());
        }

        var client = _httpClientFactory.CreateClient("MetaComptaAPI");
        var url = $"api/auth/activation-info?userId={Uri.EscapeDataString(userId)}&token={Uri.EscapeDataString(token)}";
        var response = await client.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<ActivationInfoDto>>(content, JsonOpts);

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

        var client = _httpClientFactory.CreateClient("MetaComptaAPI");
        var json = JsonSerializer.Serialize(dto, JsonOpts);
        var response = await client.PostAsync("api/auth/activate", new StringContent(json, Encoding.UTF8, "application/json"));
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<bool>>(content, JsonOpts);

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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto dto, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(dto);

        var loginPayload = new LoginDto(dto.Email, dto.Password, "backoffice");
        var client = _httpClientFactory.CreateClient("MetaComptaAPI");
        var json = JsonSerializer.Serialize(loginPayload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var response = await client.PostAsync("api/auth/login", new StringContent(json, Encoding.UTF8, "application/json"));
        var content = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<ApiResponse<TokenDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (result?.Success == true && result.Data != null)
        {
            if (result.Data.User.ClientId.HasValue
                || string.Equals(result.Data.User.Role, "Client", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "Compte client Extranet — utilisez le portail client.");
                return View(dto);
            }

            HttpContext.Session.SetString("JwtToken", result.Data.AccessToken);
            HttpContext.Session.SetString("RefreshToken", result.Data.RefreshToken);
            HttpContext.Session.SetString("UserId", result.Data.User.Id ?? "");
            HttpContext.Session.SetString("UserName", result.Data.User.FullName);
            HttpContext.Session.SetString("UserEmail", result.Data.User.Email);
            HttpContext.Session.SetString("UserRole", result.Data.User.Role ?? "");
            HttpContext.Session.SetString("CabinetId", result.Data.User.CabinetId?.ToString() ?? "");
            HttpContext.Session.SetString("CabinetName", result.Data.User.CabinetName ?? "");
            HttpContext.Session.SetString("ProfileImage", result.Data.User.ProfileImageUrl ?? "");
            HttpContext.Session.SetString("Language", result.Data.User.PreferredLanguage);

            var perms = Helpers.PermissionHelper.ParseFromJwt(result.Data.AccessToken);
            HttpContext.Session.SetString("UserPermissions", System.Text.Json.JsonSerializer.Serialize(perms.ToList()));

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Workspace", "Home");
        }

        ModelState.AddModelError("", result?.Message ?? "Identifiants incorrects.");
        return View(dto);
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    public IActionResult AccessDenied() => View();
}
