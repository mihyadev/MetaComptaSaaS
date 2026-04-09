using MetaCompta.Domain.Common;

namespace MetaCompta.Domain.Entities;

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║                     WORKFLOW ENGINE — DOMAIN ENTITIES                   ║
// ║  Enterprise-grade 9-step configurable workflow for fiduciary operations ║
// ╚══════════════════════════════════════════════════════════════════════════╝

#region ─── Enums ──────────────────────────────────────────────────────────────

/// <summary>
/// 9-step default workflow lifecycle.
/// </summary>
public enum WorkflowStepType
{
    Initialisation = 0,
    DemandePieces = 1,
    Collecte = 2,
    Saisie = 3,
    Controle = 4,
    ValidationInterne = 5,
    ValidationClient = 6,
    DepotSoumission = 7,
    ClotureArchivage = 8
}

/// <summary>
/// How a workflow step is assigned.
/// </summary>
public enum AssignmentType
{
    Employee = 0,       // Specific cabinet employee
    Role = 1,           // Any user with a given role
    Department = 2,     // Any user in a given department
    Client = 3,         // Client via extranet portal
    AutoSystem = 4      // Automatic (system) — no human action required
}

/// <summary>
/// Global status of a workflow instance.
/// </summary>
public enum WorkflowInstanceStatus
{
    Active = 0,
    Completed = 1,
    Cancelled = 2,
    Paused = 3,
    Error = 4
}

/// <summary>
/// Status of an individual workflow step instance.
/// </summary>
public enum WorkflowStepStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Skipped = 3,
    Blocked = 4,
    Failed = 5,
    WaitingForClient = 6,
    WaitingForValidation = 7
}

/// <summary>
/// Declaration type scoping for workflow templates.
/// </summary>
public enum WorkflowDeclarationType
{
    All = 0,
    TVA = 1,
    IS = 2,
    IR = 3,
    CNSS = 4,
    IR_SALAIRES = 5,
    Paie = 6,
    Juridique = 7,
    Comptabilite = 8
}

/// <summary>
/// Task auto-creation type from workflow step.
/// </summary>
public enum WorkflowTaskType
{
    Standard = 0,
    DocumentRequest = 1,
    DataEntry = 2,
    Review = 3,
    Validation = 4,
    Submission = 5,
    Archiving = 6,
    ClientAction = 7
}

/// <summary>
/// System actions that a workflow step can trigger automatically.
/// These are REAL operations, not just status labels.
/// </summary>
public enum WorkflowActionType
{
    None = 0,                   // No auto-action — manual step
    CreateDeclaration = 1,      // Auto-create a new declaration
    CompleteDeclaration = 2,    // Mark declaration as Complétée
    ValidateDeclaration = 3,    // Mark declaration as Validée
    SubmitDeclaration = 4,      // Mark declaration as Soumise
    SuspendDeclaration = 5,     // Mark declaration as Suspendue
    ArchiveDeclaration = 6,     // Mark declaration as Archivée
    RelanceClient = 7,          // Send reminder/relance to client
    NotifyEmployee = 8,         // Send internal notification to employee
    NotifyClient = 9,           // Send notification to client on extranet
    GeneratePDF = 10,           // Auto-generate PDF document
    ExportXML = 11,             // Auto-generate EDI/XML export
    SendEmail = 12,             // Send email notification
    CreateTask = 13,            // Create a task item in planning
    RequestDocuments = 14,      // Send document request to client
    ApproveByClient = 15,       // Wait for client approval on extranet
    PaymentReminder = 16,       // Send payment reminder
    Custom = 99                 // Custom action defined in ActionConfigJson
}

/// <summary>
/// Routing mode for workflow step: determines how the pipeline flows.
/// </summary>
public enum WorkflowRoutingMode
{
    Sequential = 0,             // Default: go to next step in order
    ConditionalBranch = 1,      // Evaluate conditions → go to matching branch
    ParallelSplit = 2,          // Fork: activate multiple next steps simultaneously
    WaitAll = 3,                // Join: wait for all parallel branches to complete
    SkipToStep = 4,             // Jump to a specific step (by order or name)
    EndWorkflow = 5             // Terminate workflow immediately
}

