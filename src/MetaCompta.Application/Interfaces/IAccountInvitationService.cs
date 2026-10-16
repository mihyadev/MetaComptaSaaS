using MetaCompta.Application.DTOs;
using MetaCompta.Domain.Common;

namespace MetaCompta.Application.Interfaces;

public enum ActivationPortal
{
    BackOffice,
    Extranet
}

public interface IAccountInvitationService
{
    /// <summary>Generate confirmation token and send activation email for an existing user.</summary>
    Task<ApiResponse<bool>> SendActivationEmailAsync(ApplicationUser user, ActivationPortal portal, string? contextLabel = null);

    /// <summary>Resend invitation if the account is not yet email-confirmed.</summary>
    Task<ApiResponse<bool>> ResendActivationAsync(string userId, int cabinetId, ActivationPortal portal);

    Task<ApiResponse<ActivationInfoDto>> GetActivationInfoAsync(string userId, string token);
    Task<ApiResponse<bool>> ActivateAccountAsync(ActivateAccountDto dto);
}
