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
    public async Task<IActionResult> GetContractAccess(int id)
    {
        var ctx = await _clientService.GetContractAccessContextAsync(id, CabinetId);
        return Ok(ApiResponse<ContractAccessContextDto>.Ok(ctx));
    }

    /// <summary>Get all service contracts for a client, optionally by exercise year</summary>
    [HttpGet("{id}/service-contrats")]
    public async Task<IActionResult> GetServiceContrats(int id, [FromQuery] int? exerciceFiscal = null)
    {
        var result = await _clientService.GetServiceContratsAsync(id, CabinetId, exerciceFiscal);
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
    public async Task<IActionResult> GetAll([FromQuery] int? clientId, [FromQuery] string? type, [FromQuery] string? statut, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _service.GetAllAsync(CabinetId, clientId, type, statut, page, pageSize);
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

    // Ã¢â€â‚¬Ã¢â€â‚¬ IR Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬
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
    public async Task<IActionResult> GetAll([FromQuery] int? clientId = null)
    {
        var result = await _service.GetAllPeriodsAsync(CabinetId, clientId);
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
    public async Task<IActionResult> Get()
    {
        if (ClientId == 0) return Forbid();
        return Ok(await _service.GetExtranetDashboardAsync(ClientId));
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
    public FacturesController(ICommercialService service) => _service = service;

    private int CabinetId => int.TryParse(User.FindFirst("CabinetId")?.Value, out var id) ? id : 0;
    private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? clientId, [FromQuery] string? statut, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _service.GetFacturesAsync(CabinetId, clientId, statut, page, pageSize);
        return Ok(result);
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


// Ã¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢Â
// Ã¢â€¢â€˜         COMPTABILITE (Accounting) Ã¢â‚¬â€ Demo data Ã¢â‚¬â€ Moroccan PCM          Ã¢â€¢â€˜
// Ã¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢Â
[ApiController]
[Route("api/comptabilite")]
[Authorize(Policy = "BackOfficeAccess")]
public class ComptabiliteApiController : ControllerBase
{
    private readonly MetaComptaDbContext _db;
    public ComptabiliteApiController(MetaComptaDbContext db) => _db = db;

    private int CabinetId => int.TryParse(User.FindFirst("CabinetId")?.Value, out var id) ? id : 0;

    private async Task<string> GetClientName(int? clientId)
    {
        if (!clientId.HasValue || clientId.Value <= 0) return "Client";
        var client = await _db.Clients.FindAsync(clientId.Value);
        return client?.RaisonSociale ?? "Client #" + clientId.Value;
    }

    // Ã¢â€â‚¬Ã¢â€â‚¬ Helper: Moroccan PCM chart of accounts (demo) Ã¢â€â‚¬Ã¢â€â‚¬
    private static readonly (string Num, string Lib, string Cls)[] _pcm = new[]
    {
        ("1111","Capital social","1"), ("1140","RÃƒÂ©serve lÃƒÂ©gale","1"), ("1181","RÃƒÂ©sultat net de l''exercice","1"),
        ("1481","Emprunts auprÃƒÂ¨s des ÃƒÂ©ts de crÃƒÂ©dit","1"), ("1486","Fournisseurs d''immobilisations","1"),
        ("2110","Frais de constitution","2"), ("2230","Fonds commercial","2"),
        ("2311","Terrains nus","2"), ("2321","BÃƒÂ¢timents","2"), ("2332","MatÃƒÂ©riel et outillage","2"),
        ("2340","MatÃƒÂ©riel de transport","2"), ("2351","Mobilier de bureau","2"), ("2355","MatÃƒÂ©riel informatique","2"),
        ("2810","Amort. frais prÃƒÂ©liminaires","2"), ("2832","Amort. matÃƒÂ©riel et outillage","2"),
        ("2834","Amort. matÃƒÂ©riel de transport","2"), ("2835","Amort. MMB & amÃƒÂ©nagements","2"),
        ("3111","Marchandises","3"), ("3121","MatiÃƒÂ¨res premiÃƒÂ¨res","3"), ("3151","Produits finis","3"),
        ("3421","Clients","3"), ("3424","Clients douteux ou litigieux","3"),
        ("3425","Clients - effets ÃƒÂ  recevoir","3"), ("3451","Subventions ÃƒÂ  recevoir","3"),
        ("3455","Ãƒâ€°tat - TVA rÃƒÂ©cupÃƒÂ©rable","3"), ("3458","Ãƒâ€°tat - crÃƒÂ©dit de TVA","3"),
        ("4411","Fournisseurs","4"), ("4415","Fournisseurs - effets ÃƒÂ  payer","4"),
        ("4432","RÃƒÂ©munÃƒÂ©rations dues au personnel","4"), ("4441","CNSS","4"), ("4443","Caisses de retraite","4"),
        ("4452","Ãƒâ€°tat - TVA facturÃƒÂ©e","4"), ("4453","Ãƒâ€°tat - TVA due","4"),
        ("4455","Ãƒâ€°tat - IS","4"), ("4456","Ãƒâ€°tat - Taxes sur CA","4"),
        ("5141","Banque (BMCE)","5"), ("5143","TrÃƒÂ©sorerie GÃƒÂ©nÃƒÂ©rale","5"), ("5161","Caisse centrale","5"),
        ("5520","CrÃƒÂ©dits d''escompte","5"),
        ("6111","Achats de marchandises","6"), ("6121","Achats de mat. premiÃƒÂ¨res","6"),
        ("6125","Achats non stockÃƒÂ©s","6"), ("6131","Locations et charges locatives","6"),
        ("6133","Entretien et rÃƒÂ©parations","6"), ("6134","Primes d''assurances","6"),
        ("6141","Transport du personnel","6"), ("6142","DÃƒÂ©placements et missions","6"),
        ("6143","Frais postaux","6"), ("6144","PublicitÃƒÂ©","6"),
        ("6147","Services bancaires","6"), ("6161","ImpÃƒÂ´ts et taxes","6"),
        ("6171","RÃƒÂ©munÃƒÂ©rations du personnel","6"), ("6174","Charges sociales","6"),
        ("6176","Charges sociales diverses","6"), ("6182","Pertes sur crÃƒÂ©ances irrÃƒÂ©couvrables","6"),
        ("6191","DEA des immob. en non-valeur","6"), ("6193","DEA des immob. corporelles","6"),
        ("6311","IntÃƒÂ©rÃƒÂªts des emprunts","6"), ("6386","Escomptes accordÃƒÂ©s","6"),
        ("6585","CrÃƒÂ©ances devenues irrÃƒÂ©couvrables","6"),
        ("7111","Ventes de marchandises","7"), ("7121","Ventes de biens produits","7"),
        ("7124","Ventes de services","7"), ("7127","Ventes et produits accessoires","7"),
        ("7129","RRR accordÃƒÂ©s par l''entreprise","7"), ("7141","Subventions d''exploitation","7"),
        ("7197","Transferts de charges d''exploitation","7"),
        ("7381","IntÃƒÂ©rÃƒÂªts et produits assimilÃƒÂ©s","7"), ("7386","Escomptes obtenus","7"),
        ("7585","RentrÃƒÂ©es sur crÃƒÂ©ances soldÃƒÂ©es","7"), ("7111","Ventes de marchandises","7")
    };

    // Ã¢â€â‚¬Ã¢â€â‚¬ Ãƒâ€°critures Ã¢â€â‚¬Ã¢â€â‚¬
    [HttpGet("ecritures")]
    public async Task<IActionResult> GetEcritures([FromQuery] int? clientId, [FromQuery] string? journal, [FromQuery] int page = 1, [FromQuery] string? search = null)
    {
        var clientName = await GetClientName(clientId);
        var rng = new Random(42 + (clientId ?? 1));
        var jcodes = new[] { "AC", "VE", "BQ", "CA", "OD" };
        var jnoms = new[] { "Achats", "Ventes", "Banque", "Caisse", "OpÃƒÂ©rations Diverses" };
        var all = new List<EcritureComptableDto>();
        for (int i = 1; i <= 120; i++)
        {
            var jIdx = rng.Next(jcodes.Length);
            var pcmD = _pcm[rng.Next(_pcm.Length)];
            var pcmC = _pcm[rng.Next(_pcm.Length)];
            var montant = Math.Round((decimal)(rng.NextDouble() * 50000 + 500), 2);
            all.Add(new EcritureComptableDto
            {
                Id = i,
                ClientId = clientId ?? 1,
                ClientNom = clientName,
                Journal = jcodes[jIdx],
                NumeroPiece = $"{jcodes[jIdx]}-2026-{i:D4}",
                DateEcriture = new DateTime(2026, rng.Next(1, 13), rng.Next(1, 29)),
                NumCompte = pcmD.Num,
                LibelleCompte = pcmD.Lib,
                Libelle = $"Ãƒâ€°criture {jnoms[jIdx]} #{i}",
                Debit = montant,
                Credit = 0,
                Reference = $"REF-{i:D4}",
                Statut = i % 5 == 0 ? "ValidÃƒÂ©" : "Brouillon",
                CreatedAt = DateTime.UtcNow.AddDays(-rng.Next(1, 180))
            });
            all.Add(new EcritureComptableDto
            {
                Id = 1000 + i,
                ClientId = clientId ?? 1,
                ClientNom = clientName,
                Journal = jcodes[jIdx],
                NumeroPiece = $"{jcodes[jIdx]}-2026-{i:D4}",
                DateEcriture = all.Last().DateEcriture,
                NumCompte = pcmC.Num,
                LibelleCompte = pcmC.Lib,
                Libelle = $"Ãƒâ€°criture {jnoms[jIdx]} #{i}",
                Debit = 0,
                Credit = montant,
                Reference = $"REF-{i:D4}",
                Statut = all.Last().Statut,
                CreatedAt = all.Last().CreatedAt
            });
        }
        if (!string.IsNullOrEmpty(journal)) all = all.Where(e => e.Journal == journal).ToList();
        if (!string.IsNullOrEmpty(search)) all = all.Where(e => e.Libelle!.Contains(search, StringComparison.OrdinalIgnoreCase) || e.NumCompte.Contains(search)).ToList();
        var paged = new PagedResult<EcritureComptableDto>
        {
            Items = all.OrderByDescending(e => e.DateEcriture).Skip((page - 1) * 20).Take(20).ToList(),
            TotalCount = all.Count,
            Page = page,
            PageSize = 20
        };
        return Ok(ApiResponse<PagedResult<EcritureComptableDto>>.Ok(paged));
    }

    // Ã¢â€â‚¬Ã¢â€â‚¬ Journaux Ã¢â€â‚¬Ã¢â€â‚¬
    [HttpGet("journaux")]
    public IActionResult GetJournaux([FromQuery] int? clientId)
    {
        var rng = new Random(100 + (clientId ?? 1));
        var journaux = new List<JournalDto>
        {
            new() { Code = "AC", Nom = "Journal des Achats", Type = "Achats", NombreEcritures = 45 + rng.Next(20), TotalDebit = Math.Round((decimal)(rng.NextDouble() * 500000 + 100000), 2), TotalCredit = Math.Round((decimal)(rng.NextDouble() * 500000 + 100000), 2) },
            new() { Code = "VE", Nom = "Journal des Ventes", Type = "Ventes", NombreEcritures = 62 + rng.Next(30), TotalDebit = Math.Round((decimal)(rng.NextDouble() * 800000 + 200000), 2), TotalCredit = Math.Round((decimal)(rng.NextDouble() * 800000 + 200000), 2) },
            new() { Code = "BQ", Nom = "Journal de Banque (BMCE)", Type = "Banque", NombreEcritures = 85 + rng.Next(40), TotalDebit = Math.Round((decimal)(rng.NextDouble() * 1200000 + 300000), 2), TotalCredit = Math.Round((decimal)(rng.NextDouble() * 1200000 + 300000), 2) },
            new() { Code = "CA", Nom = "Journal de Caisse", Type = "Caisse", NombreEcritures = 30 + rng.Next(15), TotalDebit = Math.Round((decimal)(rng.NextDouble() * 150000 + 30000), 2), TotalCredit = Math.Round((decimal)(rng.NextDouble() * 150000 + 30000), 2) },
            new() { Code = "OD", Nom = "OpÃƒÂ©rations Diverses", Type = "OD", NombreEcritures = 18 + rng.Next(10), TotalDebit = Math.Round((decimal)(rng.NextDouble() * 200000 + 50000), 2), TotalCredit = Math.Round((decimal)(rng.NextDouble() * 200000 + 50000), 2) },
            new() { Code = "AN", Nom = "Ãƒâ‚¬ Nouveau", Type = "AN", NombreEcritures = 12, TotalDebit = Math.Round((decimal)(rng.NextDouble() * 600000 + 100000), 2), TotalCredit = Math.Round((decimal)(rng.NextDouble() * 600000 + 100000), 2) },
            new() { Code = "SA", Nom = "Journal des Salaires", Type = "Paie", NombreEcritures = 24 + rng.Next(6), TotalDebit = Math.Round((decimal)(rng.NextDouble() * 350000 + 80000), 2), TotalCredit = Math.Round((decimal)(rng.NextDouble() * 350000 + 80000), 2) }
        };
        return Ok(ApiResponse<List<JournalDto>>.Ok(journaux));
    }

    // Ã¢â€â‚¬Ã¢â€â‚¬ Grand Livre Ã¢â€â‚¬Ã¢â€â‚¬
    [HttpGet("grandlivre")]
    public IActionResult GetGrandLivre([FromQuery] int clientId, [FromQuery] DateTime? dateDebut, [FromQuery] DateTime? dateFin)
    {
        var rng = new Random(200 + clientId);
        var gl = _pcm.DistinctBy(p => p.Num).Select(p =>
        {
            var d = Math.Round((decimal)(rng.NextDouble() * 200000), 2);
            var c = Math.Round((decimal)(rng.NextDouble() * 200000), 2);
            return new GrandLivreDto
            {
                NumeroCompte = p.Num,
                LibelleCompte = p.Lib,
                TotalDebit = d,
                TotalCredit = c,
                SoldeDebiteur = d > c ? d - c : 0,
                SoldeCrediteur = c > d ? c - d : 0
            };
        }).OrderBy(g => g.NumeroCompte).ToList();
        return Ok(ApiResponse<List<GrandLivreDto>>.Ok(gl));
    }

    // Ã¢â€â‚¬Ã¢â€â‚¬ Balance Ã¢â€â‚¬Ã¢â€â‚¬
    [HttpGet("balance")]
    public IActionResult GetBalance([FromQuery] int clientId, [FromQuery] DateTime? dateDebut, [FromQuery] DateTime? dateFin)
    {
        var rng = new Random(300 + clientId);
        var balance = _pcm.DistinctBy(p => p.Num).Select(p =>
        {
            var d = Math.Round((decimal)(rng.NextDouble() * 150000), 2);
            var c = Math.Round((decimal)(rng.NextDouble() * 150000), 2);
            return new BalanceDto
            {
                NumeroCompte = p.Num,
                LibelleCompte = p.Lib,
                TotalDebit = d,
                TotalCredit = c,
                SoldeDebiteur = d > c ? d - c : 0,
                SoldeCrediteur = c > d ? c - d : 0
            };
        }).OrderBy(b => b.NumeroCompte).ToList();
        return Ok(ApiResponse<List<BalanceDto>>.Ok(balance));
    }

    // Ã¢â€â‚¬Ã¢â€â‚¬ Balance Ãƒâ€šgÃƒÂ©e Ã¢â€â‚¬Ã¢â€â‚¬
    [HttpGet("balance-agee")]
    public IActionResult GetBalanceAgee([FromQuery] int clientId)
    {
        var rng = new Random(400 + clientId);
        var tiers = new[] {
            ("3421","Alpha Distribution SARL"), ("3421","Beta Import-Export SA"), ("3421","Gamma Services Conseil"),
            ("3424","Delta Fournitures"), ("3424","Epsilon NÃƒÂ©goce"), ("4411","Fournisseur Atlas"),
            ("4411","Fournisseur Marrakech MatÃƒÂ©riaux"), ("4411","Fournisseur Casa Parts"), ("4411","Fournisseur Rabat Industrie")
        };
        var aged = tiers.Select((t, i) =>
        {
            var total = Math.Round((decimal)(rng.NextDouble() * 80000 + 5000), 2);
            var t1 = Math.Round(total * (decimal)(0.3 + rng.NextDouble() * 0.3), 2);
            var t2 = Math.Round((total - t1) * (decimal)(0.3 + rng.NextDouble() * 0.2), 2);
            var t3 = Math.Round((total - t1 - t2) * (decimal)(0.4 + rng.NextDouble() * 0.3), 2);
            var t4 = total - t1 - t2 - t3;
            return new BalanceAgeeDto
            {
                NumeroCompte = t.Item1,
                NomTiers = t.Item2,
                Total = total,
                Tranche0_30 = t1,
                Tranche30_60 = t2,
                Tranche60_90 = t3,
                TrancheSuperieur90 = Math.Max(0, t4)
            };
        }).ToList();
        return Ok(ApiResponse<List<BalanceAgeeDto>>.Ok(aged));
    }

    // Ã¢â€â‚¬Ã¢â€â‚¬ Rapprochements Bancaires Ã¢â€â‚¬Ã¢â€â‚¬
    [HttpGet("rapprochements")]
    public async Task<IActionResult> GetRapprochements([FromQuery] int? clientId)
    {
        var clientName = await GetClientName(clientId);
        var rng = new Random(500 + (clientId ?? 1));
        var rapprochements = new List<RapprochementBancaireDto>();
        var periodes = new[] { "2026-01", "2026-02", "2026-03", "2026-04", "2026-05", "2026-06" };
        foreach (var p in periodes)
        {
            var soldeC = Math.Round((decimal)(rng.NextDouble() * 500000 + 50000), 2);
            var ecart = Math.Round((decimal)(rng.NextDouble() * 5000 - 2500), 2);
            rapprochements.Add(new RapprochementBancaireDto
            {
                Id = rapprochements.Count + 1,
                ClientId = clientId ?? 1,
                ClientNom = clientName,
                CompteBancaire = "5141 - BMCE",
                Periode = p,
                SoldeCompta = soldeC,
                SoldeBanque = soldeC + ecart,
                Ecart = ecart,
                EstRapproche = Math.Abs(ecart) < 100,
                Statut = Math.Abs(ecart) < 100 ? "RapprochÃƒÂ©" : "En Cours",
                NombreLignesNonRapprochees = Math.Abs(ecart) < 100 ? 0 : rng.Next(1, 8),
                DateRapprochement = DateTime.Parse(p + "-28")
            });
        }
        return Ok(ApiResponse<List<RapprochementBancaireDto>>.Ok(rapprochements));
    }

    // Ã¢â€â‚¬Ã¢â€â‚¬ Create Ãƒâ€°criture Ã¢â€â‚¬Ã¢â€â‚¬
    [HttpPost("ecritures")]
    [Authorize(Policy = ClaimDefinitions.ComptabiliteCreate)]
    public IActionResult CreateEcriture([FromBody] EcritureCreateDto dto)
    {
        var result = new EcritureComptableDto
        {
            Id = new Random().Next(2000, 9999),
            ClientId = dto.ClientId,
            Journal = dto.Journal,
            NumeroPiece = $"{dto.Journal}-2026-{new Random().Next(1000, 9999)}",
            DateEcriture = dto.DateEcriture,
            Libelle = dto.Libelle,
            NumCompte = dto.Lignes.FirstOrDefault()?.NumCompte ?? "",
            LibelleCompte = dto.Lignes.FirstOrDefault()?.LibelleCompte ?? "",
            Debit = dto.Lignes.Sum(l => l.Debit),
            Credit = dto.Lignes.Sum(l => l.Credit),
            Reference = dto.Reference,
            Statut = "Brouillon",
            CreatedAt = DateTime.UtcNow
        };
        return Ok(ApiResponse<EcritureComptableDto>.Ok(result, "Ãƒâ€°criture crÃƒÂ©ÃƒÂ©e avec succÃƒÂ¨s."));
    }

    // Ã¢â€â‚¬Ã¢â€â‚¬ Create Rapprochement Ã¢â€â‚¬Ã¢â€â‚¬
    [HttpPost("rapprochements")]
    [Authorize(Policy = ClaimDefinitions.ComptabiliteCreate)]
    public IActionResult CreateRapprochement([FromBody] RapprochementCreateRequest req)
    {
        var result = new RapprochementBancaireDto
        {
            Id = new Random().Next(100, 999),
            ClientId = req.ClientId,
            CompteBancaire = req.CompteBancaire,
            Periode = req.Periode,
            SoldeCompta = 0,
            SoldeBanque = 0,
            Ecart = 0,
            EstRapproche = false,
            Statut = "En Cours",
            DateRapprochement = DateTime.UtcNow
        };
        return Ok(ApiResponse<RapprochementBancaireDto>.Ok(result, "Rapprochement crÃƒÂ©ÃƒÂ© avec succÃƒÂ¨s."));
    }
}

public class RapprochementCreateRequest
{
    public int ClientId { get; set; }
    public string CompteBancaire { get; set; } = string.Empty;
    public string Periode { get; set; } = string.Empty;
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
        // Security: Extranet client users can only see their own documents
        if (IsClientUser) clientId = ClientId;
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
        [FromForm] int? clientId, [FromForm] int? declarationId)
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
            titre, categorie, clientId, CabinetId, UserId, declarationId);
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
