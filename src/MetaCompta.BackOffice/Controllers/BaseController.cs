using MetaCompta.Application.DTOs;
using MetaCompta.BackOffice.Filters;
using MetaCompta.BackOffice.Services;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.BackOffice.Controllers;

/// <summary>
/// Base controller providing access to facade services, session helpers,
/// and reference data loading. Authentication is handled by [RequireAuthentication] filter.
/// </summary>
[RequireAuthentication]
public abstract class BaseController : Controller
{
    protected readonly IReferenceDataFacade ReferenceData;
    private readonly IHttpContextAccessor _contextAccessor;

    protected BaseController(IReferenceDataFacade referenceData, IHttpContextAccessor contextAccessor)
    {
        ReferenceData = referenceData;
        _contextAccessor = contextAccessor;
    }

    // ── Session helpers ─────────────────────────────────────────
    protected string? Token => _contextAccessor.HttpContext?.Session.GetString("JwtToken");
    protected string? UserId => _contextAccessor.HttpContext?.Session.GetString("UserId");
    protected string? UserName => _contextAccessor.HttpContext?.Session.GetString("UserName");
    protected string? UserRole => _contextAccessor.HttpContext?.Session.GetString("UserRole");
    protected string? CabinetName => _contextAccessor.HttpContext?.Session.GetString("CabinetName");
    protected int? CabinetId => int.TryParse(_contextAccessor.HttpContext?.Session.GetString("CabinetId"), out var id) ? id : null;
    protected bool IsAuthenticated => !string.IsNullOrEmpty(Token);

    // ── Global Client Context (selected in topbar) ──────────────
    protected int? SelectedClientId => int.TryParse(_contextAccessor.HttpContext?.Session.GetString("SelectedClientId"), out var cid) ? cid : null;
    protected string? SelectedClientName => _contextAccessor.HttpContext?.Session.GetString("SelectedClientName");

    // ── Reference data helpers ──────────────────────────────────

    /// <summary>
    /// Load reference data and populate ViewData with standard naming: ViewData["Ref_{Category}"]
    /// Silently handles API errors so views still render even without reference data.
    /// </summary>
    protected async Task LoadReferenceDataToViewBagAsync(params string[] categories)
    {
        try
        {
            var data = await ReferenceData.GetMultipleAsync(categories);
            foreach (var kv in data)
            {
                ViewData[$"Ref_{kv.Key}"] = kv.Value;
            }
        }
        catch (Exception)
        {
            // Reference data unavailable – views will use fallback values
        }
    }

    // ── Error helper ────────────────────────────────────────────

    /// <summary>
    /// Sets TempData["Error"] with the main message and TempData["ErrorDetails"] with
    /// any additional error details from ApiResponse.Errors, joined by "|||".
    /// Use this instead of manually setting TempData["Error"] = result?.Message.
    /// </summary>
    protected void SetApiError<T>(ApiResponse<T>? result, string? fallbackMessage = null)
    {
        var message = result?.Message ?? fallbackMessage ?? "Une erreur inattendue s'est produite.";
        TempData["Error"] = message;

        if (result?.Errors?.Count > 0)
        {
            TempData["ErrorDetails"] = string.Join("|||", result.Errors);
        }
    }

    /// <summary>
    /// Sets TempData["Success"] or TempData["Error"] (with error details) based on result.
    /// Replaces the common pattern: TempData[result?.Success == true ? "Success" : "Error"] = result?.Message;
    /// </summary>
    protected void SetApiResult<T>(ApiResponse<T>? result, string? successFallback = null, string? errorFallback = null)
    {
        if (result?.Success == true)
        {
            TempData["Success"] = result.Message ?? successFallback;
        }
        else
        {
            SetApiError(result, errorFallback);
        }
    }
}