/// <summary>
/// Trigger source — who/what initiates or completes a step.
/// </summary>
public enum WorkflowTriggerSource
{
    Manual = 0,                 // Human clicks "Advance" button
    System = 1,                 // Auto-triggered by system (CRON, auto-action)
    Employee = 2,               // Specific cabinet employee action
    Client = 3,                 // Client action on extranet
    Extranet = 4,               // Client action via extranet portal
    API = 5,                    // External API webhook trigger
    Schedule = 6                // CRON schedule trigger
}

#endregion

#region ─── Department ─────────────────────────────────────────────────────────

/// <summary>
/// Cabinet department (Comptabilité, Fiscalité, Social/Paie, Juridique, etc.)
/// Used for workflow step assignment and resource management.
/// </summary>
public class Department : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;           // COMPTA, FISCAL, SOCIAL, JURIDIQUE
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public string? HeadOfDepartmentUserId { get; set; }        // Chef de département
    public virtual ApplicationUser? HeadOfDepartment { get; set; }

    // Navigation
    public virtual ICollection<DepartmentMember> Members { get; set; } = new List<DepartmentMember>();
}

/// <summary>
/// Junction table: User ↔ Department membership.
/// A cabinet employee can belong to multiple departments.
/// </summary>
public class DepartmentMember : TenantEntity
{
    public int DepartmentId { get; set; }
    public virtual Department? Department { get; set; }

    public string UserId { get; set; } = string.Empty;
    public virtual ApplicationUser? User { get; set; }

    public bool IsLead { get; set; } = false;                  // Is team lead within department
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

#endregion

#region ─── Workflow Template ──────────────────────────────────────────────────

/// <summary>
/// Workflow template definition (the "blueprint").
/// Can be global (CabinetId + ClientId null) or specific to a client/type.
/// The engine picks the most specific match: Client+Type > Client > Type > Global.
/// </summary>
public class WorkflowTemplate : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>
    /// Which declaration type this template applies to.
    /// All = default for any type.
    /// </summary>
    public WorkflowDeclarationType DeclarationType { get; set; } = WorkflowDeclarationType.All;

    /// <summary>
    /// If set, this template is specific to one client.
    /// Null = applies to all clients.
    /// </summary>
    public int? ClientId { get; set; }
    public virtual Client? Client { get; set; }

    /// <summary>
    /// Whether this is the active/default template.
    /// Only one default per CabinetId+DeclarationType+ClientId combination.
    /// </summary>
    public bool IsDefault { get; set; } = true;
    public bool IsActive { get; set; } = true;

    /// <summary>System-seeded template (Plan Marocain) = non-modifiable by users.</summary>
    public bool IsSystem { get; set; } = false;

    /// <summary>Version for template evolution tracking.</summary>
    public int Version { get; set; } = 1;

    // Navigation
    public virtual ICollection<WorkflowStepTemplate> Steps { get; set; } = new List<WorkflowStepTemplate>();
    public virtual ICollection<WorkflowInstance> Instances { get; set; } = new List<WorkflowInstance>();
}

/// <summary>
/// A single step definition within a workflow template.
/// Steps are ordered and can have blocking rules.
/// </summary>
public class WorkflowStepTemplate : TenantEntity
{
    public int WorkflowTemplateId { get; set; }
    public virtual WorkflowTemplate? WorkflowTemplate { get; set; }

    /// <summary>Step name (e.g. "Initialisation", "Demande de pièces", etc.)</summary>
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>Order of execution (0-based). Determines step sequence.</summary>
    public int StepOrder { get; set; }

    /// <summary>The type/category of this step.</summary>
    public WorkflowStepType StepType { get; set; }

    // ── Assignment Configuration ──────────────────────────────────
    /// <summary>How this step gets assigned.</summary>
    public AssignmentType AssignmentType { get; set; } = AssignmentType.Employee;

    /// <summary>Specific employee (when AssignmentType = Employee).</summary>
    public string? AssignedToUserId { get; set; }
    public virtual ApplicationUser? AssignedToUser { get; set; }

