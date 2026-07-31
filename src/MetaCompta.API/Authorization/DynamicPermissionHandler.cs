using MetaCompta.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MetaCompta.API.Authorization;

public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }
    public PermissionRequirement(string permission) => Permission = permission;
}

/// <summary>
/// Authorizes against live DB permissions (role / employee claims), not stale JWT Permission claims.
/// Removing a permission from an employee or role takes effect within the permission cache TTL.
/// </summary>
public sealed class DynamicPermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IEffectivePermissionService _permissions;

    public DynamicPermissionHandler(IEffectivePermissionService permissions)
        => _permissions = permissions;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(userId))
            return;

        var effective = await _permissions.GetEffectivePermissionsAsync(userId);
        if (effective.Any(p => string.Equals(p, requirement.Permission, StringComparison.OrdinalIgnoreCase)))
        {
            context.Succeed(requirement);
        }
    }
}
