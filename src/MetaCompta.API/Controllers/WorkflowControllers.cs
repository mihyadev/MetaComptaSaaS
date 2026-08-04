using System.Security.Claims;
using MetaCompta.Application.DTOs;
using MetaCompta.Application.Interfaces;
using MetaCompta.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.API.Controllers;

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║                     DEPARTMENT CONTROLLER                               ║
// ╚══════════════════════════════════════════════════════════════════════════╝

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "BackOfficeAccess")]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _service;
    public DepartmentsController(IDepartmentService service) => _service = service;

    private int CabinetId => int.TryParse(User.FindFirst("CabinetId")?.Value, out var id) ? id : 0;
    private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

    [HttpGet]
    [Authorize(Policy = ClaimDefinitions.DepartmentView)]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync(CabinetId));

    [HttpGet("{id}")]
    [Authorize(Policy = ClaimDefinitions.DepartmentView)]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _service.GetByIdAsync(id, CabinetId));

    [HttpPost]
    [Authorize(Policy = ClaimDefinitions.DepartmentCreate)]
    public async Task<IActionResult> Create([FromBody] DepartmentCreateDto dto)
        => Ok(await _service.CreateAsync(dto, CabinetId, UserId));

    [HttpPut("{id}")]
    [Authorize(Policy = ClaimDefinitions.DepartmentEdit)]
    public async Task<IActionResult> Update(int id, [FromBody] DepartmentCreateDto dto)
        => Ok(await _service.UpdateAsync(id, dto, CabinetId, UserId));

    [HttpDelete("{id}")]
    [Authorize(Policy = ClaimDefinitions.DepartmentDelete)]
    public async Task<IActionResult> Delete(int id)
        => Ok(await _service.DeleteAsync(id, CabinetId, UserId));

    [HttpPost("{departmentId}/members")]
    [Authorize(Policy = ClaimDefinitions.DepartmentManageMembers)]
    public async Task<IActionResult> AddMember(int departmentId, [FromQuery] string userId, [FromQuery] bool isLead = false)
        => Ok(await _service.AddMemberAsync(departmentId, userId, isLead, CabinetId, UserId));

    [HttpDelete("{departmentId}/members/{userId}")]
    [Authorize(Policy = ClaimDefinitions.DepartmentManageMembers)]
    public async Task<IActionResult> RemoveMember(int departmentId, string userId)
        => Ok(await _service.RemoveMemberAsync(departmentId, userId, CabinetId, UserId));

    [HttpGet("{departmentId}/members")]
    [Authorize(Policy = ClaimDefinitions.DepartmentView)]
    public async Task<IActionResult> GetMembers(int departmentId)
        => Ok(await _service.GetMembersAsync(departmentId, CabinetId));
}

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║                     WORKFLOW CONTROLLER                                 ║
// ╚══════════════════════════════════════════════════════════════════════════╝

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "BackOfficeAccess")]
public class WorkflowController : ControllerBase
{
    private readonly IWorkflowService _workflow;
    public WorkflowController(IWorkflowService workflow) => _workflow = workflow;

    private int CabinetId => int.TryParse(User.FindFirst("CabinetId")?.Value, out var id) ? id : 0;
    private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

    // ── Templates ────────────────────────────────────────────────

    [HttpGet("templates")]
    [Authorize(Policy = ClaimDefinitions.WorkflowView)]
    public async Task<IActionResult> GetTemplates([FromQuery] string? declarationType = null, [FromQuery] int? clientId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await _workflow.GetTemplatesAsync(CabinetId, declarationType, clientId, page, pageSize));

    [HttpGet("templates/{id}")]
    [Authorize(Policy = ClaimDefinitions.WorkflowView)]
    public async Task<IActionResult> GetTemplate(int id)
        => Ok(await _workflow.GetTemplateByIdAsync(id, CabinetId));

    [HttpPost("templates")]
    [Authorize(Policy = ClaimDefinitions.WorkflowCreate)]
    public async Task<IActionResult> CreateTemplate([FromBody] WorkflowTemplateCreateDto dto)
        => Ok(await _workflow.CreateTemplateAsync(dto, CabinetId, UserId));

