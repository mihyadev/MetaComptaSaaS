using System.Security.Claims;
using MetaCompta.Application.DTOs;
using MetaCompta.Application.Interfaces;
using MetaCompta.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "BackOfficeAccess")]
public class UnitTakesController : ControllerBase
{
    private readonly IUnitTakeService _unitTakes;
    public UnitTakesController(IUnitTakeService unitTakes) => _unitTakes = unitTakes;

    private int CabinetId => int.TryParse(User.FindFirst("CabinetId")?.Value, out var id) ? id : 0;
    private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

    [HttpGet("types")]
    [Authorize(Policy = ClaimDefinitions.TaskDetail)]
    public async Task<IActionResult> GetTypes()
        => Ok(await _unitTakes.GetUnitTypesAsync());

    [HttpGet("open")]
    [Authorize(Policy = ClaimDefinitions.TaskDetail)]
    public async Task<IActionResult> GetOpen([FromQuery] string unitTypeCode, [FromQuery] int unitEntityId)
        => Ok(await _unitTakes.GetOpenTakeAsync(unitTypeCode, unitEntityId, CabinetId));

    [HttpPost("open")]
    [Authorize(Policy = ClaimDefinitions.TaskEdit)]
    public async Task<IActionResult> Open([FromBody] OpenUnitTakeDto dto)
        => Ok(await _unitTakes.OpenTakeAsync(dto, CabinetId, UserId));

    [HttpPost("close")]
    [Authorize(Policy = ClaimDefinitions.TaskEdit)]
    public async Task<IActionResult> Close([FromBody] CloseUnitTakeDto dto)
        => Ok(await _unitTakes.CloseTakeAsync(dto, CabinetId, UserId));
}
