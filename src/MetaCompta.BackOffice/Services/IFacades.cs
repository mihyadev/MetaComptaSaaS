using MetaCompta.Application.DTOs;

namespace MetaCompta.BackOffice.Services;

/// <summary>
/// BackOffice facade service for Reference Data API operations.
/// </summary>
public interface IReferenceDataFacade
{
    /// <summary>Load multiple reference data categories in one call.</summary>
    Task<Dictionary<string, List<ReferenceDataDto>>> GetMultipleAsync(params string[] categories);
}

/// <summary>
/// BackOffice facade service for Client API operations.
/// </summary>
public interface IClientFacade
{
    Task<PagedResult<ClientDto>?> GetAllAsync(int page = 1, string? search = null, string? statut = null);
    Task<ClientDto?> GetByIdAsync(int id);
    Task<ApiResponse<ClientDto>?> CreateAsync(ClientCreateDto dto);
    Task<ApiResponse<ClientDto>?> UpdateAsync(int id, ClientCreateDto dto);
    Task<ApiResponse<bool>?> DeleteAsync(int id);
    /// <summary>Load all clients for dropdown (no paging).</summary>
    Task<List<ClientDto>> GetAllForDropdownAsync();
    /// <summary>Get active service type codes for a client (e.g. ["TVA","IS","Paie"]).</summary>
    Task<List<string>> GetActiveServicesAsync(int clientId);
    /// <summary>Returns true if the client has Manage-mode access (Valide/Actif + dates).</summary>
    Task<bool> HasActiveContractAsync(int clientId);
    /// <summary>Manage vs Consult context for the selected client dossier.</summary>
    Task<ContractAccessContextDto> GetContractAccessAsync(int clientId, int? exerciceId = null);
    /// <summary>Get all service contracts for a client</summary>
    Task<List<ServiceContratDto>> GetServiceContratsAsync(int clientId, int? exerciceFiscal = null);
    /// <summary>Initialize services for a new exercise year</summary>
    Task<ApiResponse<List<ServiceContratDto>>?> InitializeServicesForExerciceAsync(int clientId, int exerciceFiscal);
    /// <summary>Auto-generate declarations from active service contracts</summary>
    Task<ApiResponse<GenerateDeclarationsResultDto>?> GenerateDeclarationsAsync(int clientId, GenerateDeclarationsRequestDto request);
    // ── Client Users (Extranet) ──────────────────────────────────
    Task<List<ClientUserDto>> GetClientUsersAsync(int clientId);
    Task<ApiResponse<ClientUserDto>?> CreateClientUserAsync(int clientId, ClientUserCreateDto dto);
    Task<ApiResponse<ClientUserDto>?> UpdateClientUserAsync(int clientId, string userId, ClientUserUpdateDto dto);
    Task<ApiResponse<bool>?> ToggleClientUserActiveAsync(int clientId, string userId);
}

/// <summary>
/// BackOffice facade service for Employee API operations.
/// </summary>
public interface IEmployeeFacade
{
    Task<PagedResult<EmployeeDto>?> GetAllAsync(int? clientId = null, int page = 1, string? search = null);
    Task<EmployeeDto?> GetByIdAsync(int id);
    Task<ApiResponse<EmployeeDto>?> CreateAsync(EmployeeCreateDto dto);
    Task<ApiResponse<EmployeeDto>?> UpdateAsync(int id, EmployeeCreateDto dto);
    Task<ApiResponse<bool>?> DeleteAsync(int id);
}

