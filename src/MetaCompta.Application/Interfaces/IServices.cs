using MetaCompta.Application.DTOs;
using MetaCompta.Domain.Entities;

namespace MetaCompta.Application.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<TokenDto>> LoginAsync(LoginDto dto);
    Task<ApiResponse<TokenDto>> RegisterAsync(RegisterDto dto);
    Task<ApiResponse<TokenDto>> RefreshTokenAsync(string refreshToken);
    Task<ApiResponse<bool>> ChangePasswordAsync(string userId, ChangePasswordDto dto);
    Task<ApiResponse<bool>> ResetPasswordAsync(string email);
}

public interface IContractAccessService
{
    /// <summary>True when client has ≥1 Valide/Actif contract within DateDebut–DateFin (Manage mode).</summary>
    Task<bool> HasManageAccessAsync(int clientId, int cabinetId);
    Task<ContractAccessContextDto> GetContractAccessContextAsync(int clientId, int cabinetId);
}

public interface IClientService
{
    Task<ApiResponse<PagedResult<ClientDto>>> GetAllAsync(int cabinetId, int page = 1, int pageSize = 20, string? search = null, string? statut = null);
    Task<ApiResponse<ClientDto>> GetByIdAsync(int id, int cabinetId);
    Task<ApiResponse<ClientDto>> CreateAsync(ClientCreateDto dto, int cabinetId, string userId);
    Task<ApiResponse<ClientDto>> UpdateAsync(int id, ClientCreateDto dto, int cabinetId, string userId);
    Task<ApiResponse<bool>> DeleteAsync(int id, int cabinetId, string userId);
    Task<List<string>> GetActiveServicesAsync(int clientId, int cabinetId);
    /// <summary>Returns true if the client has Manage-mode access (Valide/Actif + date window).</summary>
    Task<bool> HasActiveContractAsync(int clientId, int cabinetId);
    Task<ContractAccessContextDto> GetContractAccessContextAsync(int clientId, int cabinetId);
    // Enterprise service contract management
    Task<ApiResponse<List<ServiceContratDto>>> GetServiceContratsAsync(int clientId, int cabinetId, int? exerciceFiscal = null);
    Task<ApiResponse<ServiceContratDto>> GetServiceContratByIdAsync(int serviceContratId, int cabinetId);
    Task<ApiResponse<GenerateDeclarationsResultDto>> GenerateDeclarationsFromServicesAsync(GenerateDeclarationsRequestDto request, int cabinetId, string userId);
    Task<ApiResponse<List<ServiceContratDto>>> InitializeServicesForExerciceAsync(int clientId, int exerciceFiscal, int cabinetId, string userId);
}

public interface IEmployeeService
{
    Task<ApiResponse<PagedResult<EmployeeDto>>> GetAllAsync(int cabinetId, int? clientId = null, int page = 1, int pageSize = 20, string? search = null);
    Task<ApiResponse<EmployeeDto>> GetByIdAsync(int id, int cabinetId);
    Task<ApiResponse<EmployeeDto>> CreateAsync(EmployeeCreateDto dto, int cabinetId, string userId);
    Task<ApiResponse<EmployeeDto>> UpdateAsync(int id, EmployeeCreateDto dto, int cabinetId, string userId);
    Task<ApiResponse<bool>> DeleteAsync(int id, int cabinetId, string userId);
}

