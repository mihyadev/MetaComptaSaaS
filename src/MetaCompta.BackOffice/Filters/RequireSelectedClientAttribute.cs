using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MetaCompta.BackOffice.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class RequireSelectedClientAttribute : ActionFilterAttribute
{
    public bool RedirectIfMissing { get; set; } = true;

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;
        var selectedClientIdStr = session.GetString("SelectedClientId");
        var hasClient = int.TryParse(selectedClientIdStr, out var selectedClientId);

        if (!hasClient && RedirectIfMissing)
        {
            context.Result = new RedirectToActionResult("Workspace", "Home", null);
            return;
        }

        if (hasClient && context.ActionArguments.ContainsKey("clientId"))
        {
            context.ActionArguments["clientId"] = (int?)selectedClientId;
        }

        base.OnActionExecuting(context);
    }
}
