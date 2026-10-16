using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MetaCompta.Application.DTOs;
using MetaCompta.Application.Interfaces;
using MetaCompta.Domain.Common;
using MetaCompta.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace MetaCompta.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly MetaComptaDbContext _db;
    private readonly IConfiguration _config;
    private readonly IEffectivePermissionService _permissions;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        MetaComptaDbContext db,
        IConfiguration config,
        IEffectivePermissionService permissions)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
        _config = config;
        _permissions = permissions;
    }

    public async Task<ApiResponse<TokenDto>> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return ApiResponse<TokenDto>.Fail("Email ou mot de passe incorrect.");

        if (!user.EmailConfirmed)
            return ApiResponse<TokenDto>.Fail("Compte non activé — vérifiez votre email pour activer votre compte.");

        if (!user.IsActive)
            return ApiResponse<TokenDto>.Fail("Ce compte est désactivé. Contactez votre administrateur.");

        if (!await _userManager.CheckPasswordAsync(user, dto.Password))
            return ApiResponse<TokenDto>.Fail("Email ou mot de passe incorrect.");

        var portal = (dto.Portal ?? "").Trim().ToLowerInvariant();
        if (portal is "backoffice" or "back-office")
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (user.ClientId.HasValue || roles.Any(r => string.Equals(r, "Client", StringComparison.OrdinalIgnoreCase)))
                return ApiResponse<TokenDto>.Fail("Compte client Extranet — utilisez le portail client.");
        }
        else if (portal == "extranet")
        {
            if (!user.ClientId.HasValue)
                return ApiResponse<TokenDto>.Fail("Compte cabinet — utilisez le BackOffice.");
        }

        user.LastLoginAt = DateTime.UtcNow;
        var token = await GenerateTokenAsync(user);

        user.RefreshToken = token.RefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return ApiResponse<TokenDto>.Ok(token, "Connexion réussie.");
    }

    public async Task<ApiResponse<TokenDto>> RegisterAsync(RegisterDto dto)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            return ApiResponse<TokenDto>.Fail("Un utilisateur avec cet email existe déjà.");

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            CabinetId = dto.CabinetId,
            ClientId = dto.ClientId,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return ApiResponse<TokenDto>.Fail("Erreur lors de la création.", result.Errors.Select(e => e.Description).ToList());

        if (!string.IsNullOrEmpty(dto.Role))
        {
            if (!await _roleManager.RoleExistsAsync(dto.Role))
                await _roleManager.CreateAsync(new ApplicationRole { Name = dto.Role, IsSystemRole = true });
            await _userManager.AddToRoleAsync(user, dto.Role);
        }

        var token = await GenerateTokenAsync(user);
        user.RefreshToken = token.RefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return ApiResponse<TokenDto>.Ok(token, "Inscription réussie.");
    }

    public async Task<ApiResponse<TokenDto>> RefreshTokenAsync(string refreshToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
        if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            return ApiResponse<TokenDto>.Fail("Token de rafraîchissement invalide ou expiré.");

        var token = await GenerateTokenAsync(user);
        user.RefreshToken = token.RefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return ApiResponse<TokenDto>.Ok(token);
    }

    public async Task<ApiResponse<bool>> ChangePasswordAsync(string userId, ChangePasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return ApiResponse<bool>.Fail("Utilisateur non trouvé.");

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
            return ApiResponse<bool>.Fail("Erreur.", result.Errors.Select(e => e.Description).ToList());

        return ApiResponse<bool>.Ok(true, "Mot de passe modifié avec succès.");
    }

    public async Task<ApiResponse<bool>> ResetPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return ApiResponse<bool>.Ok(true); // Don't reveal if user exists
        // In production, send email with reset token
        return ApiResponse<bool>.Ok(true, "Email de réinitialisation envoyé.");
    }

    private async Task<TokenDto> GenerateTokenAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var cabinet = user.CabinetId.HasValue ? await _db.Cabinets.FindAsync(user.CabinetId) : null;
        var client = user.ClientId.HasValue ? await _db.Clients.FindAsync(user.ClientId) : null;

        // Live effective permissions from DB (employee override or role fallback)
        var permissions = await _permissions.GetEffectivePermissionsAsync(user.Id);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.FullName),
            new("CabinetId", user.CabinetId?.ToString() ?? ""),
            new("ClientId", user.ClientId?.ToString() ?? ""),
            new("PreferredLanguage", user.PreferredLanguage),
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        claims.AddRange(permissions
            .Select(p => p.ToLowerInvariant())
            .Distinct()
            .Select(p => new Claim("Permission", p)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiration = DateTime.UtcNow.AddHours(8);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expiration,
            signingCredentials: creds
        );

        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        return new TokenDto(
            new JwtSecurityTokenHandler().WriteToken(token),
            refreshToken,
            expiration,
            new UserInfoDto(user.Id, user.Email!, user.FullName, roles.FirstOrDefault(), user.CabinetId, cabinet?.Name, user.ProfileImageUrl, user.PreferredLanguage, user.ClientId, client?.RaisonSociale)
        );
    }
}
