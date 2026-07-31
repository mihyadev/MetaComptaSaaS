using MetaCompta.BackOffice.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MetaCompta.BackOffice.Filters;

/// <summary>
/// Requires at least one of the given Permission claims in the session JWT.
/// Apply on MVC actions so BackOffice cannot bypass API micro-permissions.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class RequirePermissionAttribute : ActionFilterAttribute
{
    private readonly string[] _permissions;
    private readonly bool _requireAll;

    public RequirePermissionAttribute(params string[] permissions)
        : this(false, permissions)
    {
    }

    public RequirePermissionAttribute(bool requireAll, params string[] permissions)
    {
        _permissions = permissions ?? Array.Empty<string>();
        _requireAll = requireAll;
        Order = 50; // after authentication
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (_permissions.Length == 0)
        {
            base.OnActionExecuting(context);
            return;
        }

        var session = context.HttpContext.Session;
        var ok = _requireAll
            ? _permissions.All(p => PermissionHelper.HasPermission(session, p))
            : PermissionHelper.HasPermission(session, _permissions);

        if (ok)
        {
            base.OnActionExecuting(context);
            return;
        }

        var message = _permissions.Length == 1
            ? $"Permission '{_permissions[0]}' requise pour cette action."
            : $"Permissions requises : {string.Join(" ou ", _permissions)}.";

        var isAjax = context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest"
            || context.HttpContext.Request.Headers["Accept"].ToString().Contains("application/json");

        if (isAjax)
        {
            context.Result = new JsonResult(new { success = false, message }) { StatusCode = 403 };
            return;
        }

        if (context.Controller is Controller controller)
            controller.TempData["Error"] = message;

        var referer = context.HttpContext.Request.Headers["Referer"].ToString();
        if (!string.IsNullOrWhiteSpace(referer)
            && Uri.TryCreate(referer, UriKind.Absolute, out var uri)
            && string.Equals(uri.Host, context.HttpContext.Request.Host.Host, StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new RedirectResult(referer);
            return;
        }

        context.Result = new RedirectToActionResult("Index", "Home", null);
    }
}
