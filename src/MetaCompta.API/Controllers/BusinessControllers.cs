using MetaCompta.Infrastructure.Data;
using System.Security.Claims;
using MetaCompta.Application.DTOs;
using MetaCompta.Application.Interfaces;
using MetaCompta.Domain.Common;
using Microsoft.AspNetCore.Authorization;
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
    /// Returns true if the client has at least one Valide/Actif contract.
    /// Used to gate declaration create/edit in the BackOffice.
    /// </summary>
    [HttpGet("{id}/has-active-contract")]
    public async Task<IActionResult> HasActiveContract(int id)
    {
        var hasContract = await _clientService.HasActiveContractAsync(id, CabinetId);
        return Ok(ApiResponse<bool>.Ok(hasContract));
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

    // ── TVA ─────────────────────────────────────────────────────
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
        var result = await _service.SaveTVAAjustementsAsync(declarationId, request.TVARecuperableImmobilisations, request.CreditPrecedent, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("tva/calculate")]
    public async Task<IActionResult> CalculateTVA([FromBody] DeclarationTVADto dto)
    {
        var result = await _service.CalculateTVAAsync(dto);
        return Ok(result);
    }


    // ── TVA Invoice Lines ───────────────────────────────────────
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


    // ── IS ──────────────────────────────────────────────────────
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

    // ── IR ──────────────────────────────────────────────────────
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

    // ── CNSS ────────────────────────────────────────────────────
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

    // ── Status ──────────────────────────────────────────────────
    [HttpPut("{id}/status")]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeStatusRequest request)
    {
        var result = await _service.ChangeStatusAsync(id, request.NewStatut, CabinetId, UserId, request.Comment);
        return result.Success ? Ok(result) : BadRequest(result);
    }
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
    [Authorize(Policy = "payroll.create")]
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
    public async Task<IActionResult> UpdateVariables(int periodId, [FromBody] PaySlipVariablesDto dto)
    {
        var result = await _service.UpdateVariablesAsync(periodId, dto, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/calculate")]
    public async Task<IActionResult> CalculateAll(int id)
    {
        var result = await _service.CalculateAllAsync(id, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/validate")]
    [Authorize(Policy = "payroll.validate")]
    public async Task<IActionResult> Validate(int id)
    {
        var result = await _service.ValidateAsync(id, CabinetId, UserId);
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
        if (cab == null) return NotFound(ApiResponse<object>.Fail("Cabinet non trouvé."));
        return Ok(ApiResponse<object>.Ok(new { cab.Id, cab.Name, cab.Email, cab.ICE, cab.IsActive, cab.CreatedAt, Users = cab.Users.Select(u => new { u.Id, u.Email, u.FirstName, u.LastName, u.IsActive }) }));
    }

    [HttpPut("{id}/toggle")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var cab = await _db.Cabinets.FindAsync(id);
        if (cab == null) return NotFound();
        cab.IsActive = !cab.IsActive;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, cab.IsActive ? "Cabinet activé." : "Cabinet désactivé."));
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

// ═══════════════════════════════════════════════════════════════════════════════
// CABINET MANAGEMENT CONTROLLER (BackOffice — Current Cabinet)
// ═══════════════════════════════════════════════════════════════════════════════

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

    // ── Cabinet Info ──────────────────────────────────────────

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

    // ── Employees ─────────────────────────────────────────────

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
    [Authorize(Policy = "employee.create")]
    public async Task<IActionResult> CreateEmployee([FromBody] CabinetEmployeeCreateDto dto)
    {
        var result = await _cabinetService.CreateEmployeeAsync(dto, CabinetId, UserId);
        return result.Success ? CreatedAtAction(nameof(GetEmployee), new { id = result.Data?.Id }, result) : BadRequest(result);
    }

    [HttpPut("employees/{id}")]
    [Authorize(Policy = "employee.edit")]
    public async Task<IActionResult> UpdateEmployee(string id, [FromBody] CabinetEmployeeUpdateDto dto)
    {
        var result = await _cabinetService.UpdateEmployeeAsync(id, dto, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("employees/{id}/toggle")]
    [Authorize(Policy = "employee.edit")]
    public async Task<IActionResult> ToggleEmployee(string id)
    {
        var result = await _cabinetService.ToggleEmployeeActiveAsync(id, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── Roles & Permissions ───────────────────────────────────

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
    [Authorize(Policy = "system.admin")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
    {
        var result = await _cabinetService.CreateRoleAsync(request.Name, request.Description, request.Permissions, CabinetId, UserId);
        return result.Success ? CreatedAtAction(nameof(GetRole), new { roleId = result.Data?.Id }, result) : BadRequest(result);
    }

    [HttpPut("roles/{roleId}/permissions")]
    [Authorize(Policy = "system.admin")]
    public async Task<IActionResult> UpdateRolePermissions(string roleId, [FromBody] UpdatePermissionsRequest request)
    {
        var result = await _cabinetService.UpdateRolePermissionsAsync(roleId, request.Permissions, CabinetId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── Audit Logs ────────────────────────────────────────────

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

    // ── Profile ───────────────────────────────────────────────

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var result = await _cabinetService.GetProfileAsync(UserId, CabinetId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    // ── Claim Definitions ─────────────────────────────────────

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

// ── Request models for CabinetManagementController ──
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

// ════════════════════════════════════════════════════════════════════════════════
//  DOCUMENTS COMMERCIAUX — Devis, Bons de Commande, Contrats, Factures, Paiements
// ════════════════════════════════════════════════════════════════════════════════

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

// ── Request models for Commercial Controllers ──
public class RejetRequest
{
    public string Motif { get; set; } = string.Empty;
}


// ═══════════════════════════════════════════════════════════════════════════
// ║         COMPTABILITE (Accounting) — Demo data — Moroccan PCM          ║
// ═══════════════════════════════════════════════════════════════════════════
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

    // ── Helper: Moroccan PCM chart of accounts (demo) ──
    private static readonly (string Num, string Lib, string Cls)[] _pcm = new[]
    {
        ("1111","Capital social","1"), ("1140","Réserve légale","1"), ("1181","Résultat net de l''exercice","1"),
        ("1481","Emprunts auprès des éts de crédit","1"), ("1486","Fournisseurs d''immobilisations","1"),
        ("2110","Frais de constitution","2"), ("2230","Fonds commercial","2"),
        ("2311","Terrains nus","2"), ("2321","Bâtiments","2"), ("2332","Matériel et outillage","2"),
        ("2340","Matériel de transport","2"), ("2351","Mobilier de bureau","2"), ("2355","Matériel informatique","2"),
        ("2810","Amort. frais préliminaires","2"), ("2832","Amort. matériel et outillage","2"),
        ("2834","Amort. matériel de transport","2"), ("2835","Amort. MMB & aménagements","2"),
        ("3111","Marchandises","3"), ("3121","Matières premières","3"), ("3151","Produits finis","3"),
        ("3421","Clients","3"), ("3424","Clients douteux ou litigieux","3"),
        ("3425","Clients - effets à recevoir","3"), ("3451","Subventions à recevoir","3"),
        ("3455","État - TVA récupérable","3"), ("3458","État - crédit de TVA","3"),
        ("4411","Fournisseurs","4"), ("4415","Fournisseurs - effets à payer","4"),
        ("4432","Rémunérations dues au personnel","4"), ("4441","CNSS","4"), ("4443","Caisses de retraite","4"),
        ("4452","État - TVA facturée","4"), ("4453","État - TVA due","4"),
        ("4455","État - IS","4"), ("4456","État - Taxes sur CA","4"),
        ("5141","Banque (BMCE)","5"), ("5143","Trésorerie Générale","5"), ("5161","Caisse centrale","5"),
        ("5520","Crédits d''escompte","5"),
        ("6111","Achats de marchandises","6"), ("6121","Achats de mat. premières","6"),
        ("6125","Achats non stockés","6"), ("6131","Locations et charges locatives","6"),
        ("6133","Entretien et réparations","6"), ("6134","Primes d''assurances","6"),
        ("6141","Transport du personnel","6"), ("6142","Déplacements et missions","6"),
        ("6143","Frais postaux","6"), ("6144","Publicité","6"),
        ("6147","Services bancaires","6"), ("6161","Impôts et taxes","6"),
        ("6171","Rémunérations du personnel","6"), ("6174","Charges sociales","6"),
        ("6176","Charges sociales diverses","6"), ("6182","Pertes sur créances irrécouvrables","6"),
        ("6191","DEA des immob. en non-valeur","6"), ("6193","DEA des immob. corporelles","6"),
        ("6311","Intérêts des emprunts","6"), ("6386","Escomptes accordés","6"),
        ("6585","Créances devenues irrécouvrables","6"),
        ("7111","Ventes de marchandises","7"), ("7121","Ventes de biens produits","7"),
        ("7124","Ventes de services","7"), ("7127","Ventes et produits accessoires","7"),
        ("7129","RRR accordés par l''entreprise","7"), ("7141","Subventions d''exploitation","7"),
        ("7197","Transferts de charges d''exploitation","7"),
        ("7381","Intérêts et produits assimilés","7"), ("7386","Escomptes obtenus","7"),
        ("7585","Rentrées sur créances soldées","7"), ("7111","Ventes de marchandises","7")
    };

    // ── Écritures ──
    [HttpGet("ecritures")]
    public async Task<IActionResult> GetEcritures([FromQuery] int? clientId, [FromQuery] string? journal, [FromQuery] int page = 1, [FromQuery] string? search = null)
    {
        var clientName = await GetClientName(clientId);
        var rng = new Random(42 + (clientId ?? 1));
        var jcodes = new[] { "AC", "VE", "BQ", "CA", "OD" };
        var jnoms = new[] { "Achats", "Ventes", "Banque", "Caisse", "Opérations Diverses" };
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
                Libelle = $"Écriture {jnoms[jIdx]} #{i}",
                Debit = montant,
                Credit = 0,
                Reference = $"REF-{i:D4}",
                Statut = i % 5 == 0 ? "Validé" : "Brouillon",
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
                Libelle = $"Écriture {jnoms[jIdx]} #{i}",
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
            TotalCount = all.Count, Page = page, PageSize = 20
        };
        return Ok(ApiResponse<PagedResult<EcritureComptableDto>>.Ok(paged));
    }

    // ── Journaux ──
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
            new() { Code = "OD", Nom = "Opérations Diverses", Type = "OD", NombreEcritures = 18 + rng.Next(10), TotalDebit = Math.Round((decimal)(rng.NextDouble() * 200000 + 50000), 2), TotalCredit = Math.Round((decimal)(rng.NextDouble() * 200000 + 50000), 2) },
            new() { Code = "AN", Nom = "À Nouveau", Type = "AN", NombreEcritures = 12, TotalDebit = Math.Round((decimal)(rng.NextDouble() * 600000 + 100000), 2), TotalCredit = Math.Round((decimal)(rng.NextDouble() * 600000 + 100000), 2) },
            new() { Code = "SA", Nom = "Journal des Salaires", Type = "Paie", NombreEcritures = 24 + rng.Next(6), TotalDebit = Math.Round((decimal)(rng.NextDouble() * 350000 + 80000), 2), TotalCredit = Math.Round((decimal)(rng.NextDouble() * 350000 + 80000), 2) }
        };
        return Ok(ApiResponse<List<JournalDto>>.Ok(journaux));
    }

    // ── Grand Livre ──
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

    // ── Balance ──
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

    // ── Balance Âgée ──
    [HttpGet("balance-agee")]
    public IActionResult GetBalanceAgee([FromQuery] int clientId)
    {
        var rng = new Random(400 + clientId);
        var tiers = new[] {
            ("3421","Alpha Distribution SARL"), ("3421","Beta Import-Export SA"), ("3421","Gamma Services Conseil"),
            ("3424","Delta Fournitures"), ("3424","Epsilon Négoce"), ("4411","Fournisseur Atlas"),
            ("4411","Fournisseur Marrakech Matériaux"), ("4411","Fournisseur Casa Parts"), ("4411","Fournisseur Rabat Industrie")
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

    // ── Rapprochements Bancaires ──
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
                Statut = Math.Abs(ecart) < 100 ? "Rapproché" : "En Cours",
                NombreLignesNonRapprochees = Math.Abs(ecart) < 100 ? 0 : rng.Next(1, 8),
                DateRapprochement = DateTime.Parse(p + "-28")
            });
        }
        return Ok(ApiResponse<List<RapprochementBancaireDto>>.Ok(rapprochements));
    }

    // ── Create Écriture ──
    [HttpPost("ecritures")]
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
        return Ok(ApiResponse<EcritureComptableDto>.Ok(result, "Écriture créée avec succès."));
    }

    // ── Create Rapprochement ──
    [HttpPost("rapprochements")]
    public IActionResult CreateRapprochement([FromBody] RapprochementCreateRequest req)
    {
        var result = new RapprochementBancaireDto
        {
            Id = new Random().Next(100, 999),
            ClientId = req.ClientId,
            CompteBancaire = req.CompteBancaire,
            Periode = req.Periode,
            SoldeCompta = 0, SoldeBanque = 0, Ecart = 0,
            EstRapproche = false, Statut = "En Cours",
            DateRapprochement = DateTime.UtcNow
        };
        return Ok(ApiResponse<RapprochementBancaireDto>.Ok(result, "Rapprochement créé avec succès."));
    }
}

public class RapprochementCreateRequest
{
    public int ClientId { get; set; }
    public string CompteBancaire { get; set; } = string.Empty;
    public string Periode { get; set; } = string.Empty;
}