    /// <summary>Role name (when AssignmentType = Role). E.g. "Manager", "Assistant".</summary>
    public string? AssignedToRoleName { get; set; }

    /// <summary>Department ID (when AssignmentType = Department).</summary>
    public int? AssignedToDepartmentId { get; set; }
    public virtual Department? AssignedToDepartment { get; set; }

    /// <summary>Is this step performed by the client on the extranet? (AssignmentType = Client)</summary>
    public bool IsClientStep { get; set; } = false;

    // ── Behavior & Rules ──────────────────────────────────────────
    /// <summary>Permission claim required to complete this step.</summary>
    public string? RequiredPermission { get; set; }

    /// <summary>SLA in hours (time limit to complete this step).</summary>
    public int? SLAHours { get; set; }

    /// <summary>Is this step mandatory? If false, it can be skipped.</summary>
    public bool IsRequired { get; set; } = true;

    /// <summary>
    /// Does this step block progression until completed?
    /// If true, the workflow cannot advance to the next step until this one is done.
    /// </summary>
    public bool IsBlocking { get; set; } = true;

    /// <summary>Auto-create a task when this step activates.</summary>
    public bool AutoCreateTask { get; set; } = true;

    /// <summary>Task type to auto-create.</summary>
    public WorkflowTaskType TaskType { get; set; } = WorkflowTaskType.Standard;

    /// <summary>Task priority for auto-created tasks.</summary>
    public string TaskPriority { get; set; } = "Moyenne"; // Faible, Moyenne, Haute, Urgente

    /// <summary>Auto-send notification when this step activates.</summary>
    public bool AutoNotify { get; set; } = true;

    /// <summary>
    /// JSON-encoded validation rules (optional).
    /// E.g. {"requireDocuments": true, "minDocuments": 1, "requireComment": false}
    /// </summary>
    public string? ValidationRulesJson { get; set; }

    /// <summary>
    /// Maps this step to a Declaration status change.
    /// When step completes, the linked declaration's Statut changes.
    /// E.g. "Completee", "EnRevision", "Validee", "Soumise"
    /// </summary>
    public string? DeclarationStatusOnComplete { get; set; }

    /// <summary>
    /// Maps this step to a Declaration status change on entry.
    /// E.g. "ACompleter" when step starts
    /// </summary>
    public string? DeclarationStatusOnEntry { get; set; }

    // ── ACTION ENGINE ─────────────────────────────────────────
    /// <summary>System action to execute when this step activates or completes.</summary>
    public WorkflowActionType ActionOnEntry { get; set; } = WorkflowActionType.None;
    public WorkflowActionType ActionOnComplete { get; set; } = WorkflowActionType.None;

    /// <summary>JSON config for the action (e.g. {"emailTemplate":"relance_tva","subject":"Rappel TVA"})</summary>
    public string? ActionConfigJson { get; set; }

    // ── CRON / SCHEDULING ─────────────────────────────────────
    /// <summary>
    /// CRON expression for scheduled auto-trigger (e.g. "0 8 1 * *" = 1st of each month at 8am).
    /// Used for steps like auto-creating declarations or sending periodic reminders.
    /// </summary>
    public string? CronExpression { get; set; }

    /// <summary>User-friendly label for the CRON schedule (e.g. "Chaque 1er du mois à 08h00")</summary>
    public string? CronDescription { get; set; }

    /// <summary>Whether this step runs on a CRON schedule rather than on workflow progression.</summary>
    public bool IsScheduled { get; set; } = false;

    // ── RELANCE / RETRY ───────────────────────────────────────
    /// <summary>Maximum number of retry/relance attempts before escalating.</summary>
    public int MaxRetries { get; set; } = 0;

    /// <summary>Hours between each retry/relance attempt.</summary>
    public int? RetryIntervalHours { get; set; }

    /// <summary>What to do after max retries exhausted: NextStep, Suspend, Cancel, SkipStep, EscalateToManager</summary>
    public string? OnMaxRetriesAction { get; set; }

    // ── CONDITIONAL ROUTING / PIPELINE ────────────────────────
    /// <summary>Routing mode for this step's output.</summary>
    public WorkflowRoutingMode RoutingMode { get; set; } = WorkflowRoutingMode.Sequential;

