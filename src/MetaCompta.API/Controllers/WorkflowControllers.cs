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

    [HttpPost("instances/{id}/advance")]
    [Authorize(Policy = ClaimDefinitions.WorkflowAdvance)]
    public async Task<IActionResult> AdvanceStep(int id, [FromBody] AdvanceStepDto dto)
        => Ok(await _workflow.AdvanceStepAsync(id, dto, CabinetId, UserId));

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