/// <summary>
/// BackOffice facade service for Declaration API operations.
/// </summary>
public interface IDeclarationFacade
{
    Task<PagedResult<DeclarationDto>?> GetAllAsync(int? clientId = null, string? type = null, string? statut = null, int page = 1, int? exerciceId = null);
    Task<DeclarationDto?> GetByIdAsync(int id);
    // Generic create
    Task<ApiResponse<DeclarationDto>?> CreateAsync(CreateDeclarationDto dto);
    Task<ApiResponse<DeclarationDto>?> SaveShellAsync(int declarationId, decimal? montantDu, decimal? montantCredit, string? notes);
    Task<ApiResponse<DeclarationDto>?> SuggestAcompteAsync(int declarationId);
    Task<ApiResponse<DeclarationDto>?> SuggestRegIsAsync(int declarationId);
    Task<(byte[]? Bytes, string? Error)> ExportCnssDnsCsvAsync(int declarationId);
    Task<CnssEmployeeRosterDto?> GetCnssEmployeesAsync(int declarationId);
    Task<ApiResponse<CnssEmployeeRosterDto>?> AddCnssEmployeeAsync(int declarationId, CnssEmployeeAddDto dto);
    Task<ApiResponse<CnssEmployeeRosterDto>?> UpdateCnssEmployeeAsync(int declarationId, int employeeId, CnssEmployeeAddDto dto);
    Task<ApiResponse<CnssEmployeeImportResultDto>?> ImportCnssEmployeesAsync(int declarationId, List<CnssEmployeeImportRowDto> rows);
    Task<ApiResponse<bool>?> DeleteAsync(int id);
    // TVA
    Task<ApiResponse<DeclarationDto>?> CreateTVAAsync(int clientId);
    Task<DeclarationTVADto?> GetTVADataAsync(int declarationId);
    Task<ApiResponse<DeclarationTVADto>?> SaveTVADataAsync(int declarationId, DeclarationTVADto dto);
    Task<ApiResponse<DeclarationTVADto>?> SaveTVAAjustementsAsync(int declarationId, decimal tvaFacturee, decimal tvaDeductible, decimal creditPrecedent);
    // TVA Lignes Factures
    Task<ApiResponse<TVALigneFactureDto>?> AddTVALigneAsync(int declarationId, TVALigneFactureDto ligne);
    Task<ApiResponse<bool>?> DeleteTVALigneAsync(int declarationId, int ligneId);
    Task<ApiResponse<List<TVALigneFactureDto>>?> ImportTVALignesAsync(int declarationId, List<TVALigneFactureDto> lignes);
    // IS
    Task<ApiResponse<DeclarationDto>?> CreateISAsync(int clientId, int anneeFiscale);
    Task<DeclarationISDto?> GetISDataAsync(int declarationId);
    Task<ApiResponse<DeclarationISDto>?> SaveISDataAsync(int declarationId, DeclarationISDto dto);
    Task<List<DeclarationChecklistItemDto>> GetChecklistAsync(int declarationId);
    Task<ApiResponse<List<DeclarationChecklistItemDto>>?> EnsureChecklistAsync(int declarationId);
    Task<ApiResponse<DeclarationChecklistItemDto>?> MarkChecklistReceivedAsync(int declarationId, int itemId, int? documentId = null);
    // IR
    Task<ApiResponse<DeclarationDto>?> CreateIRAsync(int clientId, int anneeFiscale);
    Task<DeclarationIRDto?> GetIRDataAsync(int declarationId);
    Task<ApiResponse<DeclarationIRDto>?> SaveIRDataAsync(int declarationId, DeclarationIRDto dto);
    // CNSS
    Task<ApiResponse<DeclarationDto>?> CreateCNSSAsync(int clientId);
    Task<DeclarationCNSSDto?> GetCNSSDataAsync(int declarationId);
    Task<ApiResponse<DeclarationCNSSDto>?> SaveCNSSDataAsync(int declarationId, DeclarationCNSSDto dto);
    Task<DeclarationCNSSDto?> CalculateCNSSAsync(int declarationId);
    // Status
    Task<ApiResponse<bool>?> ChangeStatusAsync(int id, string newStatut, string? comment = null);
    Task<List<DeclarationHistoryDto>> GetStatusHistoryAsync(int declarationId);
}

/// <summary>
/// BackOffice facade service for Payroll API operations.
/// </summary>
public interface IPayrollFacade
{
    Task<List<PayrollPeriodDto>> GetAllPeriodsAsync(int? clientId = null, int? exerciceId = null);
    Task<PayrollPeriodDto?> GetPeriodAsync(int id);
    Task<List<PaySlipDto>> GetPaySlipsAsync(int periodId);
    Task<ApiResponse<PayrollPeriodDto>?> InitializeAsync(int clientId, int annee, int mois);
    Task<ApiResponse<PayrollPeriodDto>?> CalculateAllAsync(int periodId);
    Task<ApiResponse<bool>?> ValidateAsync(int periodId);

