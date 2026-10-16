using System.Globalization;
using MetaCompta.Application.DTOs;
using MetaCompta.Application.Interfaces;
using MetaCompta.Domain.Common;
using MetaCompta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MetaCompta.Application.Services;

/// <summary>
/// Generates draft honoraires invoices for Valide/Actif contracts based on FrequenceFacturation.
/// Idempotent per (contrat, PeriodeDebut, PeriodeFin).
/// </summary>
public class FacturationSchedulerService : IFacturationSchedulerService
{
    public const string SystemUserId = "system-facturation";

    private readonly MetaComptaDbContext _db;
    private readonly ICommercialService _commercial;
    private readonly ILogger<FacturationSchedulerService> _logger;

    public FacturationSchedulerService(
        MetaComptaDbContext db,
        ICommercialService commercial,
        ILogger<FacturationSchedulerService> logger)
    {
        _db = db;
        _commercial = commercial;
        _logger = logger;
    }

    public async Task<AutoFacturationResultDto> GenerateDueInvoicesAsync()
    {
        var today = GetTodayCasablanca();
        var result = new AutoFacturationResultDto { RunDate = today };

        var activeStatuses = RefCodes.StatutContrat.ActiveStatuses;
        var contrats = await _db.ContratsCommerciaux
            .AsNoTracking()
            .Where(c => !c.IsDeleted
                && activeStatuses.Contains(c.Statut)
                && c.FrequenceFacturation != null
                && c.FrequenceFacturation != ""
                && c.DateDebutContrat != null
                && c.DateDebutContrat.Value.Date <= today
                && (c.DateFinContrat == null || c.DateFinContrat.Value.Date >= today))
            .Include(c => c.Lignes)
            .ToListAsync();

        result.ContractsScanned = contrats.Count;

        foreach (var contrat in contrats)
        {
            var freq = RefCodes.FrequenceFacturation.Normalize(contrat.FrequenceFacturation);
            if (freq == null)
            {
                result.Skipped++;
                continue;
            }

            if (contrat.Lignes == null || !contrat.Lignes.Any())
            {
                result.Skipped++;
                continue;
            }

            var (periodStart, periodEnd, periodLabel) = ComputeCurrentPeriod(today, freq);

            // Contract must have started before or during this period
            if (contrat.DateDebutContrat!.Value.Date > periodEnd)
            {
                result.Skipped++;
                continue;
            }

            var exists = await _db.Factures.AnyAsync(f =>
                f.ContratId == contrat.Id
                && !f.IsDeleted
                && f.Statut != RefCodes.StatutFacture.Annulee
                && f.PeriodeDebut != null
                && f.PeriodeFin != null
                && f.PeriodeDebut.Value.Date == periodStart
                && f.PeriodeFin.Value.Date == periodEnd);

            if (exists)
            {
                result.Skipped++;
                continue;
            }

            var dto = new FactureCreateDto
            {
                ContratId = contrat.Id,
                Objet = $"Honoraires {periodLabel} — {contrat.Reference}",
                PeriodeDebut = periodStart,
                PeriodeFin = periodEnd,
                DateEcheance = periodEnd.AddDays(30),
                Notes = $"Génération automatique ({freq})"
            };

            try
            {
                var created = await _commercial.CreateFactureAsync(dto, contrat.CabinetId, SystemUserId);
                if (created.Success)
                {
                    result.Created++;
                    result.CreatedReferences.Add(created.Data?.Reference ?? $"contrat:{contrat.Id}");
                    _logger.LogInformation(
                        "Auto-facture créée {Ref} pour contrat {Contrat} période {Start:yyyy-MM-dd}–{End:yyyy-MM-dd}",
                        created.Data?.Reference, contrat.Reference, periodStart, periodEnd);
                }
                else
                {
                    result.Errors++;
                    _logger.LogWarning(
                        "Auto-facture échouée pour contrat {Contrat}: {Message}",
                        contrat.Reference, created.Message);
                }
            }
            catch (Exception ex)
            {
                result.Errors++;
                _logger.LogError(ex, "Auto-facture exception pour contrat {ContratId}", contrat.Id);
            }
        }

        _logger.LogInformation(
            "Auto-facturation terminée: scanned={Scanned}, created={Created}, skipped={Skipped}, errors={Errors}",
            result.ContractsScanned, result.Created, result.Skipped, result.Errors);

        return result;
    }

    /// <summary>Calendar period containing <paramref name="today"/> for the given frequency.</summary>
    public static (DateTime Start, DateTime End, string Label) ComputeCurrentPeriod(DateTime today, string frequency)
    {
        today = today.Date;
        var fr = CultureInfo.GetCultureInfo("fr-FR");
        var freq = RefCodes.FrequenceFacturation.Normalize(frequency) ?? RefCodes.FrequenceFacturation.Mensuel;

        return freq switch
        {
            RefCodes.FrequenceFacturation.Trimestriel => ComputeQuarter(today),
            RefCodes.FrequenceFacturation.Annuel => (
                new DateTime(today.Year, 1, 1),
                new DateTime(today.Year, 12, 31),
                today.Year.ToString(CultureInfo.InvariantCulture)),
            _ => (
                new DateTime(today.Year, today.Month, 1),
                new DateTime(today.Year, today.Month, 1).AddMonths(1).AddDays(-1),
                new DateTime(today.Year, today.Month, 1).ToString("MMMM yyyy", fr))
        };
    }

    private static (DateTime Start, DateTime End, string Label) ComputeQuarter(DateTime today)
    {
        var q = (today.Month - 1) / 3;
        var start = new DateTime(today.Year, q * 3 + 1, 1);
        var end = start.AddMonths(3).AddDays(-1);
        return (start, end, $"T{q + 1} {today.Year}");
    }

    private static DateTime GetTodayCasablanca()
    {
        try
        {
            var tzId = OperatingSystem.IsWindows() ? "Morocco Standard Time" : "Africa/Casablanca";
            var tz = TimeZoneInfo.FindSystemTimeZoneById(tzId);
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).Date;
        }
        catch
        {
            return DateTime.UtcNow.Date;
        }
    }
}
