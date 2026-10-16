using MetaCompta.Application.DTOs;
using MetaCompta.Application.Interfaces;
using MetaCompta.Domain.Common;
using MetaCompta.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MetaCompta.Application.Services;

/// <summary>
/// Service for cabinet management: employee CRUD, roles/permissions, cabinet info, and user profiles.
/// Follows SOLID principles — single responsibility for cabinet-scoped operations.
/// </summary>
public class CabinetService : ICabinetService
{
    private readonly MetaComptaDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IEffectivePermissionService _permissions;
    private readonly IAccountInvitationService _invitations;

    public CabinetService(
        MetaComptaDbContext db,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IEffectivePermissionService permissions,
        IAccountInvitationService invitations)
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
        _permissions = permissions;
        _invitations = invitations;
    }

    // ═══════════════════════════════════════════════════════════
    // CABINET INFO
    // ═══════════════════════════════════════════════════════════

    public async Task<ApiResponse<CabinetDetailDto>> GetCabinetAsync(int cabinetId)
    {
        var cabinet = await _db.Cabinets
            .Include(c => c.Users)
            .Include(c => c.Clients)
            .Include(c => c.SubscriptionPlan)
            .FirstOrDefaultAsync(c => c.Id == cabinetId);

        if (cabinet == null)
            return ApiResponse<CabinetDetailDto>.Fail("Cabinet non trouvé.");

        return ApiResponse<CabinetDetailDto>.Ok(new CabinetDetailDto
        {
            Id = cabinet.Id,
            Name = cabinet.Name,
            ICE = cabinet.ICE,
            IF = cabinet.IF,
            RC = cabinet.RC,
            Address = cabinet.Address,
            City = cabinet.City,
            Phone = cabinet.Phone,
            Email = cabinet.Email,
            LogoUrl = cabinet.LogoUrl,
            IsActive = cabinet.IsActive,
            CreatedAt = cabinet.CreatedAt,
            EmployeeCount = cabinet.Users.Count(u => u.IsActive),
            ClientCount = cabinet.Clients.Count,
            SubscriptionPlanName = cabinet.SubscriptionPlan?.Name,
            SubscriptionEndDate = cabinet.SubscriptionEndDate
        });
    }

    public async Task<ApiResponse<CabinetDetailDto>> UpdateCabinetAsync(int cabinetId, CabinetEditDto dto, string userId)
    {
        var cabinet = await _db.Cabinets.FindAsync(cabinetId);
        if (cabinet == null)
            return ApiResponse<CabinetDetailDto>.Fail("Cabinet non trouvé.");

        cabinet.Name = dto.Name;
        cabinet.ICE = dto.ICE;
        cabinet.IF = dto.IF;
        cabinet.RC = dto.RC;
        cabinet.Address = dto.Address;
        cabinet.City = dto.City;
        cabinet.Phone = dto.Phone;
        cabinet.Email = dto.Email;
        cabinet.LogoUrl = dto.LogoUrl;
        cabinet.UpdatedAt = DateTime.UtcNow;
        cabinet.UpdatedBy = userId;

        await _db.SaveChangesAsync();
        await LogAuditAsync(cabinetId, userId, "Modification", "Cabinet", $"Cabinet '{cabinet.Name}' modifié.");

        return await GetCabinetAsync(cabinetId);
    }

    // ═══════════════════════════════════════════════════════════
    // CABINET EMPLOYEES (BackOffice Users)
    // ═══════════════════════════════════════════════════════════

    public async Task<ApiResponse<PagedResult<CabinetEmployeeDto>>> GetEmployeesAsync(
        int cabinetId, string? departement = null, int page = 1, int pageSize = 20, string? search = null)
    {
        // Cabinet staff only — Extranet client users (ClientId set) must not appear in employee pickers.
        var query = _db.Users.Where(u => u.CabinetId == cabinetId && u.ClientId == null);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(s) ||
                u.LastName.ToLower().Contains(s) ||
                u.Email!.ToLower().Contains(s));
        }

        var totalCount = await query.CountAsync();
        var users = await query
            .OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

        var items = new List<CabinetEmployeeDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var permissions = await GetUserPermissionsAsync(user);

            items.Add(new CabinetEmployeeDto
            {
                Id = user.Id,
                Email = user.Email ?? "",
                FirstName = user.FirstName,
                LastName = user.LastName,
                Telephone = user.PhoneNumber,
                Poste = user.Poste,
                Departement = user.Departement,
                CIN = user.CIN,
                Role = roles.FirstOrDefault(),
                EstActif = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                DateEmbauche = user.DateEmbauche ?? user.CreatedAt,
                DerniereConnexion = user.LastLoginAt,
                ProfileImageUrl = user.ProfileImageUrl,
                PreferredLanguage = user.PreferredLanguage,
                Permissions = permissions
            });
        }

        // Apply departement filter on the DTO side (departement is not a DB column on ApplicationUser)
        if (!string.IsNullOrWhiteSpace(departement))
        {
            items = items.Where(e => string.Equals(e.Departement, departement, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return ApiResponse<PagedResult<CabinetEmployeeDto>>.Ok(new PagedResult<CabinetEmployeeDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<CabinetEmployeeDto>> GetEmployeeByIdAsync(string id, int cabinetId)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.Id == id && u.CabinetId == cabinetId && u.ClientId == null);
        if (user == null)
            return ApiResponse<CabinetEmployeeDto>.Fail("Employé non trouvé.");

        var roles = await _userManager.GetRolesAsync(user);
        var permissions = await GetUserPermissionsAsync(user);

        return ApiResponse<CabinetEmployeeDto>.Ok(new CabinetEmployeeDto
        {
            Id = user.Id,
            Email = user.Email ?? "",
            FirstName = user.FirstName,
            LastName = user.LastName,
            Telephone = user.PhoneNumber,
            Role = roles.FirstOrDefault(),
            Poste = user.Poste,
            Departement = user.Departement,
            CIN = user.CIN,
            Adresse = user.Adresse,
            DateNaissance = user.DateNaissance,
            LieuNaissance = user.LieuNaissance,
            Sexe = user.Sexe,
            SituationFamiliale = user.SituationFamiliale,
            NumeroCNSS = user.NumeroCNSS,
            TypeContrat = user.TypeContrat,
            EstActif = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            DateEmbauche = user.DateEmbauche ?? user.CreatedAt,
            DerniereConnexion = user.LastLoginAt,
            ProfileImageUrl = user.ProfileImageUrl,
            PreferredLanguage = user.PreferredLanguage,
            Permissions = permissions
        });
    }

    public async Task<ApiResponse<CabinetEmployeeDto>> CreateEmployeeAsync(
        CabinetEmployeeCreateDto dto, int cabinetId, string userId)
    {
        // Validate email uniqueness
        var existing = await _userManager.FindByEmailAsync(dto.Email);
        if (existing != null)
            return ApiResponse<CabinetEmployeeDto>.Fail("Un utilisateur avec cet email existe déjà.");

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PhoneNumber = dto.Telephone,
            CIN = dto.CIN,
            Adresse = dto.Adresse,
            DateNaissance = dto.DateNaissance,
            LieuNaissance = dto.LieuNaissance,
            Sexe = dto.Sexe,
            SituationFamiliale = dto.SituationFamiliale,
            Poste = dto.Poste,
            Departement = dto.Departement,
            NumeroCNSS = dto.NumeroCNSS,
            DateEmbauche = dto.DateEmbauche ?? DateTime.UtcNow,
            TypeContrat = dto.TypeContrat,
            CabinetId = cabinetId,
            IsActive = false,
            EmailConfirmed = false,
            CreatedAt = DateTime.UtcNow,
            PreferredLanguage = "fr-FR"
        };

        // Invite flow: create without password; user sets it via activation email.
        var createResult = await _userManager.CreateAsync(user);
        if (!createResult.Succeeded)
            return ApiResponse<CabinetEmployeeDto>.Fail(
                "Erreur lors de la création du compte.",
                createResult.Errors.Select(e => e.Description).ToList());

        // Assign role
        if (!string.IsNullOrEmpty(dto.Role))
        {
            if (!await _roleManager.RoleExistsAsync(dto.Role))
                await _roleManager.CreateAsync(new ApplicationRole { Name = dto.Role, CabinetId = cabinetId });
            await _userManager.AddToRoleAsync(user, dto.Role);
        }

        // Assign permissions as exclusive employee claims (UI checkboxes).
        // If none selected, seed from role matrix so removals later are effective.
        var perms = dto.Permissions?.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
                    ?? new List<string>();
        if (perms.Count == 0 && !string.IsNullOrWhiteSpace(dto.Role))
        {
            if (string.Equals(dto.Role, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
                perms = ClaimDefinitions.All.ToList();
            else if (CabinetRolePermissionMatrix.ByRole.TryGetValue(dto.Role, out var matrix))
                perms = matrix.ToList();
        }
        await SyncUserPermissionsAsync(user, perms);

        var cabinet = await _db.Cabinets.FindAsync(cabinetId);
        var invite = await _invitations.SendActivationEmailAsync(
            user, ActivationPortal.BackOffice, cabinet?.Name ?? "Cabinet");

        await LogAuditAsync(cabinetId, userId, "Creation", "CabinetEmployee",
            $"Employé '{user.FullName}' ({user.Email}) créé avec le rôle '{dto.Role}' (invitation email).");

        var created = await GetEmployeeByIdAsync(user.Id, cabinetId);
        if (created.Success && created.Data != null)
        {
            created.Message = invite.Success
                ? "Employé créé. Un email d'activation a été envoyé."
                : invite.Message ?? "Employé créé, mais l'email d'activation a échoué.";
        }
        return created;
    }

    public async Task<ApiResponse<CabinetEmployeeDto>> UpdateEmployeeAsync(
        string id, CabinetEmployeeUpdateDto dto, int cabinetId, string userId)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.Id == id && u.CabinetId == cabinetId && u.ClientId == null);
        if (user == null)
            return ApiResponse<CabinetEmployeeDto>.Fail("Employé non trouvé.");

        // Update all info
        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.PhoneNumber = dto.Telephone;
        user.CIN = dto.CIN;
        user.Adresse = dto.Adresse;
        user.DateNaissance = dto.DateNaissance;
        user.LieuNaissance = dto.LieuNaissance;
        user.Sexe = dto.Sexe;
        user.SituationFamiliale = dto.SituationFamiliale;
        user.Poste = dto.Poste;
        user.Departement = dto.Departement;
        user.NumeroCNSS = dto.NumeroCNSS;
        user.DateEmbauche = dto.DateEmbauche;
        user.TypeContrat = dto.TypeContrat;
        user.IsActive = dto.IsActive;

        await _userManager.UpdateAsync(user);

        // Update role
        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Any())
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

        if (!string.IsNullOrEmpty(dto.Role))
        {
            if (!await _roleManager.RoleExistsAsync(dto.Role))
                await _roleManager.CreateAsync(new ApplicationRole { Name = dto.Role, CabinetId = cabinetId });
            await _userManager.AddToRoleAsync(user, dto.Role);
        }

        // Update permissions
        await SyncUserPermissionsAsync(user, dto.Permissions);

        await LogAuditAsync(cabinetId, userId, "Modification", "CabinetEmployee",
            $"Employé '{user.FullName}' modifié. Rôle: '{dto.Role}', Actif: {dto.IsActive}.");

        return await GetEmployeeByIdAsync(user.Id, cabinetId);
    }

    public async Task<ApiResponse<bool>> ToggleEmployeeActiveAsync(string id, int cabinetId, string userId)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.Id == id && u.CabinetId == cabinetId && u.ClientId == null);
        if (user == null)
            return ApiResponse<bool>.Fail("Employé non trouvé.");

        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);

        await LogAuditAsync(cabinetId, userId, "Modification", "CabinetEmployee",
            $"Employé '{user.FullName}' {(user.IsActive ? "activé" : "désactivé")}.");

        return ApiResponse<bool>.Ok(true, user.IsActive ? "Employé activé." : "Employé désactivé.");
    }

    public Task<ApiResponse<bool>> ResendEmployeeActivationAsync(string id, int cabinetId)
        => _invitations.ResendActivationAsync(id, cabinetId, ActivationPortal.BackOffice);

    // ═══════════════════════════════════════════════════════════
    // ROLES & PERMISSIONS
    // ═══════════════════════════════════════════════════════════

    public async Task<ApiResponse<PagedResult<RoleDto>>> GetRolesAsync(
        int cabinetId, int page = 1, int pageSize = 20, string? search = null)
    {
        var query = _roleManager.Roles
            .Where(r => r.CabinetId == null || r.CabinetId == cabinetId);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(r => r.Name!.Contains(search));

        var total = await query.CountAsync();
        var roles = await query.OrderBy(r => r.Name)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .ToListAsync();

        var items = new List<RoleDto>();
        foreach (var role in roles)
        {
            var userCount = await _db.UserRoles.CountAsync(ur => ur.RoleId == role.Id);
            items.Add(new RoleDto
            {
                Id = role.Id,
                Name = role.Name ?? "",
                Description = role.Description,
                UserCount = userCount,
                Permissions = role.RolePermissions.Select(rp => rp.Permission?.Name ?? "").Where(n => !string.IsNullOrEmpty(n)).ToList()
            });
        }

        return ApiResponse<PagedResult<RoleDto>>.Ok(new PagedResult<RoleDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<RoleDto>> GetRoleByIdAsync(string roleId, int cabinetId)
    {
        var role = await _roleManager.Roles
            .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == roleId);

        if (role == null)
            return ApiResponse<RoleDto>.Fail("Rôle non trouvé.");

        var userCount = await _db.UserRoles.CountAsync(ur => ur.RoleId == role.Id);
        return ApiResponse<RoleDto>.Ok(new RoleDto
        {
            Id = role.Id,
            Name = role.Name ?? "",
            Description = role.Description,
            UserCount = userCount,
            Permissions = role.RolePermissions.Select(rp => rp.Permission?.Name ?? "").Where(n => !string.IsNullOrEmpty(n)).ToList()
        });
    }

    public async Task<ApiResponse<RoleDto>> CreateRoleAsync(
        string name, string? description, List<string> permissions, int cabinetId, string userId)
    {
        if (await _roleManager.RoleExistsAsync(name))
            return ApiResponse<RoleDto>.Fail($"Le rôle '{name}' existe déjà.");

        var role = new ApplicationRole
        {
            Name = name,
            Description = description,
            CabinetId = cabinetId,
            IsSystemRole = false
        };

        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
            return ApiResponse<RoleDto>.Fail("Erreur lors de la création du rôle.");

        await SyncRolePermissionsAsync(role, permissions);
        await LogAuditAsync(cabinetId, userId, "Creation", "Role",
            $"Rôle '{name}' créé avec {permissions.Count} permissions.");

        return await GetRoleByIdAsync(role.Id, cabinetId);
    }

    public async Task<ApiResponse<RoleDto>> UpdateRolePermissionsAsync(
        string roleId, List<string> permissions, int cabinetId, string userId)
    {
        var role = await _roleManager.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == roleId);

        if (role == null)
            return ApiResponse<RoleDto>.Fail("Rôle non trouvé.");

        if (role.IsSystemRole)
            return ApiResponse<RoleDto>.Fail("Les rôles système ne peuvent pas être modifiés.");

        await SyncRolePermissionsAsync(role, permissions);
        await LogAuditAsync(cabinetId, userId, "Modification", "Role",
            $"Permissions du rôle '{role.Name}' mises à jour ({permissions.Count} permissions).");

        return await GetRoleByIdAsync(roleId, cabinetId);
    }

    // ═══════════════════════════════════════════════════════════
    // AUDIT LOGS
    // ═══════════════════════════════════════════════════════════

    public async Task<ApiResponse<PagedResult<AuditLogDto>>> GetAuditLogsAsync(
        int cabinetId, string? userId = null, string? action = null,
        DateTime? dateDebut = null, DateTime? dateFin = null, int page = 1, int pageSize = 50)
    {
        var query = _db.AuditLogs.Where(a => a.CabinetId == cabinetId);

        if (!string.IsNullOrWhiteSpace(userId))
            query = query.Where(a => a.UserName != null && a.UserName.Contains(userId));
        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(a => a.Action == action);
        if (dateDebut.HasValue)
            query = query.Where(a => a.CreatedAt >= dateDebut.Value);
        if (dateFin.HasValue)
            query = query.Where(a => a.CreatedAt <= dateFin.Value.AddDays(1));

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                UserId = a.UserId,
                UserName = a.UserName,
                Action = a.Action,
                Module = a.EntityType,
                Details = a.NewValues,
                IpAddress = a.IpAddress,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<PagedResult<AuditLogDto>>.Ok(new PagedResult<AuditLogDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    // ═══════════════════════════════════════════════════════════
    // USER PROFILE
    // ═══════════════════════════════════════════════════════════

    public async Task<ApiResponse<UserProfileDto>> GetProfileAsync(string userId, int cabinetId)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId && u.CabinetId == cabinetId);
        if (user == null)
            return ApiResponse<UserProfileDto>.Fail("Utilisateur non trouvé.");

        var roles = await _userManager.GetRolesAsync(user);
        var permissions = await GetUserPermissionsAsync(user);

        return ApiResponse<UserProfileDto>.Ok(new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email ?? "",
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = roles.FirstOrDefault(),
            ProfileImageUrl = user.ProfileImageUrl,
            PreferredLanguage = user.PreferredLanguage,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Permissions = permissions
        });
    }

    // ═══════════════════════════════════════════════════════════
    // PRIVATE HELPERS
    // ═══════════════════════════════════════════════════════════

    private async Task<List<string>> GetUserPermissionsAsync(ApplicationUser user)
    {
        var list = await _permissions.GetEffectivePermissionsAsync(user.Id);
        return list.ToList();
    }

    private async Task SyncUserPermissionsAsync(ApplicationUser user, List<string> newPermissions)
    {
        // Remove existing Permission claims
        var currentClaims = await _userManager.GetClaimsAsync(user);
        var permClaims = currentClaims.Where(c => c.Type == "Permission").ToList();
        if (permClaims.Any())
            await _userManager.RemoveClaimsAsync(user, permClaims);

        // Add new Permission claims — this list becomes the exclusive effective set
        if (newPermissions.Any())
        {
            var claims = newPermissions
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(p => new System.Security.Claims.Claim("Permission", p.Trim().ToLowerInvariant()));
            await _userManager.AddClaimsAsync(user, claims);
        }

        _permissions.InvalidateUser(user.Id);
    }

    private async Task SyncRolePermissionsAsync(ApplicationRole role, List<string> permissionNames)
    {
        // Remove existing role permissions
        var existing = await _db.RolePermissions.Where(rp => rp.RoleId == role.Id).ToListAsync();
        _db.RolePermissions.RemoveRange(existing);

        foreach (var permName in permissionNames.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var perm = await _db.Permissions.FirstOrDefaultAsync(p => p.Name == permName);
            if (perm == null)
            {
                var parts = permName.Split('.');
                perm = new Permission
                {
                    Name = permName,
                    Module = parts.Length > 0 ? parts[0] : permName,
                    Action = parts.Length > 1 ? parts[1] : "view",
                    Description = $"Permission: {permName}"
                };
                _db.Permissions.Add(perm);
                await _db.SaveChangesAsync();
            }

            _db.RolePermissions.Add(new RolePermission
            {
                RoleId = role.Id,
                PermissionId = perm.Id
            });
        }

        await _db.SaveChangesAsync();
        // Role change impacts all members still on role-fallback (no direct claims)
        _permissions.InvalidateAll();
    }

    private async Task LogAuditAsync(int cabinetId, string userId, string action, string entityType, string details)
    {
        var user = await _db.Users.FindAsync(userId);
        _db.AuditLogs.Add(new AuditLog
        {
            CabinetId = cabinetId,
            UserId = userId,
            UserName = user?.FullName ?? userId,
            Action = action,
            EntityType = entityType,
            NewValues = details,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }
}