    // Engine
    Task<BulletinCompletDto?> GetBulletinAsync(int paySlipId);
    Task<List<RubriqueDto>> GetRubriquesAsync(int? clientId = null);
    Task<RubriqueDto?> GetRubriqueAsync(int id);
    Task<ApiResponse<RubriqueDto>?> CreateRubriqueAsync(RubriqueCreateDto dto);
    Task<ApiResponse<RubriqueDto>?> UpdateRubriqueAsync(int id, RubriqueCreateDto dto);
    Task<ApiResponse<bool>?> ToggleRubriqueAsync(int id);
    Task<ApiResponse<bool>?> DeleteRubriqueAsync(int id);
    Task<List<ConstanteGlobaleDto>> GetConstantesAsync(string? categorie = null);
    Task<ApiResponse<ConstanteGlobaleDto>?> CreateConstanteAsync(ConstanteGlobaleDto dto);
    Task<ApiResponse<ConstanteGlobaleDto>?> UpdateConstanteAsync(int id, ConstanteGlobaleDto dto);
    Task<ApiResponse<bool>?> DeleteConstanteAsync(int id);
    Task<List<VariableEmployeDto>> GetVariablesEmployeAsync(int employeeId);
    Task<ApiResponse<VariableEmployeDto>?> CreateVariableAsync(VariableEmployeCreateDto dto);
    Task<ApiResponse<VariableEmployeDto>?> UpdateVariableAsync(int id, VariableEmployeCreateDto dto);
    Task<ApiResponse<bool>?> DeleteVariableAsync(int id);
    Task<List<CumulEmployeDto>> GetCumulsAsync(int employeeId, int annee);
    Task<ApiResponse<bool>?> SeedRubriquesAsync();
    Task<ApiResponse<bool>?> SeedConstantesAsync();
    Task<ApiResponse<bool>?> SeedDemo2025Async(int clientId);
    // Dynamic employee variables per period
    Task<List<VariableEmployeDto>> GetVariablesForPeriodAsync(int employeeId, int annee, int mois);
    Task<ApiResponse<bool>?> UpdateVariablesBulkAsync(int employeeId, int annee, int mois, List<VariableEmployeCreateDto> variables);

    // Affectation Rubriques (ERP-style: assign rubriques to employees)
    Task<List<AffectationRubriqueDto>> GetAffectationsAsync(int employeeId);
    Task<List<RubriqueDto>> GetRubriquesDisponiblesAsync(int employeeId, int? clientId = null);
    Task<ApiResponse<AffectationRubriqueDto>?> AddAffectationAsync(AffectationRubriqueCreateDto dto);
    Task<ApiResponse<AffectationRubriqueDto>?> UpdateAffectationAsync(int id, AffectationRubriqueCreateDto dto);
    Task<ApiResponse<bool>?> RemoveAffectationAsync(int id);
}

/// <summary>
/// BackOffice facade service for Dashboard API operations.
/// </summary>
public interface IDashboardFacade
{
    Task<DashboardDto?> GetManagerDashboardAsync();
}

// ============================================================================
// NEW MODULE FACADES
// ============================================================================

/// <summary>Facade for Document management operations.</summary>
public interface IDocumentFacade
{
    Task<PagedResult<DocumentDto>?> GetAllAsync(int? clientId = null, string? categorie = null, int page = 1, string? search = null);
    Task<List<DocumentDto>?> GetByDeclarationIdAsync(int declarationId);
    Task<DocumentDto?> GetByIdAsync(int id);
    Task<ApiResponse<DocumentDto>?> UploadAsync(Stream stream, string fileName, string contentType, long size, string titre, string categorie, int? clientId, int? declarationId = null);
    Task<ApiResponse<bool>?> ChangeStatusAsync(int id, string newStatut, string? motif = null);
    Task<ApiResponse<bool>?> ToggleShareAsync(int id);
}

/// <summary>Facade for Task management operations.</summary>
public interface ITaskFacade
{
    Task<PagedResult<TaskItemDto>?> GetAllAsync(string? assignedTo = null, string? statut = null, int? clientId = null, int page = 1, string? assignedToKind = null);
    Task<TaskItemDto?> GetByIdAsync(int id);
    Task<ApiResponse<TaskItemDto>?> CreateAsync(TaskItemDto dto);
    Task<ApiResponse<TaskItemDto>?> UpdateAsync(int id, TaskItemDto dto);
    Task<ApiResponse<bool>?> ChangeStatusAsync(int id, string newStatut);
    Task<ApiResponse<WorkflowStepInstanceDto>?> CompleteWorkflowStepAsync(int instanceId, string? notes = null);
    Task<ApiResponse<TaskCommentDto>?> AddCommentAsync(int taskId, string contenu);
}

