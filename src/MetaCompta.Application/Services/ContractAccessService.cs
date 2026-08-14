using System.Text.Json;
using MetaCompta.Application.DTOs;
using MetaCompta.Application.Interfaces;
using MetaCompta.Domain.Common;
using MetaCompta.Domain.Entities;
using MetaCompta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MetaCompta.Application.Services;

public class ContractAccessService : IContractAccessService
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

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

    public async Task<ContractAccessContextDto> GetContractAccessContextAsync(
        int clientId, int cabinetId, int? exerciceComptableId = null)
    {
        var today = BusinessToday();
        var activeStatuses = RefCodes.StatutContrat.ActiveStatuses;

        var contracts = await _db.Set<ContratCommercial>()
            .AsNoTracking()
            .Where(c => c.ClientId == clientId && c.CabinetId == cabinetId && !c.IsDeleted)
            .Select(c => new { c.Statut, c.DateDebutContrat, c.DateFinContrat, c.ServicesActivesJson })
            .ToListAsync();

        int? exoAnnee = null;
        DateTime? exoDebut = null;
        DateTime? exoFin = null;
        if (exerciceComptableId is > 0)
        {
            var exo = await _db.ExercicesComptables.AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == exerciceComptableId && e.ClientId == clientId && e.CabinetId == cabinetId);
            if (exo != null)
            {
                exoAnnee = exo.Annee;
                exoDebut = exo.DateDebut.Date;
                exoFin = exo.DateFin.Date;
            }
        }

        var svcQuery = _db.ServiceContrats.AsNoTracking()
            .Where(s => s.ClientId == clientId && s.Client!.CabinetId == cabinetId && s.Actif);

        if (exoAnnee.HasValue && exoDebut.HasValue && exoFin.HasValue)
        {
            var annee = exoAnnee.Value;
            var debut = exoDebut.Value;
            var fin = exoFin.Value;
            var exoId = exerciceComptableId!.Value;
            svcQuery = svcQuery.Where(s =>
                s.ExerciceComptableId == exoId
                || (s.ExerciceComptableId == null
                    && (s.ExerciceFiscal == null || s.ExerciceFiscal == annee)
                    && s.DateDebut.Date <= fin
                    && (s.DateFin == null || s.DateFin.Value.Date >= debut)));
        }
        else
        {
            svcQuery = svcQuery.Where(s =>
                s.DateDebut.Date <= today && (s.DateFin == null || s.DateFin.Value.Date >= today));
        }

        var fromRows = await svcQuery.Select(s => s.TypeService).Distinct().ToListAsync();

        var manageWindowContracts = contracts.Where(c =>
            activeStatuses.Contains(c.Statut)
            && (c.DateDebutContrat == null || c.DateDebutContrat.Value.Date <= today)
            && (c.DateFinContrat == null || c.DateFinContrat.Value.Date >= today)).ToList();

        // JSON fallback only when no ServiceContrats for the selected exercice
        var fromJson = new List<string>();
        if (fromRows.Count == 0)
        {
            foreach (var c in manageWindowContracts)
                fromJson.AddRange(ParseServicesFromJson(c.ServicesActivesJson));
        }

        var activeServices = fromRows
            .Concat(fromJson)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

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

        var manageOk = manageWindowContracts.Count > 0;

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

    private static IEnumerable<string> ParseServicesFromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) yield break;
        List<ContratServiceActiveLine>? lines;
        try
        {
            lines = JsonSerializer.Deserialize<List<ContratServiceActiveLine>>(json, JsonOpts);
        }
        catch
        {
            yield break;
        }

        if (lines == null) yield break;
        foreach (var line in lines)
        {
            if (!string.IsNullOrWhiteSpace(line.TypeService))
                yield return line.TypeService.Trim();
        }
    }
}