public interface IDeclarationService
{
    Task<ApiResponse<PagedResult<DeclarationDto>>> GetAllAsync(int cabinetId, int? clientId = null, string? type = null, string? statut = null, int page = 1, int pageSize = 20);
    Task<ApiResponse<DeclarationDto>> GetByIdAsync(int id, int cabinetId);
    // Generic create for any type
    Task<ApiResponse<DeclarationDto>> CreateAsync(CreateDeclarationDto dto, int cabinetId, string userId);
    /// <summary>Create a shell declaration for catalogue types (ACOMPTES, REG_IS, IR_SAL, …).</summary>
    Task<ApiResponse<DeclarationDto>> CreateShellAsync(CreateDeclarationDto dto, int cabinetId, string userId);
    Task<ApiResponse<DeclarationDto>> SaveShellAsync(int declarationId, decimal? montantDu, decimal? montantCredit, string? notes, int cabinetId, string userId);
    /// <summary>Suggest ACOMPTES amount = 25% of previous year IS dû.</summary>
    Task<ApiResponse<DeclarationDto>> SuggestAcompteAsync(int declarationId, int cabinetId, string userId);
    /// <summary>Suggest REG_IS = max(IS, CM) − acomptes for the fiscal year.</summary>
    Task<ApiResponse<DeclarationDto>> SuggestRegIsAsync(int declarationId, int cabinetId, string userId);
    /// <summary>Build a simple DNS/eBDS CSV export for CNSS (Damancom-ready stub).</summary>
    Task<ApiResponse<byte[]>> ExportCnssDnsCsvAsync(int declarationId, int cabinetId);
    /// <summary>Client employees for CNSS period (entrants / sortants / present) — never cabinet staff.</summary>
    Task<ApiResponse<CnssEmployeeRosterDto>> GetCnssEmployeesAsync(int declarationId, int cabinetId);
    Task<ApiResponse<CnssEmployeeRosterDto>> AddCnssEmployeeAsync(int declarationId, CnssEmployeeAddDto dto, int cabinetId, string userId);
    Task<ApiResponse<CnssEmployeeRosterDto>> UpdateCnssEmployeeAsync(int declarationId, int employeeId, CnssEmployeeAddDto dto, int cabinetId, string userId);
    Task<ApiResponse<CnssEmployeeImportResultDto>> ImportCnssEmployeesAsync(int declarationId, List<CnssEmployeeImportRowDto> rows, int cabinetId, string userId);
    // Delete
    Task<ApiResponse<bool>> DeleteAsync(int id, int cabinetId, string userId);
    // TVA
    Task<ApiResponse<DeclarationDto>> CreateTVAAsync(int clientId, int cabinetId, string userId);
    Task<ApiResponse<DeclarationTVADto>> SaveTVADataAsync(int declarationId, DeclarationTVADto dto, int cabinetId, string userId);
    Task<ApiResponse<DeclarationTVADto>> SaveTVAAjustementsAsync(int declarationId, decimal tvaFacturee, decimal tvaDeductible, decimal creditPrecedent, int cabinetId, string userId);
    Task<ApiResponse<DeclarationTVADto>> CalculateTVAAsync(DeclarationTVADto dto);
    Task<ApiResponse<DeclarationTVADto>> GetTVADataAsync(int declarationId, int cabinetId);
    // TVA Invoice Lines
    Task<ApiResponse<TVALigneFactureDto>> AddTVALigneAsync(int declarationId, TVALigneFactureDto dto, int cabinetId, string userId);
    Task<ApiResponse<bool>> DeleteTVALigneAsync(int declarationId, int ligneId, int cabinetId, string userId);
    Task<ApiResponse<List<TVALigneFactureDto>>> ImportTVALignesAsync(int declarationId, List<TVALigneFactureDto> lignes, int cabinetId, string userId);
    // IS
    Task<ApiResponse<DeclarationDto>> CreateISAsync(int clientId, int anneeFiscale, int cabinetId, string userId);
    Task<ApiResponse<DeclarationISDto>> SaveISDataAsync(int declarationId, DeclarationISDto dto, int cabinetId, string userId);
    Task<ApiResponse<DeclarationISDto>> CalculateISAsync(DeclarationISDto dto);
    Task<ApiResponse<DeclarationISDto>> GetISDataAsync(int declarationId, int cabinetId);
    Task<ApiResponse<List<DeclarationChecklistItemDto>>> GetChecklistAsync(int declarationId, int cabinetId);
    Task<ApiResponse<List<DeclarationChecklistItemDto>>> EnsureAnnualChecklistAsync(int declarationId, int cabinetId, string userId);
    Task<ApiResponse<DeclarationChecklistItemDto>> MarkChecklistItemReceivedAsync(int declarationId, int itemId, int? documentId, int cabinetId, string userId);
    Task<ApiResponse<bool>> IsChecklistCompleteAsync(int declarationId, int cabinetId);
    // IR
    Task<ApiResponse<DeclarationDto>> CreateIRAsync(int clientId, int anneeFiscale, int cabinetId, string userId);
    Task<ApiResponse<DeclarationIRDto>> SaveIRDataAsync(int declarationId, DeclarationIRDto dto, int cabinetId, string userId);
    Task<ApiResponse<DeclarationIRDto>> CalculateIRAsync(DeclarationIRDto dto);
    Task<ApiResponse<DeclarationIRDto>> GetIRDataAsync(int declarationId, int cabinetId);
    // CNSS
    Task<ApiResponse<DeclarationDto>> CreateCNSSAsync(int clientId, int cabinetId, string userId);
    Task<ApiResponse<DeclarationCNSSDto>> SaveCNSSDataAsync(int declarationId, DeclarationCNSSDto dto, int cabinetId, string userId);
    Task<ApiResponse<DeclarationCNSSDto>> CalculateCNSSAsync(int declarationId, int cabinetId);
    Task<ApiResponse<DeclarationCNSSDto>> GetCNSSDataAsync(int declarationId, int cabinetId);
    // Status
    Task<ApiResponse<bool>> ChangeStatusAsync(int id, string newStatut, int cabinetId, string userId, string? comment = null);
    Task<ApiResponse<List<DeclarationHistoryDto>>> GetStatusHistoryAsync(int declarationId, int cabinetId);
}