/// <summary>Facade for Comptabilité (Accounting) operations.</summary>
public interface IComptabiliteFacade
{
    Task EnsureLedgerAsync(int clientId);
    Task<List<ExerciceComptableDto>> GetExercicesAsync(int clientId);
    Task<ApiResponse<ExerciceComptableDto>?> CreateExerciceAsync(ExerciceCreateDto dto);
    Task<ExerciceComptableDto?> GetExerciceByIdAsync(int id);
    Task<bool> HasOpenExerciceAsync(int clientId);
    Task<ApiResponse<ExerciceCloturePrecheckDto>?> PrecheckClotureAsync(int exerciceId);
    Task<ApiResponse<ExerciceClotureResultDto>?> CloturerExerciceAsync(ExerciceClotureRequestDto dto);
    Task<ApiResponse<ExerciceComptableDto>?> ReouvrirExerciceAsync(int exerciceId, string reason);
    Task<ComptaDashboardDto?> GetDashboardAsync(int clientId, int? exerciceId = null);
    Task<List<CompteComptableDto>> GetComptesAsync(int clientId, string? search = null);
    Task<ApiResponse<CompteComptableDto>?> CreateCompteAsync(CompteComptableCreateDto dto);
    Task<ApiResponse<CompteComptableDto>?> UpdateCompteAsync(int id, CompteComptableUpdateDto dto);
    Task<ApiResponse<CompteImportResultDto>?> ImportComptesAsync(int clientId, string csvContent);
    Task<ApiResponse<CompteImportResultDto>?> ImportJournauxAsync(int clientId, string csvContent);
    Task<ApiResponse<CompteImportResultDto>?> ImportEcrituresAsync(int clientId, string csvContent, int? exerciceId = null, bool valider = false);
    Task<ApiResponse<CompteImportResultDto>?> ImportRapprochementsAsync(int clientId, string csvContent);
    Task<ApiResponse<CompteImportResultDto>?> ImportSoldesOuvertureAsync(int clientId, string csvContent, int? exerciceId = null);
    Task<PagedResult<EcritureComptableDto>?> GetEcrituresAsync(int? clientId = null, string? journal = null, int page = 1, string? search = null, string? statut = null, int? exerciceId = null);
    Task<EcritureDetailDto?> GetEcritureByIdAsync(int id);
    Task<ApiResponse<EcritureComptableDto>?> CreateEcritureAsync(EcritureCreateDto dto);
    Task<ApiResponse<EcritureComptableDto>?> UpdateEcritureAsync(int id, EcritureCreateDto dto);
    Task<ApiResponse<bool>?> DeleteEcritureAsync(int id);
    Task<ApiResponse<EcritureComptableDto>?> ValidateEcritureAsync(int id);
    Task<List<JournalDto>> GetJournauxAsync(int? clientId = null);
    Task<ApiResponse<JournalDto>?> UpdateJournalAsync(int id, JournalUpdateDto dto);
    Task<List<GrandLivreDto>> GetGrandLivreAsync(int clientId, DateTime? dateDebut = null, DateTime? dateFin = null, int? exerciceId = null);
    Task<List<BalanceDto>> GetBalanceAsync(int clientId, DateTime? dateDebut = null, DateTime? dateFin = null, int? exerciceId = null);
    Task<List<BalanceAgeeDto>> GetBalanceAgeeAsync(int clientId);
    Task<List<RapprochementBancaireDto>> GetRapprochementsAsync(int? clientId = null);
    Task<ApiResponse<RapprochementBancaireDto>?> CreateRapprochementAsync(int clientId, string compteBancaire, string periode, decimal? soldeBanque = null);
    Task<ApiResponse<RapprochementBancaireDto>?> UpdateRapprochementAsync(int id, RapprochementUpdateDto dto);
}

/// <summary>Facade for Juridique (Legal) operations.</summary>
public interface IJuridiqueFacade
{
    Task<PagedResult<DossierJuridiqueDto>?> GetAllAsync(int? clientId = null, string? type = null, string? statut = null, int page = 1, string? search = null);
    Task<DossierJuridiqueDto?> GetByIdAsync(int id);
    Task<ApiResponse<DossierJuridiqueDto>?> CreateAsync(DossierJuridiqueCreateDto dto);
    Task<ApiResponse<DossierJuridiqueDto>?> UpdateAsync(int id, DossierJuridiqueCreateDto dto);
    Task<ApiResponse<bool>?> ChangeStatusAsync(int id, string newStatut);
}

