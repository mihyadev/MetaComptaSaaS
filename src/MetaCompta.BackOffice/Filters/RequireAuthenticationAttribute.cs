using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MetaCompta.BackOffice.Filters;

/// <summary>
/// Action filter that automatically redirects to the login page if
/// no JWT token is found in the session. Apply globally or per-controller
/// to eliminate repetitive authentication checks in every action method.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class RequireAuthenticationAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Skip authentication check if [SkipAuthentication] is present
        var hasSkip = context.ActionDescriptor.EndpointMetadata
            .OfType<SkipAuthenticationAttribute>().Any();
        if (hasSkip)
        {
            base.OnActionExecuting(context);
            return;
        }

        var session = context.HttpContext.Session;
        var token = session.GetString("JwtToken");

        if (string.IsNullOrEmpty(token))
        {
            // For AJAX requests, return 401 instead of redirect
            var isAjax = context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest"
                      || context.HttpContext.Request.Headers["Accept"].ToString().Contains("application/json");

            if (isAjax)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
            context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl });
            return;
        }

        base.OnActionExecuting(context);
    }
}

/// <summary>
/// Marks an action to skip the [RequireAuthentication] check.
/// Use on actions that must remain accessible without login (e.g. Error pages).
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class SkipAuthenticationAttribute : Attribute { }
