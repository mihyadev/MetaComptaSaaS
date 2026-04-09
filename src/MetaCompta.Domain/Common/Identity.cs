using Microsoft.AspNetCore.Identity;
using MetaCompta.Domain.Entities;

namespace MetaCompta.Domain.Common;

/// <summary>
/// Application user extending ASP.NET Core Identity
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public int? CabinetId { get; set; }
    public virtual Cabinet? Cabinet { get; set; }
    public int? ClientId { get; set; } // For Extranet users
    public virtual Client? Client { get; set; }
    public string? Poste { get; set; }
    public string? Departement { get; set; }
    public string? CIN { get; set; }
    public string? Adresse { get; set; }
    public DateTime? DateNaissance { get; set; }
    public string? LieuNaissance { get; set; }
    public string? Sexe { get; set; }
    public string? SituationFamiliale { get; set; }
    public string? NumeroCNSS { get; set; }
    public DateTime? DateEmbauche { get; set; }
    public string? TypeContrat { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string PreferredLanguage { get; set; } = "fr-FR";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
}

/// <summary>
/// Application role extending ASP.NET Core Identity
/// </summary>
public class ApplicationRole : IdentityRole
{
    public string? Description { get; set; }
    public int? CabinetId { get; set; }
    public bool IsSystemRole { get; set; } = false;
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

/// <summary>
/// Permission entity for granular access control
/// </summary>
public class Permission : BaseEntity
{
    public string Name { get; set; } = string.Empty; // e.g., "Client.View"
    public string Module { get; set; } = string.Empty; // e.g., "Client"
    public string Action { get; set; } = string.Empty; // e.g., "View"
    public string? Description { get; set; }
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

/// <summary>
/// Many-to-many: Role ↔ Permission
/// </summary>
public class RolePermission
{
    public string RoleId { get; set; } = string.Empty;
    public virtual ApplicationRole? Role { get; set; }
    public int PermissionId { get; set; }
    public virtual Permission? Permission { get; set; }
}