public interface IPayrollService
{
    Task<ApiResponse<List<PayrollPeriodDto>>> GetAllPeriodsAsync(int cabinetId, int? clientId = null);
    Task<ApiResponse<PayrollPeriodDto>> InitializeAsync(int clientId, int annee, int mois, int cabinetId, string userId);
    Task<ApiResponse<PayrollPeriodDto>> GetPeriodAsync(int id, int cabinetId);
    Task<ApiResponse<List<PaySlipDto>>> GetPaySlipsAsync(int periodId, int cabinetId);
    Task<ApiResponse<PaySlipDto>> UpdateVariablesAsync(int periodId, PaySlipVariablesDto dto, int cabinetId, string userId);
    Task<ApiResponse<List<VariableEmployeDto>>> GetVariablesForPeriodAsync(int employeeId, int annee, int mois, int cabinetId);
    Task<ApiResponse<bool>> UpdateVariablesBulkAsync(int employeeId, int annee, int mois, List<VariableEmployeCreateDto> variables, int cabinetId, string userId);
    Task<ApiResponse<PayrollPeriodDto>> CalculateAllAsync(int periodId, int cabinetId, string userId);
    Task<ApiResponse<bool>> ValidateAsync(int periodId, int cabinetId, string userId);

    // Payroll Engine
    Task<ApiResponse<BulletinCompletDto>> GetBulletinAsync(int paySlipId, int cabinetId);
    Task<ApiResponse<List<RubriqueDto>>> GetRubriquesAsync(int cabinetId, int? clientId = null);
    Task<ApiResponse<RubriqueDto>> GetRubriqueAsync(int id, int cabinetId);
    Task<ApiResponse<RubriqueDto>> CreateRubriqueAsync(RubriqueCreateDto dto, int cabinetId, string userId);
    Task<ApiResponse<RubriqueDto>> UpdateRubriqueAsync(int id, RubriqueCreateDto dto, int cabinetId, string userId);
    Task<ApiResponse<bool>> ToggleRubriqueAsync(int id, int cabinetId, string userId);
    Task<ApiResponse<bool>> DeleteRubriqueAsync(int id, int cabinetId, string userId);
    Task<ApiResponse<List<ConstanteGlobaleDto>>> GetConstantesAsync(int cabinetId, string? categorie = null);
    Task<ApiResponse<ConstanteGlobaleDto>> CreateConstanteAsync(ConstanteGlobaleDto dto, int cabinetId, string userId);
    Task<ApiResponse<ConstanteGlobaleDto>> UpdateConstanteAsync(int id, ConstanteGlobaleDto dto, int cabinetId, string userId);
    Task<ApiResponse<bool>> DeleteConstanteAsync(int id, int cabinetId, string userId);
    Task<ApiResponse<List<VariableEmployeDto>>> GetVariablesEmployeAsync(int employeeId, int cabinetId);
    Task<ApiResponse<VariableEmployeDto>> CreateVariableAsync(VariableEmployeCreateDto dto, int cabinetId, string userId);
    Task<ApiResponse<VariableEmployeDto>> UpdateVariableAsync(int id, VariableEmployeCreateDto dto, int cabinetId, string userId);
    Task<ApiResponse<bool>> DeleteVariableAsync(int id, int cabinetId, string userId);
    Task<ApiResponse<List<CumulEmployeDto>>> GetCumulsAsync(int employeeId, int annee, int cabinetId);
    Task<ApiResponse<bool>> SeedDefaultRubriquesAsync(int cabinetId, string userId);
    Task<ApiResponse<bool>> SeedDefaultConstantesAsync(int cabinetId, string userId);
    Task<ApiResponse<bool>> SeedDemo2025Async(int clientId, int cabinetId, string userId);

    // Affectation Rubriques (ERP-style: assign rubriques to employees)
    Task<ApiResponse<List<AffectationRubriqueDto>>> GetAffectationsAsync(int employeeId, int cabinetId);
    Task<ApiResponse<List<RubriqueDto>>> GetRubriquesDisponiblesAsync(int employeeId, int cabinetId, int? clientId = null);
    Task<ApiResponse<AffectationRubriqueDto>> AddAffectationAsync(AffectationRubriqueCreateDto dto, int cabinetId, string userId);
    Task<ApiResponse<AffectationRubriqueDto>> UpdateAffectationAsync(int id, AffectationRubriqueCreateDto dto, int cabinetId, string userId);
    Task<ApiResponse<bool>> RemoveAffectationAsync(int id, int cabinetId, string userId);
}

