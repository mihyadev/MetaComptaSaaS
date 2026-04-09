namespace MetaCompta.Domain.Common;

/// <summary>
/// Dynamic reference data (taux, plafonds, listes)
/// </summary>
public class ReferenceData : BaseEntity
{
    public string Category { get; set; } = string.Empty; // "TVA_RATE", "CNSS_RATE", "IR_BRACKET", etc.
    public string Code { get; set; } = string.Empty;     // Unique code within category
    public string Label { get; set; } = string.Empty;     // Display label
    public string? LabelAr { get; set; }                   // Arabic label
    public string? LabelEn { get; set; }                   // English label
    public string? Value { get; set; }                     // Can hold decimal, string, JSON
    public decimal? NumericValue { get; set; }
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
    public DateTime? EffectiveTo { get; set; }
    public int? CabinetId { get; set; } // null = global, otherwise cabinet-specific
    public string? Couleur { get; set; }   // Hex color e.g. "#059669" for UI badges/chips
    public string? CssClass { get; set; }  // CSS class e.g. "bg-success-subtle text-success"
    public string? Icone { get; set; }     // Bootstrap Icon class e.g. "bi-check-circle"
    public string? Metadata { get; set; }  // Extra JSON data
}

/// <summary>
/// Audit log for all significant actions
/// </summary>
public class AuditLog : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // Create, Update, Delete, Validate, etc.
    public string EntityType { get; set; } = string.Empty; // Client, Declaration, Employee, etc.
    public string? EntityId { get; set; }
    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public int? CabinetId { get; set; }
}

/// <summary>
/// Localization entries stored in DB for dynamic translation editing
/// </summary>
public class LocalizationEntry : BaseEntity
{
    public string ResourceKey { get; set; } = string.Empty;
    public string Culture { get; set; } = "fr-FR";
    public string Value { get; set; } = string.Empty;
    public string? Module { get; set; }
    public int? CabinetId { get; set; } // null = platform-wide
}
