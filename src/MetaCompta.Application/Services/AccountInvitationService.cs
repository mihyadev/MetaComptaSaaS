using System.Net;
using System.Text.Encodings.Web;
using MetaCompta.Application.DTOs;
using MetaCompta.Application.Interfaces;
using MetaCompta.Application.Options;
using MetaCompta.Domain.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MetaCompta.Application.Services;

public class AccountInvitationService : IAccountInvitationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly PortalUrlsOptions _portals;
    private readonly ILogger<AccountInvitationService> _logger;

    public AccountInvitationService(
        UserManager<ApplicationUser> userManager,
        IEmailSender emailSender,
        IOptions<PortalUrlsOptions> portals,
        ILogger<AccountInvitationService> logger)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _portals = portals.Value;
        _logger = logger;
    }

    public async Task<ApiResponse<bool>> SendActivationEmailAsync(
        ApplicationUser user, ActivationPortal portal, string? contextLabel = null)
    {
        if (user.EmailConfirmed)
            return ApiResponse<bool>.Ok(true, "Compte déjà activé.");

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebUtility.UrlEncode(token);
        var baseUrl = portal == ActivationPortal.Extranet
            ? _portals.Extranet.TrimEnd('/')
            : _portals.BackOffice.TrimEnd('/');
        var link = $"{baseUrl}/Account/Activate?userId={Uri.EscapeDataString(user.Id)}&token={encodedToken}";

        var subject = "Activez votre compte MetaCompta";
        var safeName = HtmlEncoder.Default.Encode(user.FullName);
        var safeContext = string.IsNullOrWhiteSpace(contextLabel)
            ? ""
            : $"<p style=\"margin:0 0 16px;color:#475569;font-size:14px;\">Espace : <strong>{HtmlEncoder.Default.Encode(contextLabel)}</strong></p>";

        var html = $@"
<!DOCTYPE html>
<html lang=""fr"">
<body style=""margin:0;padding:0;background:#f6f7fb;font-family:Segoe UI,Arial,sans-serif;"">
  <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""background:#f6f7fb;padding:32px 16px;"">
    <tr><td align=""center"">
      <table role=""presentation"" width=""560"" style=""background:#ffffff;border-radius:12px;padding:32px;border:1px solid #e4e6ef;"">
        <tr><td>
          <h1 style=""margin:0 0 8px;font-size:22px;color:#0f172a;"">Bienvenue sur MetaCompta</h1>
          <p style=""margin:0 0 16px;color:#475569;font-size:14px;"">Bonjour {safeName},</p>
          {safeContext}
          <p style=""margin:0 0 20px;color:#475569;font-size:14px;line-height:1.5;"">
            Un compte a été créé pour vous. Cliquez sur le bouton ci-dessous pour confirmer votre email
            et choisir votre mot de passe. Ce lien expire dans <strong>48 heures</strong>.
          </p>
          <p style=""margin:0 0 28px;"">
            <a href=""{link}"" style=""display:inline-block;background:#0073ea;color:#fff;text-decoration:none;
              font-weight:600;padding:12px 22px;border-radius:999px;font-size:14px;"">Activer mon compte</a>
          </p>
          <p style=""margin:0;color:#94a3b8;font-size:12px;line-height:1.4;"">
            Si le bouton ne fonctionne pas, copiez ce lien dans votre navigateur :<br/>
            <span style=""word-break:break-all;color:#64748b;"">{HtmlEncoder.Default.Encode(link)}</span>
          </p>
        </td></tr>
      </table>
    </td></tr>
  </table>
</body>
</html>";

        try
        {
            await _emailSender.SendEmailAsync(user.Email!, subject, html);
            return ApiResponse<bool>.Ok(true, "Email d'activation envoyé.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Impossible d'envoyer l'email d'activation à {Email}", user.Email);
            return ApiResponse<bool>.Fail(
                "Compte créé, mais l'email d'activation n'a pas pu être envoyé. Utilisez « Renvoyer l'invitation ».");
        }
    }

    public async Task<ApiResponse<bool>> ResendActivationAsync(string userId, int cabinetId, ActivationPortal portal)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || user.CabinetId != cabinetId)
            return ApiResponse<bool>.Fail("Utilisateur introuvable.");

        if (user.EmailConfirmed)
            return ApiResponse<bool>.Fail("Ce compte est déjà activé.");

        if (portal == ActivationPortal.Extranet && !user.ClientId.HasValue)
            return ApiResponse<bool>.Fail("Cet utilisateur n'est pas un compte Extranet.");

        if (portal == ActivationPortal.BackOffice && user.ClientId.HasValue)
            return ApiResponse<bool>.Fail("Cet utilisateur n'est pas un employé cabinet.");

        string? context = null;
        if (portal == ActivationPortal.Extranet)
            context = "Extranet client";
        else
            context = "BackOffice cabinet";

        return await SendActivationEmailAsync(user, portal, context);
    }

    public async Task<ApiResponse<ActivationInfoDto>> GetActivationInfoAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return ApiResponse<ActivationInfoDto>.Fail("Lien d'activation invalide.");

        if (user.EmailConfirmed && await _userManager.HasPasswordAsync(user))
        {
            return ApiResponse<ActivationInfoDto>.Ok(new ActivationInfoDto
            {
                UserId = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                AlreadyActivated = true,
                TokenValid = false
            }, "Compte déjà activé.");
        }

        var valid = !string.IsNullOrWhiteSpace(token)
            && await _userManager.VerifyUserTokenAsync(
                user,
                _userManager.Options.Tokens.EmailConfirmationTokenProvider,
                UserManager<ApplicationUser>.ConfirmEmailTokenPurpose,
                token);

        return ApiResponse<ActivationInfoDto>.Ok(new ActivationInfoDto
        {
            UserId = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            AlreadyActivated = false,
            TokenValid = valid
        }, valid ? null : "Le lien d'activation est invalide ou a expiré.");
    }

    public async Task<ApiResponse<bool>> ActivateAccountAsync(ActivateAccountDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.UserId) || string.IsNullOrWhiteSpace(dto.Token))
            return ApiResponse<bool>.Fail("Lien d'activation invalide.");

        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password != dto.ConfirmPassword)
            return ApiResponse<bool>.Fail("Les mots de passe ne correspondent pas.");

        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user == null)
            return ApiResponse<bool>.Fail("Utilisateur introuvable.");

        if (user.EmailConfirmed && await _userManager.HasPasswordAsync(user))
            return ApiResponse<bool>.Fail("Ce compte est déjà activé. Connectez-vous.");

        if (!user.EmailConfirmed)
        {
            var confirm = await _userManager.ConfirmEmailAsync(user, dto.Token);
            if (!confirm.Succeeded)
                return ApiResponse<bool>.Fail(
                    "Lien d'activation invalide ou expiré.",
                    confirm.Errors.Select(e => e.Description).ToList());
        }

        if (await _userManager.HasPasswordAsync(user))
        {
            var remove = await _userManager.RemovePasswordAsync(user);
            if (!remove.Succeeded)
                return ApiResponse<bool>.Fail("Impossible de définir le mot de passe.",
                    remove.Errors.Select(e => e.Description).ToList());
        }

        var addPwd = await _userManager.AddPasswordAsync(user, dto.Password);
        if (!addPwd.Succeeded)
            return ApiResponse<bool>.Fail(
                "Mot de passe invalide.",
                addPwd.Errors.Select(e => e.Description).ToList());

        user.IsActive = true;
        user.EmailConfirmed = true;
        await _userManager.UpdateAsync(user);

        return ApiResponse<bool>.Ok(true, "Compte activé. Vous pouvez vous connecter.");
    }
}
