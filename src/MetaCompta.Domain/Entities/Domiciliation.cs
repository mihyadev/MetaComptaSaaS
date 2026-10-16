using MetaCompta.Domain.Common;

namespace MetaCompta.Domain.Entities;

/// <summary>Incoming physical mail registered for a domiciled client.</summary>
public class CourrierDomiciliation : TenantEntity
{
    public int ClientId { get; set; }
    public virtual Client? Client { get; set; }

    public DateTime DateReception { get; set; } = DateTime.UtcNow;
    public string? Expediteur { get; set; }
    public string TypeCourrier { get; set; } = "Courrier";
    public string? Reference { get; set; }
    public string Confidentialite { get; set; } = "Normal"; // Normal, Confidentiel, Strict

    public int? ScanDocumentId { get; set; }
    public virtual Document? ScanDocument { get; set; }

    /// <summary>Multiple deposited scans / pieces for this courrier (photothèque).</summary>
    public virtual ICollection<CourrierPieceJointe> Pieces { get; set; } = new List<CourrierPieceJointe>();

    public bool ActionRequise { get; set; }
    public string? ResponsableUserId { get; set; }
    public virtual ApplicationUser? Responsable { get; set; }
    public DateTime? DateEcheance { get; set; }

    /// <summary>
    /// Received, ToProcess, Assigned, InProgress, Completed, ClientNotified, Archived, Closed
    /// </summary>
    public string Statut { get; set; } = "Received";

    public bool ClientNotifie { get; set; }
    public DateTime? ClientNotifieAt { get; set; }
    public bool PartageDocumentClient { get; set; }
    public string? Notes { get; set; }

    public int? LinkedTaskItemId { get; set; }
    public virtual TaskItem? LinkedTaskItem { get; set; }

    public virtual ICollection<DemandeActionCourrier> ActionsClient { get; set; } = new List<DemandeActionCourrier>();
}

/// <summary>Attached scan / document piece linked to a courrier (supports multiple files).</summary>
public class CourrierPieceJointe : TenantEntity
{
    public int CourrierId { get; set; }
    public virtual CourrierDomiciliation? Courrier { get; set; }

    public int DocumentId { get; set; }
    public virtual Document? Document { get; set; }

    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
    public string? Label { get; set; }
}

/// <summary>Client-initiated action on a notified courrier (pickup, forward, view, archive).</summary>
public class DemandeActionCourrier : TenantEntity
{
    public int CourrierId { get; set; }
    public virtual CourrierDomiciliation? Courrier { get; set; }

    /// <summary>Pickup | Forward | View | Archive</summary>
    public string TypeAction { get; set; } = "Pickup";

    public string? AdresseForward { get; set; }
    public string? ShippingReference { get; set; }
    public string Statut { get; set; } = "PickupRequested";
    public string? Notes { get; set; }

    public string? ClientUserId { get; set; }
    public string? StaffUserId { get; set; }
    public bool IdentityCheckDone { get; set; }
    public DateTime? ConfirmedAt { get; set; }

    public int? LinkedTaskItemId { get; set; }
}

/// <summary>Firm requests a document from the domiciled client.</summary>
public class DemandeDocumentDomiciliation : TenantEntity
{
    public int ClientId { get; set; }
    public virtual Client? Client { get; set; }

    public string DocumentType { get; set; } = string.Empty;
    public string? Message { get; set; }
    public string? ResponsableUserId { get; set; }
    public virtual ApplicationUser? Responsable { get; set; }
    public DateTime? DateEcheance { get; set; }

    /// <summary>Requested, ClientNotified, Uploaded, UnderReview, Validated, Rejected, WaitingClient, Closed</summary>
    public string Statut { get; set; } = "Requested";

    public int? CurrentDocumentId { get; set; }
    public virtual Document? CurrentDocument { get; set; }

    /// <summary>Expired, WrongDocument, MissingPage, Unreadable, InformationMismatch, Other</summary>
    public string? RejectionReasonCode { get; set; }
    public string? RejectionReasonText { get; set; }

    public int? LinkedTaskItemId { get; set; }
    public int? RootDocumentId { get; set; }
}

/// <summary>Chronological activity for domiciliation operations.</summary>
public class DomiciliationTimelineEvent : TenantEntity
{
    public int ClientId { get; set; }
    public virtual Client? Client { get; set; }

    public string RelatedType { get; set; } = string.Empty; // Courrier, DemandeAction, DemandeDocument
    public int RelatedId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? PreviousStatus { get; set; }
    public string? NewStatus { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? Note { get; set; }
    public int? DocumentId { get; set; }
    public DateTime At { get; set; } = DateTime.UtcNow;
}