    [HttpPut("templates/{id}")]
    [Authorize(Policy = ClaimDefinitions.WorkflowEdit)]
    public async Task<IActionResult> UpdateTemplate(int id, [FromBody] WorkflowTemplateCreateDto dto)
        => Ok(await _workflow.UpdateTemplateAsync(id, dto, CabinetId, UserId));

    [HttpDelete("templates/{id}")]
    [Authorize(Policy = ClaimDefinitions.WorkflowDelete)]
    public async Task<IActionResult> DeleteTemplate(int id)
        => Ok(await _workflow.DeleteTemplateAsync(id, CabinetId, UserId));

    [HttpPost("templates/default")]
    [Authorize(Policy = ClaimDefinitions.WorkflowCreate)]
    public async Task<IActionResult> CreateDefaultTemplate()
        => Ok(await _workflow.CreateDefaultTemplateAsync(CabinetId, UserId));

    [HttpPost("templates/seed-all-defaults")]
    [Authorize(Policy = ClaimDefinitions.WorkflowCreate)]
    public async Task<IActionResult> SeedAllDefaultTemplates()
        => Ok(await _workflow.SeedAllDefaultTemplatesAsync(CabinetId, UserId));

    // ── Instances ────────────────────────────────────────────────

    [HttpPost("start")]
    [Authorize(Policy = ClaimDefinitions.WorkflowCreate)]
    public async Task<IActionResult> StartWorkflow([FromBody] StartWorkflowDto dto)
        => Ok(await _workflow.StartWorkflowAsync(dto, CabinetId, UserId));

    [HttpGet("instances")]
    [Authorize(Policy = ClaimDefinitions.WorkflowView)]
    public async Task<IActionResult> GetInstances([FromQuery] int? clientId = null, [FromQuery] string? status = null, [FromQuery] int? declarationId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await _workflow.GetInstancesAsync(CabinetId, clientId, status, declarationId, page, pageSize));

    [HttpGet("instances/{id}")]
    [Authorize(Policy = ClaimDefinitions.WorkflowView)]
    public async Task<IActionResult> GetInstance(int id)
        => Ok(await _workflow.GetInstanceByIdAsync(id, CabinetId));

    [HttpPost("instances/{id}/cancel")]
    [Authorize(Policy = ClaimDefinitions.WorkflowCancel)]
    public async Task<IActionResult> CancelWorkflow(int id, [FromBody] string reason)
        => Ok(await _workflow.CancelWorkflowAsync(id, reason, CabinetId, UserId));

    [HttpPost("instances/{id}/pause")]
    [Authorize(Policy = ClaimDefinitions.WorkflowCancel)]
    public async Task<IActionResult> PauseWorkflow(int id)
        => Ok(await _workflow.PauseWorkflowAsync(id, CabinetId, UserId));

    [HttpPost("instances/{id}/resume")]
    [Authorize(Policy = ClaimDefinitions.WorkflowCancel)]
    public async Task<IActionResult> ResumeWorkflow(int id)
        => Ok(await _workflow.ResumeWorkflowAsync(id, CabinetId, UserId));

    // ── Step Operations ──────────────────────────────────────────

    /// <summary>
    /// Advance current step. Auth: any BackOffice user; service enforces assignee-only (or SYSTEM).
    /// Assignees without workflow.advance claim can still advance their own step.
    /// </summary>
    [HttpPost("instances/{id}/advance")]
    public async Task<IActionResult> AdvanceStep(int id, [FromBody] AdvanceStepDto dto)
        => Ok(await _workflow.AdvanceStepAsync(id, dto, CabinetId, UserId));

    /// <summary>Claim current step exclusively (Prendre / Ouvrir). Manager may forceOverride with reason.</summary>
    [HttpPost("instances/{id}/claim")]
    public async Task<IActionResult> ClaimStep(int id, [FromBody] ClaimStepRequest? request = null)
        => Ok(await _workflow.ClaimStepAsync(id, CabinetId, UserId, request?.ForceOverride ?? false, request?.Reason));

