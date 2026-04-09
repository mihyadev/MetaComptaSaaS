namespace MetaCompta.Domain.Common;

/// <summary>
/// Base entity with audit fields for all domain entities
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

/// <summary>
/// Base entity for tenant-scoped data (multi-tenant SaaS)
/// </summary>
public abstract class TenantEntity : BaseEntity
{
    public int CabinetId { get; set; }
    public virtual Cabinet? Cabinet { get; set; }
}
