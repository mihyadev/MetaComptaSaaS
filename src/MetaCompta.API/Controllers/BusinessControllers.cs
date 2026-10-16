using MetaCompta.Infrastructure.Data;
using System.Security.Claims;
using MetaCompta.Application.DTOs;
using MetaCompta.Application.Interfaces;
using MetaCompta.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MetaCompta.API.Controllers;

// ===== Employees Controller =====
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "BackOfficeAccess")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    public EmployeesController(IEmployeeService employeeService) => _employeeService = employeeService;

    private int CabinetId => int.TryParse(User.FindFirst("CabinetId")?.Value, out var id) ? id : 0;
    private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? clientId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
    {
        var result = await _employeeService.GetAllAsync(CabinetId, clientId, page, pageSize, search);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await _employeeService.GetByIdAsync(id, CabinetId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [Authorize(Policy = "employee.create")]
    public async Task<IActionResult> Create([FromBody] EmployeeCreateDto dto)
    {
        var result = await _employeeService.CreateAsync(dto, CabinetId, UserId);
        return result.Success ? CreatedAtAction(nameof(Get), new { id = result.Data?.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "employee.edit")]
    public async Task<IActionResult> Update(int id, [FromBody] EmployeeCreateDto dto)
    {
        var result = await _employeeService.UpdateAsync(id, dto, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "employee.delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _employeeService.DeleteAsync(id, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

// ===== Clients Controller =====
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "BackOfficeAccess")]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;
    public ClientsController(IClientService clientService) => _clientService = clientService;

    private int CabinetId => int.TryParse(User.FindFirst("CabinetId")?.Value, out var id) ? id : 0;
    private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, [FromQuery] string? statut = null)
    {
        var result = await _clientService.GetAllAsync(CabinetId, page, pageSize, search, statut);
        return Ok(result);
    }

    /// <summary>
    /// Clients for Nouveau contrat: without active contract first, then with (Valide/Actif + date window).
    /// </summary>
    [HttpGet("for-contrat-picker")]
    public async Task<IActionResult> GetForContratPicker()
    {
        var result = await _clientService.GetForContratPickerAsync(CabinetId);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await _clientService.GetByIdAsync(id, CabinetId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [Authorize(Policy = "client.create")]
    public async Task<IActionResult> Create([FromBody] ClientCreateDto dto)
    {
        var result = await _clientService.CreateAsync(dto, CabinetId, UserId);
        return result.Success ? CreatedAtAction(nameof(Get), new { id = result.Data?.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "client.edit")]
    public async Task<IActionResult> Update(int id, [FromBody] ClientCreateDto dto)
    {
        var result = await _clientService.UpdateAsync(id, dto, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "client.delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _clientService.DeleteAsync(id, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Returns the list of active service type codes for a client.
    /// Used by the BackOffice sidebar to show/hide menu items.
    /// </summary>
    [HttpGet("{id}/services")]
    public async Task<IActionResult> GetActiveServices(int id)
    {
        var services = await _clientService.GetActiveServicesAsync(id, CabinetId);
        return Ok(ApiResponse<List<string>>.Ok(services));
    }

    /// <summary>
    /// Returns true if the client has Manage-mode access (Valide/Actif + date window).
    /// Used to gate declaration create/edit in the BackOffice.
    /// </summary>
    [HttpGet("{id}/has-active-contract")]
    public async Task<IActionResult> HasActiveContract(int id)
    {
        var hasContract = await _clientService.HasActiveContractAsync(id, CabinetId);
        return Ok(ApiResponse<bool>.Ok(hasContract));
    }

    /// <summary>Manage vs Consult context for the dossier client.</summary>
    [HttpGet("{id}/contract-access")]
    public async Task<IActionResult> GetContractAccess(int id, [FromQuery] int? exerciceId = null)
    {
        var ctx = await _clientService.GetContractAccessContextAsync(id, CabinetId, exerciceId);
        return Ok(ApiResponse<ContractAccessContextDto>.Ok(ctx));
    }

    /// <summary>Get all service contracts for a client, optionally by exercise year / id</summary>
    [HttpGet("{id}/service-contrats")]
    public async Task<IActionResult> GetServiceContrats(int id, [FromQuery] int? exerciceFiscal = null, [FromQuery] int? exerciceId = null)
    {
        var result = await _clientService.GetServiceContratsAsync(id, CabinetId, exerciceFiscal, exerciceId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Get a single service contract detail</summary>
    [HttpGet("service-contrats/{serviceContratId}")]
    public async Task<IActionResult> GetServiceContrat(int serviceContratId)
    {
        var result = await _clientService.GetServiceContratByIdAsync(serviceContratId, CabinetId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Initialize services for a new exercise year (clone from previous year)</summary>
    [HttpPost("{id}/service-contrats/initialize")]
    [Authorize(Policy = "client.edit")]
    public async Task<IActionResult> InitializeServicesForExercice(int id, [FromQuery] int exerciceFiscal)
    {
        var result = await _clientService.InitializeServicesForExerciceAsync(id, exerciceFiscal, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Auto-generate declarations from active service contracts</summary>
    [HttpPost("{id}/generate-declarations")]
    [Authorize(Policy = "declaration.create")]
    public async Task<IActionResult> GenerateDeclarations(int id, [FromBody] GenerateDeclarationsRequestDto request)
    {
        request.ClientId = id;
        var result = await _clientService.GenerateDeclarationsFromServicesAsync(request, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

// ===== Client Users (Extranet) Controller =====
[ApiController]
[Route("api/clients/{clientId}/users")]
[Authorize(Policy = "BackOfficeAccess")]
public class ClientUsersController : ControllerBase
{
    private readonly IClientUserService _service;
    public ClientUsersController(IClientUserService service) => _service = service;

    private int CabinetId => int.TryParse(User.FindFirst("CabinetId")?.Value, out var id) ? id : 0;
    private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

    [HttpGet]
    public async Task<IActionResult> GetAll(int clientId)
    {
        var result = await _service.GetClientUsersAsync(clientId, CabinetId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> Get(int clientId, string userId)
    {
        var result = await _service.GetClientUserAsync(userId, clientId, CabinetId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [Authorize(Policy = "client.edit")]
    public async Task<IActionResult> Create(int clientId, [FromBody] ClientUserCreateDto dto)
    {
        var result = await _service.CreateClientUserAsync(dto, clientId, CabinetId, UserId);
        return result.Success ? CreatedAtAction(nameof(Get), new { clientId, userId = result.Data?.Id }, result) : BadRequest(result);
    }

    [HttpPut("{userId}")]
    [Authorize(Policy = "client.edit")]
    public async Task<IActionResult> Update(int clientId, string userId, [FromBody] ClientUserUpdateDto dto)
    {
        var result = await _service.UpdateClientUserAsync(userId, dto, clientId, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{userId}/toggle")]
    [Authorize(Policy = "client.edit")]
    public async Task<IActionResult> Toggle(int clientId, string userId)
    {
        var result = await _service.ToggleClientUserActiveAsync(userId, clientId, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{userId}/resend-activation")]
    [Authorize(Policy = "client.edit")]
    public async Task<IActionResult> ResendActivation(int clientId, string userId)
    {
        var result = await _service.ResendClientUserActivationAsync(userId, clientId, CabinetId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "BackOfficeAccess")]
public class DeclarationsController : ControllerBase
{
    private readonly IDeclarationService _service;
    public DeclarationsController(IDeclarationService service) => _service = service;

    private int CabinetId => int.TryParse(User.FindFirst("CabinetId")?.Value, out var id) ? id : 0;
    private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? clientId, [FromQuery] string? type, [FromQuery] string? statut, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] int? exerciceId = null)
    {
        var result = await _service.GetAllAsync(CabinetId, clientId, type, statut, page, pageSize, exerciceId);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await _service.GetByIdAsync(id, CabinetId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [Authorize(Policy = "declaration.create")]
    public async Task<IActionResult> Create([FromBody] CreateDeclarationDto dto)
    {
        var result = await _service.CreateAsync(dto, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/shell")]
    [Authorize(Policy = "declaration.edit")]
    public async Task<IActionResult> SaveShell(int id, [FromBody] DeclarationShellSaveDto dto)
    {
        var result = await _service.SaveShellAsync(id, dto.MontantDu, dto.MontantCredit, dto.Notes, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/suggest-acompte")]
    [Authorize(Policy = "declaration.edit")]
    public async Task<IActionResult> SuggestAcompte(int id)
    {
        var result = await _service.SuggestAcompteAsync(id, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/suggest-reg-is")]
    [Authorize(Policy = "declaration.edit")]
    public async Task<IActionResult> SuggestRegIs(int id)
    {
        var result = await _service.SuggestRegIsAsync(id, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id}/cnss/export-dns")]
    [Authorize(Policy = "declaration.view")]
    public async Task<IActionResult> ExportCnssDns(int id)
    {
        var result = await _service.ExportCnssDnsCsvAsync(id, CabinetId);
        if (!result.Success || result.Data == null) return BadRequest(result);
        return File(result.Data, "text/csv", $"DNS_CNSS_{id}.csv");
    }

    [HttpGet("{id}/cnss/employees")]
    [Authorize(Policy = "declaration.view")]
    public async Task<IActionResult> GetCnssEmployees(int id)
    {
        var result = await _service.GetCnssEmployeesAsync(id, CabinetId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/cnss/employees")]
    [Authorize(Policy = "declaration.edit")]
    public async Task<IActionResult> AddCnssEmployee(int id, [FromBody] CnssEmployeeAddDto dto)
    {
        var result = await _service.AddCnssEmployeeAsync(id, dto, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}/cnss/employees/{employeeId:int}")]
    [Authorize(Policy = "declaration.edit")]
    public async Task<IActionResult> UpdateCnssEmployee(int id, int employeeId, [FromBody] CnssEmployeeAddDto dto)
    {
        var result = await _service.UpdateCnssEmployeeAsync(id, employeeId, dto, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/cnss/employees/import")]
    [Authorize(Policy = "declaration.edit")]
    public async Task<IActionResult> ImportCnssEmployees(int id, [FromBody] List<CnssEmployeeImportRowDto> rows)
    {
        var result = await _service.ImportCnssEmployeesAsync(id, rows, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id}/cnss/employees/template")]
    [Authorize(Policy = "declaration.view")]
    public IActionResult CnssEmployeesCsvTemplate(int id)
    {
        var csv = "Matricule;CIN;CNSS;Nom;Prenom;DateEmbauche;DateSortie;MotifSortie;JoursTravailles;SalaireBrut;Sexe\r\n"
                + "E001;BE123456;1234567890;Alaoui;Karim;2026-01-05;;;26;8500;M\r\n"
                + "E002;BE654321;0987654321;Benali;Sara;2025-03-01;2026-01-20;Démission;15;7200;F\r\n";
        var bytes = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(csv)).ToArray();
        return File(bytes, "text/csv", $"CNSS_Employees_Template_{id}.csv");
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "declaration.edit")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Ã¢â€â‚¬Ã¢â€â‚¬ TVA Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬
    [HttpPost("tva/{clientId}")]
    [Authorize(Policy = "declaration.create")]
    public async Task<IActionResult> CreateTVA(int clientId)
    {
        var result = await _service.CreateTVAAsync(clientId, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("tva/{declarationId}/data")]
    public async Task<IActionResult> GetTVAData(int declarationId)
    {
        var result = await _service.GetTVADataAsync(declarationId, CabinetId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("tva/{declarationId}/data")]
    [Authorize(Policy = "declaration.edit")]
    public async Task<IActionResult> SaveTVAData(int declarationId, [FromBody] DeclarationTVADto dto)
    {
        var result = await _service.SaveTVADataAsync(declarationId, dto, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("tva/{declarationId}/ajustements")]
    [Authorize(Policy = "declaration.edit")]
    public async Task<IActionResult> SaveTVAAjustements(int declarationId, [FromBody] TVAAjustementsRequest request)
    {
        var result = await _service.SaveTVAAjustementsAsync(declarationId, request.TVAFacturee, request.TVADeductible, request.CreditPrecedent, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("tva/calculate")]
    public async Task<IActionResult> CalculateTVA([FromBody] DeclarationTVADto dto)
    {
        var result = await _service.CalculateTVAAsync(dto);
        return Ok(result);
    }


    // Ã¢â€â‚¬Ã¢â€â‚¬ TVA Invoice Lines Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬
    [HttpPost("tva/{declarationId}/lignes")]
    [Authorize(Policy = "declaration.edit")]
    public async Task<IActionResult> AddTVALigne(int declarationId, [FromBody] TVALigneFactureDto dto)
    {
        var result = await _service.AddTVALigneAsync(declarationId, dto, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("tva/{declarationId}/lignes/{ligneId}")]
    [Authorize(Policy = "declaration.edit")]
    public async Task<IActionResult> DeleteTVALigne(int declarationId, int ligneId)
    {
        var result = await _service.DeleteTVALigneAsync(declarationId, ligneId, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("tva/{declarationId}/import-lignes")]
    [Authorize(Policy = "declaration.edit")]
    public async Task<IActionResult> ImportTVALignes(int declarationId, [FromBody] List<TVALigneFactureDto> lignes)
    {
        var result = await _service.ImportTVALignesAsync(declarationId, lignes, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }


    // Ã¢â€â‚¬Ã¢â€â‚¬ IS Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬
    [HttpPost("is/{clientId}/{anneeFiscale}")]
    [Authorize(Policy = "declaration.create")]
    public async Task<IActionResult> CreateIS(int clientId, int anneeFiscale)
    {
        var result = await _service.CreateISAsync(clientId, anneeFiscale, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("is/{declarationId}/data")]
    public async Task<IActionResult> GetISData(int declarationId)
    {
        var result = await _service.GetISDataAsync(declarationId, CabinetId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("is/{declarationId}/data")]
    [Authorize(Policy = "declaration.edit")]
    public async Task<IActionResult> SaveISData(int declarationId, [FromBody] DeclarationISDto dto)
    {
        var result = await _service.SaveISDataAsync(declarationId, dto, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("is/calculate")]
    public async Task<IActionResult> CalculateIS([FromBody] DeclarationISDto dto)
    {
        var result = await _service.CalculateISAsync(dto);
        return Ok(result);
    }

    [HttpGet("is/{declarationId}/checklist")]
    public async Task<IActionResult> GetISChecklist(int declarationId)
    {
        var result = await _service.GetChecklistAsync(declarationId, CabinetId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("is/{declarationId}/checklist/ensure")]
    [Authorize(Policy = "declaration.edit")]
    public async Task<IActionResult> EnsureISChecklist(int declarationId)
    {
        var result = await _service.EnsureAnnualChecklistAsync(declarationId, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("is/{declarationId}/checklist/{itemId}/received")]
    [Authorize(Policy = "declaration.edit")]
    public async Task<IActionResult> MarkChecklistReceived(int declarationId, int itemId, [FromQuery] int? documentId = null)
    {
        var result = await _service.MarkChecklistItemReceivedAsync(declarationId, itemId, documentId, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Ã¢â€â‚¬Ã¢â€â‚¬ IR Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬
    [HttpPost("ir/{clientId}/{anneeFiscale}")]
    [Authorize(Policy = "declaration.create")]
    public async Task<IActionResult> CreateIR(int clientId, int anneeFiscale)
    {
        var result = await _service.CreateIRAsync(clientId, anneeFiscale, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("ir/{declarationId}/data")]
    public async Task<IActionResult> GetIRData(int declarationId)
    {
        var result = await _service.GetIRDataAsync(declarationId, CabinetId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("ir/{declarationId}/data")]
    [Authorize(Policy = "declaration.edit")]
    public async Task<IActionResult> SaveIRData(int declarationId, [FromBody] DeclarationIRDto dto)
    {
        var result = await _service.SaveIRDataAsync(declarationId, dto, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("ir/calculate")]
    public async Task<IActionResult> CalculateIR([FromBody] DeclarationIRDto dto)
    {
        var result = await _service.CalculateIRAsync(dto);
        return Ok(result);
    }

    // Ã¢â€â‚¬Ã¢â€â‚¬ CNSS Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬
    [HttpPost("cnss/{clientId}")]
    [Authorize(Policy = "declaration.create")]
    public async Task<IActionResult> CreateCNSS(int clientId)
    {
        var result = await _service.CreateCNSSAsync(clientId, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("cnss/{declarationId}/data")]
    public async Task<IActionResult> GetCNSSData(int declarationId)
    {
        var result = await _service.GetCNSSDataAsync(declarationId, CabinetId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("cnss/{declarationId}/data")]
    [Authorize(Policy = "declaration.edit")]
    public async Task<IActionResult> SaveCNSSData(int declarationId, [FromBody] DeclarationCNSSDto dto)
    {
        var result = await _service.SaveCNSSDataAsync(declarationId, dto, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("cnss/{declarationId}/calculate")]
    public async Task<IActionResult> CalculateCNSS(int declarationId)
    {
        var result = await _service.CalculateCNSSAsync(declarationId, CabinetId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Ã¢â€â‚¬Ã¢â€â‚¬ Status Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬
    [HttpPut("{id}/status")]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeStatusRequest request)
    {
        // Holder step actions (Payée / SIMPL / Valider) need Edit OR Submit OR Validate
        var canStatus = User.HasClaim("Permission", "declaration.edit")
            || User.HasClaim("Permission", "declaration.submit")
            || User.HasClaim("Permission", "declaration.validate");
        if (!canStatus)
            return Forbid();

        var result = await _service.ChangeStatusAsync(id, request.NewStatut, CabinetId, UserId, request.Comment);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id}/history")]
    [Authorize(Policy = "declaration.view")]
    public async Task<IActionResult> GetStatusHistory(int id)
        => Ok(await _service.GetStatusHistoryAsync(id, CabinetId));
}

public record ChangeStatusRequest(string NewStatut, string? Comment = null);

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "BackOfficeAccess")]
public class PayrollController : ControllerBase
{
    private readonly IPayrollService _service;
    public PayrollController(IPayrollService service) => _service = service;

    private int CabinetId => int.TryParse(User.FindFirst("CabinetId")?.Value, out var id) ? id : 0;
    private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? clientId = null, [FromQuery] int? exerciceId = null)
    {
        var result = await _service.GetAllPeriodsAsync(CabinetId, clientId, exerciceId);
        return Ok(result);
    }

    [HttpPost("initialize")]
    [Authorize(Policy = ClaimDefinitions.PayrollCreate)]
    public async Task<IActionResult> Initialize([FromBody] PayrollInitRequest req)
    {
        var result = await _service.InitializeAsync(req.ClientId, req.Annee, req.Mois, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPeriod(int id)
    {
        var result = await _service.GetPeriodAsync(id, CabinetId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("{id}/payslips")]
    public async Task<IActionResult> GetPaySlips(int id)
    {
        var result = await _service.GetPaySlipsAsync(id, CabinetId);
        return Ok(result);
    }

    [HttpPut("{periodId}/variables")]
    [Authorize(Policy = ClaimDefinitions.PayrollEdit)]
    public async Task<IActionResult> UpdateVariables(int periodId, [FromBody] PaySlipVariablesDto dto)
    {
        var result = await _service.UpdateVariablesAsync(periodId, dto, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("period-variables/{employeeId}/{annee}/{mois}")]
    public async Task<IActionResult> GetVariablesForPeriod(int employeeId, int annee, int mois)
    {
        var result = await _service.GetVariablesForPeriodAsync(employeeId, annee, mois, CabinetId);
        return Ok(result);
    }

    [HttpPut("period-variables/{employeeId}/{annee}/{mois}")]
    [Authorize(Policy = ClaimDefinitions.PayrollEdit)]
    public async Task<IActionResult> UpdateVariablesBulk(int employeeId, int annee, int mois, [FromBody] List<VariableEmployeCreateDto> variables)
    {
        var result = await _service.UpdateVariablesBulkAsync(employeeId, annee, mois, variables, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // === Affectation Rubriques (ERP-style: assign rubriques to employees) ===

    [HttpGet("affectations/{employeeId}")]
    public async Task<IActionResult> GetAffectations(int employeeId)
    {
        var result = await _service.GetAffectationsAsync(employeeId, CabinetId);
        return Ok(result);
    }

    [HttpGet("affectations/{employeeId}/disponibles")]
    public async Task<IActionResult> GetRubriquesDisponibles(int employeeId, [FromQuery] int? clientId = null)
    {
        var result = await _service.GetRubriquesDisponiblesAsync(employeeId, CabinetId, clientId);
        return Ok(result);
    }

    [HttpPost("affectations")]
    [Authorize(Policy = ClaimDefinitions.PayrollEdit)]
    public async Task<IActionResult> AddAffectation([FromBody] AffectationRubriqueCreateDto dto)
    {
        var result = await _service.AddAffectationAsync(dto, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("affectations/{id}")]
    [Authorize(Policy = ClaimDefinitions.PayrollEdit)]
    public async Task<IActionResult> UpdateAffectation(int id, [FromBody] AffectationRubriqueCreateDto dto)
    {
        var result = await _service.UpdateAffectationAsync(id, dto, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("affectations/{id}")]
    [Authorize(Policy = ClaimDefinitions.PayrollEdit)]
    public async Task<IActionResult> RemoveAffectation(int id)
    {
        var result = await _service.RemoveAffectationAsync(id, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/calculate")]
    [Authorize(Policy = ClaimDefinitions.PayrollEdit)]
    public async Task<IActionResult> CalculateAll(int id)
    {
        var result = await _service.CalculateAllAsync(id, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/validate")]
    [Authorize(Policy = ClaimDefinitions.PayrollValidate)]
    public async Task<IActionResult> Validate(int id)
    {
        var result = await _service.ValidateAsync(id, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Ã¢â€â‚¬Ã¢â€â‚¬ Bulletin Ã¢â€â‚¬Ã¢â€â‚¬
    [HttpGet("bulletin/{paySlipId}")]
    public async Task<IActionResult> GetBulletin(int paySlipId)
    {
        var result = await _service.GetBulletinAsync(paySlipId, CabinetId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    // Ã¢â€â‚¬Ã¢â€â‚¬ Rubriques CRUD Ã¢â€â‚¬Ã¢â€â‚¬
    [HttpGet("rubriques")]
    public async Task<IActionResult> GetRubriques([FromQuery] int? clientId = null)
    {
        var result = await _service.GetRubriquesAsync(CabinetId, clientId);
        return Ok(result);
    }

    [HttpGet("rubriques/{id}")]
    public async Task<IActionResult> GetRubrique(int id)
    {
        var result = await _service.GetRubriqueAsync(id, CabinetId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("rubriques")]
    [Authorize(Policy = ClaimDefinitions.PayrollEdit)]
    public async Task<IActionResult> CreateRubrique([FromBody] RubriqueCreateDto dto)
    {
        var result = await _service.CreateRubriqueAsync(dto, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("rubriques/{id}")]
    [Authorize(Policy = ClaimDefinitions.PayrollEdit)]
    public async Task<IActionResult> UpdateRubrique(int id, [FromBody] RubriqueCreateDto dto)
    {
        var result = await _service.UpdateRubriqueAsync(id, dto, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("rubriques/{id}/toggle")]
    [Authorize(Policy = ClaimDefinitions.PayrollEdit)]
    public async Task<IActionResult> ToggleRubrique(int id)
    {
        var result = await _service.ToggleRubriqueAsync(id, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("rubriques/{id}")]
    [Authorize(Policy = ClaimDefinitions.PayrollEdit)]
    public async Task<IActionResult> DeleteRubrique(int id)
    {
        var result = await _service.DeleteRubriqueAsync(id, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Ã¢â€â‚¬Ã¢â€â‚¬ Constantes CRUD Ã¢â€â‚¬Ã¢â€â‚¬
    [HttpGet("constantes")]
    public async Task<IActionResult> GetConstantes([FromQuery] string? categorie = null)
    {
        var result = await _service.GetConstantesAsync(CabinetId, categorie);
        return Ok(result);
    }

    [HttpPost("constantes")]
    [Authorize(Policy = ClaimDefinitions.PayrollEdit)]
    public async Task<IActionResult> CreateConstante([FromBody] ConstanteGlobaleDto dto)
    {
        var result = await _service.CreateConstanteAsync(dto, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("constantes/{id}")]
    [Authorize(Policy = ClaimDefinitions.PayrollEdit)]
    public async Task<IActionResult> UpdateConstante(int id, [FromBody] ConstanteGlobaleDto dto)
    {
        var result = await _service.UpdateConstanteAsync(id, dto, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("constantes/{id}")]
    [Authorize(Policy = ClaimDefinitions.PayrollEdit)]
    public async Task<IActionResult> DeleteConstante(int id)
    {
        var result = await _service.DeleteConstanteAsync(id, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Ã¢â€â‚¬Ã¢â€â‚¬ Variables CRUD Ã¢â€â‚¬Ã¢â€â‚¬
    [HttpGet("variables/{employeeId}")]
    public async Task<IActionResult> GetVariables(int employeeId)
    {
        var result = await _service.GetVariablesEmployeAsync(employeeId, CabinetId);
        return Ok(result);
    }

    [HttpPost("variables")]
    [Authorize(Policy = ClaimDefinitions.PayrollEdit)]
    public async Task<IActionResult> CreateVariable([FromBody] VariableEmployeCreateDto dto)
    {
        var result = await _service.CreateVariableAsync(dto, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("variables/{id}")]
    [Authorize(Policy = ClaimDefinitions.PayrollEdit)]
    public async Task<IActionResult> UpdateVariable(int id, [FromBody] VariableEmployeCreateDto dto)
    {
        var result = await _service.UpdateVariableAsync(id, dto, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("variables/{id}")]
    [Authorize(Policy = ClaimDefinitions.PayrollEdit)]
    public async Task<IActionResult> DeleteVariable(int id)
    {
        var result = await _service.DeleteVariableAsync(id, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Ã¢â€â‚¬Ã¢â€â‚¬ Cumuls Ã¢â€â‚¬Ã¢â€â‚¬
    [HttpGet("cumuls/{employeeId}/{annee}")]
    public async Task<IActionResult> GetCumuls(int employeeId, int annee)
    {
        var result = await _service.GetCumulsAsync(employeeId, annee, CabinetId);
        return Ok(result);
    }

    // Ã¢â€â‚¬Ã¢â€â‚¬ Seed Ã¢â€â‚¬Ã¢â€â‚¬
    [HttpPost("seed/rubriques")]
    [Authorize(Policy = ClaimDefinitions.SystemAdmin)]
    public async Task<IActionResult> SeedRubriques()
    {
        var result = await _service.SeedDefaultRubriquesAsync(CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("seed/constantes")]
    [Authorize(Policy = ClaimDefinitions.SystemAdmin)]
    public async Task<IActionResult> SeedConstantes()
    {
        var result = await _service.SeedDefaultConstantesAsync(CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    [HttpPost("seed/demo-2025/{clientId}")]
    [Authorize(Policy = ClaimDefinitions.SystemAdmin)]
    public async Task<IActionResult> SeedDemo2025(int clientId)
    {
        var result = await _service.SeedDemo2025Async(clientId, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

public record PayrollInitRequest(int ClientId, int Annee, int Mois);

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _service;
    public NotificationsController(INotificationService service) => _service = service;

    private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool unreadOnly = false, [FromQuery] int count = 20)
    {
        var result = await _service.GetUserNotificationsAsync(UserId, unreadOnly, count);
        return Ok(result);
    }

    [HttpGet("count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var result = await _service.GetUnreadCountAsync(UserId);
        return Ok(result);
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var result = await _service.MarkAsReadAsync(id, UserId);
        return Ok(result);
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var result = await _service.MarkAllAsReadAsync(UserId);
        return Ok(result);
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "BackOfficeAccess")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;
    public DashboardController(IDashboardService service) => _service = service;

    private int CabinetId => int.TryParse(User.FindFirst("CabinetId")?.Value, out var id) ? id : 0;
    private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

    [HttpGet("manager")]
    public async Task<IActionResult> GetManagerDashboard()
    {
        var result = await _service.GetManagerDashboardAsync(CabinetId, UserId);
        return Ok(result);
    }

    [HttpGet("assistant")]
    public async Task<IActionResult> GetAssistantDashboard()
    {
        var result = await _service.GetAssistantDashboardAsync(CabinetId, UserId);
        return Ok(result);
    }
}

[ApiController]
[Route("api/dashboard")]
[Authorize(Policy = "ExtranetAccess")]
public class ExtranetDashboardController : ControllerBase
{
    private readonly IDashboardService _service;
    public ExtranetDashboardController(IDashboardService service) => _service = service;

    private int ClientId => int.TryParse(User.FindFirst("ClientId")?.Value, out var id) ? id : 0;

    [HttpGet("extranet")]
    public async Task<IActionResult> Get([FromQuery] int? exerciceComptableId = null)
    {
        if (ClientId == 0) return Forbid();
        return Ok(await _service.GetExtranetDashboardAsync(ClientId, exerciceComptableId));
    }
}

[ApiController]
[Route("api/client-exercices")]
[Authorize(Policy = "ExtranetAccess")]
public class ClientExercicesController : ControllerBase
{
    private readonly IComptabiliteService _compta;
    public ClientExercicesController(IComptabiliteService compta) => _compta = compta;

    private int CabinetId => int.TryParse(User.FindFirst("CabinetId")?.Value, out var id) ? id : 0;
    private int ClientId => int.TryParse(User.FindFirst("ClientId")?.Value, out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (ClientId == 0) return Forbid();
        return Ok(await _compta.GetExercicesAsync(ClientId, CabinetId));
    }
}

[ApiController]
[Route("api/client-context")]
[Authorize(Policy = "ExtranetAccess")]
public class ClientContextController : ControllerBase
{
    private readonly IDashboardService _dashboard;
    public ClientContextController(IDashboardService dashboard) => _dashboard = dashboard;

    private int CabinetId => int.TryParse(User.FindFirst("CabinetId")?.Value, out var id) ? id : 0;
    private int ClientId => int.TryParse(User.FindFirst("ClientId")?.Value, out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int? exerciceComptableId = null)
    {
        if (ClientId == 0) return Forbid();
        return Ok(await _dashboard.GetExtranetContextAsync(ClientId, CabinetId, exerciceComptableId));
    }
}

[ApiController]
[Route("api/client-profile")]
[Authorize(Policy = "ExtranetAccess")]
public class ClientProfileController : ControllerBase
{
    private readonly IClientService _clientService;
    public ClientProfileController(IClientService clientService) => _clientService = clientService;

    private int CabinetId => int.TryParse(User.FindFirst("CabinetId")?.Value, out var id) ? id : 0;
    private int ClientId => int.TryParse(User.FindFirst("ClientId")?.Value, out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        if (ClientId == 0) return Forbid();
        var result = await _clientService.GetByIdAsync(ClientId, CabinetId);
        return result.Success ? Ok(result) : NotFound(result);
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReferenceDataController : ControllerBase
{
    private readonly IReferenceDataService _service;
    public ReferenceDataController(IReferenceDataService service) => _service = service;

    [HttpGet("{category}")]
    public async Task<IActionResult> GetByCategory(string category, [FromQuery] int? cabinetId = null)
    {
        var result = await _service.GetByCategoryAsync(category, cabinetId);
        return Ok(result);
    }

    [HttpGet("multi")]
    public async Task<IActionResult> GetMultipleCategories([FromQuery] string categories, [FromQuery] int? cabinetId = null)
    {
        var cats = categories.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var dict = new Dictionary<string, List<ReferenceDataDto>>();
        foreach (var cat in cats)
        {
            var result = await _service.GetByCategoryAsync(cat, cabinetId);
            dict[cat] = result.Data?.Select(r => new ReferenceDataDto
            {
                Id = r.Id,
                Category = r.Category,
                Code = r.Code,
                Label = r.Label,
                Value = r.Value,
                NumericValue = r.NumericValue,
                SortOrder = r.SortOrder,
                Couleur = r.Couleur,
                CssClass = r.CssClass,
                Icone = r.Icone
            }).ToList() ?? new List<ReferenceDataDto>();
        }
        return Ok(ApiResponse<Dictionary<string, List<ReferenceDataDto>>>.Ok(dict));
    }

    [HttpPost]
    [Authorize(Policy = "system.admin")]
    public async Task<IActionResult> CreateOrUpdate([FromBody] Domain.Common.ReferenceData data)
    {
        var result = await _service.CreateOrUpdateAsync(data);
        return Ok(result);
    }

    [HttpGet("all")]
    [Authorize(Policy = "AdminAccess")]
    public async Task<IActionResult> GetAll([FromServices] MetaComptaDbContext db)
    {
        var items = await db.ReferenceData
            .Where(r => r.CabinetId == null)
            .OrderBy(r => r.Category).ThenBy(r => r.SortOrder)
            .ToListAsync();
        var grouped = items.GroupBy(r => r.Category)
            .ToDictionary(g => g.Key, g => g.Select(r => new
            {
                r.Id,
                r.Category,
                r.Code,
                r.Label,
                r.Value,
                r.NumericValue,
                r.SortOrder,
                r.IsActive
            }).ToList());
        return Ok(ApiResponse<object>.Ok(grouped));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminAccess")]
    public async Task<IActionResult> Delete(int id, [FromServices] MetaComptaDbContext db)
    {
        var item = await db.ReferenceData.FindAsync(id);
        if (item == null) return NotFound(ApiResponse<object>.Fail("Donnee non trouvee."));
        db.ReferenceData.Remove(item);
        await db.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, "Supprime."));
    }
}

// ===== Admin: Cabinets Management =====
[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Policy = "AdminAccess")]
public class CabinetsController : ControllerBase
{
    private readonly MetaCompta.Infrastructure.Data.MetaComptaDbContext _db;
    public CabinetsController(MetaCompta.Infrastructure.Data.MetaComptaDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
    {
        var query = _db.Cabinets.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(s) || (c.ICE != null && c.ICE.Contains(s)));
        }
        var total = await query.CountAsync();
        var items = await query.OrderBy(c => c.Name).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(c => new { c.Id, c.Name, c.Email, c.ICE, c.IsActive, c.CreatedAt, UserCount = c.Users.Count })
            .ToListAsync();
        return Ok(ApiResponse<object>.Ok(new { Items = items, TotalCount = total, Page = page, PageSize = pageSize }));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var cab = await _db.Cabinets.Include(c => c.Users).FirstOrDefaultAsync(c => c.Id == id);
        if (cab == null) return NotFound(ApiResponse<object>.Fail("Cabinet non trouvÃƒÂ©."));
        return Ok(ApiResponse<object>.Ok(new { cab.Id, cab.Name, cab.Email, cab.ICE, cab.IsActive, cab.CreatedAt, Users = cab.Users.Select(u => new { u.Id, u.Email, u.FirstName, u.LastName, u.IsActive }) }));
    }

    [HttpPut("{id}/toggle")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var cab = await _db.Cabinets.FindAsync(id);
        if (cab == null) return NotFound();
        cab.IsActive = !cab.IsActive;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, cab.IsActive ? "Cabinet activÃƒÂ©." : "Cabinet dÃƒÂ©sactivÃƒÂ©."));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CabinetCreateDto dto,
        [FromServices] UserManager<ApplicationUser> userManager)
    {
        var cabinet = new Cabinet
        {
            Name = dto.Name,
            Email = dto.Email,
            ICE = dto.ICE,
            IF = dto.IF,
            RC = dto.RC,
            Address = dto.Address,
            City = dto.City,
            Phone = dto.Phone,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _db.Cabinets.Add(cabinet);
        await _db.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(dto.AdminEmail) && !string.IsNullOrWhiteSpace(dto.AdminPassword))
        {
            var adminUser = new ApplicationUser
            {
                UserName = dto.AdminEmail,
                Email = dto.AdminEmail,
                FirstName = dto.AdminFirstName ?? "Admin",
                LastName = dto.AdminLastName ?? cabinet.Name,
                CabinetId = cabinet.Id,
                IsActive = true,
                EmailConfirmed = true
            };
            var identityResult = await userManager.CreateAsync(adminUser, dto.AdminPassword);
            if (identityResult.Succeeded)
                await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
        }

        return Ok(ApiResponse<object>.Ok(new { cabinet.Id }, "Cabinet cree avec succes."));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CabinetEditAdminDto dto)
    {
        var cab = await _db.Cabinets.FindAsync(id);
        if (cab == null) return NotFound(ApiResponse<object>.Fail("Cabinet non trouve."));

        if (dto.Name != null) cab.Name = dto.Name;
        if (dto.Email != null) cab.Email = dto.Email;
        if (dto.ICE != null) cab.ICE = dto.ICE;
        if (dto.IF != null) cab.IF = dto.IF;
        if (dto.RC != null) cab.RC = dto.RC;
        if (dto.Address != null) cab.Address = dto.Address;
        if (dto.City != null) cab.City = dto.City;
        if (dto.Phone != null) cab.Phone = dto.Phone;

        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(new { cab.Id }, "Cabinet mis a jour."));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var cab = await _db.Cabinets.Include(c => c.Clients).Include(c => c.Users).FirstOrDefaultAsync(c => c.Id == id);
        if (cab == null) return NotFound(ApiResponse<object>.Fail("Cabinet non trouve."));
        if (cab.Clients.Any()) return BadRequest(ApiResponse<object>.Fail("Impossible de supprimer : ce cabinet a des clients."));

        _db.Cabinets.Remove(cab);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, "Cabinet supprime."));
    }
}

// ===== Admin: AuditLogs =====
[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Policy = "AdminAccess")]
public class AuditLogsController : ControllerBase
{
    private readonly MetaCompta.Infrastructure.Data.MetaComptaDbContext _db;
    public AuditLogsController(MetaCompta.Infrastructure.Data.MetaComptaDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] int? cabinetId = null, [FromQuery] string? action = null)
    {
        var query = _db.AuditLogs.AsQueryable();
        if (cabinetId.HasValue) query = query.Where(a => a.CabinetId == cabinetId);
        if (!string.IsNullOrWhiteSpace(action)) query = query.Where(a => a.Action == action);
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(a => a.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(ApiResponse<object>.Ok(new { Items = items, TotalCount = total, Page = page, PageSize = pageSize }));
    }
}

// Ã¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢Â
// CABINET MANAGEMENT CONTROLLER (BackOffice Ã¢â‚¬â€ Current Cabinet)
// Ã¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢Â

/// <summary>
/// Manages the current user's cabinet: employees, roles, permissions, cabinet info, and profile.
/// All endpoints are scoped to the authenticated user's CabinetId.
/// </summary>
[ApiController]
[Route("api/cabinet")]
[Authorize(Policy = "BackOfficeAccess")]
public class CabinetManagementController : ControllerBase
{
    private readonly ICabinetService _cabinetService;
    public CabinetManagementController(ICabinetService cabinetService) => _cabinetService = cabinetService;

    private int CabinetId => int.TryParse(User.FindFirst("CabinetId")?.Value, out var id) ? id : 0;
    private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

    // Ã¢â€â‚¬Ã¢â€â‚¬ Cabinet Info Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬

    [HttpGet("info")]
    public async Task<IActionResult> GetCabinet()
    {
        var result = await _cabinetService.GetCabinetAsync(CabinetId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPut("info")]
    [Authorize(Policy = "cabinet.edit")]
    public async Task<IActionResult> UpdateCabinet([FromBody] CabinetEditDto dto)
    {
        var result = await _cabinetService.UpdateCabinetAsync(CabinetId, dto, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Ã¢â€â‚¬Ã¢â€â‚¬ Employees Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬

    [HttpGet("employees")]
    public async Task<IActionResult> GetEmployees(
        [FromQuery] string? departement = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null)
    {
        var result = await _cabinetService.GetEmployeesAsync(CabinetId, departement, page, pageSize, search);
        return Ok(result);
    }

    [HttpGet("employees/{id}")]
    public async Task<IActionResult> GetEmployee(string id)
    {
        var result = await _cabinetService.GetEmployeeByIdAsync(id, CabinetId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("employees")]
    [Authorize(Policy = ClaimDefinitions.CabinetEmployeeCreate)]
    public async Task<IActionResult> CreateEmployee([FromBody] CabinetEmployeeCreateDto dto)
    {
        var result = await _cabinetService.CreateEmployeeAsync(dto, CabinetId, UserId);
        return result.Success ? CreatedAtAction(nameof(GetEmployee), new { id = result.Data?.Id }, result) : BadRequest(result);
    }

    [HttpPut("employees/{id}")]
    [Authorize(Policy = ClaimDefinitions.CabinetEmployeeEdit)]
    public async Task<IActionResult> UpdateEmployee(string id, [FromBody] CabinetEmployeeUpdateDto dto)
    {
        var result = await _cabinetService.UpdateEmployeeAsync(id, dto, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("employees/{id}/toggle")]
    [Authorize(Policy = ClaimDefinitions.CabinetEmployeeEdit)]
    public async Task<IActionResult> ToggleEmployee(string id)
    {
        var result = await _cabinetService.ToggleEmployeeActiveAsync(id, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("employees/{id}/resend-activation")]
    [Authorize(Policy = ClaimDefinitions.CabinetEmployeeEdit)]
    public async Task<IActionResult> ResendEmployeeActivation(string id)
    {
        var result = await _cabinetService.ResendEmployeeActivationAsync(id, CabinetId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Ã¢â€â‚¬Ã¢â€â‚¬ Roles & Permissions Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
    {
        var result = await _cabinetService.GetRolesAsync(CabinetId, page, pageSize, search);
        return Ok(result);
    }

    [HttpGet("roles/{roleId}")]
    public async Task<IActionResult> GetRole(string roleId)
    {
        var result = await _cabinetService.GetRoleByIdAsync(roleId, CabinetId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("roles")]
    [Authorize(Policy = ClaimDefinitions.CabinetRoleManage)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
    {
        var result = await _cabinetService.CreateRoleAsync(request.Name, request.Description, request.Permissions, CabinetId, UserId);
        return result.Success ? CreatedAtAction(nameof(GetRole), new { roleId = result.Data?.Id }, result) : BadRequest(result);
    }

    [HttpPut("roles/{roleId}/permissions")]
    [Authorize(Policy = ClaimDefinitions.CabinetRoleManage)]
    public async Task<IActionResult> UpdateRolePermissions(string roleId, [FromBody] UpdatePermissionsRequest request)
    {
        var result = await _cabinetService.UpdateRolePermissionsAsync(roleId, request.Permissions, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Ã¢â€â‚¬Ã¢â€â‚¬ Audit Logs Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬

    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] string? userId = null,
        [FromQuery] string? action = null,
        [FromQuery] DateTime? dateDebut = null,
        [FromQuery] DateTime? dateFin = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await _cabinetService.GetAuditLogsAsync(CabinetId, userId, action, dateDebut, dateFin, page, pageSize);
        return Ok(result);
    }

    // Ã¢â€â‚¬Ã¢â€â‚¬ Profile Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var result = await _cabinetService.GetProfileAsync(UserId, CabinetId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    // Ã¢â€â‚¬Ã¢â€â‚¬ Claim Definitions Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬

    [HttpGet("claims")]
    public IActionResult GetAllClaims()
    {
        return Ok(ApiResponse<object>.Ok(new
        {
            All = ClaimDefinitions.All,
            ByDomain = ClaimDefinitions.ByDomain
        }));
    }
}

// Ã¢â€â‚¬Ã¢â€â‚¬ Request models for CabinetManagementController Ã¢â€â‚¬Ã¢â€â‚¬
public class CreateRoleRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> Permissions { get; set; } = new();
}

public class UpdatePermissionsRequest
{
    public List<string> Permissions { get; set; } = new();
}

// Ã¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢Â
//  DOCUMENTS COMMERCIAUX Ã¢â‚¬â€ Devis, Bons de Commande, Contrats, Factures, Paiements
// Ã¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢Â

// ===== Devis Controller =====
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "BackOfficeAccess")]
public class DevisController : ControllerBase
{
    private readonly ICommercialService _service;
    public DevisController(ICommercialService service) => _service = service;

    private int CabinetId => int.TryParse(User.FindFirst("CabinetId")?.Value, out var id) ? id : 0;
    private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? clientId, [FromQuery] string? statut, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _service.GetDevisAsync(CabinetId, clientId, statut, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await _service.GetDevisByIdAsync(id, CabinetId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [Authorize(Policy = "commercial.create")]
    public async Task<IActionResult> Create([FromBody] DevisCreateDto dto)
    {
        var result = await _service.CreateDevisAsync(dto, CabinetId, UserId);
        return result.Success ? CreatedAtAction(nameof(Get), new { id = result.Data?.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "commercial.edit")]
    public async Task<IActionResult> Update(int id, [FromBody] DevisCreateDto dto)
    {
        var result = await _service.UpdateDevisAsync(id, dto, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "commercial.delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteDevisAsync(id, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/soumettre")]
    [Authorize(Policy = "commercial.edit")]
    public async Task<IActionResult> Soumettre(int id)
    {
        var result = await _service.SoumettreDevisAsync(id, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/approuver")]
    [Authorize(Policy = "commercial.validate")]
    public async Task<IActionResult> Approuver(int id)
    {
        var result = await _service.ApprouverDevisAsync(id, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/rejeter")]
    [Authorize(Policy = "commercial.validate")]
    public async Task<IActionResult> Rejeter(int id, [FromBody] RejetRequest req)
    {
        var result = await _service.RejeterDevisAsync(id, req.Motif, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/convertir")]
    [Authorize(Policy = "commercial.convert")]
    public async Task<IActionResult> Convertir(int id)
    {
        var result = await _service.ConvertirDevisEnBonCommandeAsync(id, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/convertir-en-contrat")]
    [Authorize(Policy = "commercial.convert")]
    public async Task<IActionResult> ConvertirEnContrat(int id, [FromBody] ConvertDevisEnContratDto dto)
    {
        if (dto == null)
            return BadRequest(ApiResponse<ContratCommercialDto>.Fail("Requête invalide : corps JSON manquant ou incorrect."));

        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<ContratCommercialDto>.Fail("Requête invalide : vérifiez les données soumises."));

        dto.DevisId = id;
        var result = await _service.ConvertirDevisEnContratAsync(dto, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/dupliquer")]
    [Authorize(Policy = "commercial.create")]
    public async Task<IActionResult> Dupliquer(int id)
    {
        var result = await _service.DupliquerDevisAsync(id, CabinetId, UserId);
        return result.Success ? CreatedAtAction(nameof(Get), new { id = result.Data?.Id }, result) : BadRequest(result);
    }
}

// ===== Bons de Commande Controller =====
[ApiController]
[Route("api/bons-commande")]
[Authorize(Policy = "BackOfficeAccess")]
public class BonsCommandeController : ControllerBase
{
    private readonly ICommercialService _service;
    public BonsCommandeController(ICommercialService service) => _service = service;

    private int CabinetId => int.TryParse(User.FindFirst("CabinetId")?.Value, out var id) ? id : 0;
    private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? clientId, [FromQuery] string? statut, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _service.GetBonsCommandeAsync(CabinetId, clientId, statut, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await _service.GetBonCommandeByIdAsync(id, CabinetId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("{id}/confirmer")]
    [Authorize(Policy = "commercial.edit")]
    public async Task<IActionResult> Confirmer(int id)
    {
        var result = await _service.ConfirmerBonCommandeAsync(id, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/approuver")]
    [Authorize(Policy = "commercial.validate")]
    public async Task<IActionResult> Approuver(int id)
    {
        var result = await _service.ApprouverBonCommandeAsync(id, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("convertir")]
    [Authorize(Policy = "commercial.convert")]
    public async Task<IActionResult> ConvertirEnContrat([FromBody] ConvertBonCommandeEnContratDto dto)
    {
        var result = await _service.ConvertirBonCommandeEnContratAsync(dto, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/dupliquer")]
    [Authorize(Policy = "commercial.create")]
    public async Task<IActionResult> Dupliquer(int id)
    {
        var result = await _service.DupliquerBonCommandeAsync(id, CabinetId, UserId);
        return result.Success ? CreatedAtAction(nameof(Get), new { id = result.Data?.Id }, result) : BadRequest(result);
    }
}

// ===== Contrats Controller =====
[ApiController]
[Route("api/contrats")]
[Authorize(Policy = "BackOfficeAccess")]
public class ContratsController : ControllerBase
{
    private readonly ICommercialService _service;
    public ContratsController(ICommercialService service) => _service = service;

    private int CabinetId => int.TryParse(User.FindFirst("CabinetId")?.Value, out var id) ? id : 0;
    private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? clientId, [FromQuery] string? statut, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _service.GetContratsAsync(CabinetId, clientId, statut, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await _service.GetContratByIdAsync(id, CabinetId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [Authorize(Policy = "commercial.create")]
    public async Task<IActionResult> Create([FromBody] ContratCommercialCreateDto dto)
    {
        var result = await _service.CreateContratAsync(dto, CabinetId, UserId);
        return result.Success ? CreatedAtAction(nameof(Get), new { id = result.Data?.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "commercial.edit")]
    public async Task<IActionResult> Update(int id, [FromBody] ContratCommercialCreateDto dto)
    {
        var result = await _service.UpdateContratAsync(id, dto, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}/equipe")]
    [Authorize(Policy = "commercial.edit")]
    public async Task<IActionResult> UpdateEquipe(int id, [FromBody] ContratEquipeUpdateDto dto)
    {
        var result = await _service.UpdateContratEquipeAsync(id, dto, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/signer")]
    [Authorize(Policy = "commercial.sign")]
    public async Task<IActionResult> Signer(int id, [FromBody] ContratSignatureDto dto)
    {
        var result = await _service.SignerContratAsync(id, dto, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/valider")]
    [Authorize(Policy = "commercial.validate")]
    public async Task<IActionResult> Valider(int id)
    {
        var result = await _service.ValiderContratAsync(id, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/suspendre")]
    [Authorize(Policy = "commercial.edit")]
    public async Task<IActionResult> Suspendre(int id)
    {
        var result = await _service.SuspendreContratAsync(id, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/reactiver")]
    [Authorize(Policy = "commercial.validate")]
    public async Task<IActionResult> Reactiver(int id)
    {
        var result = await _service.ReactiverContratAsync(id, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/terminer")]
    [Authorize(Policy = "commercial.edit")]
    public async Task<IActionResult> Terminer(int id)
    {
        var result = await _service.TerminerContratAsync(id, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "commercial.delete")]
    public async Task<IActionResult> DeleteContrat(int id)
    {
        var result = await _service.DeleteContratAsync(id, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/sync-services")]
    [Authorize(Policy = "commercial.validate")]
    public async Task<IActionResult> SyncServices(int id)
    {
        var result = await _service.SyncServicesFromContratAsync(id, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/dupliquer")]
    [Authorize(Policy = "commercial.create")]
    public async Task<IActionResult> Dupliquer(int id)
    {
        var result = await _service.DupliquerContratAsync(id, CabinetId, UserId);
        return result.Success ? CreatedAtAction(nameof(Get), new { id = result.Data?.Id }, result) : BadRequest(result);
    }

    [HttpPost("{id}/rectificatif")]
    [Authorize(Policy = "commercial.create")]
    public async Task<IActionResult> CreateRectificatif(int id, [FromBody] ContratRectificatifRequestDto? dto = null)
    {
        var result = await _service.CreateRectificatifContratAsync(id, CabinetId, UserId, dto?.Motif);
        return result.Success ? CreatedAtAction(nameof(Get), new { id = result.Data?.Id }, result) : BadRequest(result);
    }
}

// ===== Client Contrats (Extranet read-only synthesis) =====
[ApiController]
[Route("api/client-contrats")]
[Authorize(Policy = "ExtranetAccess")]
public class ClientContratsController : ControllerBase
{
    private readonly ICommercialService _service;
    private readonly IDeclarationService _declarations;
    public ClientContratsController(ICommercialService service, IDeclarationService declarations)
    {
        _service = service;
        _declarations = declarations;
    }

    private int CabinetId => int.TryParse(User.FindFirst("CabinetId")?.Value, out var id) ? id : 0;
    private int ClientId => int.TryParse(User.FindFirst("ClientId")?.Value, out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (ClientId == 0) return Forbid();
        var result = await _service.GetContratsAsync(CabinetId, ClientId, null, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        if (ClientId == 0) return Forbid();
        var result = await _service.GetContratByIdAsync(id, CabinetId);
        if (!result.Success || result.Data == null || result.Data.ClientId != ClientId)
            return NotFound(ApiResponse<ContratCommercialDto>.Fail("Contrat introuvable."));
        return Ok(result);
    }

    [HttpGet("{id}/declarations")]
    public async Task<IActionResult> GetDeclarations(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        if (ClientId == 0) return Forbid();
        var contrat = await _service.GetContratByIdAsync(id, CabinetId);
        if (!contrat.Success || contrat.Data == null || contrat.Data.ClientId != ClientId)
            return NotFound(ApiResponse<ContratCommercialDto>.Fail("Contrat introuvable."));
        return Ok(await _declarations.GetAllAsync(CabinetId, ClientId, null, null, page, pageSize));
    }
}

// ===== Factures Controller =====
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "BackOfficeAccess")]
public class FacturesController : ControllerBase
{
    private readonly ICommercialService _service;
    private readonly IFacturationSchedulerService _facturationScheduler;

    public FacturesController(ICommercialService service, IFacturationSchedulerService facturationScheduler)
    {
        _service = service;
        _facturationScheduler = facturationScheduler;
    }

    private int CabinetId => int.TryParse(User.FindFirst("CabinetId")?.Value, out var id) ? id : 0;
    private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? clientId, [FromQuery] string? statut, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _service.GetFacturesAsync(CabinetId, clientId, statut, page, pageSize);
        return Ok(result);
    }

    /// <summary>Manually trigger auto honoraires invoice generation (same as daily Hangfire job).</summary>
    [HttpPost("generate-due")]
    [Authorize(Policy = "facture.create")]
    public async Task<IActionResult> GenerateDue()
    {
        var result = await _facturationScheduler.GenerateDueInvoicesAsync();
        return Ok(ApiResponse<AutoFacturationResultDto>.Ok(result,
            $"{result.Created} facture(s) générée(s), {result.Skipped} ignorée(s)."));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await _service.GetFactureByIdAsync(id, CabinetId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [Authorize(Policy = "facture.create")]
    public async Task<IActionResult> Create([FromBody] FactureCreateDto dto)
    {
        var result = await _service.CreateFactureAsync(dto, CabinetId, UserId);
        return result.Success ? CreatedAtAction(nameof(Get), new { id = result.Data?.Id }, result) : BadRequest(result);
    }

    [HttpPost("{id}/envoyer")]
    [Authorize(Policy = "facture.send")]
    public async Task<IActionResult> Envoyer(int id)
    {
        var result = await _service.EnvoyerFactureAsync(id, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/annuler")]
    [Authorize(Policy = "facture.cancel")]
    public async Task<IActionResult> Annuler(int id)
    {
        var result = await _service.AnnulerFactureAsync(id, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/dupliquer")]
    [Authorize(Policy = "facture.create")]
    public async Task<IActionResult> Dupliquer(int id)
    {
        var result = await _service.DupliquerFactureAsync(id, CabinetId, UserId);
        return result.Success ? CreatedAtAction(nameof(Get), new { id = result.Data?.Id }, result) : BadRequest(result);
    }

    [HttpGet("{id}/paiements")]
    public async Task<IActionResult> GetPaiements(int id)
    {
        var result = await _service.GetPaiementsFactureAsync(id, CabinetId);
        return Ok(result);
    }

    [HttpPost("{id}/paiements")]
    [Authorize(Policy = "facture.managepayments")]
    public async Task<IActionResult> EnregistrerPaiement(int id, [FromBody] PaiementCreateDto dto)
    {
        dto.FactureId = id;
        var result = await _service.EnregistrerPaiementAsync(dto, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

// ===== Dashboard Commercial Controller =====
[ApiController]
[Route("api/commercial/dashboard")]
[Authorize(Policy = "BackOfficeAccess")]
public class CommercialDashboardController : ControllerBase
{
    private readonly ICommercialService _service;
    public CommercialDashboardController(ICommercialService service) => _service = service;

    private int CabinetId => int.TryParse(User.FindFirst("CabinetId")?.Value, out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await _service.GetDashboardAsync(CabinetId);
        return Ok(result);
    }
}

// Ã¢â€â‚¬Ã¢â€â‚¬ Request models for Commercial Controllers Ã¢â€â‚¬Ã¢â€â‚¬
public class RejetRequest
{
    public string Motif { get; set; } = string.Empty;
}


// ═══════════════════════════════════════════════════════════════════════════
// COMPTABILITÉ (Accounting) — Persistent Moroccan PCGM ledger
// ═══════════════════════════════════════════════════════════════════════════
[ApiController]
[Route("api/comptabilite")]
[Authorize(Policy = "BackOfficeAccess")]
public class ComptabiliteApiController : ControllerBase
{
    private readonly IComptabiliteService _service;
    public ComptabiliteApiController(IComptabiliteService service) => _service = service;

    private int CabinetId => int.TryParse(User.FindFirst("CabinetId")?.Value, out var id) ? id : 0;
    private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

    [HttpPost("ensure/{clientId:int}")]
    public async Task<IActionResult> EnsureLedger(int clientId)
    {
        await _service.EnsureClientLedgerAsync(clientId, CabinetId, UserId);
        return Ok(ApiResponse<bool>.Ok(true, "Plan comptable et journaux prêts."));
    }

    [HttpGet("exercices")]
    public async Task<IActionResult> GetExercices([FromQuery] int clientId)
        => Ok(await _service.GetExercicesAsync(clientId, CabinetId));

    [HttpGet("exercices/{id:int}")]
    public async Task<IActionResult> GetExercice(int id)
        => Ok(await _service.GetExerciceByIdAsync(id, CabinetId));

    [HttpPost("exercices")]
    [Authorize(Policy = ClaimDefinitions.ComptabiliteCreate)]
    public async Task<IActionResult> CreateExercice([FromBody] ExerciceCreateDto dto)
        => Ok(await _service.CreateExerciceAsync(dto, CabinetId, UserId));

    [HttpGet("exercices/{id:int}/precheck-cloture")]
    [Authorize(Policy = ClaimDefinitions.ComptabiliteCloture)]
    public async Task<IActionResult> PrecheckCloture(int id, [FromServices] IExerciceClotureService cloture)
        => Ok(await cloture.PrecheckAsync(id, CabinetId));

    [HttpPost("exercices/cloturer")]
    [Authorize(Policy = ClaimDefinitions.ComptabiliteCloture)]
    public async Task<IActionResult> CloturerExercice([FromBody] ExerciceClotureRequestDto dto, [FromServices] IExerciceClotureService cloture)
        => Ok(await cloture.CloturerAsync(dto, CabinetId, UserId));

    [HttpPost("exercices/{id:int}/reouvrir")]
    [Authorize(Policy = ClaimDefinitions.ComptabiliteCloture)]
    public async Task<IActionResult> ReouvrirExercice(int id, [FromBody] ExerciceReouvrirDto dto, [FromServices] IExerciceClotureService cloture)
        => Ok(await cloture.ReouvrirAsync(id, CabinetId, UserId, dto.Reason ?? ""));

    [HttpGet("exercices/has-open")]
    public async Task<IActionResult> HasOpenExercice([FromQuery] int clientId, [FromServices] IExerciceContextService exoCtx)
        => Ok(ApiResponse<bool>.Ok(await exoCtx.HasOpenExerciceAsync(clientId, CabinetId)));

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard([FromQuery] int clientId, [FromQuery] int? exerciceId = null)
        => Ok(await _service.GetDashboardAsync(clientId, CabinetId, exerciceId));

    [HttpGet("comptes")]
    public async Task<IActionResult> GetComptes([FromQuery] int clientId, [FromQuery] string? search = null)
        => Ok(await _service.GetComptesAsync(clientId, CabinetId, search));

    [HttpPost("comptes")]
    [Authorize(Policy = ClaimDefinitions.ComptabiliteCreate)]
    public async Task<IActionResult> CreateCompte([FromBody] CompteComptableCreateDto dto)
        => Ok(await _service.CreateCompteAsync(dto, CabinetId, UserId));

    [HttpPut("comptes/{id:int}")]
    [Authorize(Policy = ClaimDefinitions.ComptabiliteEdit)]
    public async Task<IActionResult> UpdateCompte(int id, [FromBody] CompteComptableUpdateDto dto)
        => Ok(await _service.UpdateCompteAsync(id, dto, CabinetId, UserId));

    [HttpPost("comptes/import")]
    [Authorize(Policy = ClaimDefinitions.ComptabiliteCreate)]
    public async Task<IActionResult> ImportComptes([FromBody] CompteImportRequestDto dto)
        => Ok(await _service.ImportComptesAsync(dto.ClientId, dto.CsvContent, CabinetId, UserId));

    [HttpPost("journaux/import")]
    [Authorize(Policy = ClaimDefinitions.ComptabiliteEdit)]
    public async Task<IActionResult> ImportJournaux([FromBody] CsvImportRequestDto dto)
        => Ok(await _service.ImportJournauxAsync(dto.ClientId, dto.CsvContent, CabinetId, UserId));

    [HttpPost("ecritures/import")]
    [Authorize(Policy = ClaimDefinitions.ComptabiliteCreate)]
    public async Task<IActionResult> ImportEcritures([FromBody] CsvImportRequestDto dto)
        => Ok(await _service.ImportEcrituresAsync(dto.ClientId, dto.CsvContent, CabinetId, UserId, dto.ExerciceComptableId, dto.Valider));

    [HttpPost("rapprochements/import")]
    [Authorize(Policy = ClaimDefinitions.ComptabiliteCreate)]
    public async Task<IActionResult> ImportRapprochements([FromBody] CsvImportRequestDto dto)
        => Ok(await _service.ImportRapprochementsAsync(dto.ClientId, dto.CsvContent, CabinetId, UserId));

    [HttpPost("soldes-ouverture/import")]
    [Authorize(Policy = ClaimDefinitions.ComptabiliteCreate)]
    public async Task<IActionResult> ImportSoldesOuverture([FromBody] CsvImportRequestDto dto)
        => Ok(await _service.ImportSoldesOuvertureAsync(dto.ClientId, dto.CsvContent, CabinetId, UserId, dto.ExerciceComptableId));

    [HttpGet("ecritures")]
    public async Task<IActionResult> GetEcritures([FromQuery] int? clientId, [FromQuery] string? journal, [FromQuery] int page = 1, [FromQuery] string? search = null, [FromQuery] string? statut = null, [FromQuery] int? exerciceId = null)
        => Ok(await _service.GetEcrituresAsync(CabinetId, clientId, journal, page, search, statut, exerciceId));

    [HttpGet("ecritures/{id:int}")]
    public async Task<IActionResult> GetEcriture(int id)
        => Ok(await _service.GetEcritureByIdAsync(id, CabinetId));

    [HttpPost("ecritures")]
    [Authorize(Policy = ClaimDefinitions.ComptabiliteCreate)]
    public async Task<IActionResult> CreateEcriture([FromBody] EcritureCreateDto dto)
        => Ok(await _service.CreateEcritureAsync(dto, CabinetId, UserId));

    [HttpPut("ecritures/{id:int}")]
    [Authorize(Policy = ClaimDefinitions.ComptabiliteEdit)]
    public async Task<IActionResult> UpdateEcriture(int id, [FromBody] EcritureCreateDto dto)
        => Ok(await _service.UpdateEcritureAsync(id, dto, CabinetId, UserId));

    [HttpDelete("ecritures/{id:int}")]
    [Authorize(Policy = ClaimDefinitions.ComptabiliteEdit)]
    public async Task<IActionResult> DeleteEcriture(int id)
        => Ok(await _service.DeleteEcritureAsync(id, CabinetId, UserId));

    [HttpPost("ecritures/{id:int}/validate")]
    [Authorize(Policy = ClaimDefinitions.ComptabiliteCreate)]
    public async Task<IActionResult> ValidateEcriture(int id)
        => Ok(await _service.ValidateEcritureAsync(id, CabinetId, UserId));

    [HttpGet("journaux")]
    public async Task<IActionResult> GetJournaux([FromQuery] int clientId)
        => Ok(await _service.GetJournauxAsync(clientId, CabinetId));

    [HttpPut("journaux/{id:int}")]
    [Authorize(Policy = ClaimDefinitions.ComptabiliteEdit)]
    public async Task<IActionResult> UpdateJournal(int id, [FromBody] JournalUpdateDto dto)
        => Ok(await _service.UpdateJournalAsync(id, dto, CabinetId, UserId));

    [HttpGet("grandlivre")]
    public async Task<IActionResult> GetGrandLivre([FromQuery] int clientId, [FromQuery] DateTime? dateDebut, [FromQuery] DateTime? dateFin, [FromQuery] string? numCompte = null, [FromQuery] int? exerciceId = null)
        => Ok(await _service.GetGrandLivreAsync(clientId, CabinetId, dateDebut, dateFin, numCompte, exerciceId));

    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance([FromQuery] int clientId, [FromQuery] DateTime? dateDebut, [FromQuery] DateTime? dateFin, [FromQuery] int? exerciceId = null)
        => Ok(await _service.GetBalanceAsync(clientId, CabinetId, dateDebut, dateFin, exerciceId));

    [HttpGet("balance-agee")]
    public async Task<IActionResult> GetBalanceAgee([FromQuery] int clientId)
        => Ok(await _service.GetBalanceAgeeAsync(clientId, CabinetId));

    [HttpGet("rapprochements")]
    public async Task<IActionResult> GetRapprochements([FromQuery] int? clientId)
        => Ok(await _service.GetRapprochementsAsync(CabinetId, clientId));

    [HttpPost("rapprochements")]
    [Authorize(Policy = ClaimDefinitions.ComptabiliteCreate)]
    public async Task<IActionResult> CreateRapprochement([FromBody] RapprochementCreateDto dto)
        => Ok(await _service.CreateRapprochementAsync(dto, CabinetId, UserId));

    [HttpPut("rapprochements/{id:int}")]
    [Authorize(Policy = ClaimDefinitions.ComptabiliteCreate)]
    public async Task<IActionResult> UpdateRapprochement(int id, [FromBody] RapprochementUpdateDto dto)
        => Ok(await _service.UpdateRapprochementAsync(id, dto, CabinetId, UserId));
}

// ===== Tasks Controller =====
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "BackOfficeAccess")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    public TasksController(ITaskService taskService) => _taskService = taskService;

    private int CabinetId => int.TryParse(User.FindFirst("CabinetId")?.Value, out var id) ? id : 0;
    private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? assignedTo, [FromQuery] string? statut, [FromQuery] int? clientId = null, [FromQuery] string? assignedToKind = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (assignedTo == "me") assignedTo = UserId;
        return Ok(await _taskService.GetAllAsync(CabinetId, assignedTo, statut, clientId, assignedToKind, page, pageSize));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _taskService.GetByIdAsync(id, CabinetId));

    [HttpPost]
    [Authorize(Policy = ClaimDefinitions.TaskCreate)]
    public async Task<IActionResult> Create([FromBody] TaskItemDto dto)
    {
        var result = await _taskService.CreateAsync(dto, CabinetId, UserId);
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = ClaimDefinitions.TaskEdit)]
    public async Task<IActionResult> Update(int id, [FromBody] TaskItemDto dto)
    {
        var result = await _taskService.UpdateAsync(id, dto, CabinetId, UserId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPut("{id}/status")]
    [Authorize(Policy = ClaimDefinitions.TaskEdit)]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] TaskStatusRequest request)
    {
        var result = await _taskService.ChangeStatusAsync(id, request.Statut, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/comments")]
    [Authorize(Policy = ClaimDefinitions.TaskDetail)]
    public async Task<IActionResult> AddComment(int id, [FromBody] AddTaskCommentDto dto)
    {
        var userName = User.FindFirst("FullName")?.Value
            ?? User.FindFirst(ClaimTypes.Name)?.Value
            ?? "";
        var result = await _taskService.AddCommentAsync(id, dto.Contenu, CabinetId, UserId, userName);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = ClaimDefinitions.TaskEdit)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _taskService.ChangeStatusAsync(id, "Annule", CabinetId, UserId);
        return result.Success ? Ok(result) : NotFound(result);
    }
}

public class TaskStatusRequest
{
    public string Statut { get; set; } = string.Empty;
}

// ============================================================================
// DOCUMENTS CONTROLLER
// ============================================================================
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    public DocumentsController(IDocumentService documentService) => _documentService = documentService;

    private int CabinetId => int.TryParse(User.FindFirst("CabinetId")?.Value, out var id) ? id : 0;
    private int ClientId => int.TryParse(User.FindFirst("ClientId")?.Value, out var id) ? id : 0;
    private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
    private bool IsClientUser => ClientId > 0;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? clientId, [FromQuery] string? categorie,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
    {
        // Security: Extranet client users can only see their own shared (+ self-uploaded) documents
        if (IsClientUser)
        {
            clientId = ClientId;
            return Ok(await _documentService.GetAllAsync(CabinetId, clientId, categorie, page, pageSize, search,
                extranetVisibleOnly: true, extranetUserId: UserId));
        }
        return Ok(await _documentService.GetAllAsync(CabinetId, clientId, categorie, page, pageSize, search));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _documentService.GetByIdAsync(id, CabinetId);
        if (IsClientUser && result.Success && result.Data != null && result.Data.ClientId != ClientId)
            return NotFound(ApiResponse<DocumentDto>.Fail("Document introuvable."));
        return Ok(result);
    }

    [HttpGet("declaration/{declarationId}")]
    public async Task<IActionResult> GetByDeclaration(int declarationId)
        => Ok(await _documentService.GetByDeclarationIdAsync(declarationId, CabinetId));

    /// <summary>
    /// Upload document. Staff need document.upload; Extranet Client role may upload for own clientId.
    /// </summary>
    [HttpPost("upload")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> Upload(IFormFile file, [FromForm] string titre, [FromForm] string categorie,
        [FromForm] int? clientId, [FromForm] int? declarationId, [FromForm] int? checklistItemId = null)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<DocumentDto>.Fail("Aucun fichier fourni."));

        if (!IsClientUser)
        {
            var canUpload = User.HasClaim(c =>
                c.Type == "Permission" && c.Value == ClaimDefinitions.DocumentUpload)
                || User.IsInRole("SuperAdmin") || User.IsInRole("Manager");
            if (!canUpload)
                return Forbid();
        }
        else
        {
            clientId = ClientId;
        }

        using var stream = file.OpenReadStream();
        var result = await _documentService.UploadAsync(
            stream, file.FileName, file.ContentType, file.Length,
            titre, categorie, clientId, CabinetId, UserId, declarationId, checklistItemId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}/status")]
    [Authorize(Policy = ClaimDefinitions.DocumentDetail)]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] DocumentStatusRequest request)
    {
        if (IsClientUser) return Forbid();
        var result = await _documentService.ChangeStatusAsync(id, request.NewStatut, CabinetId, UserId, request.Motif);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}/share")]
    [Authorize(Policy = ClaimDefinitions.DocumentShare)]
    public async Task<IActionResult> ToggleShare(int id)
    {
        if (IsClientUser) return Forbid();
        var result = await _documentService.ToggleShareAsync(id, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> Download(int id)
    {
        var docInfo = await _documentService.GetByIdAsync(id, CabinetId);
        if (!docInfo.Success || docInfo.Data == null) return NotFound();
        // Security: Extranet client users can only download documents belonging to their client
        if (IsClientUser)
        {
            var docClientId = docInfo.Data.ClientId;
            // If document has no direct ClientId but belongs to a declaration, check via declaration
            if (docClientId == null && docInfo.Data.DeclarationId.HasValue)
            {
                var decl = await _documentService.GetDeclarationClientIdAsync(docInfo.Data.DeclarationId.Value, CabinetId);
                docClientId = decl;
            }
            if (docClientId != ClientId) return NotFound();
        }

        var result = await _documentService.DownloadAsync(id, CabinetId);
        if (!result.Success || result.Data == null) return NotFound();

        return File(result.Data, docInfo.Data.ContentType ?? "application/octet-stream", docInfo.Data.NomFichier);
    }
}

public class DocumentStatusRequest
{
    public string NewStatut { get; set; } = string.Empty;
    public string? Motif { get; set; }
}

// ===== États Fiscaux Controller (Bilan, Liasse, CPC, Amortissements, États de Synthèse) =====
[ApiController]
[Route("api/etats-fiscaux")]
[Authorize(Policy = "BackOfficeAccess")]
public class EtatsFiscauxApiController : ControllerBase
{
    private static readonly HashSet<string> FiscalTypes = new(StringComparer.OrdinalIgnoreCase)
        { "BILAN", "LIASSEFISCALE", "ETATSSYNTHESE", "CPC", "AMORTISSEMENTS", "ETAT9421" };

    private readonly IDeclarationService _service;
    public EtatsFiscauxApiController(IDeclarationService service) => _service = service;

    private int CabinetId => int.TryParse(User.FindFirst("CabinetId")?.Value, out var id) ? id : 0;
    private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

    private static EtatFiscalDto Map(DeclarationDto d) => new()
    {
        Id = d.Id,
        ClientId = d.ClientId,
        ClientName = d.ClientName,
        Type = d.Type,
        Periode = d.Periode,
        Exercice = int.TryParse(d.Periode, out var y) ? y : d.DateFin.Year,
        Statut = d.Statut,
        DateGeneration = d.DateDebut == default ? null : d.DateDebut,
        DateValidation = d.DateSoumission,
        PdfUrl = d.PdfUrl,
        MontantTotal = d.MontantDu,
    };

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? clientId,
        [FromQuery] string? type,
        [FromQuery] int? exercice,
        [FromQuery] int page = 1,
        [FromQuery] string? search = null)
    {
        string? typeFilter = !string.IsNullOrWhiteSpace(type) && FiscalTypes.Contains(type) ? type : null;
        var result = await _service.GetAllAsync(CabinetId, clientId, typeFilter, null, page, 100);
        if (!result.Success) return BadRequest(result);

        var items = result.Data!.Items
            .Where(d => FiscalTypes.Contains(d.Type))
            .Where(d => !exercice.HasValue ||
                (int.TryParse(d.Periode, out var y) && y == exercice.Value) ||
                d.DateFin.Year == exercice.Value)
            .Where(d => string.IsNullOrWhiteSpace(search) ||
                (d.ClientName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false))
            .Select(Map)
            .ToList();

        var paged = new PagedResult<EtatFiscalDto>
        {
            Items = items,
            TotalCount = items.Count,
            Page = page,
            PageSize = 100
        };
        return Ok(ApiResponse<PagedResult<EtatFiscalDto>>.Ok(paged));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await _service.GetByIdAsync(id, CabinetId);
        if (!result.Success || result.Data == null) return NotFound(result);
        if (!FiscalTypes.Contains(result.Data.Type)) return NotFound();
        return Ok(ApiResponse<EtatFiscalDto>.Ok(Map(result.Data)));
    }

    [HttpPost("generate")]
    [Authorize(Policy = "declaration.create")]
    public async Task<IActionResult> Generate([FromBody] GenerateEtatFiscalRequest req)
    {
        var type = req.Type?.ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(type) || !FiscalTypes.Contains(type))
            return BadRequest(ApiResponse<EtatFiscalDto>.Fail("Type d'état fiscal invalide."));

        var dto = new CreateDeclarationDto
        {
            ClientId = req.ClientId,
            Type = type,
            AnneeFiscale = req.Exercice
        };
        var result = await _service.CreateAsync(dto, CabinetId, UserId);
        if (!result.Success)
            return BadRequest(ApiResponse<EtatFiscalDto>.Fail(result.Message ?? "Échec de la génération."));
        return Ok(ApiResponse<EtatFiscalDto>.Ok(Map(result.Data!)));
    }

    [HttpPut("{id:int}/validate")]
    [Authorize(Policy = "declaration.edit")]
    public async Task<IActionResult> Validate(int id)
    {
        var result = await _service.ChangeStatusAsync(id, RefCodes.StatutDeclaration.Validee, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

public class GenerateEtatFiscalRequest
{
    public int ClientId { get; set; }
    public string Type { get; set; } = string.Empty;
    public int Exercice { get; set; }
}