    [HttpPost("steps/{stepInstanceId}/reassign")]
    [Authorize(Policy = ClaimDefinitions.WorkflowReassign)]
    public async Task<IActionResult> ReassignStep(int stepInstanceId, [FromBody] ReassignStepDto dto)
        => Ok(await _workflow.ReassignStepAsync(stepInstanceId, dto, CabinetId, UserId));

    [HttpGet("instances/{id}/current-step")]
    [Authorize(Policy = ClaimDefinitions.WorkflowView)]
    public async Task<IActionResult> GetCurrentStep(int id)
        => Ok(await _workflow.GetCurrentStepAsync(id, CabinetId));

    [HttpGet("my-steps")]
    [Authorize(Policy = ClaimDefinitions.WorkflowView)]
    public async Task<IActionResult> GetMyPendingSteps([FromQuery] int? clientId = null)
        => Ok(await _workflow.GetMyPendingStepsAsync(CabinetId, UserId, clientId));

    // ── Client Config ────────────────────────────────────────────

    [HttpGet("client-config/{clientId}")]
    [Authorize(Policy = ClaimDefinitions.WorkflowConfigure)]
    public async Task<IActionResult> GetClientConfigs(int clientId)
        => Ok(await _workflow.GetClientConfigsAsync(clientId, CabinetId));

    [HttpPost("client-config")]
    [Authorize(Policy = ClaimDefinitions.WorkflowConfigure)]
    public async Task<IActionResult> SaveClientConfig([FromBody] ClientWorkflowConfigCreateDto dto)
        => Ok(await _workflow.SaveClientConfigAsync(dto, CabinetId, UserId));

    [HttpPost("client-config/ensure-template")]
    [Authorize(Policy = ClaimDefinitions.WorkflowConfigure)]
    public async Task<IActionResult> EnsureClientTemplate([FromBody] EnsureClientWorkflowDto dto)
        => Ok(await _workflow.EnsureClientTemplateAsync(dto.ClientId, dto.SourceTemplateId, dto.DeclarationType, CabinetId, UserId));

    [HttpPost("client-config/customize")]
    [Authorize(Policy = ClaimDefinitions.WorkflowConfigure)]
    public async Task<IActionResult> SaveClientWorkflowCustomization([FromBody] ClientWorkflowCustomizeDto dto)
        => Ok(await _workflow.SaveClientWorkflowCustomizationAsync(dto, CabinetId, UserId));

    // ── Dashboard & History ──────────────────────────────────────

    [HttpGet("dashboard")]
    [Authorize(Policy = ClaimDefinitions.WorkflowView)]
    public async Task<IActionResult> GetDashboard()
        => Ok(await _workflow.GetDashboardAsync(CabinetId, UserId));

    [HttpGet("instances/{id}/history")]
    [Authorize(Policy = ClaimDefinitions.WorkflowView)]
    public async Task<IActionResult> GetHistory(int id)
        => Ok(await _workflow.GetHistoryAsync(id, CabinetId));
}

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║               CLIENT WORKFLOW CONTROLLER (Extranet)                     ║
// ╚══════════════════════════════════════════════════════════════════════════╝

[ApiController]
[Route("api/client-workflow")]
[Authorize(Policy = "ExtranetAccess")]
public class ClientWorkflowController : ControllerBase
{
    private readonly IWorkflowService _workflow;
    public ClientWorkflowController(IWorkflowService workflow) => _workflow = workflow;

    private int CabinetId => int.TryParse(User.FindFirst("CabinetId")?.Value, out var id) ? id : 0;
    private int ClientId => int.TryParse(User.FindFirst("ClientId")?.Value, out var id) ? id : 0;
    private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

