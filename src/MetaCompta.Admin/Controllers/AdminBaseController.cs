using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.Admin.Controllers;

public abstract class AdminBaseController : Controller
{
    protected readonly IHttpClientFactory _httpFactory;
    protected readonly JsonSerializerOptions _jsonOpts = new() { PropertyNameCaseInsensitive = true };

    protected AdminBaseController(IHttpClientFactory httpFactory) => _httpFactory = httpFactory;

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

    protected async Task<bool> PostAsync<T>(string url, T data)
    {
        var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
        var resp = await CreateClient().PostAsync(url, content);
        return resp.IsSuccessStatusCode;
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
}
