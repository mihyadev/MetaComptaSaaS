using MetaCompta.Domain.Common;

namespace MetaCompta.Application.Interfaces;

/// <summary>
/// Resolves the live effective permission set for a user from the database
/// (not from a stale JWT). Used by API authorization and token issuance.
/// </summary>
public interface IEffectivePermissionService
{
    /// <summary>
    /// Effective claims for authorization.
    /// If the user has any direct Permission claims → those alone (employee override from Edit UI).
    /// Otherwise → role RolePermissions (seeded users not yet customized).
    /// Inactive users → empty set.
    /// </summary>
    Task<IReadOnlyCollection<string>> GetEffectivePermissionsAsync(string userId);

    void InvalidateUser(string userId);
    void InvalidateAll();
}