    [HttpPost("instances/{id}/approve")]
    public async Task<IActionResult> ApproveStep(int id, [FromBody] AdvanceStepDto dto)
    {
        // Security: verify instance belongs to this client before approving
        if (ClientId > 0)
        {
            var inst = await _workflow.GetInstanceByIdAsync(id, CabinetId);
            if (inst.Success && inst.Data != null && inst.Data.ClientId != ClientId)
                return NotFound(ApiResponse<WorkflowStepInstanceDto>.Fail("Instance introuvable."));
        }
        var result = await _workflow.AdvanceStepAsync(id, dto, CabinetId, UserId);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("instances/{id}/current-step")]
    public async Task<IActionResult> GetCurrentStep(int id)
    {
        // Security: verify instance belongs to this client
        if (ClientId > 0)
        {
            var inst = await _workflow.GetInstanceByIdAsync(id, CabinetId);
            if (inst.Success && inst.Data != null && inst.Data.ClientId != ClientId)
                return NotFound(ApiResponse<WorkflowStepInstanceDto>.Fail("Instance introuvable."));
        }
        return Ok(await _workflow.GetCurrentStepAsync(id, CabinetId));
    }

    [HttpGet("instances/{id}")]
    public async Task<IActionResult> GetInstance(int id)
    {
        var result = await _workflow.GetInstanceByIdAsync(id, CabinetId);
        // Security: verify instance belongs to this client
        if (ClientId > 0 && result.Success && result.Data != null && result.Data.ClientId != ClientId)
            return NotFound(ApiResponse<WorkflowInstanceDto>.Fail("Instance introuvable."));
        return Ok(result);
    }

    [HttpGet("instances")]
    public async Task<IActionResult> GetInstances([FromQuery] int? declarationId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await _workflow.GetInstancesAsync(CabinetId, ClientId > 0 ? ClientId : null, "Active", declarationId, page, pageSize));

    [HttpGet("my-tasks")]
    public async Task<IActionResult> GetMyTasks()
        => Ok(await _workflow.GetMyPendingStepsAsync(CabinetId, UserId, ClientId > 0 ? ClientId : null));

    [HttpGet("my-tasks/count")]
    public async Task<IActionResult> GetMyTasksCount()
    {
        var result = await _workflow.GetMyPendingStepsAsync(CabinetId, UserId, ClientId > 0 ? ClientId : null);
        var count = result?.Data?.Count ?? 0;
        return Ok(ApiResponse<int>.Ok(count));
    }
}

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║           CLIENT DECLARATIONS CONTROLLER (Extranet)                     ║
// ╚══════════════════════════════════════════════════════════════════════════╝

[ApiController]
[Route("api/client-declarations")]
[Authorize(Policy = "ExtranetAccess")]
public class ClientDeclarationsController : ControllerBase
{
    private readonly IDeclarationService _service;
    private readonly IWorkflowService _workflows;
    public ClientDeclarationsController(IDeclarationService service, IWorkflowService workflows)
    {
        _service = service;
        _workflows = workflows;
    }

