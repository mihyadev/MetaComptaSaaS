using MetaCompta.Application.DTOs;

namespace MetaCompta.BackOffice.Services;

// ============================================================================
// Facade implementations — thin typed wrappers over IApiClient
// Each facade encapsulates URL construction, parameter serialization,
// and response unwrapping for a specific API domain.
// ============================================================================

public sealed class ReferenceDataFacade : IReferenceDataFacade
{
    private readonly IApiClient _api;

    public ReferenceDataFacade(IApiClient api) => _api = api;

    public async Task<Dictionary<string, List<ReferenceDataDto>>> GetMultipleAsync(params string[] categories)
    {
        var cats = string.Join(",", categories);
        var result = await _api.GetAsync<Dictionary<string, List<ReferenceDataDto>>>(
            $"api/referencedata/multi?categories={cats}");
        return result?.Data ?? new Dictionary<string, List<ReferenceDataDto>>();
    }
}

public sealed class ClientFacade : IClientFacade
{
    private readonly IApiClient _api;

    public ClientFacade(IApiClient api) => _api = api;

    public async Task<PagedResult<ClientDto>?> GetAllAsync(int page = 1, string? search = null, string? statut = null)
    {
        var url = QueryBuilder.For("api/clients")
            .Add("page", page)
            .Add("search", search)
            .Add("statut", statut);
        var result = await _api.GetAsync<PagedResult<ClientDto>>(url);
        return result?.Data;
    }

    public async Task<ClientDto?> GetByIdAsync(int id)
    {
        var result = await _api.GetAsync<ClientDto>($"api/clients/{id}");
        return result?.Data;
    }

    public async Task<ApiResponse<ClientDto>?> CreateAsync(ClientCreateDto dto)
        => await _api.PostAsync<ClientDto>("api/clients", dto);

    public async Task<ApiResponse<ClientDto>?> UpdateAsync(int id, ClientCreateDto dto)
        => await _api.PutAsync<ClientDto>($"api/clients/{id}", dto);

    public async Task<ApiResponse<bool>?> DeleteAsync(int id)
        => await _api.DeleteAsync<bool>($"api/clients/{id}");

    public async Task<List<ClientDto>> GetAllForDropdownAsync()
    {
        var result = await _api.GetAsync<PagedResult<ClientDto>>("api/clients?pageSize=500");
        return result?.Data?.Items ?? new List<ClientDto>();
    }