public interface IDocumentService
{
    Task<ApiResponse<PagedResult<DocumentDto>>> GetAllAsync(int cabinetId, int? clientId = null, string? categorie = null, int page = 1, int pageSize = 20, string? search = null);
    Task<ApiResponse<List<DocumentDto>>> GetByDeclarationIdAsync(int declarationId, int cabinetId);
    Task<ApiResponse<DocumentDto>> GetByIdAsync(int id, int cabinetId);
    Task<ApiResponse<DocumentDto>> UploadAsync(Stream fileStream, string fileName, string contentType, long size, string titre, string categorie, int? clientId, int cabinetId, string userId, int? declarationId = null, int? checklistItemId = null);
    Task<ApiResponse<bool>> ChangeStatusAsync(int id, string newStatut, int cabinetId, string userId, string? motif = null);
    Task<ApiResponse<bool>> ToggleShareAsync(int id, int cabinetId, string userId);
    Task<ApiResponse<Stream>> DownloadAsync(int id, int cabinetId);
    Task<int?> GetDeclarationClientIdAsync(int declarationId, int cabinetId);
}

public interface INotificationService
{
    Task<ApiResponse<List<NotificationDto>>> GetUserNotificationsAsync(string userId, bool unreadOnly = false, int count = 20);
    Task<ApiResponse<int>> GetUnreadCountAsync(string userId);
    Task<ApiResponse<bool>> MarkAsReadAsync(int id, string userId);
    Task<ApiResponse<bool>> MarkAllAsReadAsync(string userId);
    Task CreateAsync(string userId, string titre, string message, string type = "Info", string? module = null, string? url = null, int? cabinetId = null);
}

public interface IMessageService
{
    Task<ApiResponse<List<MessageDto>>> GetByDeclarationAsync(int declarationId, int cabinetId, int? clientId = null);
    Task<ApiResponse<List<MessageDto>>> GetByClientAsync(int clientId, int cabinetId);
    Task<ApiResponse<List<MessageDto>>> GetConversationsAsync(int cabinetId, string? userId = null, int? clientId = null);
    Task<ApiResponse<MessageDto>> SendAsync(SendMessageDto dto, int cabinetId, string userId, string userName, string senderRole);
    Task<ApiResponse<bool>> MarkAsReadAsync(int id, string userId);
    Task<ApiResponse<int>> GetUnreadCountAsync(int cabinetId, string? userId = null, int? clientId = null);
}

public interface ITaskService
{
    /// <param name="assignedToKind">Optional: "Client" = assignee is extranet user; "Employee" = cabinet staff (or unassigned).</param>
    Task<ApiResponse<PagedResult<TaskItemDto>>> GetAllAsync(int cabinetId, string? assignedToUserId = null, string? statut = null, int? clientId = null, string? assignedToKind = null, int page = 1, int pageSize = 20);
    Task<ApiResponse<TaskItemDto>> GetByIdAsync(int id, int cabinetId);
    Task<ApiResponse<TaskItemDto>> CreateAsync(TaskItemDto dto, int cabinetId, string userId);
    Task<ApiResponse<TaskItemDto>> UpdateAsync(int id, TaskItemDto dto, int cabinetId, string userId);
    Task<ApiResponse<bool>> ChangeStatusAsync(int id, string newStatut, int cabinetId, string userId);
    Task<ApiResponse<TaskCommentDto>> AddCommentAsync(int taskId, string contenu, int cabinetId, string userId, string userName);
}

public interface IDashboardService
{
    Task<ApiResponse<DashboardDto>> GetManagerDashboardAsync(int cabinetId, string userId);
    Task<ApiResponse<DashboardDto>> GetAssistantDashboardAsync(int cabinetId, string userId);
    Task<ApiResponse<ExtranetDashboardDto>> GetExtranetDashboardAsync(int clientId);
}

public interface IReferenceDataService
{
    Task<ApiResponse<List<Domain.Common.ReferenceData>>> GetByCategoryAsync(string category, int? cabinetId = null);
    Task<ApiResponse<Domain.Common.ReferenceData>> CreateOrUpdateAsync(Domain.Common.ReferenceData data);
    Task<decimal> GetNumericValueAsync(string category, string code);
}

/// <summary>
/// Service for cabinet management operations: employees (BackOffice users), roles, permissions, and cabinet info.
/// </summary>
public interface ICabinetService
{
    // ── Cabinet Info ─────────────────────────────────────────────
    Task<ApiResponse<CabinetDetailDto>> GetCabinetAsync(int cabinetId);
    Task<ApiResponse<CabinetDetailDto>> UpdateCabinetAsync(int cabinetId, CabinetEditDto dto, string userId);

    // ── Cabinet Employees (BackOffice Users) ─────────────────────
    Task<ApiResponse<PagedResult<CabinetEmployeeDto>>> GetEmployeesAsync(int cabinetId, string? departement = null, int page = 1, int pageSize = 20, string? search = null);
    Task<ApiResponse<CabinetEmployeeDto>> GetEmployeeByIdAsync(string id, int cabinetId);
    Task<ApiResponse<CabinetEmployeeDto>> CreateEmployeeAsync(CabinetEmployeeCreateDto dto, int cabinetId, string userId);
    Task<ApiResponse<CabinetEmployeeDto>> UpdateEmployeeAsync(string id, CabinetEmployeeUpdateDto dto, int cabinetId, string userId);
    Task<ApiResponse<bool>> ToggleEmployeeActiveAsync(string id, int cabinetId, string userId);