/// <summary>Facade for Planning & Calendar operations.</summary>
public interface IPlanningFacade
{
    Task<List<PlanningEventDto>> GetEventsAsync(DateTime? debut = null, DateTime? fin = null, int? clientId = null, string? userId = null);
    Task<ApiResponse<PlanningEventDto>?> CreateAsync(PlanningEventDto dto);
    Task<ApiResponse<PlanningEventDto>?> UpdateAsync(PlanningEventDto dto);
    Task<ApiResponse<bool>?> DeleteAsync(int id);
}

/// <summary>Facade for Cabinet (Firm) management operations.</summary>
public interface ICabinetFacade
{
    // Cabinet Info
    Task<CabinetDetailDto?> GetCabinetAsync();
    Task<ApiResponse<CabinetDetailDto>?> UpdateCabinetAsync(CabinetEditDto dto);

    // Employees
    Task<PagedResult<CabinetEmployeeDto>?> GetEmployeesAsync(string? departement = null, int page = 1, string? search = null, int pageSize = 20);
    Task<CabinetEmployeeDto?> GetEmployeeByIdAsync(string id);
    Task<ApiResponse<CabinetEmployeeDto>?> CreateEmployeeAsync(CabinetEmployeeCreateDto dto);
    Task<ApiResponse<CabinetEmployeeDto>?> UpdateEmployeeAsync(string id, CabinetEmployeeUpdateDto dto);
    Task<ApiResponse<bool>?> ToggleEmployeeActiveAsync(string id);

    // Roles & Permissions
    Task<PagedResult<RoleDto>?> GetRolesAsync(int page = 1, string? search = null);
    Task<RoleDto?> GetRoleByIdAsync(string roleId);
    Task<ApiResponse<RoleDto>?> CreateRoleAsync(string name, string? description, List<string> permissions);
    Task<ApiResponse<RoleDto>?> UpdateRolePermissionsAsync(string roleId, List<string> permissions);

    // Audit
    Task<PagedResult<AuditLogDto>?> GetAuditLogsAsync(string? userId = null, string? action = null, DateTime? dateDebut = null, DateTime? dateFin = null, int page = 1);

    // Profile & Claims
    Task<UserProfileDto?> GetProfileAsync();
    Task<List<string>> GetAllClaimsAsync();
    Task<Dictionary<string, List<string>>> GetClaimsByDomainAsync();
}

/// <summary>Facade for Rapports (Reports) operations.</summary>
public interface IRapportFacade
{
    Task<PagedResult<RapportDto>?> GetAllAsync(int? clientId = null, string? type = null, int page = 1, string? search = null);
    Task<RapportDto?> GetByIdAsync(int id);
    Task<ApiResponse<RapportDto>?> GenerateAsync(string type, int? clientId, DateTime? dateDebut, DateTime? dateFin);
}

/// <summary>Facade for Paramètres (Settings) operations.</summary>
public interface IParametresFacade
{
    Task<List<SettingsDto>> GetAllAsync(string? categorie = null);
    Task<ApiResponse<SettingsDto>?> UpdateSettingAsync(string cle, string? valeur);
    Task<List<DocumentTemplateDto>> GetTemplatesAsync();
    Task<ApiResponse<DocumentTemplateDto>?> UpdateTemplateAsync(int id, string contenu);
}

/// <summary>Facade for États Fiscaux (Fiscal Statements) operations.</summary>
public interface IEtatFiscalFacade
{
    Task<PagedResult<EtatFiscalDto>?> GetAllAsync(int? clientId = null, string? type = null, int? exercice = null, int page = 1, string? search = null);
    Task<EtatFiscalDto?> GetByIdAsync(int id);
    Task<ApiResponse<EtatFiscalDto>?> GenerateAsync(int clientId, string type, int exercice);
    Task<ApiResponse<bool>?> ValidateAsync(int id);
}

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║                     WORKFLOW ENGINE — FACADES                           ║
// ╚══════════════════════════════════════════════════════════════════════════╝

