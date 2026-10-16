using MetaCompta.Application.DTOs;
using MetaCompta.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MetaCompta.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IAccountInvitationService _invitations;

    public AuthController(IAuthService authService, IAccountInvitationService invitations)
    {
        _authService = authService;
        _invitations = invitations;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("register")]
    [Authorize(Policy = "system.admin")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] string refreshToken)
    {
        var result = await _authService.RefreshTokenAsync(refreshToken);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!;
        var result = await _authService.ChangePasswordAsync(userId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("activation-info")]
    [AllowAnonymous]
    public async Task<IActionResult> ActivationInfo([FromQuery] string userId, [FromQuery] string token)
    {
        var result = await _invitations.GetActivationInfoAsync(userId, token ?? "");
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("activate")]
    [AllowAnonymous]
    public async Task<IActionResult> Activate([FromBody] ActivateAccountDto dto)
    {
        var result = await _invitations.ActivateAccountAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
