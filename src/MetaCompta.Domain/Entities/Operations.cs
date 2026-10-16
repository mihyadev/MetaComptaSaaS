using MetaCompta.Domain.Common;

namespace MetaCompta.Domain.Entities;

/// <summary>
/// Document management
/// </summary>
public class Document : TenantEntity
{
    public string Titre { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Categorie { get; set; } = string.Empty; // Legal, Fiscal, Social, Comptable, RH, Admin
    public string NomFichier { get; set; } = string.Empty;
    public string CheminStockage { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long TailleFichier { get; set; } // bytes
    public string? Tags { get; set; } // comma-separated
    public DateTime? DateDocument { get; set; }

    // Relations
    public int? ClientId { get; set; }
    public virtual Client? Client { get; set; }
    public int? EmployeeId { get; set; }
    public virtual Employee? Employee { get; set; }
    public int? DeclarationId { get; set; }
    public virtual Declaration? Declaration { get; set; }

    // Workflow
    public string Statut { get; set; } = "EnAttente"; // EnAttente, Valide, Rejete, Archive
    public string? MotifRejet { get; set; }

    // Versioning
    public int Version { get; set; } = 1;
    public int? ParentDocumentId { get; set; }
    public virtual Document? ParentDocument { get; set; }

    // Sharing
    public bool PartageExtranet { get; set; } = false;

    // Upload info
    public string? UploadedByUserId { get; set; }
    public virtual ApplicationUser? UploadedByUser { get; set; }
}

/// <summary>
/// Notification system
/// </summary>
public class Notification : TenantEntity
{
    public string UserId { get; set; } = string.Empty;
    public string Titre { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "Info"; // Info, Warning, Urgent, Critical
    public string? Module { get; set; } // Declaration, Payroll, Document, Task, System
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public string? Url { get; set; } // Link to related page
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
    public bool EmailSent { get; set; } = false;
}

/// <summary>
/// Task management
/// </summary>
public class TaskItem : TenantEntity
{
    public string Titre { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ClientId { get; set; }
    public virtual Client? Client { get; set; }
    public int? DeclarationId { get; set; }
    public virtual Declaration? Declaration { get; set; }

    // Assignment
    public string? AssignedToUserId { get; set; }
    public virtual ApplicationUser? AssignedToUser { get; set; }
    public string? AssignedByUserId { get; set; }
    public virtual ApplicationUser? AssignedByUser { get; set; }

    // Dates
    public DateTime? DateDebut { get; set; }
    public DateTime DateEcheance { get; set; }

    // Status
    public string Priorite { get; set; } = "Moyenne"; // Faible, Moyenne, Haute, Urgente
    public string Statut { get; set; } = "AFaire"; // AFaire, EnCours, EnAttenteRevision, Termine, Annule

    public string? PiecesJointesJson { get; set; }

    /// <summary>Optional link to a non-declaration entity (e.g. Courrier, DemandeDocument).</summary>
    public string? RelatedEntityType { get; set; }
    public int? RelatedEntityId { get; set; }
    public string? TaskType { get; set; }

    // Navigation
    public virtual ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
}

/// <summary>
/// Task comments/activity
/// </summary>
public class TaskComment : TenantEntity
{
    public int TaskItemId { get; set; }
    public virtual TaskItem? TaskItem { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Contenu { get; set; } = string.Empty;
}

/// <summary>
/// Client-cabinet messaging
/// </summary>
public class Message : TenantEntity
{
    public int? ClientId { get; set; }
    public virtual Client? Client { get; set; }
    public int? DeclarationId { get; set; }
    public virtual Declaration? Declaration { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string Sujet { get; set; } = string.Empty;
    public string Contenu { get; set; } = string.Empty;
    public bool IsReadByRecipient { get; set; } = false;
    public DateTime? ReadAt { get; set; }
    public int? ParentMessageId { get; set; }
    public virtual Message? ParentMessage { get; set; }
    public string? PiecesJointesJson { get; set; }
    public string SenderRole { get; set; } = "Cabinet"; // Cabinet or Client
}