    // ── Roles & Permissions ──────────────────────────────────────
    Task<ApiResponse<PagedResult<RoleDto>>> GetRolesAsync(int cabinetId, int page = 1, int pageSize = 20, string? search = null);
    Task<ApiResponse<RoleDto>> GetRoleByIdAsync(string roleId, int cabinetId);
    Task<ApiResponse<RoleDto>> CreateRoleAsync(string name, string? description, List<string> permissions, int cabinetId, string userId);
    Task<ApiResponse<RoleDto>> UpdateRolePermissionsAsync(string roleId, List<string> permissions, int cabinetId, string userId);

    // ── Audit Logs ───────────────────────────────────────────────
    Task<ApiResponse<PagedResult<AuditLogDto>>> GetAuditLogsAsync(int cabinetId, string? userId = null, string? action = null, DateTime? dateDebut = null, DateTime? dateFin = null, int page = 1, int pageSize = 50);

    // ── User Profile ─────────────────────────────────────────────
    Task<ApiResponse<UserProfileDto>> GetProfileAsync(string userId, int cabinetId);
}

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║                     WORKFLOW ENGINE — SERVICE INTERFACES                ║
// ╚══════════════════════════════════════════════════════════════════════════╝

/// <summary>
/// Service for department management within a cabinet.
/// </summary>
public interface IDepartmentService
{
    Task<ApiResponse<List<DepartmentDto>>> GetAllAsync(int cabinetId);
    Task<ApiResponse<DepartmentDto>> GetByIdAsync(int id, int cabinetId);
    Task<ApiResponse<DepartmentDto>> CreateAsync(DepartmentCreateDto dto, int cabinetId, string userId);
    Task<ApiResponse<DepartmentDto>> UpdateAsync(int id, DepartmentCreateDto dto, int cabinetId, string userId);
    Task<ApiResponse<bool>> DeleteAsync(int id, int cabinetId, string userId);
    Task<ApiResponse<bool>> AddMemberAsync(int departmentId, string userId, bool isLead, int cabinetId, string actionUserId);
    Task<ApiResponse<bool>> RemoveMemberAsync(int departmentId, string userId, int cabinetId, string actionUserId);
    Task<ApiResponse<List<DepartmentMemberDto>>> GetMembersAsync(int departmentId, int cabinetId);
}

/// <summary>
/// Core workflow engine service — manages templates, instances, step execution,
/// task auto-creation, notifications, and workflow lifecycle.
/// </summary>
public interface IWorkflowService
{
    // ── Templates ────────────────────────────────────────────────
    Task<ApiResponse<PagedResult<WorkflowTemplateDto>>> GetTemplatesAsync(int cabinetId, string? declarationType = null, int? clientId = null, int page = 1, int pageSize = 20);
    Task<ApiResponse<WorkflowTemplateDto>> GetTemplateByIdAsync(int id, int cabinetId);
    Task<ApiResponse<WorkflowTemplateDto>> CreateTemplateAsync(WorkflowTemplateCreateDto dto, int cabinetId, string userId);
    Task<ApiResponse<WorkflowTemplateDto>> UpdateTemplateAsync(int id, WorkflowTemplateCreateDto dto, int cabinetId, string userId);
    Task<ApiResponse<bool>> DeleteTemplateAsync(int id, int cabinetId, string userId);
    Task<ApiResponse<WorkflowTemplateDto>> CreateDefaultTemplateAsync(int cabinetId, string userId);
    Task<ApiResponse<List<WorkflowTemplateDto>>> SeedAllDefaultTemplatesAsync(int cabinetId, string userId);
    Task<ApiResponse<WorkflowTemplateDto>> RefreshTvaDefaultTemplateAsync(int cabinetId, string userId);
    Task<ApiResponse<WorkflowTemplateDto>> RefreshIsDefaultTemplateAsync(int cabinetId, string userId);
    Task<int> ProcessDueRelancesAsync();

    // ── Instances (Workflow Execution) ───────────────────────────
    Task<ApiResponse<WorkflowInstanceDto>> StartWorkflowAsync(StartWorkflowDto dto, int cabinetId, string userId);
    Task<ApiResponse<WorkflowInstanceDto>> GetInstanceByIdAsync(int id, int cabinetId);
    Task<ApiResponse<PagedResult<WorkflowInstanceDto>>> GetInstancesAsync(int cabinetId, int? clientId = null, string? status = null, int? declarationId = null, int page = 1, int pageSize = 20);
    Task<ApiResponse<bool>> CancelWorkflowAsync(int instanceId, string reason, int cabinetId, string userId);
    Task<ApiResponse<bool>> PauseWorkflowAsync(int instanceId, int cabinetId, string userId);
    Task<ApiResponse<bool>> ResumeWorkflowAsync(int instanceId, int cabinetId, string userId);