    /// <summary>
    /// JSON-encoded routing conditions for ConditionalBranch mode.
    /// E.g. [{"condition":"step.status=='Completed' && declaration.montant > 50000","gotoStep":5},
    ///        {"condition":"step.retryCount >= 3","gotoStep":7,"label":"Suspension après 3 relances"},
    ///        {"condition":"true","gotoStep":3,"label":"Par défaut"}]
    /// </summary>
    public string? RoutingConditionsJson { get; set; }

    /// <summary>For SkipToStep routing: target step order to jump to.</summary>
    public int? GotoStepOrder { get; set; }

    // ── TRIGGER SOURCE ────────────────────────────────────────
    /// <summary>Who/what can trigger completion of this step.</summary>
    public WorkflowTriggerSource TriggerSource { get; set; } = WorkflowTriggerSource.Manual;

    // ── EMPLOYEE AFFECTATION PER SERVICE ──────────────────────
    /// <summary>
    /// If true, assignment is resolved from the client's ServiceContrat affectation
    /// (the employee assigned to this client's service contract for this declaration type).
    /// </summary>
    public bool UseServiceContratAffectation { get; set; } = false;

    /// <summary>Service contract type code to match (e.g. "TVA", "IS", "CNSS", "COMPTABILITE")</summary>
    public string? ServiceContratTypeCode { get; set; }
}

#endregion

#region ─── Workflow Instance ──────────────────────────────────────────────────

/// <summary>
/// A running instance of a workflow (attached to a Declaration or stand-alone).
/// Tracks the current state and progression through template steps.
/// </summary>
public class WorkflowInstance : TenantEntity
{
    public int WorkflowTemplateId { get; set; }
    public virtual WorkflowTemplate? WorkflowTemplate { get; set; }

    /// <summary>The declaration this workflow is processing (if applicable).</summary>
    public int? DeclarationId { get; set; }
    public virtual Declaration? Declaration { get; set; }

    /// <summary>The client this workflow relates to.</summary>
    public int ClientId { get; set; }
    public virtual Client? Client { get; set; }

    /// <summary>Current step order (tracks progression).</summary>
    public int CurrentStepOrder { get; set; } = 0;

    /// <summary>Global status of this workflow instance.</summary>
    public WorkflowInstanceStatus Status { get; set; } = WorkflowInstanceStatus.Active;

    /// <summary>Who initiated this workflow.</summary>
    public string? StartedByUserId { get; set; }
    public virtual ApplicationUser? StartedByUser { get; set; }

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }

    /// <summary>JSON metadata for this instance (e.g. context data, override configs).</summary>
    public string? MetadataJson { get; set; }

    // Navigation
    public virtual ICollection<WorkflowStepInstance> StepInstances { get; set; } = new List<WorkflowStepInstance>();
    public virtual ICollection<WorkflowHistory> History { get; set; } = new List<WorkflowHistory>();
}

/// <summary>
/// A single step's execution within a workflow instance.
/// Tracks who did what, when, and the result.
/// </summary>
public class WorkflowStepInstance : TenantEntity
{
    public int WorkflowInstanceId { get; set; }
    public virtual WorkflowInstance? WorkflowInstance { get; set; }

    public int WorkflowStepTemplateId { get; set; }
    public virtual WorkflowStepTemplate? WorkflowStepTemplate { get; set; }

    /// <summary>Step order within this instance (copied from template).</summary>
    public int StepOrder { get; set; }

    /// <summary>Display name (copied from template for history).</summary>
    public string StepName { get; set; } = string.Empty;

    /// <summary>Current status of this step.</summary>
    public WorkflowStepStatus Status { get; set; } = WorkflowStepStatus.Pending;

    // ── Assignment (resolved at activation time) ──────────────────
    /// <summary>The user actually assigned to this step instance.</summary>
    public string? AssignedToUserId { get; set; }
    public virtual ApplicationUser? AssignedToUser { get; set; }

    /// <summary>The user who completed this step.</summary>
    public string? CompletedByUserId { get; set; }
    public virtual ApplicationUser? CompletedByUser { get; set; }

