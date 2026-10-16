using System.Security.Claims;
using MetaCompta.Application.DTOs;
using MetaCompta.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.API.Controllers;

[ApiController]
[Route("api/domiciliation")]
[Authorize(Policy = "BackOfficeAccess")]
public class DomiciliationController : ControllerBase
{
    private readonly IDomiciliationService _service;
    public DomiciliationController(IDomiciliationService service) => _service = service;

    private int CabinetId => int.TryParse(User.FindFirst("CabinetId")?.Value, out var id) ? id : 0;
    private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
    private string UserName => User.FindFirst(ClaimTypes.Name)?.Value ?? User.Identity?.Name ?? "Staff";

    [HttpGet("dashboard")]
    [Authorize(Policy = "domiciliation.view")]
    public async Task<IActionResult> Dashboard()
    {
        var result = await _service.GetDashboardAsync(CabinetId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("clients/{clientId:int}")]
    [Authorize(Policy = "domiciliation.view")]
    public async Task<IActionResult> ClientWorkspace(int clientId)
    {
        var result = await _service.GetClientWorkspaceAsync(clientId, CabinetId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("clients/{clientId:int}/timeline")]
    [Authorize(Policy = "domiciliation.view")]
    public async Task<IActionResult> Timeline(int clientId, [FromQuery] int take = 50)
    {
        var result = await _service.GetTimelineAsync(clientId, CabinetId, take);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("courriers")]
    [Authorize(Policy = "domiciliation.receive")]
    public async Task<IActionResult> Receive([FromBody] CourrierReceiveDto dto)
    {
        var result = await _service.ReceiveCourrierAsync(dto, CabinetId, UserId, UserName);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("courriers/{id:int}")]
    [Authorize(Policy = "domiciliation.view")]
    public async Task<IActionResult> GetCourrier(int id)
    {
        var result = await _service.GetCourrierAsync(id, CabinetId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("courriers/{id:int}/scan/{documentId:int}")]
    [Authorize(Policy = "domiciliation.receive")]
    public async Task<IActionResult> AttachScan(int id, int documentId)
    {
        var result = await _service.AttachScanAsync(id, documentId, CabinetId, UserId, UserName);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("courriers/{id:int}/pieces/{pieceId:int}")]
    [Authorize(Policy = "domiciliation.receive")]
    public async Task<IActionResult> RemovePiece(int id, int pieceId)
    {
        var result = await _service.RemovePieceAsync(id, pieceId, CabinetId, UserId, UserName);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("courriers/{id:int}/pieces/{pieceId:int}/primary")]
    [Authorize(Policy = "domiciliation.receive")]
    public async Task<IActionResult> SetPrimaryPiece(int id, int pieceId)
    {
        var result = await _service.SetPrimaryPieceAsync(id, pieceId, CabinetId, UserId, UserName);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("courriers/{id:int}/advance")]
    [Authorize(Policy = "BackOfficeAccess")]
    public async Task<IActionResult> AdvanceCourrier(int id, [FromBody] CourrierStatusActionDto dto)
    {
        var action = (dto.Action ?? "").Trim().ToLowerInvariant();
        var hasProcess = User.HasClaim(c => c.Type == "Permission" && c.Value == "domiciliation.process") || User.IsInRole("SuperAdmin");
        var hasNotify = User.HasClaim(c => c.Type == "Permission" && c.Value == "domiciliation.notify") || hasProcess;

        if (action == "notify")
        {
            if (!hasNotify)
                return Forbid();
        }
        else if (!hasProcess)
        {
            return Forbid();
        }

        var result = await _service.AdvanceCourrierAsync(id, dto, CabinetId, UserId, UserName);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("actions/{actionId:int}/advance")]
    [Authorize(Policy = "domiciliation.process")]
    public async Task<IActionResult> AdvanceAction(int actionId, [FromBody] DemandeActionAdvanceDto dto)
    {
        var result = await _service.AdvanceClientActionAsync(actionId, dto, CabinetId, UserId, UserName);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("document-requests")]
    [Authorize(Policy = "domiciliation.request")]
    public async Task<IActionResult> CreateDocumentRequest([FromBody] DemandeDocumentCreateDto dto)
    {
        var result = await _service.CreateDocumentRequestAsync(dto, CabinetId, UserId, UserName);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("document-requests/{id:int}/attach/{documentId:int}")]
    [Authorize(Policy = "domiciliation.process")]
    public async Task<IActionResult> AttachDocument(int id, int documentId)
    {
        var result = await _service.AttachDocumentVersionAsync(id, documentId, CabinetId, UserId, UserName, fromClient: false);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("document-requests/{id:int}/review")]
    [Authorize(Policy = "domiciliation.process")]
    public async Task<IActionResult> ReviewDocument(int id, [FromBody] DemandeDocumentReviewDto dto)
    {
        var result = await _service.ReviewDocumentRequestAsync(id, dto, CabinetId, UserId, UserName);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

[ApiController]
[Route("api/client-domiciliation")]
[Authorize(Policy = "ExtranetAccess")]
public class ClientDomiciliationController : ControllerBase
{
    private readonly IDomiciliationService _service;
    public ClientDomiciliationController(IDomiciliationService service) => _service = service;

    private int CabinetId => int.TryParse(User.FindFirst("CabinetId")?.Value, out var id) ? id : 0;
    private int ClientId => int.TryParse(User.FindFirst("ClientId")?.Value, out var id) ? id : 0;
    private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
    private string UserName => User.FindFirst(ClaimTypes.Name)?.Value ?? "Client";

    [HttpGet("courriers")]
    public async Task<IActionResult> MyCourriers()
    {
        if (ClientId <= 0) return BadRequest(ApiResponse<object>.Fail("Client non identifié."));
        var result = await _service.GetClientNotifiedCourriersAsync(ClientId, CabinetId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("document-requests")]
    public async Task<IActionResult> MyDocumentRequests()
    {
        if (ClientId <= 0) return BadRequest(ApiResponse<object>.Fail("Client non identifié."));
        var result = await _service.GetClientDocumentRequestsAsync(ClientId, CabinetId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("courriers/{id:int}/actions")]
    public async Task<IActionResult> RequestAction(int id, [FromBody] DemandeActionCreateDto dto)
    {
        if (ClientId <= 0) return BadRequest(ApiResponse<object>.Fail("Client non identifié."));
        var result = await _service.CreateClientActionAsync(id, dto, CabinetId, UserId, ClientId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("document-requests/{id:int}/attach/{documentId:int}")]
    public async Task<IActionResult> AttachDocument(int id, int documentId)
    {
        if (ClientId <= 0) return BadRequest(ApiResponse<object>.Fail("Client non identifié."));
        var result = await _service.AttachDocumentVersionAsync(id, documentId, CabinetId, UserId, UserName, fromClient: true);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