    // ── Step Operations ──────────────────────────────────────────
    Task<ApiResponse<WorkflowStepInstanceDto>> AdvanceStepAsync(int instanceId, AdvanceStepDto dto, int cabinetId, string userId);
    /// <summary>Refuse current step and reopen the previous actionable step (Contrôle → Saisie, etc.).</summary>
    Task<ApiResponse<WorkflowStepInstanceDto>> RejectStepAsync(int instanceId, RejectStepDto dto, int cabinetId, string userId);
    Task<ApiResponse<WorkflowStepInstanceDto>> ClaimStepAsync(int instanceId, int cabinetId, string userId, bool forceOverride = false, string? reason = null);
    Task<ApiResponse<bool>> ReassignStepAsync(int stepInstanceId, ReassignStepDto dto, int cabinetId, string userId);
    Task<ApiResponse<WorkflowStepInstanceDto>> GetCurrentStepAsync(int instanceId, int cabinetId);
    Task<ApiResponse<List<WorkflowStepInstanceDto>>> GetMyPendingStepsAsync(int cabinetId, string userId, int? clientId = null);
    /// <summary>Create/link declarations for active typed workflows missing DeclarationId (Extranet Mes déclarations).</summary>
    Task HealMissingDeclarationsForClientAsync(int cabinetId, int clientId, string userId);

    // ── Client Workflow Config ───────────────────────────────────
    Task<ApiResponse<List<ClientWorkflowConfigDto>>> GetClientConfigsAsync(int clientId, int cabinetId);
    Task<ApiResponse<ClientWorkflowConfigDto>> SaveClientConfigAsync(ClientWorkflowConfigCreateDto dto, int cabinetId, string userId);
    /// <summary>Clone a cabinet template into a client-owned copy (or return existing). Used by Personnaliser.</summary>
    Task<ApiResponse<WorkflowTemplateDto>> EnsureClientTemplateAsync(int clientId, int sourceTemplateId, string? declarationType, int cabinetId, string userId);
    /// <summary>Save client config + update the client-owned template steps in one shot.</summary>
    Task<ApiResponse<ClientWorkflowConfigDto>> SaveClientWorkflowCustomizationAsync(ClientWorkflowCustomizeDto dto, int cabinetId, string userId);

    // ── Dashboard / Monitoring ───────────────────────────────────
    Task<ApiResponse<WorkflowDashboardDto>> GetDashboardAsync(int cabinetId, string userId);
    Task<ApiResponse<List<WorkflowHistoryDto>>> GetHistoryAsync(int instanceId, int cabinetId);

    // ── Template Resolution ──────────────────────────────────────
    /// <summary>
    /// Resolves the most specific workflow template for a given client + declaration type.
    /// Priority: Client+Type > Client+All > Type > Global default.
    /// </summary>
    Task<WorkflowTemplate?> ResolveTemplateAsync(int cabinetId, int clientId, string? declarationType);
}

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║             DOCUMENTS COMMERCIAUX — SERVICE INTERFACE                  ║
// ║  Devis → Bon de Commande → Contrat → Facture + Paiements             ║
// ╚══════════════════════════════════════════════════════════════════════════╝

public interface ICommercialService
{
    // ── Devis ────────────────────────────────────────────────────
    Task<ApiResponse<PagedResult<DevisDto>>> GetDevisAsync(int cabinetId, int? clientId = null, string? statut = null, int page = 1, int pageSize = 20);
    Task<ApiResponse<DevisDto>> GetDevisByIdAsync(int id, int cabinetId);
    Task<ApiResponse<DevisDto>> CreateDevisAsync(DevisCreateDto dto, int cabinetId, string userId);
    Task<ApiResponse<DevisDto>> UpdateDevisAsync(int id, DevisCreateDto dto, int cabinetId, string userId);
    Task<ApiResponse<bool>> DeleteDevisAsync(int id, int cabinetId, string userId);
    Task<ApiResponse<DevisDto>> SoumettreDevisAsync(int id, int cabinetId, string userId);
    Task<ApiResponse<DevisDto>> ApprouverDevisAsync(int id, int cabinetId, string userId);
    Task<ApiResponse<DevisDto>> RejeterDevisAsync(int id, string motif, int cabinetId, string userId);
    Task<ApiResponse<DevisDto>> DupliquerDevisAsync(int id, int cabinetId, string userId);

