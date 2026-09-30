using MetaCompta.BackOffice.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MetaCompta.BackOffice.Filters;

/// <summary>
/// Ensures the session WorkspaceMode matches one of the allowed métier modes.
/// Redirects to ClientSpaces when the user is in the wrong space (keeps client selected).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class RequireWorkspaceModeAttribute : ActionFilterAttribute
{
    private readonly string[] _allowed;

    public RequireWorkspaceModeAttribute(params string[] allowedModes)
    {
        _allowed = allowedModes?.Length > 0
            ? allowedModes.Select(WorkspaceModes.Normalize).Distinct().ToArray()
            : Array.Empty<string>();
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;
        var mode = WorkspaceModes.Normalize(session.GetString("WorkspaceMode"));
        var hasClient = !string.IsNullOrEmpty(session.GetString("SelectedClientId"));

        // Auto-map legacy Client → Fiscal
        if (string.Equals(session.GetString("WorkspaceMode"), WorkspaceModes.LegacyClient, StringComparison.OrdinalIgnoreCase))
        {
            session.SetString("WorkspaceMode", WorkspaceModes.Fiscal);
            mode = WorkspaceModes.Fiscal;
        }

        // Soft enter: client selected but still on Cabinet / empty → adopt first allowed mode
        if (hasClient && (string.IsNullOrEmpty(session.GetString("WorkspaceMode")) || mode == WorkspaceModes.Cabinet)
            && _allowed.Length > 0)
        {
            session.SetString("WorkspaceMode", _allowed[0]);
            mode = _allowed[0];
        }

        if (_allowed.Length == 0 || _allowed.Contains(mode, StringComparer.OrdinalIgnoreCase))
        {
            base.OnActionExecuting(context);
            return;
        }

        if (!hasClient)
        {
            context.Result = new RedirectToActionResult("Workspace", "Home", null);
            return;
        }

        context.Result = new RedirectToActionResult("ClientSpaces", "Home", null);
    }
}
