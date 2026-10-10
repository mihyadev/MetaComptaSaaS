using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MetaCompta.Application.DTOs;

namespace MetaCompta.BackOffice.Services;

/// <summary>
/// Generic typed HTTP client implementation for the MetaCompta API.
/// Automatically injects JWT token from session, serializes requests/responses,
/// and provides consistent error handling with detailed error messages.
/// </summary>
public sealed class ApiClient : IApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly ILogger<ApiClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ApiClient(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor contextAccessor,
        ILogger<ApiClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _contextAccessor = contextAccessor;
        _logger = logger;
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient("MetaComptaAPI");
        var token = _contextAccessor.HttpContext?.Session.GetString("JwtToken");
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public async Task<ApiResponse<T>?> GetAsync<T>(string url, CancellationToken ct = default)
    {
        try
        {
            var client = CreateClient();
            var response = await client.GetAsync(url, ct);
            return await DeserializeAsync<T>(response, url, "GET", ct);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API GET connection failed: {Url}", url);
            return ApiResponse<T>.Fail($"Impossible de contacter l'API ({url}): {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "API GET timeout: {Url}", url);
            return ApiResponse<T>.Fail($"Délai d'attente dépassé pour l'API ({url}). Veuillez réessayer.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API GET failed: {Url}", url);
            return ApiResponse<T>.Fail($"Erreur inattendue (GET {url}): {ex.Message}");
        }
    }

    public async Task<byte[]?> GetBytesAsync(string url, CancellationToken ct = default)
    {
        try
        {
            var client = CreateClient();
            var response = await client.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("API GET bytes failed: {Url} → HTTP {Status}", url, (int)response.StatusCode);
                return null;
            }
            return await response.Content.ReadAsByteArrayAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API GET bytes failed: {Url}", url);
            return null;
        }
    }

    public async Task<ApiResponse<T>?> PostAsync<T>(string url, object data, CancellationToken ct = default)
    {
        try
        {
            var client = CreateClient();
            var json = JsonSerializer.Serialize(data, JsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content, ct);
            return await DeserializeAsync<T>(response, url, "POST", ct);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API POST connection failed: {Url}", url);
            return ApiResponse<T>.Fail($"Impossible de contacter l'API ({url}): {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "API POST timeout: {Url}", url);
            return ApiResponse<T>.Fail($"Délai d'attente dépassé pour l'API ({url}). Veuillez réessayer.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API POST failed: {Url}", url);
            return ApiResponse<T>.Fail($"Erreur inattendue (POST {url}): {ex.Message}");
        }
    }

    public async Task<ApiResponse<T>?> PutAsync<T>(string url, object data, CancellationToken ct = default)
    {
        try
        {
            var client = CreateClient();
            var json = JsonSerializer.Serialize(data, JsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PutAsync(url, content, ct);
            return await DeserializeAsync<T>(response, url, "PUT", ct);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API PUT connection failed: {Url}", url);
            return ApiResponse<T>.Fail($"Impossible de contacter l'API ({url}): {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "API PUT timeout: {Url}", url);
            return ApiResponse<T>.Fail($"Délai d'attente dépassé pour l'API ({url}). Veuillez réessayer.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API PUT failed: {Url}", url);
            return ApiResponse<T>.Fail($"Erreur inattendue (PUT {url}): {ex.Message}");
        }
    }

    public async Task<ApiResponse<T>?> DeleteAsync<T>(string url, CancellationToken ct = default)
    {
        try
        {
            var client = CreateClient();
            var response = await client.DeleteAsync(url, ct);
            return await DeserializeAsync<T>(response, url, "DELETE", ct);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API DELETE connection failed: {Url}", url);
            return ApiResponse<T>.Fail($"Impossible de contacter l'API ({url}): {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "API DELETE timeout: {Url}", url);
            return ApiResponse<T>.Fail($"Délai d'attente dépassé pour l'API ({url}). Veuillez réessayer.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API DELETE failed: {Url}", url);
            return ApiResponse<T>.Fail($"Erreur inattendue (DELETE {url}): {ex.Message}");
        }
    }

    /// <summary>
    /// Deserializes the HTTP response into an ApiResponse, with comprehensive error handling.
    /// Handles: HTTP error status codes, empty bodies, ProblemDetails, validation errors,
    /// plain text errors, and JSON deserialization failures.
    /// </summary>
    private async Task<ApiResponse<T>?> DeserializeAsync<T>(HttpResponseMessage response, string url, string method, CancellationToken ct)
    {
        var statusCode = (int)response.StatusCode;
        string body;

        try
        {
            body = await response.Content.ReadAsStringAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read response body from {Method} {Url}", method, url);
            return ApiResponse<T>.Fail($"Impossible de lire la réponse de l'API (HTTP {statusCode}): {ex.Message}");
        }

        // ── Empty body ───────────────────────────────────────────────
        if (string.IsNullOrWhiteSpace(body))
        {
            if (response.IsSuccessStatusCode)
            {
                // Some successful responses (e.g. 204 No Content) have no body — that's OK
                return ApiResponse<T>.Ok(default!, "Opération réussie.");
            }

            var reason = response.ReasonPhrase ?? "Erreur inconnue";
            _logger.LogWarning("Empty error response: {Method} {Url} → HTTP {StatusCode} {Reason}", method, url, statusCode, reason);
            if (statusCode >= 500)
                return ApiResponse<T>.Fail("Impossible d'enregistrer pour le moment. Vérifiez les informations saisies puis réessayez.");
            return ApiResponse<T>.Fail("La requête a échoué. Vérifiez les informations saisies puis réessayez.");
        }

        // ── Try to deserialize as ApiResponse<T> (our standard envelope) ─
        if (body.TrimStart().StartsWith('{') || body.TrimStart().StartsWith('['))
        {
            try
            {
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(body, JsonOptions);
                if (apiResponse != null)
                {
                    // If the HTTP status is an error but the JSON says success, trust HTTP status
                    if (!response.IsSuccessStatusCode && apiResponse.Success)
                    {
                        apiResponse.Success = false;
                        apiResponse.Message ??= $"Erreur HTTP {statusCode}";
                    }

                    // Enrich message with errors list if present
                    if (!apiResponse.Success && apiResponse.Errors?.Count > 0 && string.IsNullOrEmpty(apiResponse.Message))
                    {
                        apiResponse.Message = string.Join(" | ", apiResponse.Errors);
                    }

                    return apiResponse;
                }
            }
            catch (JsonException)
            {
                // Not our ApiResponse format — continue to try other formats
            }

            // ── Try ASP.NET ProblemDetails / ValidationProblemDetails ─────
            try
            {
                var problemDoc = JsonSerializer.Deserialize<JsonElement>(body, JsonOptions);
                var errors = new List<string>();
                string? title = null;
                string? detail = null;

                if (problemDoc.TryGetProperty("title", out var titleProp))
                    title = titleProp.GetString();
                if (problemDoc.TryGetProperty("detail", out var detailProp))
                    detail = detailProp.GetString();

                // Extract validation errors from "errors" object
                if (problemDoc.TryGetProperty("errors", out var errorsProp) && errorsProp.ValueKind == JsonValueKind.Object)
                {
                    foreach (var field in errorsProp.EnumerateObject())
                    {
                        if (field.Value.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var errMsg in field.Value.EnumerateArray())
                            {
                                var fieldName = field.Name == "" ? "" : $"[{field.Name}] ";
                                errors.Add($"{fieldName}{errMsg.GetString()}");
                            }
                        }
                    }
                }

                // Build a meaningful message
                var message = detail ?? title ?? $"Erreur HTTP {statusCode}";
                if (errors.Count > 0)
                    message = $"{message} — {string.Join(" | ", errors)}";

                _logger.LogWarning("API error response: {Method} {Url} → HTTP {StatusCode}: {Message}", method, url, statusCode, message);
                if (statusCode >= 500)
                    return ApiResponse<T>.Fail("Impossible d'enregistrer pour le moment. Vérifiez les informations saisies (champs obligatoires, identifiants uniques) puis réessayez.", errors.Count > 0 ? errors : null);
                return ApiResponse<T>.Fail(message, errors.Count > 0 ? errors : null);
            }
            catch (JsonException)
            {
                // Not valid JSON at all — fall through to plain text
            }
        }

        // ── Non-success with plain text body ─────────────────────────
        if (!response.IsSuccessStatusCode)
        {
            var truncatedBody = body.Length > 500 ? body[..500] + "…" : body;
            _logger.LogWarning("API plain text error: {Method} {Url} → HTTP {StatusCode}: {Body}", method, url, statusCode, truncatedBody);
            if (statusCode >= 500)
                return ApiResponse<T>.Fail("Impossible d'enregistrer. Vérifiez les informations saisies puis réessayez.");
            return ApiResponse<T>.Fail(truncatedBody.Length > 200 ? truncatedBody[..200] + "…" : truncatedBody);
        }

        // ── Success but body isn't parseable as ApiResponse<T> ───────
        _logger.LogWarning("API returned unparseable success body: {Method} {Url} → HTTP {StatusCode}", method, url, statusCode);
        return ApiResponse<T>.Fail("Réponse inattendue de l'API. Veuillez réessayer.");
    }
}