    public async Task<List<string>> GetActiveServicesAsync(int clientId)
    {
        try
        {
            var result = await _api.GetAsync<List<string>>($"api/clients/{clientId}/services");
            return result?.Data ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    public async Task<bool> HasActiveContractAsync(int clientId)
    {
        try
        {
            var result = await _api.GetAsync<bool>($"api/clients/{clientId}/has-active-contract");
            return result?.Data ?? false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<ContractAccessContextDto> GetContractAccessAsync(int clientId)
    {
        try
        {
            var result = await _api.GetAsync<ContractAccessContextDto>($"api/clients/{clientId}/contract-access");
            return result?.Data ?? new ContractAccessContextDto();
        }
        catch
        {
            return new ContractAccessContextDto();
        }
    }

    public async Task<List<ServiceContratDto>> GetServiceContratsAsync(int clientId, int? exerciceFiscal = null)
    {
        try
        {
            var url = $"api/clients/{clientId}/service-contrats";
            if (exerciceFiscal.HasValue) url += $"?exerciceFiscal={exerciceFiscal}";
            var result = await _api.GetAsync<List<ServiceContratDto>>(url);
            return result?.Data ?? new List<ServiceContratDto>();
        }
        catch { return new List<ServiceContratDto>(); }
    }

    public async Task<ApiResponse<List<ServiceContratDto>>?> InitializeServicesForExerciceAsync(int clientId, int exerciceFiscal)
        => await _api.PostAsync<List<ServiceContratDto>>($"api/clients/{clientId}/service-contrats/initialize?exerciceFiscal={exerciceFiscal}", new { });

    public async Task<ApiResponse<GenerateDeclarationsResultDto>?> GenerateDeclarationsAsync(int clientId, GenerateDeclarationsRequestDto request)
        => await _api.PostAsync<GenerateDeclarationsResultDto>($"api/clients/{clientId}/generate-declarations", request);

    // ── Client Users (Extranet) ──────────────────────────────────

    public async Task<List<ClientUserDto>> GetClientUsersAsync(int clientId)
    {
        try
        {
            var result = await _api.GetAsync<List<ClientUserDto>>($"api/clients/{clientId}/users");
            return result?.Data ?? new List<ClientUserDto>();
        }
        catch { return new List<ClientUserDto>(); }
    }

    public async Task<ApiResponse<ClientUserDto>?> CreateClientUserAsync(int clientId, ClientUserCreateDto dto)
        => await _api.PostAsync<ClientUserDto>($"api/clients/{clientId}/users", dto);

    public async Task<ApiResponse<ClientUserDto>?> UpdateClientUserAsync(int clientId, string userId, ClientUserUpdateDto dto)
        => await _api.PutAsync<ClientUserDto>($"api/clients/{clientId}/users/{userId}", dto);

    public async Task<ApiResponse<bool>?> ToggleClientUserActiveAsync(int clientId, string userId)
        => await _api.PutAsync<bool>($"api/clients/{clientId}/users/{userId}/toggle", new { });
}

public sealed class EmployeeFacade : IEmployeeFacade
{
    private readonly IApiClient _api;

    public EmployeeFacade(IApiClient api) => _api = api;

    public async Task<PagedResult<EmployeeDto>?> GetAllAsync(int? clientId = null, int page = 1, string? search = null)
    {
        var url = QueryBuilder.For("api/employees")
            .Add("page", page)
            .Add("search", search)
            .Add("clientId", clientId);
        var result = await _api.GetAsync<PagedResult<EmployeeDto>>(url);
        return result?.Data;
    }

    public async Task<EmployeeDto?> GetByIdAsync(int id)
    {
        var result = await _api.GetAsync<EmployeeDto>($"api/employees/{id}");
        return result?.Data;
    }

    public async Task<ApiResponse<EmployeeDto>?> CreateAsync(EmployeeCreateDto dto)
        => await _api.PostAsync<EmployeeDto>("api/employees", dto);

    public async Task<ApiResponse<EmployeeDto>?> UpdateAsync(int id, EmployeeCreateDto dto)
        => await _api.PutAsync<EmployeeDto>($"api/employees/{id}", dto);

    public async Task<ApiResponse<bool>?> DeleteAsync(int id)
        => await _api.DeleteAsync<bool>($"api/employees/{id}");
}

public sealed class DeclarationFacade : IDeclarationFacade
{
    private readonly IApiClient _api;

    public DeclarationFacade(IApiClient api) => _api = api;

    public async Task<PagedResult<DeclarationDto>?> GetAllAsync(int? clientId = null, string? type = null, string? statut = null, int page = 1)
    {
        var url = QueryBuilder.For("api/declarations")
            .Add("page", page)
            .Add("clientId", clientId)
            .Add("type", type)
            .Add("statut", statut);
        var result = await _api.GetAsync<PagedResult<DeclarationDto>>(url);
        return result?.Data;
    }

    public async Task<DeclarationDto?> GetByIdAsync(int id)
    {
        var result = await _api.GetAsync<DeclarationDto>($"api/declarations/{id}");
        return result?.Data;
    }

    // Generic create
    public async Task<ApiResponse<DeclarationDto>?> CreateAsync(CreateDeclarationDto dto)
        => await _api.PostAsync<DeclarationDto>("api/declarations", dto);

    public async Task<ApiResponse<bool>?> DeleteAsync(int id)
        => await _api.DeleteAsync<bool>($"api/declarations/{id}");

    // TVA
    public async Task<ApiResponse<DeclarationDto>?> CreateTVAAsync(int clientId)
        => await _api.PostAsync<DeclarationDto>($"api/declarations/tva/{clientId}", new { });

    public async Task<DeclarationTVADto?> GetTVADataAsync(int declarationId)
    {
        var result = await _api.GetAsync<DeclarationTVADto>($"api/declarations/tva/{declarationId}/data");
        return result?.Data;
    }

    public async Task<ApiResponse<DeclarationTVADto>?> SaveTVADataAsync(int declarationId, DeclarationTVADto dto)
        => await _api.PostAsync<DeclarationTVADto>($"api/declarations/tva/{declarationId}/data", dto);

    public async Task<ApiResponse<DeclarationTVADto>?> SaveTVAAjustementsAsync(int declarationId, decimal tvaFacturee, decimal tvaDeductible, decimal creditPrecedent)
        => await _api.PostAsync<DeclarationTVADto>($"api/declarations/tva/{declarationId}/ajustements", new { TVAFacturee = tvaFacturee, TVADeductible = tvaDeductible, CreditPrecedent = creditPrecedent });

    // TVA Lignes Factures
    public async Task<ApiResponse<TVALigneFactureDto>?> AddTVALigneAsync(int declarationId, TVALigneFactureDto ligne)
        => await _api.PostAsync<TVALigneFactureDto>($"api/declarations/tva/{declarationId}/lignes", ligne);

    public async Task<ApiResponse<bool>?> DeleteTVALigneAsync(int declarationId, int ligneId)
        => await _api.DeleteAsync<bool>($"api/declarations/tva/{declarationId}/lignes/{ligneId}");

    public async Task<ApiResponse<List<TVALigneFactureDto>>?> ImportTVALignesAsync(int declarationId, List<TVALigneFactureDto> lignes)
        => await _api.PostAsync<List<TVALigneFactureDto>>($"api/declarations/tva/{declarationId}/import-lignes", lignes);

    // IS
    public async Task<ApiResponse<DeclarationDto>?> CreateISAsync(int clientId, int anneeFiscale)
        => await _api.PostAsync<DeclarationDto>($"api/declarations/is/{clientId}/{anneeFiscale}", new { });

    public async Task<DeclarationISDto?> GetISDataAsync(int declarationId)
    {
        var result = await _api.GetAsync<DeclarationISDto>($"api/declarations/is/{declarationId}/data");
        return result?.Data;
    }

    public async Task<ApiResponse<DeclarationISDto>?> SaveISDataAsync(int declarationId, DeclarationISDto dto)
        => await _api.PostAsync<DeclarationISDto>($"api/declarations/is/{declarationId}/data", dto);

    // IR
    public async Task<ApiResponse<DeclarationDto>?> CreateIRAsync(int clientId, int anneeFiscale)
        => await _api.PostAsync<DeclarationDto>($"api/declarations/ir/{clientId}/{anneeFiscale}", new { });

    public async Task<DeclarationIRDto?> GetIRDataAsync(int declarationId)
    {
        var result = await _api.GetAsync<DeclarationIRDto>($"api/declarations/ir/{declarationId}/data");
        return result?.Data;
    }

    public async Task<ApiResponse<DeclarationIRDto>?> SaveIRDataAsync(int declarationId, DeclarationIRDto dto)
        => await _api.PostAsync<DeclarationIRDto>($"api/declarations/ir/{declarationId}/data", dto);

    // CNSS
    public async Task<ApiResponse<DeclarationDto>?> CreateCNSSAsync(int clientId)
        => await _api.PostAsync<DeclarationDto>($"api/declarations/cnss/{clientId}", new { });

    public async Task<DeclarationCNSSDto?> GetCNSSDataAsync(int declarationId)
    {
        var result = await _api.GetAsync<DeclarationCNSSDto>($"api/declarations/cnss/{declarationId}/data");
        return result?.Data;
    }

    public async Task<ApiResponse<DeclarationCNSSDto>?> SaveCNSSDataAsync(int declarationId, DeclarationCNSSDto dto)
        => await _api.PostAsync<DeclarationCNSSDto>($"api/declarations/cnss/{declarationId}/data", dto);

    public async Task<DeclarationCNSSDto?> CalculateCNSSAsync(int declarationId)
    {
        var result = await _api.PostAsync<DeclarationCNSSDto>($"api/declarations/cnss/{declarationId}/calculate", new { });
        return result?.Data;
    }

    // Status
    public async Task<ApiResponse<bool>?> ChangeStatusAsync(int id, string newStatut, string? comment = null)
        => await _api.PutAsync<bool>($"api/declarations/{id}/status", new { NewStatut = newStatut, Comment = comment });

    public async Task<List<DeclarationHistoryDto>> GetStatusHistoryAsync(int declarationId)
    {
        var result = await _api.GetAsync<List<DeclarationHistoryDto>>($"api/declarations/{declarationId}/history");
        return result?.Data ?? new List<DeclarationHistoryDto>();
    }
}

public sealed class PayrollFacade : IPayrollFacade
{
    private readonly IApiClient _api;

    public PayrollFacade(IApiClient api) => _api = api;

    public async Task<List<PayrollPeriodDto>> GetAllPeriodsAsync(int? clientId = null)
    {
        var url = QueryBuilder.For("api/payroll").Add("clientId", clientId);
        var result = await _api.GetAsync<List<PayrollPeriodDto>>(url);
        return result?.Data ?? new List<PayrollPeriodDto>();
    }

    public async Task<PayrollPeriodDto?> GetPeriodAsync(int id)
    {
        var result = await _api.GetAsync<PayrollPeriodDto>($"api/payroll/{id}");
        return result?.Data;
    }

    public async Task<List<PaySlipDto>> GetPaySlipsAsync(int periodId)
    {
        var result = await _api.GetAsync<List<PaySlipDto>>($"api/payroll/{periodId}/payslips");
        return result?.Data ?? new List<PaySlipDto>();
    }

    public async Task<ApiResponse<PayrollPeriodDto>?> InitializeAsync(int clientId, int annee, int mois)
        => await _api.PostAsync<PayrollPeriodDto>("api/payroll/initialize", new { ClientId = clientId, Annee = annee, Mois = mois });

    public async Task<ApiResponse<PayrollPeriodDto>?> CalculateAllAsync(int periodId)
        => await _api.PostAsync<PayrollPeriodDto>($"api/payroll/{periodId}/calculate", new { });

    public async Task<ApiResponse<bool>?> ValidateAsync(int periodId)
        => await _api.PostAsync<bool>($"api/payroll/{periodId}/validate", new { });

    // Engine
    public async Task<BulletinCompletDto?> GetBulletinAsync(int paySlipId)
    {
        var result = await _api.GetAsync<BulletinCompletDto>($"api/payroll/bulletin/{paySlipId}");
        return result?.Data;
    }

    public async Task<List<RubriqueDto>> GetRubriquesAsync(int? clientId = null)
    {
        var url = QueryBuilder.For("api/payroll/rubriques").Add("clientId", clientId);
        var result = await _api.GetAsync<List<RubriqueDto>>(url);
        return result?.Data ?? new List<RubriqueDto>();
    }

    public async Task<RubriqueDto?> GetRubriqueAsync(int id)
    {
        var result = await _api.GetAsync<RubriqueDto>($"api/payroll/rubriques/{id}");
        return result?.Data;
    }

    public async Task<ApiResponse<RubriqueDto>?> CreateRubriqueAsync(RubriqueCreateDto dto)
        => await _api.PostAsync<RubriqueDto>("api/payroll/rubriques", dto);

    public async Task<ApiResponse<RubriqueDto>?> UpdateRubriqueAsync(int id, RubriqueCreateDto dto)
        => await _api.PutAsync<RubriqueDto>($"api/payroll/rubriques/{id}", dto);

    public async Task<ApiResponse<bool>?> ToggleRubriqueAsync(int id)
        => await _api.PostAsync<bool>($"api/payroll/rubriques/{id}/toggle", new { });

    public async Task<ApiResponse<bool>?> DeleteRubriqueAsync(int id)
        => await _api.DeleteAsync<bool>($"api/payroll/rubriques/{id}");

    public async Task<List<ConstanteGlobaleDto>> GetConstantesAsync(string? categorie = null)
    {
        var url = QueryBuilder.For("api/payroll/constantes").Add("categorie", categorie);
        var result = await _api.GetAsync<List<ConstanteGlobaleDto>>(url);
        return result?.Data ?? new List<ConstanteGlobaleDto>();
    }

    public async Task<ApiResponse<ConstanteGlobaleDto>?> CreateConstanteAsync(ConstanteGlobaleDto dto)
        => await _api.PostAsync<ConstanteGlobaleDto>("api/payroll/constantes", dto);

    public async Task<ApiResponse<ConstanteGlobaleDto>?> UpdateConstanteAsync(int id, ConstanteGlobaleDto dto)
        => await _api.PutAsync<ConstanteGlobaleDto>($"api/payroll/constantes/{id}", dto);

    public async Task<ApiResponse<bool>?> DeleteConstanteAsync(int id)
        => await _api.DeleteAsync<bool>($"api/payroll/constantes/{id}");

    public async Task<List<VariableEmployeDto>> GetVariablesEmployeAsync(int employeeId)
    {
        var result = await _api.GetAsync<List<VariableEmployeDto>>($"api/payroll/variables/{employeeId}");
        return result?.Data ?? new List<VariableEmployeDto>();
    }

    public async Task<ApiResponse<VariableEmployeDto>?> CreateVariableAsync(VariableEmployeCreateDto dto)
        => await _api.PostAsync<VariableEmployeDto>("api/payroll/variables", dto);

    public async Task<ApiResponse<VariableEmployeDto>?> UpdateVariableAsync(int id, VariableEmployeCreateDto dto)
        => await _api.PutAsync<VariableEmployeDto>($"api/payroll/variables/{id}", dto);

    public async Task<ApiResponse<bool>?> DeleteVariableAsync(int id)
        => await _api.DeleteAsync<bool>($"api/payroll/variables/{id}");

    public async Task<List<CumulEmployeDto>> GetCumulsAsync(int employeeId, int annee)
    {
        var result = await _api.GetAsync<List<CumulEmployeDto>>($"api/payroll/cumuls/{employeeId}/{annee}");
        return result?.Data ?? new List<CumulEmployeDto>();
    }

    public async Task<ApiResponse<bool>?> SeedRubriquesAsync()
        => await _api.PostAsync<bool>("api/payroll/seed/rubriques", new { });

    public async Task<ApiResponse<bool>?> SeedConstantesAsync()
        => await _api.PostAsync<bool>("api/payroll/seed/constantes", new { });

    public async Task<ApiResponse<bool>?> SeedDemo2025Async(int clientId)
        => await _api.PostAsync<bool>($"api/payroll/seed/demo-2025/{clientId}", new { });

    // Dynamic employee variables per period
    public async Task<List<VariableEmployeDto>> GetVariablesForPeriodAsync(int employeeId, int annee, int mois)
    {
        var result = await _api.GetAsync<List<VariableEmployeDto>>($"api/payroll/period-variables/{employeeId}/{annee}/{mois}");
        return result?.Data ?? new List<VariableEmployeDto>();
    }

    public async Task<ApiResponse<bool>?> UpdateVariablesBulkAsync(int employeeId, int annee, int mois, List<VariableEmployeCreateDto> variables)
        => await _api.PutAsync<bool>($"api/payroll/period-variables/{employeeId}/{annee}/{mois}", variables);

    // Affectation Rubriques (ERP-style)
    public async Task<List<AffectationRubriqueDto>> GetAffectationsAsync(int employeeId)
    {
        var result = await _api.GetAsync<List<AffectationRubriqueDto>>($"api/payroll/affectations/{employeeId}");
        return result?.Data ?? new List<AffectationRubriqueDto>();
    }

    public async Task<List<RubriqueDto>> GetRubriquesDisponiblesAsync(int employeeId, int? clientId = null)
    {
        var url = $"api/payroll/affectations/{employeeId}/disponibles";
        if (clientId.HasValue) url += $"?clientId={clientId}";
        var result = await _api.GetAsync<List<RubriqueDto>>(url);
        return result?.Data ?? new List<RubriqueDto>();
    }

    public async Task<ApiResponse<AffectationRubriqueDto>?> AddAffectationAsync(AffectationRubriqueCreateDto dto)
        => await _api.PostAsync<AffectationRubriqueDto>("api/payroll/affectations", dto);

    public async Task<ApiResponse<AffectationRubriqueDto>?> UpdateAffectationAsync(int id, AffectationRubriqueCreateDto dto)
        => await _api.PutAsync<AffectationRubriqueDto>($"api/payroll/affectations/{id}", dto);

    public async Task<ApiResponse<bool>?> RemoveAffectationAsync(int id)
        => await _api.DeleteAsync<bool>($"api/payroll/affectations/{id}");
}

public sealed class DashboardFacade : IDashboardFacade
{
    private readonly IApiClient _api;

    public DashboardFacade(IApiClient api) => _api = api;

    public async Task<DashboardDto?> GetManagerDashboardAsync()
    {
        var result = await _api.GetAsync<DashboardDto>("api/dashboard/manager");
        return result?.Data;
    }
}

// ============================================================================
// NEW MODULE FACADE IMPLEMENTATIONS
// ============================================================================

public sealed class DocumentFacade : IDocumentFacade
{
    private readonly IApiClient _api;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _contextAccessor;

    public DocumentFacade(IApiClient api, IHttpClientFactory httpClientFactory, IHttpContextAccessor contextAccessor)
    {
        _api = api;
        _httpClientFactory = httpClientFactory;
        _contextAccessor = contextAccessor;
    }

    public async Task<PagedResult<DocumentDto>?> GetAllAsync(int? clientId = null, string? categorie = null, int page = 1, string? search = null)
    {
        var url = QueryBuilder.For("api/documents")
            .Add("page", page).Add("clientId", clientId).Add("categorie", categorie).Add("search", search);
        var result = await _api.GetAsync<PagedResult<DocumentDto>>(url);
        return result?.Data;
    }

    public async Task<List<DocumentDto>?> GetByDeclarationIdAsync(int declarationId)
    {
        var result = await _api.GetAsync<List<DocumentDto>>($"api/documents/declaration/{declarationId}");
        return result?.Data;
    }

    public async Task<DocumentDto?> GetByIdAsync(int id)
    {
        var result = await _api.GetAsync<DocumentDto>($"api/documents/{id}");
        return result?.Data;
    }

    public async Task<ApiResponse<DocumentDto>?> UploadAsync(Stream stream, string fileName, string contentType, long size, string titre, string categorie, int? clientId, int? declarationId = null)
    {
        var client = _httpClientFactory.CreateClient("MetaComptaAPI");
        var token = _contextAccessor.HttpContext?.Session.GetString("JwtToken");
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        content.Add(streamContent, "file", fileName);
        content.Add(new StringContent(titre), "titre");
        content.Add(new StringContent(categorie), "categorie");
        if (clientId.HasValue) content.Add(new StringContent(clientId.Value.ToString()), "clientId");
        if (declarationId.HasValue) content.Add(new StringContent(declarationId.Value.ToString()), "declarationId");

        var response = await client.PostAsync("api/documents/upload", content);
        var json = await response.Content.ReadAsStringAsync();
        return System.Text.Json.JsonSerializer.Deserialize<ApiResponse<DocumentDto>>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<ApiResponse<bool>?> ChangeStatusAsync(int id, string newStatut, string? motif = null)
        => await _api.PutAsync<bool>($"api/documents/{id}/status", new { NewStatut = newStatut, Motif = motif });

    public async Task<ApiResponse<bool>?> ToggleShareAsync(int id)
        => await _api.PutAsync<bool>($"api/documents/{id}/share", new { });
}

public sealed class TaskFacade : ITaskFacade
{
    private readonly IApiClient _api;
    public TaskFacade(IApiClient api) => _api = api;

    public async Task<PagedResult<TaskItemDto>?> GetAllAsync(string? assignedTo = null, string? statut = null, int? clientId = null, int page = 1, string? assignedToKind = null)
    {
        var url = QueryBuilder.For("api/tasks")
            .Add("page", page).Add("assignedTo", assignedTo).Add("statut", statut).Add("clientId", clientId).Add("assignedToKind", assignedToKind);
        var result = await _api.GetAsync<PagedResult<TaskItemDto>>(url);
        return result?.Data;
    }

    public async Task<TaskItemDto?> GetByIdAsync(int id)
    {
        var result = await _api.GetAsync<TaskItemDto>($"api/tasks/{id}");
        return result?.Data;
    }

    public async Task<ApiResponse<TaskItemDto>?> CreateAsync(TaskItemDto dto)
        => await _api.PostAsync<TaskItemDto>("api/tasks", dto);

    public async Task<ApiResponse<TaskItemDto>?> UpdateAsync(int id, TaskItemDto dto)
        => await _api.PutAsync<TaskItemDto>($"api/tasks/{id}", dto);

    public async Task<ApiResponse<bool>?> ChangeStatusAsync(int id, string newStatut)
        => await _api.PutAsync<bool>($"api/tasks/{id}/status", new { Statut = newStatut });

    public async Task<ApiResponse<WorkflowStepInstanceDto>?> CompleteWorkflowStepAsync(int instanceId, string? notes = null)
        => await _api.PostAsync<WorkflowStepInstanceDto>($"api/workflow/instances/{instanceId}/advance", new { Notes = notes });

    public async Task<ApiResponse<TaskCommentDto>?> AddCommentAsync(int taskId, string contenu)
        => await _api.PostAsync<TaskCommentDto>($"api/tasks/{taskId}/comments", new { Contenu = contenu });
}

public sealed class ComptabiliteFacade : IComptabiliteFacade
{
    private readonly IApiClient _api;
    public ComptabiliteFacade(IApiClient api) => _api = api;

    public async Task<PagedResult<EcritureComptableDto>?> GetEcrituresAsync(int? clientId = null, string? journal = null, int page = 1, string? search = null)
    {
        var url = QueryBuilder.For("api/comptabilite/ecritures")
            .Add("page", page).Add("clientId", clientId).Add("journal", journal).Add("search", search);
        var result = await _api.GetAsync<PagedResult<EcritureComptableDto>>(url);
        return result?.Data;
    }

    public async Task<ApiResponse<EcritureComptableDto>?> CreateEcritureAsync(EcritureCreateDto dto)
        => await _api.PostAsync<EcritureComptableDto>("api/comptabilite/ecritures", dto);

    public async Task<List<JournalDto>> GetJournauxAsync(int? clientId = null)
    {
        var url = QueryBuilder.For("api/comptabilite/journaux").Add("clientId", clientId);
        var result = await _api.GetAsync<List<JournalDto>>(url);
        return result?.Data ?? new List<JournalDto>();
    }

    public async Task<List<GrandLivreDto>> GetGrandLivreAsync(int clientId, DateTime? dateDebut = null, DateTime? dateFin = null)
    {
        var url = QueryBuilder.For("api/comptabilite/grandlivre")
            .Add("clientId", clientId)
            .Add("dateDebut", dateDebut?.ToString("yyyy-MM-dd"))
            .Add("dateFin", dateFin?.ToString("yyyy-MM-dd"));
        var result = await _api.GetAsync<List<GrandLivreDto>>(url);
        return result?.Data ?? new List<GrandLivreDto>();
    }

    public async Task<List<BalanceDto>> GetBalanceAsync(int clientId, DateTime? dateDebut = null, DateTime? dateFin = null)
    {
        var url = QueryBuilder.For("api/comptabilite/balance")
            .Add("clientId", clientId)
            .Add("dateDebut", dateDebut?.ToString("yyyy-MM-dd"))
            .Add("dateFin", dateFin?.ToString("yyyy-MM-dd"));
        var result = await _api.GetAsync<List<BalanceDto>>(url);
        return result?.Data ?? new List<BalanceDto>();
    }

    public async Task<List<BalanceAgeeDto>> GetBalanceAgeeAsync(int clientId)
    {
        var result = await _api.GetAsync<List<BalanceAgeeDto>>($"api/comptabilite/balance-agee?clientId={clientId}");
        return result?.Data ?? new List<BalanceAgeeDto>();
    }

    public async Task<List<RapprochementBancaireDto>> GetRapprochementsAsync(int? clientId = null)
    {
        var url = QueryBuilder.For("api/comptabilite/rapprochements").Add("clientId", clientId);
        var result = await _api.GetAsync<List<RapprochementBancaireDto>>(url);
        return result?.Data ?? new List<RapprochementBancaireDto>();
    }

    public async Task<ApiResponse<RapprochementBancaireDto>?> CreateRapprochementAsync(int clientId, string compteBancaire, string periode)
        => await _api.PostAsync<RapprochementBancaireDto>("api/comptabilite/rapprochements",
            new { ClientId = clientId, CompteBancaire = compteBancaire, Periode = periode });
}

public sealed class JuridiqueFacade : IJuridiqueFacade
{
    private readonly IApiClient _api;
    public JuridiqueFacade(IApiClient api) => _api = api;

    public async Task<PagedResult<DossierJuridiqueDto>?> GetAllAsync(int? clientId = null, string? type = null, string? statut = null, int page = 1, string? search = null)
    {
        var url = QueryBuilder.For("api/juridique")
            .Add("page", page).Add("clientId", clientId).Add("type", type).Add("statut", statut).Add("search", search);
        var result = await _api.GetAsync<PagedResult<DossierJuridiqueDto>>(url);
        return result?.Data;
    }

    public async Task<DossierJuridiqueDto?> GetByIdAsync(int id)
    {
        var result = await _api.GetAsync<DossierJuridiqueDto>($"api/juridique/{id}");
        return result?.Data;
    }

    public async Task<ApiResponse<DossierJuridiqueDto>?> CreateAsync(DossierJuridiqueCreateDto dto)
        => await _api.PostAsync<DossierJuridiqueDto>("api/juridique", dto);

    public async Task<ApiResponse<DossierJuridiqueDto>?> UpdateAsync(int id, DossierJuridiqueCreateDto dto)
        => await _api.PutAsync<DossierJuridiqueDto>($"api/juridique/{id}", dto);

    public async Task<ApiResponse<bool>?> ChangeStatusAsync(int id, string newStatut)
        => await _api.PutAsync<bool>($"api/juridique/{id}/status", new { NewStatut = newStatut });
}

public sealed class PlanningFacade : IPlanningFacade
{
    private readonly IApiClient _api;
    public PlanningFacade(IApiClient api) => _api = api;

    public async Task<List<PlanningEventDto>> GetEventsAsync(DateTime? debut = null, DateTime? fin = null, int? clientId = null, string? userId = null)
    {
        var url = QueryBuilder.For("api/planning")
            .Add("debut", debut?.ToString("yyyy-MM-dd"))
            .Add("fin", fin?.ToString("yyyy-MM-dd"))
            .Add("clientId", clientId)
            .Add("userId", userId);
        var result = await _api.GetAsync<List<PlanningEventDto>>(url);
        return result?.Data ?? new List<PlanningEventDto>();
    }

    public async Task<ApiResponse<PlanningEventDto>?> CreateAsync(PlanningEventDto dto)
        => await _api.PostAsync<PlanningEventDto>("api/planning", dto);

    public async Task<ApiResponse<PlanningEventDto>?> UpdateAsync(PlanningEventDto dto)
        => await _api.PutAsync<PlanningEventDto>($"api/planning/{dto.Id}", dto);

    public async Task<ApiResponse<bool>?> DeleteAsync(int id)
        => await _api.DeleteAsync<bool>($"api/planning/{id}");
}

public sealed class CabinetFacade : ICabinetFacade
{
    private readonly IApiClient _api;
    public CabinetFacade(IApiClient api) => _api = api;

    // ── Cabinet Info ──────────────────────────────────────────

    public async Task<CabinetDetailDto?> GetCabinetAsync()
    {
        var result = await _api.GetAsync<CabinetDetailDto>("api/cabinet/info");
        return result?.Data;
    }

    public async Task<ApiResponse<CabinetDetailDto>?> UpdateCabinetAsync(CabinetEditDto dto)
        => await _api.PutAsync<CabinetDetailDto>("api/cabinet/info", dto);

    // ── Employees ─────────────────────────────────────────────

    public async Task<PagedResult<CabinetEmployeeDto>?> GetEmployeesAsync(string? departement = null, int page = 1, string? search = null, int pageSize = 20)
    {
        var url = QueryBuilder.For("api/cabinet/employees")
            .Add("page", page).Add("pageSize", pageSize).Add("departement", departement).Add("search", search);
        var result = await _api.GetAsync<PagedResult<CabinetEmployeeDto>>(url);
        return result?.Data;
    }

    public async Task<CabinetEmployeeDto?> GetEmployeeByIdAsync(string id)
    {
        var result = await _api.GetAsync<CabinetEmployeeDto>($"api/cabinet/employees/{id}");
        return result?.Data;
    }

    public async Task<ApiResponse<CabinetEmployeeDto>?> CreateEmployeeAsync(CabinetEmployeeCreateDto dto)
        => await _api.PostAsync<CabinetEmployeeDto>("api/cabinet/employees", dto);

    public async Task<ApiResponse<CabinetEmployeeDto>?> UpdateEmployeeAsync(string id, CabinetEmployeeUpdateDto dto)
        => await _api.PutAsync<CabinetEmployeeDto>($"api/cabinet/employees/{id}", dto);

    public async Task<ApiResponse<bool>?> ToggleEmployeeActiveAsync(string id)
        => await _api.PutAsync<bool>($"api/cabinet/employees/{id}/toggle", new { });

    // ── Roles & Permissions ───────────────────────────────────

    public async Task<PagedResult<RoleDto>?> GetRolesAsync(int page = 1, string? search = null)
    {
        var url = QueryBuilder.For("api/cabinet/roles")
            .Add("page", page).Add("search", search);
        var result = await _api.GetAsync<PagedResult<RoleDto>>(url);
        return result?.Data;
    }

    public async Task<RoleDto?> GetRoleByIdAsync(string roleId)
    {
        var result = await _api.GetAsync<RoleDto>($"api/cabinet/roles/{roleId}");
        return result?.Data;
    }

    public async Task<ApiResponse<RoleDto>?> CreateRoleAsync(string name, string? description, List<string> permissions)
        => await _api.PostAsync<RoleDto>("api/cabinet/roles", new { Name = name, Description = description, Permissions = permissions });

    public async Task<ApiResponse<RoleDto>?> UpdateRolePermissionsAsync(string roleId, List<string> permissions)
        => await _api.PutAsync<RoleDto>($"api/cabinet/roles/{roleId}/permissions", new { Permissions = permissions });

    // ── Audit Logs ────────────────────────────────────────────

    public async Task<PagedResult<AuditLogDto>?> GetAuditLogsAsync(string? userId = null, string? action = null, DateTime? dateDebut = null, DateTime? dateFin = null, int page = 1)
    {
        var url = QueryBuilder.For("api/cabinet/audit-logs")
            .Add("page", page).Add("userId", userId).Add("action", action)
            .Add("dateDebut", dateDebut?.ToString("yyyy-MM-dd"))
            .Add("dateFin", dateFin?.ToString("yyyy-MM-dd"));
        var result = await _api.GetAsync<PagedResult<AuditLogDto>>(url);
        return result?.Data;
    }

    // ── Profile & Claims ──────────────────────────────────────

    public async Task<UserProfileDto?> GetProfileAsync()
    {
        var result = await _api.GetAsync<UserProfileDto>("api/cabinet/profile");
        return result?.Data;
    }

    public async Task<List<string>> GetAllClaimsAsync()
    {
        var result = await _api.GetAsync<ClaimsResponse>("api/cabinet/claims");
        return result?.Data?.All ?? new List<string>();
    }

    public async Task<Dictionary<string, List<string>>> GetClaimsByDomainAsync()
    {
        var result = await _api.GetAsync<ClaimsResponse>("api/cabinet/claims");
        return result?.Data?.ByDomain ?? new Dictionary<string, List<string>>();
    }

    // Internal DTO for claims endpoint response
    private class ClaimsResponse
    {
        public List<string> All { get; set; } = new();
        public Dictionary<string, List<string>> ByDomain { get; set; } = new();
    }
}

public sealed class RapportFacade : IRapportFacade
{
    private readonly IApiClient _api;
    public RapportFacade(IApiClient api) => _api = api;

    public async Task<PagedResult<RapportDto>?> GetAllAsync(int? clientId = null, string? type = null, int page = 1, string? search = null)
    {
        var url = QueryBuilder.For("api/rapports")
            .Add("clientId", clientId).Add("page", page).Add("type", type).Add("search", search);
        var result = await _api.GetAsync<PagedResult<RapportDto>>(url);
        return result?.Data;
    }

    public async Task<RapportDto?> GetByIdAsync(int id)
    {
        var result = await _api.GetAsync<RapportDto>($"api/rapports/{id}");
        return result?.Data;
    }

    public async Task<ApiResponse<RapportDto>?> GenerateAsync(string type, int? clientId, DateTime? dateDebut, DateTime? dateFin)
        => await _api.PostAsync<RapportDto>("api/rapports/generate", new { Type = type, ClientId = clientId, DateDebut = dateDebut, DateFin = dateFin });
}

public sealed class ParametresFacade : IParametresFacade
{
    private readonly IApiClient _api;
    public ParametresFacade(IApiClient api) => _api = api;

    public async Task<List<SettingsDto>> GetAllAsync(string? categorie = null)
    {
        var url = QueryBuilder.For("api/parametres").Add("categorie", categorie);
        var result = await _api.GetAsync<List<SettingsDto>>(url);
        return result?.Data ?? new List<SettingsDto>();
    }

    public async Task<ApiResponse<SettingsDto>?> UpdateSettingAsync(string cle, string? valeur)
        => await _api.PutAsync<SettingsDto>($"api/parametres/{cle}", new { Valeur = valeur });

    public async Task<List<DocumentTemplateDto>> GetTemplatesAsync()
    {
        var result = await _api.GetAsync<List<DocumentTemplateDto>>("api/parametres/templates");
        return result?.Data ?? new List<DocumentTemplateDto>();
    }

    public async Task<ApiResponse<DocumentTemplateDto>?> UpdateTemplateAsync(int id, string contenu)
        => await _api.PutAsync<DocumentTemplateDto>($"api/parametres/templates/{id}", new { Contenu = contenu });
}

public sealed class EtatFiscalFacade : IEtatFiscalFacade
{
    private readonly IApiClient _api;
    public EtatFiscalFacade(IApiClient api) => _api = api;

    public async Task<PagedResult<EtatFiscalDto>?> GetAllAsync(int? clientId = null, string? type = null, int? exercice = null, int page = 1, string? search = null)
    {
        var url = QueryBuilder.For("api/etats-fiscaux")
            .Add("page", page).Add("clientId", clientId).Add("type", type).Add("exercice", exercice).Add("search", search);
        var result = await _api.GetAsync<PagedResult<EtatFiscalDto>>(url);
        return result?.Data;
    }

    public async Task<EtatFiscalDto?> GetByIdAsync(int id)
    {
        var result = await _api.GetAsync<EtatFiscalDto>($"api/etats-fiscaux/{id}");
        return result?.Data;
    }

    public async Task<ApiResponse<EtatFiscalDto>?> GenerateAsync(int clientId, string type, int exercice)
        => await _api.PostAsync<EtatFiscalDto>("api/etats-fiscaux/generate", new { ClientId = clientId, Type = type, Exercice = exercice });

    public async Task<ApiResponse<bool>?> ValidateAsync(int id)
        => await _api.PutAsync<bool>($"api/etats-fiscaux/{id}/validate", new { });
}

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║                     WORKFLOW ENGINE — FACADES                           ║
// ╚══════════════════════════════════════════════════════════════════════════╝

public sealed class DepartmentFacade : IDepartmentFacade
{
    private readonly IApiClient _api;
    public DepartmentFacade(IApiClient api) => _api = api;

    public async Task<List<DepartmentDto>> GetAllAsync()
    {
        var result = await _api.GetAsync<List<DepartmentDto>>("api/departments");
        return result?.Data ?? new List<DepartmentDto>();
    }

    public async Task<DepartmentDto?> GetByIdAsync(int id)
    {
        var result = await _api.GetAsync<DepartmentDto>($"api/departments/{id}");
        return result?.Data;
    }

    public async Task<ApiResponse<DepartmentDto>?> CreateAsync(DepartmentCreateDto dto)
        => await _api.PostAsync<DepartmentDto>("api/departments", dto);

    public async Task<ApiResponse<DepartmentDto>?> UpdateAsync(int id, DepartmentCreateDto dto)
        => await _api.PutAsync<DepartmentDto>($"api/departments/{id}", dto);

    public async Task<ApiResponse<bool>?> DeleteAsync(int id)
        => await _api.DeleteAsync<bool>($"api/departments/{id}");

    public async Task<ApiResponse<bool>?> AddMemberAsync(int departmentId, string userId, bool isLead)
        => await _api.PostAsync<bool>($"api/departments/{departmentId}/members?userId={Uri.EscapeDataString(userId)}&isLead={isLead}", new { });

    public async Task<ApiResponse<bool>?> RemoveMemberAsync(int departmentId, string userId)
        => await _api.DeleteAsync<bool>($"api/departments/{departmentId}/members/{Uri.EscapeDataString(userId)}");

    public async Task<List<DepartmentMemberDto>> GetMembersAsync(int departmentId)
    {
        var result = await _api.GetAsync<List<DepartmentMemberDto>>($"api/departments/{departmentId}/members");
        return result?.Data ?? new List<DepartmentMemberDto>();
    }
}

public sealed class WorkflowFacade : IWorkflowFacade
{
    private readonly IApiClient _api;
    public WorkflowFacade(IApiClient api) => _api = api;

    // ── Templates ────────────────────────────────────────────────

    public async Task<PagedResult<WorkflowTemplateDto>?> GetTemplatesAsync(string? declarationType = null, int? clientId = null, int page = 1)
    {
        var url = QueryBuilder.For("api/workflow/templates")
            .Add("page", page)
            .Add("declarationType", declarationType)
            .Add("clientId", clientId);
        var result = await _api.GetAsync<PagedResult<WorkflowTemplateDto>>(url);
        return result?.Data;
    }

    public async Task<WorkflowTemplateDto?> GetTemplateByIdAsync(int id)
    {
        var result = await _api.GetAsync<WorkflowTemplateDto>($"api/workflow/templates/{id}");
        return result?.Data;
    }

    public async Task<ApiResponse<WorkflowTemplateDto>?> CreateTemplateAsync(WorkflowTemplateCreateDto dto)
        => await _api.PostAsync<WorkflowTemplateDto>("api/workflow/templates", dto);

    public async Task<ApiResponse<WorkflowTemplateDto>?> UpdateTemplateAsync(int id, WorkflowTemplateCreateDto dto)
        => await _api.PutAsync<WorkflowTemplateDto>($"api/workflow/templates/{id}", dto);

    public async Task<ApiResponse<bool>?> DeleteTemplateAsync(int id)
        => await _api.DeleteAsync<bool>($"api/workflow/templates/{id}");

    public async Task<ApiResponse<WorkflowTemplateDto>?> CreateDefaultTemplateAsync()
        => await _api.PostAsync<WorkflowTemplateDto>("api/workflow/templates/default", new { });

    public async Task<ApiResponse<List<WorkflowTemplateDto>>?> SeedAllDefaultTemplatesAsync()
        => await _api.PostAsync<List<WorkflowTemplateDto>>("api/workflow/templates/seed-all-defaults", new { });

    // ── Instances ────────────────────────────────────────────────

    public async Task<ApiResponse<WorkflowInstanceDto>?> StartWorkflowAsync(StartWorkflowDto dto)
        => await _api.PostAsync<WorkflowInstanceDto>("api/workflow/start", dto);

    public async Task<PagedResult<WorkflowInstanceDto>?> GetInstancesAsync(int? clientId = null, string? status = null, int? declarationId = null, int page = 1)
    {
        var url = QueryBuilder.For("api/workflow/instances")
            .Add("page", page)
            .Add("clientId", clientId)
            .Add("status", status)
            .Add("declarationId", declarationId);
        var result = await _api.GetAsync<PagedResult<WorkflowInstanceDto>>(url);
        return result?.Data;
    }

    public async Task<WorkflowInstanceDto?> GetInstanceByIdAsync(int id)
    {
        var result = await _api.GetAsync<WorkflowInstanceDto>($"api/workflow/instances/{id}");
        return result?.Data;
    }

    public async Task<ApiResponse<bool>?> CancelWorkflowAsync(int id, string reason)
        => await _api.PostAsync<bool>($"api/workflow/instances/{id}/cancel", reason);

    public async Task<ApiResponse<bool>?> PauseWorkflowAsync(int id)
        => await _api.PostAsync<bool>($"api/workflow/instances/{id}/pause", new { });

    public async Task<ApiResponse<bool>?> ResumeWorkflowAsync(int id)
        => await _api.PostAsync<bool>($"api/workflow/instances/{id}/resume", new { });

    // ── Steps ────────────────────────────────────────────────────

    public async Task<ApiResponse<WorkflowStepInstanceDto>?> AdvanceStepAsync(int instanceId, AdvanceStepDto dto)
        => await _api.PostAsync<WorkflowStepInstanceDto>($"api/workflow/instances/{instanceId}/advance", dto);

    public async Task<ApiResponse<WorkflowStepInstanceDto>?> ClaimStepAsync(int instanceId, bool forceOverride = false, string? reason = null)
        => await _api.PostAsync<WorkflowStepInstanceDto>($"api/workflow/instances/{instanceId}/claim", new { ForceOverride = forceOverride, Reason = reason });

    public async Task<ApiResponse<bool>?> ReassignStepAsync(int stepInstanceId, ReassignStepDto dto)
        => await _api.PostAsync<bool>($"api/workflow/steps/{stepInstanceId}/reassign", dto);

    public async Task<WorkflowStepInstanceDto?> GetCurrentStepAsync(int instanceId)
    {
        var result = await _api.GetAsync<WorkflowStepInstanceDto>($"api/workflow/instances/{instanceId}/current-step");
        return result?.Data;
    }

    public async Task<List<WorkflowStepInstanceDto>> GetMyPendingStepsAsync(int? clientId = null)
    {
        var url = clientId.HasValue ? $"api/workflow/my-steps?clientId={clientId}" : "api/workflow/my-steps";
        var result = await _api.GetAsync<List<WorkflowStepInstanceDto>>(url);
        return result?.Data ?? new List<WorkflowStepInstanceDto>();
    }

    // ── Client Config ────────────────────────────────────────────

    public async Task<List<ClientWorkflowConfigDto>> GetClientConfigsAsync(int clientId)
    {
        var result = await _api.GetAsync<List<ClientWorkflowConfigDto>>($"api/workflow/client-config/{clientId}");
        return result?.Data ?? new List<ClientWorkflowConfigDto>();
    }

    public async Task<ApiResponse<ClientWorkflowConfigDto>?> SaveClientConfigAsync(ClientWorkflowConfigCreateDto dto)
        => await _api.PostAsync<ClientWorkflowConfigDto>("api/workflow/client-config", dto);

    // ── Dashboard & History ──────────────────────────────────────

    public async Task<WorkflowDashboardDto?> GetDashboardAsync()
    {
        var result = await _api.GetAsync<WorkflowDashboardDto>("api/workflow/dashboard");
        return result?.Data;
    }

    public async Task<List<WorkflowHistoryDto>> GetHistoryAsync(int instanceId)
    {
        var result = await _api.GetAsync<List<WorkflowHistoryDto>>($"api/workflow/instances/{instanceId}/history");
        return result?.Data ?? new List<WorkflowHistoryDto>();
    }
}

// ════════════════════════════════════════════════════════════════════════════════
//  DOCUMENTS COMMERCIAUX — Facade Implementation
// ════════════════════════════════════════════════════════════════════════════════

public sealed class CommercialFacade : ICommercialFacade
{
    private readonly IApiClient _api;
    public CommercialFacade(IApiClient api) => _api = api;

    // ── Devis ────────────────────────────────────────────────────

    public async Task<PagedResult<DevisDto>?> GetDevisAsync(int? clientId = null, string? statut = null, int page = 1)
    {
        var url = QueryBuilder.For("api/devis")
            .Add("clientId", clientId)
            .Add("statut", statut)
            .Add("page", page);
        var result = await _api.GetAsync<PagedResult<DevisDto>>(url);
        return result?.Data;
    }

    public async Task<DevisDto?> GetDevisByIdAsync(int id)
    {
        var result = await _api.GetAsync<DevisDto>($"api/devis/{id}");
        return result?.Data;
    }

    public async Task<ApiResponse<DevisDto>?> CreateDevisAsync(DevisCreateDto dto)
        => await _api.PostAsync<DevisDto>("api/devis", dto);

    public async Task<ApiResponse<DevisDto>?> UpdateDevisAsync(int id, DevisCreateDto dto)
        => await _api.PutAsync<DevisDto>($"api/devis/{id}", dto);

    public async Task<ApiResponse<bool>?> DeleteDevisAsync(int id)
        => await _api.DeleteAsync<bool>($"api/devis/{id}");

    public async Task<ApiResponse<DevisDto>?> SoumettreDevisAsync(int id)
        => await _api.PostAsync<DevisDto>($"api/devis/{id}/soumettre", new { });

    public async Task<ApiResponse<DevisDto>?> ApprouverDevisAsync(int id)
        => await _api.PostAsync<DevisDto>($"api/devis/{id}/approuver", new { });

    public async Task<ApiResponse<DevisDto>?> RejeterDevisAsync(int id, string motif)
        => await _api.PostAsync<DevisDto>($"api/devis/{id}/rejeter", new { Motif = motif });

    public async Task<ApiResponse<DevisDto>?> DupliquerDevisAsync(int id)
        => await _api.PostAsync<DevisDto>($"api/devis/{id}/dupliquer", new { });

    public async Task<ApiResponse<BonCommandeDto>?> ConvertirDevisAsync(int id)
        => await _api.PostAsync<BonCommandeDto>($"api/devis/{id}/convertir", new { });

    public async Task<ApiResponse<ContratCommercialDto>?> ConvertirDevisEnContratAsync(int devisId, ConvertDevisEnContratDto dto)
        => await _api.PostAsync<ContratCommercialDto>($"api/devis/{devisId}/convertir-en-contrat", dto);

    // ── Bons de Commande ─────────────────────────────────────────

    public async Task<PagedResult<BonCommandeDto>?> GetBonsCommandeAsync(int? clientId = null, string? statut = null, int page = 1)
    {
        var url = QueryBuilder.For("api/bons-commande")
            .Add("clientId", clientId)
            .Add("statut", statut)
            .Add("page", page);
        var result = await _api.GetAsync<PagedResult<BonCommandeDto>>(url);
        return result?.Data;
    }

    public async Task<BonCommandeDto?> GetBonCommandeByIdAsync(int id)
    {
        var result = await _api.GetAsync<BonCommandeDto>($"api/bons-commande/{id}");
        return result?.Data;
    }

    public async Task<ApiResponse<BonCommandeDto>?> ConfirmerBonCommandeAsync(int id)
        => await _api.PostAsync<BonCommandeDto>($"api/bons-commande/{id}/confirmer", new { });

    public async Task<ApiResponse<BonCommandeDto>?> ApprouverBonCommandeAsync(int id)
        => await _api.PostAsync<BonCommandeDto>($"api/bons-commande/{id}/approuver", new { });

    public async Task<ApiResponse<ContratCommercialDto>?> ConvertirBonCommandeAsync(ConvertBonCommandeEnContratDto dto)
        => await _api.PostAsync<ContratCommercialDto>("api/bons-commande/convertir", dto);

    public async Task<ApiResponse<BonCommandeDto>?> DupliquerBonCommandeAsync(int id)
        => await _api.PostAsync<BonCommandeDto>($"api/bons-commande/{id}/dupliquer", new { });

    // ── Contrats ─────────────────────────────────────────────────

    public async Task<PagedResult<ContratCommercialDto>?> GetContratsAsync(int? clientId = null, string? statut = null, int page = 1)
    {
        var url = QueryBuilder.For("api/contrats")
            .Add("clientId", clientId)
            .Add("statut", statut)
            .Add("page", page);
        var result = await _api.GetAsync<PagedResult<ContratCommercialDto>>(url);
        return result?.Data;
    }

    public async Task<ContratCommercialDto?> GetContratByIdAsync(int id)
    {
        var result = await _api.GetAsync<ContratCommercialDto>($"api/contrats/{id}");
        return result?.Data;
    }

    public async Task<ApiResponse<ContratCommercialDto>?> CreateContratAsync(ContratCommercialCreateDto dto)
        => await _api.PostAsync<ContratCommercialDto>("api/contrats", dto);

    public async Task<ApiResponse<ContratCommercialDto>?> UpdateContratAsync(int id, ContratCommercialCreateDto dto)
        => await _api.PutAsync<ContratCommercialDto>($"api/contrats/{id}", dto);

    public async Task<ApiResponse<ContratCommercialDto>?> UpdateContratEquipeAsync(int id, ContratEquipeUpdateDto dto)
        => await _api.PutAsync<ContratCommercialDto>($"api/contrats/{id}/equipe", dto);

    public async Task<ApiResponse<ContratCommercialDto>?> SignerContratAsync(int id, ContratSignatureDto dto)
        => await _api.PostAsync<ContratCommercialDto>($"api/contrats/{id}/signer", dto);

    public async Task<ApiResponse<ContratCommercialDto>?> ValiderContratAsync(int id)
        => await _api.PostAsync<ContratCommercialDto>($"api/contrats/{id}/valider", new { });

    public async Task<ApiResponse<ContratCommercialDto>?> SuspendreContratAsync(int id)
        => await _api.PostAsync<ContratCommercialDto>($"api/contrats/{id}/suspendre", new { });

    public async Task<ApiResponse<ContratCommercialDto>?> ReactiverContratAsync(int id)
        => await _api.PostAsync<ContratCommercialDto>($"api/contrats/{id}/reactiver", new { });

    public async Task<ApiResponse<ContratCommercialDto>?> TerminerContratAsync(int id)
        => await _api.PostAsync<ContratCommercialDto>($"api/contrats/{id}/terminer", new { });

    public async Task<ApiResponse<ContratCommercialDto>?> DupliquerContratAsync(int id)
        => await _api.PostAsync<ContratCommercialDto>($"api/contrats/{id}/dupliquer", new { });

    public async Task<ApiResponse<int>?> SyncServicesFromContratAsync(int contratId)
        => await _api.PostAsync<int>($"api/contrats/{contratId}/sync-services", new { });

    // ── Factures ─────────────────────────────────────────────────

    public async Task<PagedResult<FactureDto>?> GetFacturesAsync(int? clientId = null, string? statut = null, int page = 1)
    {
        var url = QueryBuilder.For("api/factures")
            .Add("clientId", clientId)
            .Add("statut", statut)
            .Add("page", page);
        var result = await _api.GetAsync<PagedResult<FactureDto>>(url);
        return result?.Data;
    }

    public async Task<FactureDto?> GetFactureByIdAsync(int id)
    {
        var result = await _api.GetAsync<FactureDto>($"api/factures/{id}");
        return result?.Data;
    }

    public async Task<ApiResponse<FactureDto>?> CreateFactureAsync(FactureCreateDto dto)
        => await _api.PostAsync<FactureDto>("api/factures", dto);

    public async Task<ApiResponse<FactureDto>?> EnvoyerFactureAsync(int id)
        => await _api.PostAsync<FactureDto>($"api/factures/{id}/envoyer", new { });

    public async Task<ApiResponse<FactureDto>?> AnnulerFactureAsync(int id)
        => await _api.PostAsync<FactureDto>($"api/factures/{id}/annuler", new { });

    public async Task<ApiResponse<FactureDto>?> DupliquerFactureAsync(int id)
        => await _api.PostAsync<FactureDto>($"api/factures/{id}/dupliquer", new { });

    // ── Paiements ────────────────────────────────────────────────

    public async Task<ApiResponse<PaiementDto>?> EnregistrerPaiementAsync(int factureId, PaiementCreateDto dto)
    {
        dto.FactureId = factureId;
        return await _api.PostAsync<PaiementDto>($"api/factures/{factureId}/paiements", dto);
    }

    public async Task<List<PaiementDto>> GetPaiementsFactureAsync(int factureId)
    {
        var result = await _api.GetAsync<List<PaiementDto>>($"api/factures/{factureId}/paiements");
        return result?.Data ?? new List<PaiementDto>();
    }

    // ── Dashboard ────────────────────────────────────────────────

    public async Task<DashboardCommercialDto?> GetDashboardAsync()
    {
        var result = await _api.GetAsync<DashboardCommercialDto>("api/commercial/dashboard");
        return result?.Data;
    }
}

// ============================================================================
// MESSAGE FACADE
// ============================================================================

public sealed class MessageFacade : IMessageFacade
{
    private readonly IApiClient _api;
    public MessageFacade(IApiClient api) => _api = api;

    public async Task<List<MessageDto>?> GetByDeclarationAsync(int declarationId)
    {
        var result = await _api.GetAsync<List<MessageDto>>($"api/messages/declaration/{declarationId}");
        return result?.Data;
    }

    public async Task<List<MessageDto>?> GetConversationsAsync()
    {
        var result = await _api.GetAsync<List<MessageDto>>("api/messages/conversations");
        return result?.Data;
    }

    public async Task<ApiResponse<MessageDto>?> SendAsync(SendMessageDto dto)
        => await _api.PostAsync<MessageDto>("api/messages", dto);

    public async Task<int> GetUnreadCountAsync(int? clientId = null)
    {
        var url = clientId.HasValue ? $"api/messages/unread-count?clientId={clientId}" : "api/messages/unread-count";
        var result = await _api.GetAsync<int>(url);
        return result?.Data ?? 0;
    }
}
