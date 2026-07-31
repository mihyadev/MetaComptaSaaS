using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MetaCompta.Domain.Common;

namespace MetaCompta.BackOffice.Helpers;

/// <summary>
/// Reads Permission claims from the session JWT (role-linked claims only).
/// </summary>
public static class PermissionHelper
{
    public const string ClaimType = "Permission";

    public static HashSet<string> GetPermissions(ISession session)
    {
        var cached = session.GetString("UserPermissions");
        if (!string.IsNullOrEmpty(cached))
        {
            try
            {
                var list = System.Text.Json.JsonSerializer.Deserialize<List<string>>(cached);
                return new HashSet<string>(list ?? new List<string>(), StringComparer.OrdinalIgnoreCase);
            }
            catch { /* fall through to JWT parse */ }
        }

        var token = session.GetString("JwtToken");
        var perms = ParseFromJwt(token);
        session.SetString("UserPermissions", System.Text.Json.JsonSerializer.Serialize(perms.ToList()));
        return perms;
    }

    public static bool HasPermission(ISession session, string permission)
        => GetPermissions(session).Contains(permission);

    public static bool HasPermission(ISession session, params string[] permissions)
    {
        var set = GetPermissions(session);
        return permissions.Any(p => set.Contains(p));
    }

    /// <summary>Replace cached session permissions with a live list from the API/DB.</summary>
    public static void SetPermissions(ISession session, IEnumerable<string> permissions)
    {
        var list = permissions?
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? new List<string>();
        session.SetString("UserPermissions", System.Text.Json.JsonSerializer.Serialize(list));
    }

    /// <summary>True if the user has any claim in the given domain prefixes (e.g. "comptabilite", "tva").</summary>
    public static bool HasAnyDomain(ISession session, params string[] domains)
    {
        var set = GetPermissions(session);
        return set.Any(p => domains.Any(d =>
            p.Equals(d, StringComparison.OrdinalIgnoreCase)
            || p.StartsWith(d + ".", StringComparison.OrdinalIgnoreCase)));
    }

    public static HashSet<string> ParseFromJwt(string? token)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(token)) return result;

        try
        {
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(token)) return result;
            var jwt = handler.ReadJwtToken(token);
            foreach (var claim in jwt.Claims.Where(c =>
                c.Type == ClaimType || c.Type == "permission" || c.Type == ClaimTypes.Role))
            {
                // Only Permission claim type for CRUD; Role is separate
                if (string.Equals(claim.Type, ClaimType, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(claim.Type, "permission", StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrWhiteSpace(claim.Value))
                        result.Add(claim.Value);
                }
            }
        }
        catch { /* ignore malformed token */ }

        return result;
    }

    // Convenience constants mirroring ClaimDefinitions
    public static class Declaration
    {
        public const string View = ClaimDefinitions.DeclarationView;
        public const string Create = ClaimDefinitions.DeclarationCreate;
        public const string Edit = ClaimDefinitions.DeclarationEdit;
        public const string Validate = ClaimDefinitions.DeclarationValidate;
        public const string Submit = ClaimDefinitions.DeclarationSubmit;
        public const string Delete = ClaimDefinitions.DeclarationDelete;
    }
}
