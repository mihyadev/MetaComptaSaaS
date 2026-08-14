using System.Collections.Concurrent;
using MetaCompta.Application.Interfaces;
using MetaCompta.Domain.Common;
using MetaCompta.Domain.Entities;
using MetaCompta.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MetaCompta.Application.Services;

public sealed class EffectivePermissionService : IEffectivePermissionService
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(10);
    private static readonly ConcurrentDictionary<string, (DateTime Expires, IReadOnlyCollection<string> Perms)> Cache = new(StringComparer.OrdinalIgnoreCase);
    private static int _version;

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly MetaComptaDbContext _db;

    public EffectivePermissionService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        MetaComptaDbContext db)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
    }

    public async Task<IReadOnlyCollection<string>> GetEffectivePermissionsAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Array.Empty<string>();

        var cacheKey = CacheKey(userId);
        if (Cache.TryGetValue(cacheKey, out var hit) && hit.Expires > DateTime.UtcNow)
            return hit.Perms;

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || !user.IsActive)
        {
            IReadOnlyCollection<string> empty = Array.Empty<string>();
            Cache[cacheKey] = (DateTime.UtcNow.Add(CacheTtl), empty);
            return empty;
        }

        // Union direct user claims + role matrix (direct alone must not hide newer role claims like comptabilite.cloture)
        var direct = await GetDirectClaimsAsync(user);
        var fromRoles = await GetRoleClaimsAsync(user);
        IReadOnlyCollection<string> effective = direct
            .Concat(fromRoles)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(p => p)
            .ToList();

        Cache[cacheKey] = (DateTime.UtcNow.Add(CacheTtl), effective);
        return effective;
    }

    public void InvalidateUser(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return;
        Cache.TryRemove(CacheKey(userId), out _);
    }

    public void InvalidateAll()
    {
        Interlocked.Increment(ref _version);
        Cache.Clear();
    }

    private static string CacheKey(string userId) => $"{_version}:{userId}";

    private async Task<List<string>> GetDirectClaimsAsync(ApplicationUser user)
    {
        var claims = await _userManager.GetClaimsAsync(user);
        return claims
            .Where(c => string.Equals(c.Type, "Permission", StringComparison.OrdinalIgnoreCase)
                        && !string.IsNullOrWhiteSpace(c.Value))
            .Select(c => c.Value.Trim().ToLowerInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(p => p)
            .ToList();
    }

    private async Task<List<string>> GetRoleClaimsAsync(ApplicationUser user)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var roles = await _userManager.GetRolesAsync(user);
        foreach (var roleName in roles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null) continue;

            var names = await _db.RolePermissions
                .Where(rp => rp.RoleId == role.Id)
                .Include(rp => rp.Permission)
                .Select(rp => rp.Permission!.Name)
                .ToListAsync();

            foreach (var n in names.Where(n => !string.IsNullOrWhiteSpace(n)))
                result.Add(n.Trim().ToLowerInvariant());
        }

        return result.OrderBy(p => p).ToList();
    }
}