/// <summary>Facade for Department management operations.</summary>
public interface IDepartmentFacade
{
    Task<List<DepartmentDto>> GetAllAsync();
    Task<DepartmentDto?> GetByIdAsync(int id);
    Task<ApiResponse<DepartmentDto>?> CreateAsync(DepartmentCreateDto dto);
    Task<ApiResponse<DepartmentDto>?> UpdateAsync(int id, DepartmentCreateDto dto);
    Task<ApiResponse<bool>?> DeleteAsync(int id);
    Task<ApiResponse<bool>?> AddMemberAsync(int departmentId, string userId, bool isLead);
    Task<ApiResponse<bool>?> RemoveMemberAsync(int departmentId, string userId);
    Task<List<DepartmentMemberDto>> GetMembersAsync(int departmentId);
}

/// <summary>Facade for Workflow Engine operations.</summary>
public interface IWorkflowFacade
{
    // Templates
    Task<PagedResult<WorkflowTemplateDto>?> GetTemplatesAsync(string? declarationType = null, int? clientId = null, int page = 1, int pageSize = 20, bool cabinetOnly = false, bool activeOnly = false, bool excludeAncien = false);
    Task<WorkflowTemplateDto?> GetTemplateByIdAsync(int id);
    Task<ApiResponse<WorkflowTemplateDto>?> CreateTemplateAsync(WorkflowTemplateCreateDto dto);
    Task<ApiResponse<WorkflowTemplateDto>?> UpdateTemplateAsync(int id, WorkflowTemplateCreateDto dto);
    Task<ApiResponse<bool>?> DeleteTemplateAsync(int id);
    Task<ApiResponse<WorkflowTemplateDto>?> CreateDefaultTemplateAsync();
    Task<ApiResponse<List<WorkflowTemplateDto>>?> SeedAllDefaultTemplatesAsync(bool forceReset = false);

    // Instances
    Task<ApiResponse<WorkflowInstanceDto>?> StartWorkflowAsync(StartWorkflowDto dto);
    Task<PagedResult<WorkflowInstanceDto>?> GetInstancesAsync(int? clientId = null, string? status = null, int? declarationId = null, int page = 1, int? exerciceId = null);
    Task<WorkflowInstanceDto?> GetInstanceByIdAsync(int id);
    Task<ApiResponse<bool>?> CancelWorkflowAsync(int id, string reason);
    Task<ApiResponse<bool>?> PauseWorkflowAsync(int id);
    Task<ApiResponse<bool>?> ResumeWorkflowAsync(int id);

    // Steps
    Task<ApiResponse<WorkflowStepInstanceDto>?> AdvanceStepAsync(int instanceId, AdvanceStepDto dto);
    Task<ApiResponse<WorkflowStepInstanceDto>?> RejectStepAsync(int instanceId, RejectStepDto dto);
    Task<ApiResponse<WorkflowStepInstanceDto>?> ClaimStepAsync(int instanceId, bool forceOverride = false, string? reason = null);
    Task<ApiResponse<bool>?> ReassignStepAsync(int stepInstanceId, ReassignStepDto dto);
    Task<WorkflowStepInstanceDto?> GetCurrentStepAsync(int instanceId);
    Task<List<WorkflowStepInstanceDto>> GetMyPendingStepsAsync(int? clientId = null);

    // Client Config
    Task<List<ClientWorkflowConfigDto>> GetClientConfigsAsync(int clientId);
    Task<ApiResponse<ClientWorkflowConfigDto>?> SaveClientConfigAsync(ClientWorkflowConfigCreateDto dto);
    Task<ApiResponse<bool>?> DeleteClientConfigAsync(int configId, int clientId);
    Task<ApiResponse<WorkflowTemplateDto>?> EnsureClientTemplateAsync(EnsureClientWorkflowDto dto);
    Task<ApiResponse<ClientWorkflowConfigDto>?> SaveClientWorkflowCustomizationAsync(ClientWorkflowCustomizeDto dto);

    // Dashboard & History
    Task<WorkflowDashboardDto?> GetDashboardAsync();
    Task<List<WorkflowHistoryDto>> GetHistoryAsync(int instanceId);
}

// ════════════════════════════════════════════════════════════════════════════════
//  DOCUMENTS COMMERCIAUX — Facade Interface
// ════════════════════════════════════════════════════════════════════════════════