    // ── Conversion Devis → Bon de Commande ──────────────────────
    Task<ApiResponse<BonCommandeDto>> ConvertirDevisEnBonCommandeAsync(int devisId, int cabinetId, string userId);

    // ── Conversion Devis → Contrat (directe) ────────────────────
    Task<ApiResponse<ContratCommercialDto>> ConvertirDevisEnContratAsync(ConvertDevisEnContratDto dto, int cabinetId, string userId);

    // ── Bons de Commande ─────────────────────────────────────────
    Task<ApiResponse<PagedResult<BonCommandeDto>>> GetBonsCommandeAsync(int cabinetId, int? clientId = null, string? statut = null, int page = 1, int pageSize = 20);
    Task<ApiResponse<BonCommandeDto>> GetBonCommandeByIdAsync(int id, int cabinetId);
    Task<ApiResponse<BonCommandeDto>> ConfirmerBonCommandeAsync(int id, int cabinetId, string userId);
    Task<ApiResponse<BonCommandeDto>> ApprouverBonCommandeAsync(int id, int cabinetId, string userId);
    Task<ApiResponse<BonCommandeDto>> DupliquerBonCommandeAsync(int id, int cabinetId, string userId);

    // ── Conversion Bon de Commande → Contrat ─────────────────────
    Task<ApiResponse<ContratCommercialDto>> ConvertirBonCommandeEnContratAsync(ConvertBonCommandeEnContratDto dto, int cabinetId, string userId);

    // ── Contrats ─────────────────────────────────────────────────
    Task<ApiResponse<PagedResult<ContratCommercialDto>>> GetContratsAsync(int cabinetId, int? clientId = null, string? statut = null, int page = 1, int pageSize = 20);
    Task<ApiResponse<ContratCommercialDto>> GetContratByIdAsync(int id, int cabinetId);
    Task<ApiResponse<ContratCommercialDto>> CreateContratAsync(ContratCommercialCreateDto dto, int cabinetId, string userId);
    Task<ApiResponse<ContratCommercialDto>> UpdateContratAsync(int id, ContratCommercialCreateDto dto, int cabinetId, string userId);
    Task<ApiResponse<ContratCommercialDto>> UpdateContratEquipeAsync(int id, ContratEquipeUpdateDto dto, int cabinetId, string userId);
    Task<ApiResponse<ContratCommercialDto>> SignerContratAsync(int id, ContratSignatureDto dto, int cabinetId, string userId);
    Task<ApiResponse<ContratCommercialDto>> ValiderContratAsync(int id, int cabinetId, string userId);
    Task<ApiResponse<ContratCommercialDto>> SuspendreContratAsync(int id, int cabinetId, string userId);
    Task<ApiResponse<ContratCommercialDto>> ReactiverContratAsync(int id, int cabinetId, string userId);
    Task<ApiResponse<ContratCommercialDto>> TerminerContratAsync(int id, int cabinetId, string userId);
    Task<ApiResponse<ContratCommercialDto>> DupliquerContratAsync(int id, int cabinetId, string userId);

    /// <summary>
    /// Re-sync services from a specific validated contract.
    /// Used when services need to be re-activated (e.g. after code migration).
    /// </summary>
    Task<ApiResponse<int>> SyncServicesFromContratAsync(int contratId, int cabinetId, string userId);

    // ── Factures ─────────────────────────────────────────────────
    Task<ApiResponse<PagedResult<FactureDto>>> GetFacturesAsync(int cabinetId, int? clientId = null, string? statut = null, int page = 1, int pageSize = 20);
    Task<ApiResponse<FactureDto>> GetFactureByIdAsync(int id, int cabinetId);
    Task<ApiResponse<FactureDto>> CreateFactureAsync(FactureCreateDto dto, int cabinetId, string userId);
    Task<ApiResponse<FactureDto>> EnvoyerFactureAsync(int id, int cabinetId, string userId);
    Task<ApiResponse<FactureDto>> AnnulerFactureAsync(int id, int cabinetId, string userId);
    Task<ApiResponse<FactureDto>> DupliquerFactureAsync(int id, int cabinetId, string userId);

    // ── Paiements ────────────────────────────────────────────────
    Task<ApiResponse<PaiementDto>> EnregistrerPaiementAsync(PaiementCreateDto dto, int cabinetId, string userId);
    Task<ApiResponse<List<PaiementDto>>> GetPaiementsFactureAsync(int factureId, int cabinetId);

    // ── Dashboard ────────────────────────────────────────────────
    Task<ApiResponse<DashboardCommercialDto>> GetDashboardAsync(int cabinetId);
}


// Workflow Scheduler Service
public interface IWorkflowSchedulerService
{
    Task SyncScheduledJobsAsync();
    Task ExecuteScheduledStepAsync(int stepTemplateId, int cabinetId);
    Task ProcessDueRelancesAsync();
}

