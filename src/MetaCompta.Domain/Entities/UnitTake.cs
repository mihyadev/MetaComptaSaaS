using MetaCompta.Domain.Common;

namespace MetaCompta.Domain.Entities;

/// <summary>
/// Catalog of claimable unit kinds (Declaration, WorkflowStep, Task, …).
/// Generic — not limited to TVA.
/// </summary>
public class UnitType : BaseEntity
{
    /// <summary>Stable code, e.g. DECLARATION, WORKFLOW_STEP, TASK.</summary>
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>Logical source table / aggregate (e.g. Declarations, WorkflowStepInstances, TaskItems).</summary>
    public string? EntityTable { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual ICollection<UnitTake> Takes { get; set; } = new List<UnitTake>();
}

/// <summary>
/// Exclusive open/close take of any unit. One open take (ClosedAt == null) per unit at a time.
/// Open = user started/took the unit; Closed = user finished (ClosedAt set).
/// </summary>
public class UnitTake : TenantEntity
{
    public int UnitTypeId { get; set; }
    public virtual UnitType? UnitType { get; set; }

    /// <summary>Polymorphic id of the unit (Declaration.Id, WorkflowStepInstance.Id, TaskItem.Id, …).</summary>
    public int UnitEntityId { get; set; }

    public string TakenByUserId { get; set; } = string.Empty;
    public virtual ApplicationUser? TakenByUser { get; set; }

    public DateTime TakenAt { get; set; } = DateTime.UtcNow;

    /// <summary>Null while the take is open; set when the user closes/finishes.</summary>
    public DateTime? ClosedAt { get; set; }

    /// <summary>Open | Closed | Released (forced).</summary>
    public string Status { get; set; } = UnitTakeStatuses.Open;

    public string? Comment { get; set; }

    /// <summary>Optional links for convenience / reporting.</summary>
    public int? TaskItemId { get; set; }
    public virtual TaskItem? TaskItem { get; set; }

    public int? WorkflowStepInstanceId { get; set; }
    public virtual WorkflowStepInstance? WorkflowStepInstance { get; set; }

    public int? DeclarationId { get; set; }
    public virtual Declaration? Declaration { get; set; }
}

public static class UnitTakeStatuses
{
    public const string Open = "Open";
    public const string Closed = "Closed";
    public const string Released = "Released";
}

public static class UnitTypeCodes
{
    public const string Declaration = "DECLARATION";
    public const string WorkflowStep = "WORKFLOW_STEP";
    public const string Task = "TASK";
}
