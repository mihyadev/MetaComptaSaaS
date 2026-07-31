using MetaCompta.Application.DTOs;
using MetaCompta.Application.Interfaces;
using MetaCompta.Domain.Common;
using MetaCompta.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MetaCompta.Application.Services;

public class ClientUserService : IClientUserService
{
    private readonly MetaComptaDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public ClientUserService(
        MetaComptaDbContext db,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<ApiResponse<List<ClientUserDto>>> GetClientUsersAsync(int clientId, int cabinetId)
    {
        // Verify client belongs to cabinet
        var client = await _db.Clients.FirstOrDefaultAsync(c => c.Id == clientId && c.CabinetId == cabinetId && !c.IsDeleted);
        if (client == null)
            return ApiResponse<List<ClientUserDto>>.Fail("Client introuvable.");

        var users = await _userManager.Users
            .Where(u => u.ClientId == clientId && u.CabinetId == cabinetId)
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new ClientUserDto
            {
                Id = u.Id,
                Email = u.Email!,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Telephone = u.PhoneNumber,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt
            })
            .ToListAsync();

        return ApiResponse<List<ClientUserDto>>.Ok(users);
    }

    public async Task<ApiResponse<ClientUserDto>> GetClientUserAsync(string userId, int clientId, int cabinetId)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.ClientId == clientId && u.CabinetId == cabinetId);

        if (user == null)
            return ApiResponse<ClientUserDto>.Fail("Utilisateur introuvable.");

        return ApiResponse<ClientUserDto>.Ok(new ClientUserDto
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Telephone = user.PhoneNumber,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        });
    }

    public async Task<ApiResponse<ClientUserDto>> CreateClientUserAsync(
        ClientUserCreateDto dto, int clientId, int cabinetId, string createdBy)
    {
        // Verify client belongs to cabinet
        var client = await _db.Clients.FirstOrDefaultAsync(c => c.Id == clientId && c.CabinetId == cabinetId && !c.IsDeleted);
        if (client == null)
            return ApiResponse<ClientUserDto>.Fail("Client introuvable.");

        // Validate email uniqueness
        var existing = await _userManager.FindByEmailAsync(dto.Email);
        if (existing != null)
            return ApiResponse<ClientUserDto>.Fail("Un utilisateur avec cet email existe déjà.");

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PhoneNumber = dto.Telephone,
            ClientId = clientId,
            CabinetId = cabinetId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            PreferredLanguage = "fr-FR"
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
            return ApiResponse<ClientUserDto>.Fail(
                "Erreur lors de la création du compte.",
                createResult.Errors.Select(e => e.Description).ToList());

        // Assign role "Client" for ExtranetAccess
        if (!await _roleManager.RoleExistsAsync("Client"))
            await _roleManager.CreateAsync(new ApplicationRole { Name = "Client", CabinetId = cabinetId });
        await _userManager.AddToRoleAsync(user, "Client");

        return ApiResponse<ClientUserDto>.Ok(new ClientUserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Telephone = user.PhoneNumber,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = null
        }, "Utilisateur Extranet créé avec succès.");
    }

    public async Task<ApiResponse<ClientUserDto>> UpdateClientUserAsync(
        string userId, ClientUserUpdateDto dto, int clientId, int cabinetId, string updatedBy)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.ClientId == clientId && u.CabinetId == cabinetId);

        if (user == null)
            return ApiResponse<ClientUserDto>.Fail("Utilisateur introuvable.");

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.PhoneNumber = dto.Telephone;
        user.IsActive = dto.IsActive;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return ApiResponse<ClientUserDto>.Fail("Erreur lors de la mise à jour.");

        return ApiResponse<ClientUserDto>.Ok(new ClientUserDto
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Telephone = user.PhoneNumber,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        }, "Utilisateur mis à jour avec succès.");
    }

    public async Task<ApiResponse<bool>> ToggleClientUserActiveAsync(
        string userId, int clientId, int cabinetId, string updatedBy)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.ClientId == clientId && u.CabinetId == cabinetId);

        if (user == null)
            return ApiResponse<bool>.Fail("Utilisateur introuvable.");

        user.IsActive = !user.IsActive;
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return ApiResponse<bool>.Fail("Erreur lors de la mise à jour.");

        var status = user.IsActive ? "activé" : "désactivé";
        return ApiResponse<bool>.Ok(true, $"Utilisateur {status} avec succès.");
    }
}
