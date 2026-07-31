using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MetaCompta.Application.DTOs;
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
        var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
        var resp = await CreateClient().PostAsync(url, content);
        if (!resp.IsSuccessStatusCode) return default;
        var json = await resp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TResult>(json, _jsonOpts);
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
        }
        await next();
    }
}