    // ── Dates ─────────────────────────────────────────────────────
    public DateTime? ActivatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? DueDate { get; set; }

    // ── Data ──────────────────────────────────────────────────────
    /// <summary>Notes / comments left by the user completing the step.</summary>
    public string? Notes { get; set; }

    /// <summary>Reason if step is blocked.</summary>
    public string? BlockReason { get; set; }

    /// <summary>The auto-created task linked to this step (if any).</summary>
    public int? TaskItemId { get; set; }
    public virtual TaskItem? TaskItem { get; set; }

    /// <summary>JSON-encoded result/output data from this step.</summary>
    public string? ResultDataJson { get; set; }

    // ── RETRY / RELANCE TRACKING ──────────────────────────────
    /// <summary>Number of retries/relances already executed for this step.</summary>
    public int RetryCount { get; set; } = 0;

    /// <summary>Date of last retry/relance attempt.</summary>
    public DateTime? LastRetryAt { get; set; }

    /// <summary>Date of next scheduled retry.</summary>
    public DateTime? NextRetryAt { get; set; }

    /// <summary>Who/what triggered this step's current state.</summary>
    public string? TriggerSource { get; set; }

    /// <summary>Which routing branch was taken from this step (for conditional workflows).</summary>
    public int? RoutedToStepOrder { get; set; }
}

#endregion

#region ─── Workflow History / Audit ───────────────────────────────────────────

/// <summary>
/// Full audit trail for workflow actions.
/// Every state change, assignment, comment, etc. is recorded.
/// </summary>
public class WorkflowHistory : TenantEntity
{
    public int WorkflowInstanceId { get; set; }
    public virtual WorkflowInstance? WorkflowInstance { get; set; }

    /// <summary>Related step instance (optional — some events are instance-level).</summary>
    public int? WorkflowStepInstanceId { get; set; }
    public virtual WorkflowStepInstance? WorkflowStepInstance { get; set; }

    /// <summary>Action performed (e.g. "StepStarted", "StepCompleted", "WorkflowPaused", "Reassigned")</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>Previous status value (for status transitions).</summary>
    public string? PreviousStatus { get; set; }

    /// <summary>New status value.</summary>
    public string? NewStatus { get; set; }

    /// <summary>Comment / reason for this action.</summary>
    public string? Comment { get; set; }

    /// <summary>User who performed the action.</summary>
    public string? UserId { get; set; }
    public string? UserName { get; set; }

    public DateTime ActionDate { get; set; } = DateTime.UtcNow;
}

#endregion

#region ─── Client Workflow Configuration ──────────────────────────────────────

/// <summary>
/// Per-client workflow configuration overrides.
/// Allows customizing which steps apply, SLAs, assignments, etc.
/// </summary>
public class ClientWorkflowConfig : TenantEntity
{
    public int ClientId { get; set; }
    public virtual Client? Client { get; set; }

    /// <summary>Which declaration type this config overrides.</summary>
    public WorkflowDeclarationType DeclarationType { get; set; } = WorkflowDeclarationType.All;

    /// <summary>Override: specific workflow template to use for this client.</summary>
    public int? WorkflowTemplateId { get; set; }
    public virtual WorkflowTemplate? WorkflowTemplate { get; set; }

    /// <summary>Whether client extranet validation step is enabled.</summary>
    public bool ClientValidationEnabled { get; set; } = true;

    /// <summary>Whether document request step is enabled.</summary>
    public bool DocumentRequestEnabled { get; set; } = true;

    /// <summary>Default SLA multiplier (e.g. 1.5 = 50% more time than standard).</summary>
    public decimal SLAMultiplier { get; set; } = 1.0m;

    /// <summary>Auto-start workflow when declaration is created.</summary>
    public bool AutoStartWorkflow { get; set; } = true;

    /// <summary>Specific employee assigned as primary for this client's workflows.</summary>
    public string? PrimaryAssigneeUserId { get; set; }
    public virtual ApplicationUser? PrimaryAssignee { get; set; }

    /// <summary>JSON-encoded additional settings.</summary>
    public string? SettingsJson { get; set; }

    public bool IsActive { get; set; } = true;
}

#endregion