// ===== Client Users (Extranet) =====
public interface IClientUserService
{
    Task<ApiResponse<List<ClientUserDto>>> GetClientUsersAsync(int clientId, int cabinetId);
    Task<ApiResponse<ClientUserDto>> GetClientUserAsync(string userId, int clientId, int cabinetId);
    Task<ApiResponse<ClientUserDto>> CreateClientUserAsync(ClientUserCreateDto dto, int clientId, int cabinetId, string createdBy);
    Task<ApiResponse<ClientUserDto>> UpdateClientUserAsync(string userId, ClientUserUpdateDto dto, int clientId, int cabinetId, string updatedBy);
    Task<ApiResponse<bool>> ToggleClientUserActiveAsync(string userId, int clientId, int cabinetId, string updatedBy);
}

// ===== Comptabilité PCGM =====
public interface IComptabiliteService
{
    Task EnsureClientLedgerAsync(int clientId, int cabinetId, string? userId = null);
    Task<ApiResponse<List<ExerciceComptableDto>>> GetExercicesAsync(int clientId, int cabinetId);
    Task<ApiResponse<ExerciceComptableDto>> CreateExerciceAsync(ExerciceCreateDto dto, int cabinetId, string userId);
    Task<ApiResponse<ExerciceComptableDto>> GetExerciceByIdAsync(int id, int cabinetId);
    Task<ApiResponse<ComptaDashboardDto>> GetDashboardAsync(int clientId, int cabinetId, int? exerciceId = null);
    Task<ApiResponse<List<CompteComptableDto>>> GetComptesAsync(int clientId, int cabinetId, string? search = null);
    Task<ApiResponse<CompteComptableDto>> CreateCompteAsync(CompteComptableCreateDto dto, int cabinetId, string userId);
    Task<ApiResponse<CompteComptableDto>> UpdateCompteAsync(int id, CompteComptableUpdateDto dto, int cabinetId, string userId);
    Task<ApiResponse<CompteImportResultDto>> ImportComptesAsync(int clientId, string csvContent, int cabinetId, string userId);
    Task<ApiResponse<CompteImportResultDto>> ImportJournauxAsync(int clientId, string csvContent, int cabinetId, string userId);
    Task<ApiResponse<CompteImportResultDto>> ImportEcrituresAsync(int clientId, string csvContent, int cabinetId, string userId, int? exerciceId = null, bool valider = false);
    Task<ApiResponse<CompteImportResultDto>> ImportRapprochementsAsync(int clientId, string csvContent, int cabinetId, string userId);
    Task<ApiResponse<CompteImportResultDto>> ImportSoldesOuvertureAsync(int clientId, string csvContent, int cabinetId, string userId, int? exerciceId = null);
    Task<ApiResponse<List<JournalDto>>> GetJournauxAsync(int clientId, int cabinetId);
    Task<ApiResponse<JournalDto>> UpdateJournalAsync(int id, JournalUpdateDto dto, int cabinetId, string userId);
    Task<ApiResponse<PagedResult<EcritureComptableDto>>> GetEcrituresAsync(int cabinetId, int? clientId, string? journal, int page, string? search, string? statut = null, int? exerciceId = null);
    Task<ApiResponse<EcritureDetailDto>> GetEcritureByIdAsync(int id, int cabinetId);
    Task<ApiResponse<EcritureComptableDto>> CreateEcritureAsync(EcritureCreateDto dto, int cabinetId, string userId);
    Task<ApiResponse<EcritureComptableDto>> UpdateEcritureAsync(int id, EcritureCreateDto dto, int cabinetId, string userId);
    Task<ApiResponse<bool>> DeleteEcritureAsync(int id, int cabinetId, string userId);
    Task<ApiResponse<EcritureComptableDto>> ValidateEcritureAsync(int id, int cabinetId, string userId);
    Task<ApiResponse<List<GrandLivreDto>>> GetGrandLivreAsync(int clientId, int cabinetId, DateTime? dateDebut, DateTime? dateFin, string? numCompte = null, int? exerciceId = null);
    Task<ApiResponse<List<BalanceDto>>> GetBalanceAsync(int clientId, int cabinetId, DateTime? dateDebut, DateTime? dateFin, int? exerciceId = null);
    Task<ApiResponse<List<BalanceAgeeDto>>> GetBalanceAgeeAsync(int clientId, int cabinetId);
    Task<ApiResponse<List<RapprochementBancaireDto>>> GetRapprochementsAsync(int cabinetId, int? clientId);
    Task<ApiResponse<RapprochementBancaireDto>> CreateRapprochementAsync(RapprochementCreateDto dto, int cabinetId, string userId);
    Task<ApiResponse<RapprochementBancaireDto>> UpdateRapprochementAsync(int id, RapprochementUpdateDto dto, int cabinetId, string userId);
}