    private int CabinetId => int.TryParse(User.FindFirst("CabinetId")?.Value, out var id) ? id : 0;
    private int ClientId => int.TryParse(User.FindFirst("ClientId")?.Value, out var id) ? id : 0;
    private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? type = null, [FromQuery] string? statut = null,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        // Security: always enforce ClientId from JWT — client cannot browse other clients
        if (ClientId == 0) return Forbid();
        // Open CNSS/TVA workflows started without CreateDeclaration → create so Extranet list is not empty
        await _workflows.HealMissingDeclarationsForClientAsync(CabinetId, ClientId, UserId);
        return Ok(await _service.GetAllAsync(CabinetId, ClientId, type, statut, page, pageSize));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await _service.GetByIdAsync(id, CabinetId);
        // Security: verify declaration belongs to this client
        if (ClientId > 0 && result.Success && result.Data != null && result.Data.ClientId != ClientId)
            return NotFound(ApiResponse<DeclarationDto>.Fail("Déclaration introuvable."));
        return Ok(result);
    }

    // --- Type-specific financial data for Extranet clients ---

    [HttpGet("{id}/tva")]
    public async Task<IActionResult> GetTVA(int id)
    {
        var decl = await _service.GetByIdAsync(id, CabinetId);
        if (!decl.Success || decl.Data == null || (ClientId > 0 && decl.Data.ClientId != ClientId))
            return NotFound(ApiResponse<DeclarationTVADto>.Fail("Introuvable."));
        return Ok(await _service.GetTVADataAsync(id, CabinetId));
    }

    [HttpGet("{id}/is")]
    public async Task<IActionResult> GetIS(int id)
    {
        var decl = await _service.GetByIdAsync(id, CabinetId);
        if (!decl.Success || decl.Data == null || (ClientId > 0 && decl.Data.ClientId != ClientId))
            return NotFound(ApiResponse<DeclarationISDto>.Fail("Introuvable."));
        return Ok(await _service.GetISDataAsync(id, CabinetId));
    }

    [HttpGet("{id}/ir")]
    public async Task<IActionResult> GetIR(int id)
    {
        var decl = await _service.GetByIdAsync(id, CabinetId);
        if (!decl.Success || decl.Data == null || (ClientId > 0 && decl.Data.ClientId != ClientId))
            return NotFound(ApiResponse<DeclarationIRDto>.Fail("Introuvable."));
        return Ok(await _service.GetIRDataAsync(id, CabinetId));
    }

    [HttpGet("{id}/cnss")]
    public async Task<IActionResult> GetCNSS(int id)
    {
        var decl = await _service.GetByIdAsync(id, CabinetId);
        if (!decl.Success || decl.Data == null || (ClientId > 0 && decl.Data.ClientId != ClientId))
            return NotFound(ApiResponse<DeclarationCNSSDto>.Fail("Introuvable."));
        return Ok(await _service.GetCNSSDataAsync(id, CabinetId));
    }

    [HttpGet("{id}/cnss/employees")]
    public async Task<IActionResult> GetCnssEmployees(int id)
    {
        var decl = await _service.GetByIdAsync(id, CabinetId);
        if (!decl.Success || decl.Data == null || (ClientId > 0 && decl.Data.ClientId != ClientId))
            return NotFound(ApiResponse<CnssEmployeeRosterDto>.Fail("Introuvable."));
        return Ok(await _service.GetCnssEmployeesAsync(id, CabinetId));
    }
}

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║                     MESSAGES CONTROLLER                                 ║
// ╚══════════════════════════════════════════════════════════════════════════╝

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _service;
    public MessagesController(IMessageService service) => _service = service;

    private int CabinetId => int.TryParse(User.FindFirst("CabinetId")?.Value, out var id) ? id : 0;
    private int ClientId => int.TryParse(User.FindFirst("ClientId")?.Value, out var id) ? id : 0;
    private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
    private string UserName => User.FindFirst(ClaimTypes.Name)?.Value ?? User.Identity?.Name ?? "";
    private bool IsClientUser => ClientId > 0;

    [HttpGet("declaration/{declarationId}")]
    public async Task<IActionResult> GetByDeclaration(int declarationId)
        => Ok(await _service.GetByDeclarationAsync(declarationId, CabinetId, IsClientUser ? ClientId : null));

    [HttpGet("client/{clientId}")]
    public async Task<IActionResult> GetByClient(int clientId)
    {
        // Security: client users can only access their own messages
        if (IsClientUser) clientId = ClientId;
        return Ok(await _service.GetByClientAsync(clientId, CabinetId));
    }

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations()
        => Ok(await _service.GetConversationsAsync(CabinetId, UserId, IsClientUser ? ClientId : null));

    [HttpPost]
    public async Task<IActionResult> Send([FromBody] SendMessageDto dto)
    {
        var isClient = User.IsInRole("Client");
        var role = isClient ? "Client" : "Cabinet";
        // Security: client users can only send messages for their own client
        if (IsClientUser && dto.ClientId.HasValue && dto.ClientId.Value != ClientId)
            return BadRequest(ApiResponse<MessageDto>.Fail("Accès non autorisé."));
        if (IsClientUser) dto.ClientId = ClientId;
        return Ok(await _service.SendAsync(dto, CabinetId, UserId, UserName, role));
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
        => Ok(await _service.MarkAsReadAsync(id, UserId));

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount([FromQuery] int? clientId = null)
    {
        // Security: client users can only see their own unread count
        if (IsClientUser) clientId = ClientId;
        return Ok(await _service.GetUnreadCountAsync(CabinetId, UserId, clientId));
    }
}

public record ClaimStepRequest(bool ForceOverride = false, string? Reason = null);
