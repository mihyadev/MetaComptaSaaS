using MetaCompta.Application.DTOs;
using MetaCompta.Application.Interfaces;
using MetaCompta.Domain.Common;
using MetaCompta.Domain.Entities;
using MetaCompta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MetaCompta.Application.Services;

public class ContractAccessService : IContractAccessService
{
    private readonly MetaComptaDbContext _db;

    public ContractAccessService(MetaComptaDbContext db) => _db = db;

    /// <summary>Calendar "today" for Morocco fiduciary dates (date-only fields).</summary>
    internal static DateTime BusinessToday()
    {
        try
        {
            var tzId = OperatingSystem.IsWindows() ? "Morocco Standard Time" : "Africa/Casablanca";
            var tz = TimeZoneInfo.FindSystemTimeZoneById(tzId);
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).Date;
        }
        catch
        {
            return DateTime.UtcNow.AddHours(1).Date; // Morocco UTC+1 fallback
        }
    }

    public Task<bool> HasManageAccessAsync(int clientId, int cabinetId)
        => IsManageContractQuery(clientId, cabinetId).AnyAsync();

    public async Task<ContractAccessContextDto> GetContractAccessContextAsync(int clientId, int cabinetId)
    {
        var today = BusinessToday();
        var activeStatuses = RefCodes.StatutContrat.ActiveStatuses;

        var contracts = await _db.Set<ContratCommercial>()
            .AsNoTracking()
            .Where(c => c.ClientId == clientId && c.CabinetId == cabinetId && !c.IsDeleted)
            .Select(c => new { c.Statut, c.DateDebutContrat, c.DateFinContrat })
            .ToListAsync();

        var activeServices = await _db.ServiceContrats
            .AsNoTracking()
            .Where(s => s.ClientId == clientId && s.Client!.CabinetId == cabinetId
                && s.Actif && s.DateDebut.Date <= today && (s.DateFin == null || s.DateFin.Value.Date >= today))
            .Select(s => s.TypeService)
            .Distinct()
            .ToListAsync();

        if (contracts.Count == 0)
        {
            return new ContractAccessContextDto
            {
                Mode = ContractAccessMode.Consult,
                Reason = ContractAccessReason.NoContract,
                ReasonLabel = "Aucun contrat associé à ce client.",
                ActiveServices = activeServices
            };
        }

        var manageOk = contracts.Any(c =>
            activeStatuses.Contains(c.Statut)
            && (c.DateDebutContrat == null || c.DateDebutContrat.Value.Date <= today)
            && (c.DateFinContrat == null || c.DateFinContrat.Value.Date >= today));

        if (manageOk)
        {
            return new ContractAccessContextDto
            {
                Mode = ContractAccessMode.Manage,
                Reason = ContractAccessReason.Ok,
                ReasonLabel = "Contrat validé — mode gestion.",
                ActiveServices = activeServices
            };
        }

        var hasSignedAwaiting = contracts.Any(c => c.Statut == RefCodes.StatutContrat.Signe);
        var hasSuspended = contracts.Any(c => c.Statut == RefCodes.StatutContrat.Suspendu);
        var hasExpiredWindow = contracts.Any(c =>
            activeStatuses.Contains(c.Statut)
            && c.DateFinContrat != null
            && c.DateFinContrat.Value.Date < today);
        var hasNotStarted = contracts.Any(c =>
            activeStatuses.Contains(c.Statut)
            && c.DateDebutContrat != null
            && c.DateDebutContrat.Value.Date > today);

        string reason;
        string label;
        if (hasNotStarted)
        {
            reason = ContractAccessReason.Expired;
            label = "Le contrat n'a pas encore commencé. Mode consultation uniquement.";
        }
        else if (hasExpiredWindow)
        {
            reason = ContractAccessReason.Expired;
            label = "Le contrat est expiré (date de fin dépassée). Mode consultation uniquement.";
        }
        else if (hasSuspended && !hasSignedAwaiting)
        {
            reason = ContractAccessReason.Suspended;
            label = "Le contrat est suspendu. Mode consultation uniquement.";
        }
        else if (hasSignedAwaiting)
        {
            reason = ContractAccessReason.AwaitingValidation;
            label = "Contrat signé en attente de validation. Mode consultation uniquement.";
        }
        else
        {
            reason = ContractAccessReason.NoContract;
            label = "Aucun contrat actif ou validé. Mode consultation uniquement.";
        }

        return new ContractAccessContextDto
        {
            Mode = ContractAccessMode.Consult,
            Reason = reason,
            ReasonLabel = label,
            ActiveServices = activeServices
        };
    }

    private IQueryable<ContratCommercial> IsManageContractQuery(int clientId, int cabinetId)
    {
        var today = BusinessToday();
        var activeStatuses = RefCodes.StatutContrat.ActiveStatuses;
        return _db.Set<ContratCommercial>()
            .Where(c => c.ClientId == clientId && c.CabinetId == cabinetId
                && activeStatuses.Contains(c.Statut) && !c.IsDeleted
                && (c.DateDebutContrat == null || c.DateDebutContrat <= today)
                && (c.DateFinContrat == null || c.DateFinContrat >= today));
    }
}
