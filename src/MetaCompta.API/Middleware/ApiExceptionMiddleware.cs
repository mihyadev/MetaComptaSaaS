using System.Net;
using System.Text.Json;
using MetaCompta.Application.DTOs;
using MetaCompta.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace MetaCompta.API.Middleware;

/// <summary>
/// Converts unhandled exceptions into ApiResponse JSON with actionable French messages
/// instead of opaque HTTP 500 responses.
/// </summary>
public sealed class ApiExceptionMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ApiExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await WriteErrorAsync(context, ex);
        }
    }

    private async Task WriteErrorAsync(HttpContext context, Exception ex)
    {
        if (context.Response.HasStarted)
        {
            _logger.LogError(ex, "Unhandled exception after response started");
            throw ex;
        }

        var (status, message, errors) = MapException(ex);
        _logger.LogError(ex, "API error → HTTP {Status}: {Message}", (int)status, message);

        context.Response.Clear();
        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/json; charset=utf-8";

        var payload = ApiResponse<object>.Fail(message, errors);
        if (_env.IsDevelopment() && status == HttpStatusCode.InternalServerError)
        {
            payload.Errors ??= new List<string>();
            payload.Errors.Add(ex.GetType().Name + ": " + Truncate(ex.Message, 300));
        }

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
    }

    private static (HttpStatusCode Status, string Message, List<string> Errors) MapException(Exception ex)
    {
        if (ex is DbUpdateException dbEx)
        {
            var mapped = DbErrorMapper.Map(dbEx);
            return (HttpStatusCode.BadRequest, mapped.Message, mapped.Errors);
        }

        if (ex is ArgumentException or InvalidOperationException or FormatException)
        {
            return (HttpStatusCode.BadRequest, ex.Message, new List<string> { ex.Message });
        }

        if (ex is UnauthorizedAccessException)
        {
            return (HttpStatusCode.Forbidden, "Accès non autorisé.", new List<string> { ex.Message });
        }

        // Unwrap common wrappers
        if (ex.InnerException is DbUpdateException innerDb)
            return MapException(innerDb);

        return (
            HttpStatusCode.InternalServerError,
            "Une erreur inattendue s'est produite. Réessayez ou contactez le support si le problème continue.",
            new List<string>());
    }

    private static string Truncate(string? text, int max)
        => string.IsNullOrEmpty(text) ? "" : text.Length <= max ? text : text[..max] + "…";
}
