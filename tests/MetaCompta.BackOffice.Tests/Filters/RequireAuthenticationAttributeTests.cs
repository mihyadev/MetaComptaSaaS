using MetaCompta.BackOffice.Tests.Helpers;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace MetaCompta.BackOffice.Tests.Filters;

/// <summary>
/// Tests for RequireAuthenticationAttribute — verifies redirect behavior and SkipAuthentication.
/// </summary>
public class RequireAuthenticationAttributeTests
{
    private static ActionExecutingContext CreateContext(bool hasToken, bool hasSkipAttribute = false)
    {
        var session = new MockSession();
        if (hasToken)
            session.SetString("JwtToken", "test-jwt-token");

        var httpContext = new DefaultHttpContext { Session = session };
        httpContext.Request.Path = "/Clients/Index";
        httpContext.Request.QueryString = new QueryString("?page=1");

        var actionDescriptor = new ActionDescriptor();
        if (hasSkipAttribute)
        {
            actionDescriptor.EndpointMetadata = new List<object> { new SkipAuthenticationAttribute() };
        }
        else
        {
            actionDescriptor.EndpointMetadata = new List<object>();
        }

        var actionContext = new ActionContext(httpContext, new RouteData(), actionDescriptor);

        return new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            controller: null!);
    }

    [Fact]
    public void OnActionExecuting_WithToken_Passes()
    {
        var filter = new RequireAuthenticationAttribute();
        var context = CreateContext(hasToken: true);

        filter.OnActionExecuting(context);

        context.Result.Should().BeNull("authenticated users should not be redirected");
    }

    [Fact]
    public void OnActionExecuting_WithoutToken_RedirectsToLogin()
    {
        var filter = new RequireAuthenticationAttribute();
        var context = CreateContext(hasToken: false);

        filter.OnActionExecuting(context);

        context.Result.Should().NotBeNull();
        var redirect = context.Result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ControllerName.Should().Be("Account");
        redirect.ActionName.Should().Be("Login");
        redirect.RouteValues.Should().ContainKey("returnUrl");
        redirect.RouteValues!["returnUrl"].Should().Be("/Clients/Index?page=1");
    }

    [Fact]
    public void OnActionExecuting_WithSkipAttribute_PassesEvenWithoutToken()
    {
        var filter = new RequireAuthenticationAttribute();
        var context = CreateContext(hasToken: false, hasSkipAttribute: true);

        filter.OnActionExecuting(context);

        context.Result.Should().BeNull("SkipAuthentication should bypass the check");
    }
}