/// <summary>Facade for Commercial Document lifecycle operations.</summary>
public interface ICommercialFacade
{
    // Devis
    Task<PagedResult<DevisDto>?> GetDevisAsync(int? clientId = null, string? statut = null, int page = 1);
    Task<DevisDto?> GetDevisByIdAsync(int id);
    Task<ApiResponse<DevisDto>?> CreateDevisAsync(DevisCreateDto dto);
    Task<ApiResponse<DevisDto>?> UpdateDevisAsync(int id, DevisCreateDto dto);
    Task<ApiResponse<bool>?> DeleteDevisAsync(int id);
    Task<ApiResponse<DevisDto>?> SoumettreDevisAsync(int id);
    Task<ApiResponse<DevisDto>?> ApprouverDevisAsync(int id);
    Task<ApiResponse<DevisDto>?> RejeterDevisAsync(int id, string motif);
    Task<ApiResponse<DevisDto>?> DupliquerDevisAsync(int id);
    Task<ApiResponse<BonCommandeDto>?> ConvertirDevisAsync(int id);
    Task<ApiResponse<ContratCommercialDto>?> ConvertirDevisEnContratAsync(int devisId, ConvertDevisEnContratDto dto);

    // Bons de Commande
    Task<PagedResult<BonCommandeDto>?> GetBonsCommandeAsync(int? clientId = null, string? statut = null, int page = 1);
    Task<BonCommandeDto?> GetBonCommandeByIdAsync(int id);
    Task<ApiResponse<BonCommandeDto>?> ConfirmerBonCommandeAsync(int id);
    Task<ApiResponse<BonCommandeDto>?> ApprouverBonCommandeAsync(int id);
    Task<ApiResponse<BonCommandeDto>?> DupliquerBonCommandeAsync(int id);
    Task<ApiResponse<ContratCommercialDto>?> ConvertirBonCommandeAsync(ConvertBonCommandeEnContratDto dto);

    // Contrats
    Task<PagedResult<ContratCommercialDto>?> GetContratsAsync(int? clientId = null, string? statut = null, int page = 1);
    Task<ContratCommercialDto?> GetContratByIdAsync(int id);
    Task<ApiResponse<ContratCommercialDto>?> CreateContratAsync(ContratCommercialCreateDto dto);
    Task<ApiResponse<ContratCommercialDto>?> UpdateContratAsync(int id, ContratCommercialCreateDto dto);
    Task<ApiResponse<ContratCommercialDto>?> UpdateContratEquipeAsync(int id, ContratEquipeUpdateDto dto);
    Task<ApiResponse<ContratCommercialDto>?> SignerContratAsync(int id, ContratSignatureDto dto);
    Task<ApiResponse<ContratCommercialDto>?> ValiderContratAsync(int id);
    Task<ApiResponse<ContratCommercialDto>?> SuspendreContratAsync(int id);
    Task<ApiResponse<ContratCommercialDto>?> ReactiverContratAsync(int id);
    Task<ApiResponse<ContratCommercialDto>?> TerminerContratAsync(int id);
    Task<ApiResponse<ContratCommercialDto>?> DupliquerContratAsync(int id);
    Task<ApiResponse<ContratCommercialDto>?> CreateRectificatifContratAsync(int id, string? motif = null);
    Task<ApiResponse<int>?> SyncServicesFromContratAsync(int contratId);

    // Factures
    Task<PagedResult<FactureDto>?> GetFacturesAsync(int? clientId = null, string? statut = null, int page = 1);
    Task<FactureDto?> GetFactureByIdAsync(int id);
    Task<ApiResponse<FactureDto>?> CreateFactureAsync(FactureCreateDto dto);
    Task<ApiResponse<FactureDto>?> EnvoyerFactureAsync(int id);
    Task<ApiResponse<FactureDto>?> AnnulerFactureAsync(int id);
    Task<ApiResponse<FactureDto>?> DupliquerFactureAsync(int id);

    // Paiements
    Task<ApiResponse<PaiementDto>?> EnregistrerPaiementAsync(int factureId, PaiementCreateDto dto);
    Task<List<PaiementDto>> GetPaiementsFactureAsync(int factureId);

    // Dashboard
    Task<DashboardCommercialDto?> GetDashboardAsync();
}

/// <summary>Facade for Messages (cabinet-client communication).</summary>
public interface IMessageFacade
{
    Task<List<MessageDto>?> GetByDeclarationAsync(int declarationId);
    Task<List<MessageDto>?> GetConversationsAsync();
    Task<ApiResponse<MessageDto>?> SendAsync(SendMessageDto dto);
    Task<int> GetUnreadCountAsync(int? clientId = null);
}
